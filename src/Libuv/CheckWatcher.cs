using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class CheckWatcher : Watcher {
		[DllImport("uv")]
		internal static extern int uv_check_init(IntPtr check);
		[DllImport("uv")]
		internal static extern int uv_check_start(IntPtr check, uv_watcher_cb cb);
		[DllImport("uv")]
		internal static extern int uv_check_stop(IntPtr check);

		public CheckWatcher(Action callback) : base(callback, Sizes.CheckWatcher) {}

		internal override int InitImpl()
		{
			return uv_check_init(this._handle);
		}
		internal override int StartImpl()
		{
			return uv_check_start(this._handle, StaticCallback);
		}
		internal override int StopImpl()
		{
			return uv_check_stop(this._handle);
		}
	}
}
