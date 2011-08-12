using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class TimerWatcher : Watcher {
		[DllImport("uv")]
		internal static extern int uv_timer_init(IntPtr timer);
		[DllImport("uv")]
		internal static extern int uv_timer_start(IntPtr timer, uv_watcher_cb cb, double after, double repeat);
		[DllImport("uv")]
		internal static extern int uv_timer_stop(IntPtr timer);
		[DllImport("uv")]
		internal static extern int uv_timer_again(IntPtr timer);
		[DllImport("uv")]
		internal static extern int uv_timer_set_repeat(IntPtr timer, double time);

		private TimeSpan delay;
		private TimeSpan repeat;
		public TimeSpan Repeat {
			get { return repeat; }
			set {
				if (value < TimeSpan.Zero)
					throw new ArgumentException("value");
				repeat = value;
				Util.CheckError(uv_timer_set_repeat(this._handle, repeat.TotalMilliseconds));
			}
		}
		public TimerWatcher(TimeSpan repeat, Action callback)
			: this (TimeSpan.Zero, repeat, callback)
		{
		}
		public TimerWatcher(TimeSpan after, TimeSpan repeat, Action callback) : base(callback, Sizes.TimerWatcher)
		{
			this.repeat = repeat;
			this.delay = after;

		}
		internal override int InitImpl()
		{
			return uv_timer_init(this._handle);
		}
		internal override int StartImpl()
		{
			return uv_timer_start(this._handle, StaticCallback, delay.TotalMilliseconds, repeat.TotalMilliseconds);
		}
		internal override int StopImpl()
		{
			return uv_timer_stop(this._handle);
		}
		public void Again()
		{
			Util.CheckError(uv_timer_again(this._handle));
		}
	}
}
