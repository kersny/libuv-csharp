using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class PrepareWatcher : Watcher {
		[DllImport("uv")]
		internal static extern int uv_prepare_init(IntPtr prepare);
		[DllImport("uv")]
		internal static extern int uv_prepare_start(IntPtr prepare, uv_watcher_cb cb);
		[DllImport("uv")]
		internal static extern int uv_prepare_stop(IntPtr prepare);

		public PrepareWatcher(Action callback) : base(callback, Sizes.PrepareWatcher) {}

		internal override int InitImpl()
		{
			return uv_prepare_init(this._handle);
		}
		internal override int StartImpl()
		{
			return uv_prepare_start(this._handle, StaticCallback);
		}
		internal override int StopImpl()
		{
			return uv_prepare_stop(this._handle);
		}
	}
}
