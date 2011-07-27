using System;
using System.Runtime.InteropServices;

namespace Libuv {
	enum uv_err_code
	{
		UV_UNKNOWN = -1,
		UV_OK = 0,
		UV_EOF,
		UV_EACCESS,
		UV_EAGAIN,
		UV_EADDRINUSE,
		UV_EADDRNOTAVAIL,
		UV_EAFNOSUPPORT,
		UV_EALREADY,
		UV_EBADF,
		UV_EBUSY,
		UV_ECONNABORTED,
		UV_ECONNREFUSED,
		UV_ECONNRESET,
		UV_EDESTADDRREQ,
		UV_EFAULT,
		UV_EHOSTUNREACH,
		UV_EINTR,
		UV_EINVAL,
		UV_EISCONN,
		UV_EMFILE,
		UV_ENETDOWN,
		UV_ENETUNREACH,
		UV_ENFILE,
		UV_ENOBUFS,
		UV_ENOMEM,
		UV_ENONET,
		UV_ENOPROTOOPT,
		UV_ENOTCONN,
		UV_ENOTSOCK,
		UV_ENOTSUP,
		UV_EPROTO,
		UV_EPROTONOSUPPORT,
		UV_EPROTOTYPE,
		UV_ETIMEDOUT,
		UV_ECHARSET,
		UV_EAIFAMNOSUPPORT,
		UV_EAINONAME,
		UV_EAISERVICE,
		UV_EAISOCKTYPE,
		UV_ESHUTDOWN
	}
	struct uv_err_t
	{
		public uv_err_code code;
		int sys_errno_;
	}
	public abstract class TcpEntity : IDisposable {
		internal IntPtr _handle;
		internal GCHandle gc_handle;
		public TcpEntity()
		{
			this._handle = manos_uv_tcp_t_create();
			int err = uv_tcp_init(this._handle);
			if (err != 0) throw new Exception(uv_last_error().code.ToString());
		}
		private void Cleanup()
		{
			manos_uv_destroy(this._handle);
			this._handle = IntPtr.Zero;
		}
		~TcpEntity()
		{
			Cleanup();
		}
		public void Dispose()
		{
			Cleanup();
			System.GC.SuppressFinalize(this);
		}
		public void Close()
		{
			int err = uv_close(this._handle, (x) => {
				this.Dispose();
			});
			if (err != 0) throw new Exception(uv_last_error().code.ToString());
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_connection_cb(IntPtr socket, int status);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_close_cb(IntPtr socket);
		[DllImport ("uvwrap")]
		internal static extern int uv_tcp_init (IntPtr socket);
		[DllImport ("uvwrap")]
		internal static extern IntPtr manos_uv_tcp_t_create();
		[DllImport ("uvwrap")]
		internal static extern void manos_uv_destroy(IntPtr uv_tcp_t_ptr);
		[DllImport ("uvwrap")]
		internal static extern int uv_close(IntPtr handle, uv_close_cb cb);
		[DllImport ("uvwrap")]
		internal static extern uv_err_t uv_last_error();
	}
}
