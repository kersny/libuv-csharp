using System;
using System.Runtime.InteropServices;
namespace Libuv {
	class TcpServer : TcpEntity {
		public TcpServer() : base() 
		{
			int err = uv_tcp_init(this.Handle);
			if (err != 0) throw new Exception(uv_last_err().code.ToString());
		}
		public void Listen(string ip, int port, Action<TcpSocket> OnConnect)
		{
			int err = manos_uv_tcp_bind(this.Handle, ip, port);
			if (err != 0 ) throw new Exception(uv_last_err().code.ToString());
			err = uv_tcp_listen(this.Handle, 128, (sock, status) => {
				OnConnect(new TcpSocket(this.Handle));
			});
			if (err != 0 ) throw new Exception(uv_last_err().code.ToString());
		}
		[DllImport ("uvwrap")]
		public static extern int manos_uv_tcp_bind (IntPtr socket, string host, int port);
		[DllImport ("uvwrap")]
		public static extern int uv_tcp_listen(IntPtr socket, int backlog, uv_connection_cb callback);
	}
}
