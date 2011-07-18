using System;
using System.Runtime.InteropServices;

namespace Libuv {
	public class TcpSocket : TcpEntity {
		public event Action<byte[], int> OnData;
		public event Action OnClose;
		private event Action OnConnect;
		private IntPtr Connection = IntPtr.Zero;
		public TcpSocket()
		{
			this.Connection = manos_uv_connect_t_create();
			int err = uv_tcp_init(this.Handle);
			if (err != 0) throw new Exception(uv_last_error().code.ToString());
		}
		public TcpSocket(IntPtr ServerHandle) : base()
		{
			int err = uv_tcp_init(this.Handle);
			if (err != 0) throw new Exception(uv_last_error().code.ToString());
			err = uv_accept(ServerHandle, this.Handle);
			if (err != 0) throw new Exception(uv_last_error().code.ToString());
			err = manos_uv_read_start(this.Handle, (socket, count, data) => {
				RaiseData(data, count);
			}, () => {
				RaiseClose();
				this.Dispose();
			});
			if (err != 0) throw new Exception(uv_last_error().code.ToString());
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
			int err = manos_uv_tcp_connect(this.Connection, this.Handle, ip, port, (sock, status) => {
				err = manos_uv_read_start(this.Handle, (socket, count, data) => {
					RaiseData(data, count);
				}, () => {
					RaiseClose();
					this.Dispose();
				});
				if (err != 0) throw new Exception(uv_last_error().code.ToString());
				OnConnect();
			});
			if (err != 0) throw new Exception(uv_last_error().code.ToString());
		}
		public void Write(byte[] data, int length)
		{
			int err = manos_uv_write(this.Handle, data, length);
			if (err != 0) throw new Exception(uv_last_error().code.ToString());
		}
		public new void Dispose()
		{
			if (this.Connection != IntPtr.Zero)
			{
				manos_uv_destroy(this.Connection);
			}
			base.Dispose();
		}
		public new void Close()
		{
			uv_close(this.Handle, (ptr) => {
					});
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void manos_uv_read_cb(IntPtr socket, int count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] data);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void manos_uv_eof_cb();
		[DllImport ("uvwrap")]
		internal static extern int uv_accept(IntPtr socket, IntPtr stream);
		[DllImport ("uvwrap")]
		internal static extern int manos_uv_read_start(IntPtr stream, manos_uv_read_cb cb, manos_uv_eof_cb done);
		[DllImport ("uvwrap")]
		internal static extern int manos_uv_write(IntPtr uv_tcp_t_ptr, byte[] data, int length);
		[DllImport ("uvwrap")]
		internal static extern int manos_uv_tcp_connect(IntPtr uv_connect_t_ptr, IntPtr handle, string ip, int port, uv_connection_cb cb);
		[DllImport ("uvwrap")]
		internal static extern IntPtr manos_uv_connect_t_create();
	}
}
