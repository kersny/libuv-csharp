using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class PrepareWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_prepare_cb(IntPtr socket, int status);
		[DllImport("uvwrap")]
		internal static extern IntPtr create_prepare_watcher(IntPtr myself, uv_prepare_cb cb);
		[DllImport("uvwrap")]
		internal static extern int uv_prepare_init(IntPtr prepare);
		[DllImport("uvwrap")]
		internal static extern int manos_prepare_start(IntPtr prepare);
		[DllImport("uvwrap")]
		internal static extern int uv_prepare_stop(IntPtr prepare);
		private static uv_prepare_cb unmanaged_callback;
		private Action<PrepareWatcher, int> callback;
		static PrepareWatcher()
		{
			unmanaged_callback = StaticCallback;
		}
		public PrepareWatcher(Action<PrepareWatcher, int> callback)
		{
			watcher = create_prepare_watcher(GCHandle.ToIntPtr(gc_handle), unmanaged_callback);
			uv_prepare_init(this.watcher);
			this.callback = callback;
		}
		private static void StaticCallback(IntPtr watcher, int status)
		{
			var handle = GCHandle.FromIntPtr(watcher);
			var watcher_instance = (PrepareWatcher)handle.Target;
			watcher_instance.callback(watcher_instance, status);
		}
		public void Start()
		{
			manos_prepare_start(this.watcher);
		}
		public void Stop()
		{
			uv_prepare_stop(this.watcher);
		}
		~PrepareWatcher()
		{
			Cleanup();
		}
		private void Cleanup()
		{
			gc_handle.Free();
			destroy_watcher(this.watcher);
		}
		public override void Dispose()
		{
			Cleanup();
			GC.SuppressFinalize(this);

		}
	}
}
