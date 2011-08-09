using System.Runtime.InteropServices;
using System;

namespace Libuv {
	public abstract class Watcher : IDisposable {
		internal Action callback;
		internal IntPtr _handle;
		internal GCHandle me;

		internal Watcher()
		{
		}
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
		//	Cleanup();
		//	GC.SuppressFinalize(this);
		}
	}
}
