#include "../../deps/libuv/include/uv.h"
#include <stdlib.h>

typedef struct {
	uv_req_t req;
	uv_buf_t buf;
} write_req_t;

typedef void (*manos_uv_read_cb)(uv_stream_t* tcp, long nread, char* data);
typedef void (*manos_uv_eof_cb)();

typedef struct {
	manos_uv_read_cb read;
	manos_uv_eof_cb done;
} read_settings;

int manos_uv_tcp_bind(uv_tcp_t *handle, const char* host, int port)
{
	uv_tcp_bind(handle, uv_ip4_addr(host, port));
}

int manos_uv_tcp_connect(uv_connect_t* req, uv_tcp_t* handle, const char* host, int port, uv_connect_cb cb)
{
	uv_tcp_connect(req, handle, uv_ip4_addr(host, port), cb);
}
static uv_buf_t alloc_cb(uv_stream_t* handle, size_t suggested_size)
{
	uv_buf_t buf;
	buf.base = (char*) malloc(suggested_size);
	buf.len = suggested_size;
	return buf;
}
static void on_close(uv_handle_t* peer) {
	read_settings *sett = peer->data;
	sett->done();
	free(sett);
}
static void after_shutdown(uv_shutdown_t* req, int status) {
	uv_close((uv_handle_t *)req->handle, on_close);
	free(req);
}
static void read_cb(uv_stream_t* handle, ssize_t nread, uv_buf_t buf)
{
	uv_shutdown_t* req;
	read_settings* sett;
	if (nread < 0) {
		/* error or eof */
		if (buf.base) {
			free(buf.base);
		}
		req = malloc(sizeof(uv_shutdown_t));
		uv_shutdown(req, handle, after_shutdown);
		return;
	}
	if (nread == 0) {
		free(buf.base);
		return;
	}
	sett = handle->data;
	sett->read(handle, nread, buf.base);
	free(buf.base);
	//((manos_uv_read_cb)handle->data)(handle, nread, buf.base);
}
static void after_write(uv_write_t* req, int status)
{
	free(req);
}
int manos_uv_write(uv_tcp_t* handle, unsigned char* data, int length)
{
	uv_buf_t buf;
	uv_write_t *wr = malloc(sizeof(uv_write_t));
	buf.base = data;
	buf.len = length;
	return uv_write(wr, (uv_stream_t *)handle, &buf, 1, after_write);
}
int manos_uv_read_start(uv_stream_t* handle, manos_uv_read_cb manos_read_cb, manos_uv_eof_cb manos_done_cb)
{
	read_settings *sett = malloc(sizeof(read_settings));
	sett->read = manos_read_cb;	
	sett->done = manos_done_cb;
	handle->data = sett;
	uv_read_start(handle, alloc_cb, read_cb);
}
uv_tcp_t* manos_uv_tcp_t_create()
{
	return malloc(sizeof(uv_tcp_t));
}
uv_connect_t* manos_uv_connect_t_create()
{
	return malloc(sizeof(uv_connect_t));
}
void manos_uv_destroy(uv_tcp_t* ptr)
{
	free(ptr);
}
uv_prepare_t* create_prepare_watcher()
{
	return malloc(sizeof(uv_prepare_t));
}
void destroy_watcher(uv_prepare_t* ptr)
{
	free(ptr);
}
