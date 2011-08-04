using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public struct uv_handle_t {
		public uv_handle_type type;
		public uv_close_cb close_cb;
		public IntPtr data;
	}
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_close_cb(IntPtr handle);
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
	public static class Sizes {
		public static readonly int PrepareWatcherSize = 64;
		public static readonly int IdleWatcherSize = 64;
		public static readonly int CheckWatcherSize = 64;
		public static readonly int TimerWatcherSize = 80;
	}
}
