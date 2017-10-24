using System;

namespace p2p_irc
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("This project only implements the protocol (no UI).");
			Console.WriteLine("Please run p2p_ui project if you want to test it.\n");
			Console.WriteLine("Starting...");
			Protocol p = new Protocol();
			p.Run();
		}
	}
}
