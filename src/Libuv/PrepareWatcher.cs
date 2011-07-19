using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class PrepareWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_prepare_cb(IntPtr socket, int status);
		[DllImport("uvwrap")]
		internal static extern IntPtr create_prepare_watcher();
		[DllImport("uvwrap")]
		internal static extern int uv_prepare_init(HandleRef prepare);
		[DllImport("uvwrap")]
		internal static extern int uv_prepare_start(HandleRef prepare, uv_prepare_cb cb);
		[DllImport("uvwrap")]
		internal static extern int uv_prepare_stop(HandleRef prepare);
		private uv_prepare_cb _callback;
		public PrepareWatcher(uv_prepare_cb callback)
		{
			IntPtr h = create_prepare_watcher();
			this._handle = new HandleRef(this, h);
			uv_prepare_init(this._handle);
			this._callback = callback;
		}
		~PrepareWatcher()
		{
			Cleanup();
		}
		private void Cleanup()
		{
			destroy_watcher(this._handle);
			this._handle = new HandleRef(this, IntPtr.Zero);
		}
		public override void Dispose()
		{
			Cleanup();
			GC.SuppressFinalize(this);

		}
		public void Start()
		{
			uv_prepare_start(this._handle, this._callback);
		}
		public void Stop()
		{
			uv_prepare_stop(this._handle);
		}
	}
}
