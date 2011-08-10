using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Libuv {
	public class TcpServer {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_connection_cb(IntPtr server, int status);
		[DllImport("uv")]
		internal static extern int uv_tcp_init(IntPtr prepare);
		[DllImport("uv")]
		internal static extern int uv_tcp_bind(IntPtr prepare, sockaddr_in address);
		[DllImport("uv")]
		internal static extern int uv_listen(IntPtr stream, int backlog, uv_connection_cb cb);
		[DllImport("uv")]
		internal static extern sockaddr_in uv_ip4_addr(string ip, int port);

		private static uv_connection_cb unmanaged_callback;

		static TcpServer()
		{
			unmanaged_callback = StaticCallback;
		}

		private Action<TcpSocket> callback;
		private IntPtr _handle;
		private GCHandle me;

		public TcpServer(Action<TcpSocket> callback)
		{
			this.callback = callback;
			this._handle = Marshal.AllocHGlobal(Sizes.TcpTSize);
			uv_tcp_init(this._handle);
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
		}
		public void Listen(IPEndPoint endpoint)
		{
			var info = uv_ip4_addr(endpoint.Address.ToString(), endpoint.Port);
			uv_tcp_bind(this._handle, info);
			uv_listen(this._handle, 128, unmanaged_callback);
		}
		public static void StaticCallback(IntPtr server_ptr, int status)
		{
			var handle = (uv_handle_t)Marshal.PtrToStructure(server_ptr, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var server = (TcpServer)instance.Target;
			server.callback(new TcpSocket(server._handle));
		}
	}
}
