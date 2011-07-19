using System.Runtime.InteropServices;
using System;

namespace Libuv {
	public abstract class Watcher : IDisposable {
		[DllImport("uvwrap")]
		internal static extern void destroy_watcher(HandleRef watcher);
		internal HandleRef _handle;
		public abstract void Dispose();
	}
}
