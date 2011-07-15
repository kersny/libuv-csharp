using System;
using System.Runtime.InteropServices;
using System.IO;

namespace webserver {
	class webserver {
		[DllImport ("uvwrap")]
		public static extern void uv_init ();
		[DllImport ("uvwrap")]
		public static extern void uv_run ();
		[DllImport ("uvwrap")]
		public static extern void uv_unref ();
		static int clientcount = 0;
		static void Main ()
		{
			uv_init();
			var server = new TcpServer();
			server.Listen("0.0.0.0", 8080, (socket) => {
				clientcount++;
				socket.Write(System.Text.Encoding.ASCII.GetBytes(clientcount.ToString()), 1);
				if (clientcount > 5) {
					socket.Close();
					server.Close();
				}
				Console.WriteLine("Client Connected");
				socket.OnData += (data, len) => {
					Console.WriteLine("Data Recieved: {0}", System.Text.Encoding.ASCII.GetString(data, 0, len));
					socket.Write(data, len);
				};
				socket.OnClose += () => {
					Console.WriteLine("Client Disconnected");
				};
			});
			var client = new TcpSocket();
			client.OnData += (data, len) => {
				Console.WriteLine("Client Recieved: {0}", System.Text.Encoding.ASCII.GetString(data, 0, len));
				client.Close();
			};
			client.Connect("127.0.0.1", 8080, () => {
				byte[] message = System.Text.Encoding.ASCII.GetBytes("Hello World\n");
				client.Write(message, message.Length);
			});
			uv_run();
			client.Dispose();
			server.Dispose();
		}
	}
	abstract class TcpEntity : IDisposable {
		public IntPtr Handle;
		public TcpEntity()
		{
			this.Handle = manos_uv_tcp_t_create();
			uv_tcp_init(this.Handle);
		}
		public  void Dispose()
		{
			manos_uv_destroy(this.Handle);
		}
		public void Close()
		{
			uv_close(this.Handle, (x) => {});
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void uv_connection_cb(IntPtr socket, int status);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void uv_close_cb(IntPtr socket);
		[DllImport ("uvwrap")]
		public static extern void uv_tcp_init (IntPtr socket);
		[DllImport ("uvwrap")]
		public static extern IntPtr manos_uv_tcp_t_create();
		[DllImport ("uvwrap")]
		public static extern void manos_uv_destroy(IntPtr uv_tcp_t_ptr);
		[DllImport ("uvwrap")]
		public static extern int uv_close(IntPtr handle, uv_close_cb cb);
	}
	class TcpSocket : TcpEntity {
		public event Action<byte[], int> OnData;
		public event Action OnClose;
		private event Action OnConnect;
		private IntPtr Parent = IntPtr.Zero;
		public TcpSocket()
		{
			this.Handle = manos_uv_connect_t_create();
			this.Parent = manos_uv_tcp_t_create();
			uv_tcp_init(this.Parent);
		}
		public TcpSocket(IntPtr ServerHandle) : base()
		{
			uv_accept(ServerHandle, this.Handle);
			manos_uv_read_start(this.Handle, (socket, count, data) => {
				RaiseData(data, count);
			}, () => {
				RaiseClose();
				this.Dispose();
			});
		}
		private void RaiseData(byte[] data, int count)
		{
			if (OnData != null) 
			{
				OnData(data, count);
			}
		}
		private void RaiseClose()
		{
			if (OnClose != null)
			{
				OnClose();
			}
		}
		private void HandleConnect()
		{
			if (OnConnect != null)
			{
				OnConnect();
			}
		}
		public void Connect(string ip, int port, Action OnConnect)
		{
			manos_uv_tcp_connect(this.Handle, this.Parent, ip, port, (sock, status) => {
				manos_uv_read_start(this.Parent, (socket, count, data) => {
					RaiseData(data, count);
				}, () => {
					RaiseClose();
					this.Dispose();
				});
				OnConnect();
			});	
		}
		public void Write(byte[] data, int length)
		{
			if (this.Parent == IntPtr.Zero) {
				manos_uv_write(this.Handle, data, length);
			} else {
				manos_uv_write(this.Parent, data, length);
			}
		}
		public new void Dispose()
		{
			if (this.Parent != IntPtr.Zero)
			{
				manos_uv_destroy(this.Parent);
			}
			base.Dispose();
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void manos_uv_read_cb(IntPtr socket, int count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] data);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void manos_uv_eof_cb();
		[DllImport ("uvwrap")]
		public static extern void uv_accept(IntPtr socket, IntPtr stream);
		[DllImport ("uvwrap")]
		public static extern int manos_uv_read_start(IntPtr stream, manos_uv_read_cb cb, manos_uv_eof_cb done);
		[DllImport ("uvwrap")]
		public static extern int manos_uv_write(IntPtr uv_tcp_t_ptr, byte[] data, int length);
		[DllImport ("uvwrap")]
		public static extern int manos_uv_tcp_connect(IntPtr uv_connect_t_ptr, IntPtr handle, string ip, int port, uv_connection_cb cb);
		[DllImport ("uvwrap")]
		public static extern IntPtr manos_uv_connect_t_create();
	}
	class TcpServer : TcpEntity {
		public TcpServer() : base() {}
		public void Listen(string ip, int port, Action<TcpSocket> OnConnect)
		{
			manos_uv_tcp_bind(this.Handle, ip, port);
			uv_tcp_listen(this.Handle, 128, (sock, status) => {
				OnConnect(new TcpSocket(this.Handle));
			});
		}
		[DllImport ("uvwrap")]
		public static extern void manos_uv_tcp_bind (IntPtr socket, string host, int port);
		[DllImport ("uvwrap")]
		public static extern void uv_tcp_listen(IntPtr socket, int backlog, uv_connection_cb callback);
	}
}
