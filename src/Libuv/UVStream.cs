using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class UVStream {
		private IntPtr handle;
		private GCHandle gchandle;
		public event Action<byte[]> OnRead;
		public event Action OnClose;
		// Pass an already init-ed uv_handle_t subclass to stream from
		public UVStream(IntPtr Handle) {
			this.gchandle = GCHandle.Alloc(this);
			this.handle = Handle;
			//attach handle of ourself to the c struct so we can access instance callbacks in static callbacks
			//static callbacks are used as to not force the clr to thunk a new one for every callback on every object
			var handle = (uv_handle_t)Marshal.PtrToStructure(this.handle, typeof(uv_handle_t));
			this.gchandle = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.gchandle);
			Marshal.StructureToPtr(handle, this.handle, true);
		}
		// Instance Methods
		public void ReadStart()
		{
			Util.CheckError(uv_read_start(this.handle, static_alloc, static_read));
		}
		public void ReadStop()
		{
			Util.CheckError(uv_read_stop(this.handle));
		}
		public void Write(byte[] data, int length)
		{
			IntPtr write_request = Marshal.AllocHGlobal(Sizes.WriteT);
			var dataptrhandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			// This is not being freed, which needs to be fixed
			IntPtr dat = dataptrhandle.AddrOfPinnedObject();
			uv_buf_t[] buf = new uv_buf_t[1];
			buf[0].data = dat;
			#if __MonoCS__
			buf[0].len = (IntPtr)length;
			#else
			buf[0].len = (ulong)length;
			#endif
			var req = (uv_req_t)Marshal.PtrToStructure(write_request, typeof(uv_req_t));
			req.data = dat;
			Marshal.StructureToPtr(req, write_request, true);
			Util.CheckError(uv_write(write_request, this.handle, buf, 1, after_write));
		}
		private void HandleRead(byte[] data)
		{
			if (OnRead != null)
			{
				OnRead(data);
			}
		}
		private void HandleClose()
		{
			if (OnClose != null)
			{
				OnClose();
			}
		}
		// Static Callbacks
		static uv_buf_t static_alloc(IntPtr tcp, IntPtr size)
		{
			uv_buf_t buf;
			buf.data = Marshal.AllocHGlobal((int)size);
			#if __MonoCS__
			buf.len = size;
			#else
			buf.len = (ulong)size;
			#endif
			return buf;
		}
		static void static_read(IntPtr stream, IntPtr nread, uv_buf_t buf)
		{
			int size = (int)nread;
			if (size < 0) {
				if ((int)buf.data != 0)
					Marshal.FreeHGlobal(buf.data);
				IntPtr shutdown = Marshal.AllocHGlobal(Sizes.ShutdownT);
				Util.CheckError(uv_shutdown(shutdown, stream, after_shutdown));
				return;
			}
			if (size == 0) {
				Marshal.FreeHGlobal(buf.data);
				return;
			}
			byte[] data = new byte[size];
			Marshal.Copy(buf.data, data, 0, size);
			var handle = (uv_handle_t)Marshal.PtrToStructure(stream, typeof(uv_handle_t));
			var instance_gchandle = GCHandle.FromIntPtr(handle.data);
			var stream_instance = (UVStream)instance_gchandle.Target;
			stream_instance.HandleRead(data);
			Marshal.FreeHGlobal(buf.data);
		}
		static void after_shutdown(IntPtr shutdown, int status)
		{
			Util.CheckError(status);
			var tmp = (uv_shutdown_t)Marshal.PtrToStructure(shutdown, typeof(uv_shutdown_t));
			var handle = (uv_handle_t)Marshal.PtrToStructure(tmp.handle, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (UVStream)instance.Target;
			watcher_instance.HandleClose();
			Marshal.FreeHGlobal(shutdown);
		}
		static void after_write(IntPtr write_req, int status)
		{
			Util.CheckError(status);
			var req = (uv_req_t)Marshal.PtrToStructure(write_req, typeof(uv_req_t));
			//var handle = GCHandle.FromIntPtr(req.data);
			//handle.Free();
			Marshal.FreeHGlobal(write_req);
		}
		// Externs
		[DllImport ("uv")]
		internal static extern int uv_read_start(IntPtr stream, uv_alloc_cb alloc_cb, uv_read_cb read);
		[DllImport ("uv")]
		internal static extern int uv_read_stop(IntPtr stream);
		[DllImport ("uv")]
		internal static extern int uv_write(IntPtr req, IntPtr handle, uv_buf_t[] bufs, int bufcnt, uv_write_cb cb);
		[DllImport ("uv")]
		internal static extern int uv_shutdown(IntPtr req, IntPtr handle, uv_shutdown_cb cb);
	}
}
