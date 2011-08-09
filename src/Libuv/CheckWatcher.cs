using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class CheckWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_check_cb(IntPtr socket, int status);
		[DllImport("uv")]
		internal static extern int uv_check_init(IntPtr check);
		[DllImport("uv")]
		internal static extern int uv_check_start(IntPtr check, uv_check_cb cb);
		[DllImport("uv")]
		internal static extern int uv_check_stop(IntPtr check);

		private static uv_check_cb unmanaged_callback;

		static CheckWatcher()
		{
			unmanaged_callback = StaticCallback;
		}
		public CheckWatcher(Action callback)
		{
			this._handle = Marshal.AllocHGlobal(Sizes.CheckWatcherSize);
			uv_check_init(this._handle);
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this, GCHandleType.Pinned);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
			this.callback = callback;
		}
		private static void StaticCallback(IntPtr watcher, int status)
		{
			var handle = (uv_handle_t)Marshal.PtrToStructure(watcher, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = instance.Target as CheckWatcher;
			watcher_instance.callback();
		}
		public void Start()
		{
			uv_check_start(this._handle, unmanaged_callback);
		}
		public void Stop()
		{
			uv_check_stop(this._handle);
		}
	}
}
