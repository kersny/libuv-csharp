using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class PrepareWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_prepare_cb(IntPtr prepare, int status);
		[DllImport("uv")]
		internal static extern int uv_prepare_init(IntPtr prepare);
		[DllImport("uv")]
		internal static extern int uv_prepare_start(IntPtr prepare, uv_prepare_cb cb);
		[DllImport("uv")]
		internal static extern int uv_prepare_stop(IntPtr prepare);

		private static uv_prepare_cb unmanaged_callback;

		static PrepareWatcher()
		{
			unmanaged_callback = StaticCallback;
		}
		
		public PrepareWatcher(Action<int> callback)
		{
			this._handle = Marshal.AllocHGlobal(Sizes.PrepareWatcherSize);
			uv_prepare_init(this._handle);
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this, GCHandleType.Pinned);
			handle.data = GCHandle.ToIntPtr(this.me);
			this.callback = callback;
		}
		private static void StaticCallback(IntPtr watcher, int status)
		{
			var handle = (uv_handle_t)Marshal.PtrToStructure(watcher, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (PrepareWatcher)instance.Target;
			watcher_instance.callback(status);
		}
		public void Start()
		{
			uv_prepare_start(this._handle, unmanaged_callback);
		}
		public void Stop()
		{
			uv_prepare_stop(this._handle);
		}
	}
}
