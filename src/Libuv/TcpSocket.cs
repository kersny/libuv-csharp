using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Libuv {
	public class TcpSocket {
		static uv_buf_t alloc_cb(IntPtr tcp, IntPtr size)
		{
			uv_buf_t buf;
			buf.data = Marshal.AllocHGlobal(size);
			buf.len =  size;
			return buf;
		}
		static void unmanaged_read_cb(IntPtr stream, IntPtr nread, uv_buf_t buf)
		{
			int size = (int)nread;
			if (size < 0) {
				if ((int)buf.data != 0)
					Marshal.FreeHGlobal(buf.data);
				IntPtr shutdown = Marshal.AllocHGlobal(Sizes.ShutdownTSize);
				uv_req_t tmp = (uv_req_t)Marshal.PtrToStructure(shutdown, typeof(uv_req_t));
				tmp.data = stream;
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
			var watcher_instance = (TcpSocket)instance.Target;
			watcher_instance.HandleRead(data, size);
			Marshal.FreeHGlobal(buf.data);
		}
		static void unmanaged_connect_cb(IntPtr connection, int status)
		{
			var handle = (uv_handle_t)Marshal.PtrToStructure(connection, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (TcpSocket)instance.Target;
			uv_read_start(watcher_instance._handle, alloc_cb, unmanaged_read_cb);
		}
		static void after_shutdown(IntPtr shutdown, int status)
		{
			// It'd be very difficult to get handle out of req
			// So we'll store it in data & cast to uv_req_t
			uv_req_t tmp = (uv_req_t)Marshal.PtrToStructure(shutdown, typeof(uv_req_t));
			uv_close(tmp.data, on_close);
			Marshal.FreeHGlobal(shutdown);
		}
		static void on_close(IntPtr socket)
		{
			var handle = (uv_handle_t)Marshal.PtrToStructure(socket, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (TcpSocket)instance.Target;
			//dont think this is what should happen here
			watcher_instance.me.Free();
			Marshal.FreeHGlobal(socket);
		}
		static void after_write(IntPtr write_req, int status)
		{
			var req = (uv_req_t)Marshal.PtrToStructure(write_req, typeof(uv_req_t));
			var handle = GCHandle.FromIntPtr(req.data);
			handle.Free();
			Marshal.FreeHGlobal(write_req);
		}
		private IntPtr _handle;
		public event Action<byte[]> OnData;
		private GCHandle me;
		private IntPtr connection;
		public TcpSocket()
		{
			this._handle = Marshal.AllocHGlobal(Sizes.TcpTSize);
			uv_tcp_init(this._handle);		
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this, GCHandleType.Pinned);
			handle.data = GCHandle.ToIntPtr(this.me);
			this.connection = Marshal.AllocHGlobal(Sizes.TcpTSize);
			var connhandle = (uv_handle_t)Marshal.PtrToStructure(this.connection, typeof(uv_handle_t));
			connhandle.data = handle.data;
		}
		public void Connect(IPEndPoint endpoint, Action OnConnect)
		{
			var info = uv_ip4_addr(endpoint.Address.ToString(), endpoint.Port);
			uv_tcp_connect(this.connection, this._handle, info, unmanaged_connect_cb);
		}
		public TcpSocket(IntPtr ServerHandle)
		{
			uv_accept(ServerHandle, this._handle);
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this, GCHandleType.Pinned);
			handle.data = GCHandle.ToIntPtr(this.me);
			uv_read_start(this._handle, alloc_cb, unmanaged_read_cb);
		}
		public void Write(byte[] data, int length)
		{
			IntPtr write_request = Marshal.AllocHGlobal(68);
			var dataptrhandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr dat = dataptrhandle.AddrOfPinnedObject();
			uv_buf_t[] buf = new uv_buf_t[1];
			buf[0].data = dat;
			buf[0].len = (IntPtr)length;
			var req = (uv_req_t)Marshal.PtrToStructure(write_request, typeof(uv_req_t));
			req.data = dat;
			uv_write(write_request, this._handle, buf, 1, after_write);
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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_shutdown_cb(IntPtr req, int status);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate uv_buf_t uv_alloc_cb(IntPtr stream, IntPtr suggested_size);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_read_cb(IntPtr req, IntPtr nread, uv_buf_t buf);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_write_cb(IntPtr req, int status);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_connect_cb(IntPtr conn, int status);
		[DllImport("uv")]
		internal static extern int uv_tcp_init(IntPtr prepare);
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
		internal static extern int uv_tcp_connect(IntPtr connect, IntPtr tcp_handle, sockaddr_in address, uv_connect_cb cb);
		[DllImport("uv")]
		internal static extern sockaddr_in uv_ip4_addr(string ip, int port);
	}
}
