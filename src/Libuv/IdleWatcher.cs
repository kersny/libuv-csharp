using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class IdleWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_idle_cb(IntPtr socket, int status);
		[DllImport("uvwrap")]
		internal static extern IntPtr create_idle_watcher(IntPtr myself, uv_idle_cb cb);
		[DllImport("uvwrap")]
		internal static extern int uv_idle_init(IntPtr idle);
		[DllImport("uvwrap")]
		internal static extern int manos_idle_start(IntPtr idle);
		[DllImport("uvwrap")]
		internal static extern int uv_idle_stop(IntPtr idle);
		private static uv_idle_cb unmanaged_callback;
		private Action<IdleWatcher, int> callback;
		static IdleWatcher()
		{
			unmanaged_callback = StaticCallback;
		}
		public IdleWatcher(Action<IdleWatcher, int> callback)
		{
			watcher = create_idle_watcher(GCHandle.ToIntPtr(gc_handle), unmanaged_callback);
			uv_idle_init(this.watcher);
			this.callback = callback;
		}
		private static void StaticCallback(IntPtr watcher, int status)
		{
			var handle = GCHandle.FromIntPtr(watcher);
			var watcher_instance = (IdleWatcher)handle.Target;
			watcher_instance.callback(watcher_instance, status);
		}
		public void Start()
		{
			manos_idle_start(this.watcher);
		}
		public void Stop()
		{
			uv_idle_stop(this.watcher);
		}
	}
}
