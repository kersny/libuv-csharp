using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class CheckWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_check_cb(IntPtr socket, int status);
		[DllImport("uvwrap")]
		internal static extern IntPtr create_check_watcher(IntPtr myself, uv_check_cb cb);
		[DllImport("uvwrap")]
		internal static extern int uv_check_init(IntPtr check);
		[DllImport("uvwrap")]
		internal static extern int manos_check_start(IntPtr check);
		[DllImport("uvwrap")]
		internal static extern int uv_check_stop(IntPtr check);
		private static uv_check_cb unmanaged_callback;
		private Action<CheckWatcher, int> callback;
		static CheckWatcher()
		{
			unmanaged_callback = StaticCallback;
		}
		public CheckWatcher(Action<CheckWatcher, int> callback)
		{
			watcher = create_check_watcher(GCHandle.ToIntPtr(gc_handle), unmanaged_callback);
			uv_check_init(this.watcher);
			this.callback = callback;
		}
		private static void StaticCallback(IntPtr watcher, int status)
		{
			var handle = GCHandle.FromIntPtr(watcher);
			var watcher_instance = (CheckWatcher)handle.Target;
			watcher_instance.callback(watcher_instance, status);
		}
		public void Start()
		{
			manos_check_start(this.watcher);
		}
		public void Stop()
		{
			uv_check_stop(this.watcher);
		}
	}
}
