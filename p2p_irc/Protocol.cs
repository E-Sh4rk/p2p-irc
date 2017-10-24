using System;
using System.Net.Sockets;
using System.Threading;

namespace p2p_irc
{
	public class Protocol
	{
		ulong ID;
		Random r;
		Peers p;
		Communications com;

		public Protocol()
		{
			r = new Random();
			ID = (ulong)(r.NextDouble() * ulong.MaxValue);
			com = new Communications();
			p = new Peers(new System.Collections.Generic.List<PeerAddress>(), ID, com);
		}

		Thread t;

		void thread_procedure()
		{
			while (true)
			{
				p.SayHello();
				Thread.Sleep(Peers.helloNeighborsDelay * 1000);
			}
		}

		public void Run()
		{
			t = new Thread(new ThreadStart(thread_procedure));
			t.Start();

			// TODO
		}
	}
}
