using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Libuv {
	public class PipeSocket {
		static uv_buf_t alloc_cb(IntPtr pipe, IntPtr size)
		{
			uv_buf_t buf;
			buf.data = Marshal.AllocHGlobal((int)size);
			#if __MonoCS__
			buf.len =  size;
			#else
			buf.len = (ulong)size;
			#endif
			return buf;
		}
		static void unmanaged_read_cb(IntPtr stream, IntPtr nread, uv_buf_t buf)
		{
			int size = (int)nread;
			if (size < 0) {
				if ((int)buf.data != 0)
					Marshal.FreeHGlobal(buf.data);
				IntPtr shutdown = Marshal.AllocHGlobal(Sizes.ShutdownT);
				uv_shutdown(shutdown, stream, after_shutdown);
				return;
			}
			if (size == 0) {
				Marshal.FreeHGlobal(buf.data);
				return;
			}
			byte[] data = new byte[size];
			Marshal.Copy(buf.data, data, 0, size);
			var handle = (uv_handle_t)Marshal.PtrToStructure(stream, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (PipeSocket)instance.Target;
			watcher_instance.HandleRead(data, size);
			Marshal.FreeHGlobal(buf.data);
		}
		static void unmanaged_connect_cb(IntPtr connection, int status)
		{
			if (status != 0) {
				throw new Exception(uv_strerror(uv_last_error()));
			}
			var tmp = (uv_connect_t)Marshal.PtrToStructure(connection, typeof(uv_connect_t));
			var handle = (uv_handle_t)Marshal.PtrToStructure(tmp.handle, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (PipeSocket)instance.Target;
			watcher_instance.HandleConnect();
			uv_read_start(tmp.handle, alloc_cb, unmanaged_read_cb);
		}
		static void after_shutdown(IntPtr shutdown, int status)
		{
			// It'd be very difficult to get handle out of req
			// So we'll store it in data & cast to uv_req_t
			uv_shutdown_t tmp = (uv_shutdown_t)Marshal.PtrToStructure(shutdown, typeof(uv_shutdown_t));
			uv_close(tmp.handle, on_close);
			Marshal.FreeHGlobal(shutdown);
		}
		static void on_close(IntPtr socket)
		{
			var handle = (uv_handle_t)Marshal.PtrToStructure(socket, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (PipeSocket)instance.Target;
			//dont think this is what should happen here
			watcher_instance.me.Free();
			Marshal.FreeHGlobal(socket);
		}
		static void after_write(IntPtr write_req, int status)
		{
			var req = (uv_req_t)Marshal.PtrToStructure(write_req, typeof(uv_req_t));
			//var handle = GCHandle.FromIntPtr(req.data);
			//handle.Free();
			Marshal.FreeHGlobal(write_req);
		}
		private IntPtr _handle;
		public event Action<byte[]> OnData;
		public event Action OnConnect;
		private GCHandle me;
		private IntPtr connection;
		public PipeSocket()
		{
			this._handle = Marshal.AllocHGlobal(Sizes.PipeT);
			uv_pipe_init(this._handle);
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
			this.connection = Marshal.AllocHGlobal(Sizes.ConnectT);
			//can't attach anything to connect_t, it would get nulled
		}
		public void Connect(string path, Action OnConnect)
		{
			uv_pipe_connect(this.connection, this._handle, path, unmanaged_connect_cb);
			this.OnConnect += OnConnect;
		}
		public PipeSocket(IntPtr ServerHandle)
		{
			this._handle = Marshal.AllocHGlobal(Sizes.PipeT);
			uv_pipe_init(this._handle);
			uv_accept(ServerHandle, this._handle);
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
			uv_read_start(this._handle, alloc_cb, unmanaged_read_cb);
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
			uv_write(write_request, this._handle, buf, 1, after_write);
		}
		private void HandleConnect()
		{
			if (OnConnect != null)
			{
				OnConnect();
			}
		}
		private void HandleRead(byte[] data, int nread)
		{
			if (OnData != null)
			{
				OnData(data);
			}
		}
		public void Close()
		{
			uv_close(this._handle, on_close);
		}
		[DllImport("uv")]
		internal static extern int uv_pipe_init(IntPtr prepare);
		[DllImport ("uv")]
		internal static extern int uv_accept(IntPtr socket, IntPtr stream);
		[DllImport ("uv")]
		internal static extern int uv_read_start(IntPtr stream, uv_alloc_cb alloc_cb, uv_read_cb read);
		[DllImport ("uv")]
		internal static extern int uv_write(IntPtr req, IntPtr handle, uv_buf_t[] bufs, int bufcnt, uv_write_cb cb);
		[DllImport ("uv")]
		internal static extern int uv_shutdown(IntPtr shutdown, IntPtr handle, uv_shutdown_cb cb);
		[DllImport ("uv")]
		internal static extern int uv_close(IntPtr handle, uv_close_cb cb);
		[DllImport ("uv")]
		internal static extern int uv_pipe_connect(IntPtr connect, IntPtr tcp_handle, string path, uv_connect_cb cb);
		[DllImport ("uv")]
		public static extern uv_err_t uv_last_error();
		[DllImport ("uv")]
		public static extern string uv_strerror(uv_err_t err);
	}
}
