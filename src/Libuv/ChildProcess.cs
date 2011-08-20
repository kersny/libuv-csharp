using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Libuv {
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void uv_exit_cb(IntPtr handle, int exit_status, int term_signal);
	[StructLayout(LayoutKind.Sequential)]
	public struct uv_process_options_t {
		public uv_exit_cb exit_cb;
		public string file;
		public IntPtr[] args;
		public IntPtr[] env;
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
		[DllImport ("uv")]
		public static extern int uv_pipe_init(IntPtr pipe);
		[DllImport ("uv")]
		public static extern int uv_close(IntPtr process, uv_close_cb cb);
		static void static_exit(IntPtr handle, int exit_status, int term_signal)
		{
			uv_close(handle, static_close);
		}
		static void static_close(IntPtr handle)
		{
			Marshal.FreeHGlobal(handle);
		}
		public string File { get; set; }
		public string CurrentWorkingDirectory { get; set; }
		public List<string> Environment { get; private set; }
		public List<string> Arguments { get; private set; }
		private IntPtr _handle;
		public UVStream StdOut;
		public UVStream StdErr;
		private IntPtr _stdout;
		private IntPtr _stderr;
		//public event Action<string, string> OnExit;
		//private void HandleExit(string stdout, string stderr);

		public ChildProcess(string command)
		{
			this.File = command;
			this.CurrentWorkingDirectory = Directory.GetCurrentDirectory();
			this._stdout = Marshal.AllocHGlobal(Sizes.PipeT);
			uv_pipe_init(_stdout);
			this.StdOut = new UVStream(_stdout);
			this._stderr = Marshal.AllocHGlobal(Sizes.PipeT);
			uv_pipe_init(_stdout);
			this.StdOut = new UVStream(_stdout);
			this._handle  = Marshal.AllocHGlobal(Sizes.ProcessT);
		}
		public void Spawn()
		{
			var options = new uv_process_options_t();
			options.exit_cb = static_exit;
			options.file = this.File;
			var args = new IntPtr[Arguments.Count + 3];
			args[0] = Marshal.StringToHGlobalAuto(this.CurrentWorkingDirectory);
			args[1] = Marshal.StringToHGlobalAuto(this.File);
			args[args.Length - 1] = IntPtr.Zero;
			for (int i = 0; i < Arguments.Count; i++)
			{
				args[i + 2] = Marshal.StringToHGlobalAuto(Arguments[i]);
			}
			options.args = args;
			var env = new IntPtr[Environment.Count + 1];
			env[Environment.Count - 1] = IntPtr.Zero;
			for (int i = 0; i < Environment.Count; i++)
			{
				env[i] = Marshal.StringToHGlobalAuto(Environment[i]);
			}
			options.env = env;
			options.stdout_stream = _stdout;
			options.stderr_stream = _stderr;
			Util.CheckError(uv_spawn(_handle, options));
			StdOut.ReadStart();
			StdErr.ReadStart();
		}
	}
}
