using System;
using System.Runtime.InteropServices;
namespace Libuv {
	public class TcpServer : TcpEntity {
		public TcpServer() : base() 
		{
		}
		public void Listen(string ip, int port, Action<TcpSocket> OnConnect)
		{
			int err = manos_uv_tcp_bind(this._handle, ip, port);
			//if (err != 0 ) throw new Exception(uv_last_error().code.ToString());
			err = uv_tcp_listen(this._handle, 128, (sock, status) => {
				OnConnect(new TcpSocket(this._handle));
			});
			if (err != 0 ) throw new Exception(uv_last_error().code.ToString());
		}
		[DllImport ("uvwrap")]
		internal static extern int manos_uv_tcp_bind (HandleRef socket, string host, int port);
		[DllImport ("uvwrap")]
		internal static extern int uv_tcp_listen(HandleRef socket, int backlog, uv_connection_cb callback);
	}
}
