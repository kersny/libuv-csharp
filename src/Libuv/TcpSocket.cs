using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Libuv {
	public class TcpSocket {
		public UVStream Stream { get; private set; }
		static void unmanaged_connect_cb(IntPtr connection, int status)
		{
			Util.CheckError(status);
			var tmp = (uv_connect_t)Marshal.PtrToStructure(connection, typeof(uv_connect_t));
			var handle = (uv_handle_t)Marshal.PtrToStructure(tmp.handle, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var socket_instance = (TcpSocket)instance.Target;
			socket_instance.Stream = new UVStream(socket_instance._handle);
			socket_instance.Stream.ReadStart();
			socket_instance.HandleConnect();
		}
		static void on_close(IntPtr socket)
		{
			/*
			var handle = (uv_handle_t)Marshal.PtrToStructure(socket, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var watcher_instance = (TcpSocket)instance.Target;
			//dont think this is what should happen here
			watcher_instance.me.Free();
			Marshal.FreeHGlobal(socket);
			*/
		}
		private IntPtr _handle;
		public event Action OnConnect;
		private GCHandle me;
		private IntPtr connection;
		public TcpSocket()
		{
			this._handle = Marshal.AllocHGlobal(Sizes.TcpT);
			Util.CheckError(uv_tcp_init(this._handle));
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
			this.connection = Marshal.AllocHGlobal(Sizes.ConnectT);
			//can't attach anything to connect_t, it would get nulled
		}
		public void Connect(IPEndPoint endpoint, Action OnConnect)
		{
			var info = uv_ip4_addr(endpoint.Address.ToString(), endpoint.Port);
			Util.CheckError(uv_tcp_connect(this.connection, this._handle, info, unmanaged_connect_cb));
			this.OnConnect += OnConnect;
		}
		public TcpSocket(IntPtr ServerHandle)
		{
			this._handle = Marshal.AllocHGlobal(Sizes.TcpT);
			Util.CheckError(uv_tcp_init(this._handle));
			Util.CheckError(uv_accept(ServerHandle, this._handle));
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
			this.Stream = new UVStream(this._handle);
		}
		private void HandleConnect()
		{
			if (OnConnect != null)
			{
				OnConnect();
			}
		}
		public void Close()
		{
			uv_close(this._handle, on_close);
		}
		[DllImport("uv")]
		internal static extern int uv_tcp_init(IntPtr prepare);
		[DllImport ("uv")]
		internal static extern int uv_accept(IntPtr socket, IntPtr stream);
		[DllImport ("uv")]
		internal static extern int uv_close(IntPtr handle, uv_close_cb cb);
		[DllImport ("uv")]
		internal static extern int uv_tcp_connect(IntPtr connect, IntPtr tcp_handle, sockaddr_in address, uv_connect_cb cb);
		[DllImport ("uv")]
		internal static extern sockaddr_in uv_ip4_addr(string ip, int port);
	}
}
