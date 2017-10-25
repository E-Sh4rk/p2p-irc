using System;

namespace p2p_irc
{
	class MainClass
	{
		static void on_new_message(ulong sender, string msg)
		{
			Console.WriteLine(msg);
		}

		public static void Main(string[] args)
		{
			Console.WriteLine("This project only implements the protocol (no UI).");
			Console.WriteLine("Please run p2p_ui project if you want to test it.\n");
			Console.WriteLine("Starting...");
			Protocol p = new Protocol(null, on_new_message);
			p.Run();
		}
	}
}
