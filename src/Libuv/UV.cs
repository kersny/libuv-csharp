using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Libuv {
	public static class Util {
		public static void CheckError(int code)
		{
			if (code != 0) throw new Exception(uv_strerror(uv_last_error()));
		}
		[DllImport ("uv")]
		public static extern uv_err_t uv_last_error();
		[DllImport ("uv")]
		public static extern string uv_strerror(uv_err_t err);
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct uv_handle_t {
		public uv_handle_type type;
		public IntPtr close_cb;
		public IntPtr data;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct uv_req_t {
		public uv_req_type type;
		public IntPtr data;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct uv_connect_t {
		public uv_req_type type;
		public IntPtr data;
		#if !__MonoCS__
		NativeOverlapped overlapped;
		IntPtr queued_bytes;
		uv_err_t error;
		IntPtr next_req;
		#endif
		public IntPtr cb;
		public IntPtr handle;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct uv_shutdown_t {
		public uv_req_type type;
		public IntPtr data;
		#if !__MonoCS__
		NativeOverlapped overlapped;
		IntPtr queued_bytes;
		uv_err_t error;
		IntPtr next_req;
		#endif
		public IntPtr handle;
		public IntPtr cb;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct uv_err_t
	{
		public uv_err_code code;
		int sys_errno_;
	}
	public enum uv_err_code
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
	public enum uv_handle_type {
		UV_UNKNOWN_HANDLE = 0,
		UV_TCP,
		UV_NAMED_PIPE,
		UV_TTY,
		UV_FILE,
		UV_TIMER,
		UV_PREPARE,
		UV_CHECK,
		UV_IDLE,
		UV_ASYNC,
		UV_ARES_TASK,
		UV_ARES_EVENT,
		UV_GETADDRINFO,
		UV_PROCESS
	}
	public enum uv_req_type {
		UV_UNKNOWN_REQ = 0,
		UV_CONNECT,
		UV_ACCEPT,
		UV_READ,
		UV_WRITE,
		UV_SHUTDOWN,
		UV_WAKEUP,
		UV_REQ_TYPE_PRIVATE
	}
	public static class Sizes {
		#if __MonoCS__
		public static readonly int PrepareWatcher = 64;
		public static readonly int IdleWatcher = 64;
		public static readonly int CheckWatcher = 64;
		public static readonly int TimerWatcher = 80;
		public static readonly int TcpT = 152;
		public static readonly int PipeT = 156;
		public static readonly int ShutdownT = 16;
		public static readonly int ConnectT = 24;
		public static readonly int WriteT = 68;
		public static readonly int ProcessT = 88;
		#else
		public static readonly int PrepareWatcher = 40;
		public static readonly int IdleWatcher = 40;
		public static readonly int CheckWatcher = 40;
		public static readonly int TimerWatcher = 72;
		public static readonly int TcpT = 436;
		public static readonly int ShutdownT = 52;
		public static readonly int ConnectT = 52;
		public static readonly int WriteT = 52;
		#endif
	}
	[StructLayout(LayoutKind.Sequential)]
	#if __MonoCS__
	public struct uv_buf_t
	{
		public IntPtr data;
		public IntPtr len;
	}
	#else
	public struct uv_buf_t
	{
		public ulong len;
		public IntPtr data;
	}
	#endif
	// From: http://www.elitepvpers.com/forum/co2-programming/159327-advanced-winsock-c.html
	[StructLayout(LayoutKind.Sequential, Size=16)]
	public struct sockaddr_in
	{
		public const int Size = 16;

		public short sin_family;
		public ushort sin_port;
		public struct in_addr
		{
			public uint S_addr;
			public struct _S_un_b
			{
				public byte s_b1, s_b2, s_b3, s_b4;
			}
			public _S_un_b S_un_b;
			public struct _S_un_w
			{
				public ushort s_w1, s_w2;
			}
			public _S_un_w S_un_w;
		}
		public in_addr sin_addr;
	}
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_shutdown_cb(IntPtr req, int status);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate uv_buf_t uv_alloc_cb(IntPtr stream, IntPtr suggested_size);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_read_cb(IntPtr req, IntPtr nread, uv_buf_t buf);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_write_cb(IntPtr req, int status);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_connect_cb(IntPtr conn, int status);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_close_cb(IntPtr conn);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_connection_cb(IntPtr server, int status);
}
