using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Libuv {
	public class PipeServer {
		[DllImport("uv")]
		internal static extern int uv_pipe_init(IntPtr prepare);
		[DllImport("uv")]
		internal static extern int uv_pipe_bind(IntPtr prepare, string name);
		[DllImport("uv")]
		internal static extern int uv_listen(IntPtr stream, int backlog, uv_connection_cb cb);

		private static uv_connection_cb unmanaged_callback;

		static PipeServer()
		{
			unmanaged_callback = StaticCallback;
		}

		private Action<PipeSocket> callback;
		private IntPtr _handle;
		private GCHandle me;

		public PipeServer(Action<PipeSocket> callback)
		{
			this.callback = callback;
			this._handle = Marshal.AllocHGlobal(Sizes.PipeT);
			Util.CheckError(uv_pipe_init(this._handle));
			var handle = (uv_handle_t)Marshal.PtrToStructure(this._handle, typeof(uv_handle_t));
			this.me = GCHandle.Alloc(this);
			handle.data = GCHandle.ToIntPtr(this.me);
			Marshal.StructureToPtr(handle, this._handle, true);
		}
		public void Listen(string endpoint)
		{
			Util.CheckError(uv_pipe_bind(this._handle, endpoint));
			Util.CheckError(uv_listen(this._handle, 128, unmanaged_callback));
		}
		public static void StaticCallback(IntPtr server_ptr, int status)
		{
			Util.CheckError(status);
			var handle = (uv_handle_t)Marshal.PtrToStructure(server_ptr, typeof(uv_handle_t));
			var instance = GCHandle.FromIntPtr(handle.data);
			var server = (PipeServer)instance.Target;
			server.callback(new PipeSocket(server._handle));
		}
	}
}
