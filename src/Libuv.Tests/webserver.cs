using System;
using System.Runtime.InteropServices;
using System.IO;
using Libuv;

namespace Libuv.Tests {
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

			var watch = new PrepareWatcher((ptr, stat) => {
		//		Console.WriteLine("Prepare Watcher Called");
			});
			watch.Start();
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
				watch.Stop();
				watch.Dispose();
				client.Close();
			};
			client.Connect("127.0.0.1", 8080, () => {
				byte[] message = System.Text.Encoding.ASCII.GetBytes("Hello World\n");
				client.Write(message, message.Length);
			});
			var watch2 = new PrepareWatcher((ptr, stat) => {
		//		Console.WriteLine("Prepare Watcher 2 Called");
			});
			watch2.Start();
			var check = new CheckWatcher((ptr, stat) => {
		//		Console.WriteLine("Check Watcher Called");
			});
			check.Start();
			var idle = new IdleWatcher((ptr, stat) => {
		//		Console.WriteLine("Idle Watcher Called");
			});
			idle.Start();
			var after = new TimerWatcher(new TimeSpan(0,0,5), new TimeSpan(1,0,0), (ptr, stat) => {
				Console.WriteLine("After 5 Seconds");
			});
			after.Start();
			var every = new TimerWatcher(new TimeSpan(0,0,5), (ptr, stat) => {
				Console.WriteLine("Every 5 Seconds");
			//	after.Stop();
			});
			every.Start();
			uv_run();
		}
	}
}
