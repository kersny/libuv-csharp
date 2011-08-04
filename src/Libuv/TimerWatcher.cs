using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class TimerWatcher : Watcher {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_timer_cb(IntPtr tmr, int status);
		[DllImport("uv")]
		internal static extern int uv_timer_init(IntPtr timer);
		[DllImport("uv")]
		internal static extern int uv_timer_start(IntPtr timer, uv_timer_cb cb, double after, double repeat);
		[DllImport("uv")]
		internal static extern int uv_timer_stop(IntPtr timer);
		[DllImport("uv")]
		internal static extern void uv_timer_again(IntPtr timer);
		[DllImport("uv")]
		internal static extern void uv_timer_set_repeat(IntPtr timer, double time);

		private static uv_timer_cb unmanaged_callback;

		static TimerWatcher()
		{
			unmanaged_callback = StaticCallback;
		}

		private TimeSpan delay;
		private TimeSpan repeat;
		public TimeSpan Repeat {
			get { return repeat; }
			set {
				if (value < TimeSpan.Zero)
					throw new ArgumentException("value");
				repeat = value;
				uv_timer_set_repeat(this._handle, repeat.TotalMilliseconds);
			}
		}
		public TimerWatcher(TimeSpan repeat, Action<int> callback)
			: this (TimeSpan.Zero, repeat, callback)
		{
		}
		public TimerWatcher(TimeSpan after, TimeSpan repeat, Action<int> callback) : base()
		{
			this.repeat = repeat;
			this.delay = after;

			this._handle = Marshal.AllocHGlobal(Sizes.TimerWatcherSize);
			uv_timer_init(this._handle);
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this, GCHandleType.Pinned);
			handle.data = GCHandle.ToIntPtr(this.me);
			this.callback = callback;
		}
		private static void StaticCallback(IntPtr watcher, int status)
		{
			var handle = (uv_handle_t)Marshal.PtrToStructure(watcher, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (TimerWatcher)instance.Target;
			watcher_instance.callback(status);
		}
		public void Start()
		{
			uv_timer_start(this._handle, unmanaged_callback, delay.TotalMilliseconds, repeat.TotalMilliseconds);
		}
		public void Stop()
		{
			uv_timer_stop(this._handle);
		}
		public void Again()
		{
			uv_timer_again(this._handle);
		}
	}
}
