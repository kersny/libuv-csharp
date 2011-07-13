#include "libuv/include/uv.h"
#include <stdlib.h>

typedef struct {
	uv_req_t req;
	uv_buf_t buf;
} write_req_t;
typedef void (*manos_uv_read_cb)(uv_stream_t* tcp, long nread, char* data);

int manos_uv_tcp_bind(uv_tcp_t *handle, const char* host, int port)
{
	uv_tcp_bind(handle, uv_ip4_addr(host, port));
}

static uv_buf_t alloc_cb(uv_stream_t* handle, size_t suggested_size)
{
	uv_buf_t buf;
	buf.base = (char*) malloc(suggested_size);
	buf.len = suggested_size;
	return buf;
}
static void on_close(uv_handle_t* peer) {
	free(peer);
}
static void after_shutdown(uv_req_t* req, int status) {
	uv_close(req->handle, on_close);
	free(req);
}
static void read_cb(uv_stream_t* handle, ssize_t nread, uv_buf_t buf)
{
	uv_req_t* req;
	if (nread < 0) {
		/* error or eof */
		if (buf.base) {
			free(buf.base);
		}
		req = (uv_req_t*) malloc(sizeof *req);
		uv_req_init(req, (uv_handle_t*)handle, (void *(*)(void *))after_shutdown);
		uv_shutdown(req);
		return;
	}
	if (nread == 0) {
		free(buf.base);
		return;
	}
	((manos_uv_read_cb)handle->data)(handle, nread, buf.base);
}
static void after_write(uv_req_t* req, int status)
{
	write_req_t* wr;
	wr = (write_req_t *)req;
	//free(wr->buf.base);
	free(wr);
}
int manos_uv_write(uv_tcp_t* handle, unsigned char* data, int length)
{
	write_req_t *wr = malloc(sizeof(write_req_t));
	uv_req_init(&wr->req, (uv_handle_t *) handle, (void*(*)(void *))after_write);
	wr->buf.base = data;
	wr->buf.len = length;
	return uv_write(&wr->req, &wr->buf, 1);
}
int manos_uv_read_start(uv_stream_t* handle, manos_uv_read_cb manos_read_cb)
{
	handle->data = manos_read_cb;
	uv_read_start(handle, alloc_cb, read_cb);
}
uv_tcp_t* manos_uv_tcp_t_create()
{
	return malloc(sizeof(uv_tcp_t));
}
void manos_uv_tcp_t_destroy(uv_tcp_t* ptr)
{
	free(ptr);
}
