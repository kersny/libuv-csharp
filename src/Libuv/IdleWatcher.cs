using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class IdleWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_idle_cb(IntPtr socket, int status);
		[DllImport("uv")]
		internal static extern int uv_idle_init(IntPtr idle);
		[DllImport("uv")]
		internal static extern int uv_idle_start(IntPtr idle, uv_idle_cb cb);
		[DllImport("uv")]
		internal static extern int uv_idle_stop(IntPtr idle);

		private static uv_idle_cb unmanaged_callback;

		static IdleWatcher()
		{
			unmanaged_callback = StaticCallback;
		}
		public IdleWatcher(Action callback)
		{
			this._handle = Marshal.AllocHGlobal(Sizes.IdleWatcherSize);
			uv_idle_init(this._handle);
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
			this.callback = callback;
		}
		private static void StaticCallback(IntPtr watcher, int status)
		{
			var handle = (uv_handle_t)Marshal.PtrToStructure(watcher, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (IdleWatcher)instance.Target;
			watcher_instance.callback();
		}
		public void Start()
		{
			uv_idle_start(this._handle, unmanaged_callback);
		}
		public void Stop()
		{
			uv_idle_stop(this._handle);
		}
	}
}
