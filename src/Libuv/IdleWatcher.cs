using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class IdleWatcher : Watcher {
		[DllImport("uv")]
		internal static extern int uv_idle_init(IntPtr idle);
		[DllImport("uv")]
		internal static extern int uv_idle_start(IntPtr idle, uv_watcher_cb cb);
		[DllImport("uv")]
		internal static extern int uv_idle_stop(IntPtr idle);

		public IdleWatcher(Action callback) : base(callback, Sizes.IdleWatcher) {}

		internal override int InitImpl()
		{
			return uv_idle_init(this._handle);
		}
		internal override int StartImpl()
		{
			return uv_idle_start(this._handle, StaticCallback);
		}
		internal override int StopImpl()
		{
			return uv_idle_stop(this._handle);
		}
	}
}
