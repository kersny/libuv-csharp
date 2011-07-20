using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class TimerWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_timer_cb(IntPtr tmr, int status);
		[DllImport("uvwrap")]
		internal static extern IntPtr create_timer_watcher(uv_timer_cb cb, IntPtr myself);
		[DllImport("uvwrap")]
		internal static extern int uv_timer_init(IntPtr timer);
		[DllImport("uvwrap")]
		internal static extern int manos_timer_start(IntPtr timer, double after, double repeat);
		[DllImport("uvwrap")]
		internal static extern int uv_timer_stop(IntPtr timer);
		[DllImport("uvwrap")]
		internal static extern void uv_timer_again(IntPtr timer);
		[DllImport("uvwrap")]
		internal static extern void uv_timer_set_repeat(IntPtr timer, double time);
		private static uv_timer_cb unmanaged_callback;
		private Action<TimerWatcher, int> callback;
		private TimeSpan delay;
		private TimeSpan repeat;
		public TimeSpan Repeat {
			get { return repeat; }
			set {
				if (value < TimeSpan.Zero)
					throw new ArgumentException("value");
				repeat = value;
				uv_timer_set_repeat(this.watcher, repeat.TotalMilliseconds);
			}
		}
		static TimerWatcher()
		{
			unmanaged_callback = StaticCallback;
		}
		public TimerWatcher(TimeSpan repeat, Action<TimerWatcher, int> callback)
			: this (TimeSpan.Zero, repeat, callback)
		{
		}
		public TimerWatcher(TimeSpan after, TimeSpan repeat, Action<TimerWatcher, int> callback) : base()
		{
			this.callback = callback;
			this.repeat = repeat;
			this.delay = after;
			watcher = create_timer_watcher(unmanaged_callback, GCHandle.ToIntPtr(gc_handle));
			uv_timer_init(this.watcher);
		}
		private static void StaticCallback(IntPtr watcher, int status)
		{
			var handle = GCHandle.FromIntPtr(watcher);
			var watcher_instance = (TimerWatcher)handle.Target;
			watcher_instance.callback(watcher_instance, status);
		}
		public void Start()
		{
			manos_timer_start(this.watcher, delay.TotalMilliseconds, repeat.TotalMilliseconds);
		}
		public void Stop()
		{
			uv_timer_stop(this.watcher);
		}
		public void Again()
		{
			uv_timer_again(this.watcher);
		}
	}
}
