using System;
using System.Runtime.InteropServices;
using System.IO;

namespace webserver {
	class webserver {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void uv_connection_cb(IntPtr socket, int status);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void manos_uv_read_cb(IntPtr socket, int count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] data);
		static void Main ()
		{
			uv_init();
			//THIS NEEDS TO BE CHANGED... size changes based on OS, we need some way to get that
			//152 is what it is on my mac
			int size = manos_uv_tcp_t_size();
			IntPtr server = Marshal.AllocHGlobal(size);
			uv_tcp_init(server);
			manos_uv_tcp_bind(server, "0.0.0.0", 8080);
			uv_tcp_listen(server, 128, (sock, status) => {
				IntPtr handle = Marshal.AllocHGlobal(size);
			       	uv_tcp_init(handle);
			       	uv_accept(sock, handle); 
				manos_uv_read_start(handle, (socket, count, data) => {
					Console.WriteLine(BitConverter.ToString(data, 0, count));
					Console.WriteLine(System.Text.Encoding.ASCII.GetString(data, 0, count));
				});
			});
			Console.WriteLine ("Hello World");
			uv_run();
			Marshal.FreeHGlobal(server);
		}

		[DllImport ("uvwrap")]
		public static extern void uv_init ();
		[DllImport ("uvwrap")]
		public static extern void uv_run ();
		[DllImport ("uvwrap")]
		public static extern void uv_tcp_init (IntPtr socket);
		[DllImport ("uvwrap")]
		public static extern void manos_uv_tcp_bind (IntPtr socket, string host, int port);
		[DllImport ("uvwrap")]
		public static extern void uv_tcp_listen(IntPtr socket, int backlog, uv_connection_cb callback);
		[DllImport ("uvwrap")]
		public static extern void uv_accept(IntPtr socket, IntPtr stream);
		[DllImport ("uvwrap")]
		public static extern int manos_uv_read_start(IntPtr stream, manos_uv_read_cb cb);
		[DllImport ("uvwrap")]
		public static extern int manos_uv_tcp_t_size();
	}
}
