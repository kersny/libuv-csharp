using System.Runtime.InteropServices;
using System;

namespace Libuv {
	public abstract class Watcher : IDisposable {
		[DllImport("uvwrap")]
		internal static extern void destroy_watcher(IntPtr watcher);
		internal IntPtr watcher;
		internal GCHandle gc_handle;
		internal Watcher()
		{
			gc_handle = GCHandle.Alloc(this);
		}
		~Watcher()
		{
			Cleanup();
		}
		private void Cleanup()
		{
			gc_handle.Free();
			destroy_watcher(this.watcher);
		}
		public void Dispose()
		{
			Cleanup();
			GC.SuppressFinalize(this);
		}
	}
}
