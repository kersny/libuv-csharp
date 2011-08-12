using System.Runtime.InteropServices;
using System;

namespace Libuv {
	public abstract class Watcher : IDisposable {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void uv_watcher_cb(IntPtr watcher, int status);

		internal Action callback;
		internal IntPtr _handle;
		internal GCHandle me;

		public Watcher(Action callback, int size)
		{
			this.callback = callback;
			this._handle = Marshal.AllocHGlobal(size);
			Util.CheckError(this.InitImpl());
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
		}
		internal static void StaticCallback(IntPtr watcher, int status)
		{
			Util.CheckError(status);
			var handle = (uv_handle_t)Marshal.PtrToStructure(watcher, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (Watcher)instance.Target;
			watcher_instance.callback();
		}
		public void Start()
		{
			Util.CheckError(this.StartImpl());
		}
		public void Stop()
		{
			Util.CheckError(this.StopImpl());
		}

		internal abstract int InitImpl();
		internal abstract int StartImpl();
		internal abstract int StopImpl();

		~Watcher()
		{
			Cleanup();
		}
		private void Cleanup()
		{
			me.Free();
			Marshal.FreeHGlobal(this._handle);
		}
		public void Dispose()
		{
			Cleanup();
			GC.SuppressFinalize(this);
		}
	}
}
