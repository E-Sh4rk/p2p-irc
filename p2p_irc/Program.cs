using System;
using System.Net;
using System.Threading;

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
			Console.WriteLine("This project is only used for debugging.");
			Console.WriteLine("Please use p2p_ui project if you want to have an UI interface.\n");
			Console.WriteLine("Starting the chat...");

			PeerAddress pa = new PeerAddress();
			pa.port = 1212;
			pa.ip = Dns.GetHostEntry("jch.irif.fr").AddressList[0];
			Protocol p = new Protocol(null, new PeerAddress[] { pa }, on_new_message);

			Thread th = new Thread(new ThreadStart(p.Run));
			th.Start();
			while (true)
			{
				p.SendMessage(Console.ReadLine());
			}
		}
	}
}
