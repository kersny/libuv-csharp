using System.Runtime.InteropServices;
using System;

namespace Libuv {
	public abstract class Watcher : IDisposable {
		[DllImport("uvwrap")]
		internal static extern void destroy_watcher(IntPtr watcher);
		internal IntPtr watcher;
		internal GCHandle gc_handle;
		public abstract void Dispose();
		internal Watcher()
		{
			gc_handle = GCHandle.Alloc(this);
		}
	}
}
