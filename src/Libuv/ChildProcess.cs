using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Libuv {
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_exit_cb(IntPtr handle, int exit_status, int term_signal);
	[StructLayout(LayoutKind.Sequential)]
	public struct uv_process_options_t {
		public uv_exit_cb exit_cb;
		public string file;
		public IntPtr[] args;
		public string[] env;
		public string cwd;
		public int windows_verbatim_arguments;

		public IntPtr stdin_stream;
		public IntPtr stdout_stream;
		public IntPtr stderr_stream;
	}
	public class ChildProcess {
		[DllImport ("uv")]
		public static extern int uv_spawn(IntPtr process, uv_process_options_t options);
		[DllImport ("uv")]
		public static extern int uv_process_kill(IntPtr process, int signum);
		[DllImport("uv")]
		internal static extern int uv_pipe_init(IntPtr prepare);
		[DllImport ("uv")]
		public static extern int uv_close(IntPtr process, uv_close_cb cb);
		[DllImport ("uv")]
		internal static extern int uv_read_start(IntPtr stream, uv_alloc_cb alloc_cb, uv_read_cb read);
		static uv_buf_t static_alloc(IntPtr pipe, IntPtr size)
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
		static void static_read(IntPtr stream, IntPtr nread, uv_buf_t buf)
		{
			int size = (int)nread;
			if (size < 0) {
				if ((int)buf.data != 0)
					Marshal.FreeHGlobal(buf.data);
				//IntPtr shutdown = Marshal.AllocHGlobal(Sizes.ShutdownTSize);
				//uv_shutdown(shutdown, stream, after_shutdown);
				return;
			}
			if (size == 0) {
				Marshal.FreeHGlobal(buf.data);
				return;
			}
			byte[] data = new byte[size];
			Marshal.Copy(buf.data, data, 0, size);
			Console.WriteLine("FROM PROC: {0}", System.Text.Encoding.ASCII.GetString(data));
			//var handle = (uv_handle_t)Marshal.PtrToStructure(stream, typeof(uv_handle_t));
			//var instance = GCHandle.FromIntPtr(handle.data);
			//var watcher_instance = (PipeSocket)instance.Target;
			//watcher_instance.HandleRead(data, size);
			Marshal.FreeHGlobal(buf.data);
		}
		static void static_exit(IntPtr handle, int exit_status, int term_signal)
		{
			uv_close(handle, static_close);
		}
		static void static_close(IntPtr handle)
		{
			Marshal.FreeHGlobal(handle);
		}
		uv_process_options_t options;
		public ChildProcess()
		{
			IntPtr stdout = Marshal.AllocHGlobal(Sizes.PipeT);
			uv_pipe_init(stdout);
			IntPtr process = Marshal.AllocHGlobal(Sizes.ProcessT);
			var pwd = Directory.GetCurrentDirectory();
			pwd = Path.Combine(pwd, "t");
			options = new uv_process_options_t();
			options.exit_cb = static_exit;
			options.file = pwd;
			var args = new IntPtr[3];
			args[0] = Marshal.StringToHGlobalAuto(pwd);
			args[1] = Marshal.StringToHGlobalAuto("t");
			args[2] = IntPtr.Zero;
			options.args = args;
			options.stdout_stream = stdout;
			uv_spawn(process, options);
			uv_read_start(stdout, static_alloc, static_read);
		}
	}
}
