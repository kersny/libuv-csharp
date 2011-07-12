#include "libuv/include/uv.h"
#include <stdlib.h>

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
int manos_uv_read_start(uv_stream_t* handle, manos_uv_read_cb manos_read_cb)
{
	handle->data = manos_read_cb;
	uv_read_start(handle, alloc_cb, read_cb);
}
int manos_uv_tcp_t_size()
{
	return sizeof(uv_tcp_t);
}
