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
		Messages messages;

		public Protocol()
		{
			r = new Random();
			ID = (ulong)(r.NextDouble() * ulong.MaxValue);
			com = new Communications();
			p = new Peers(new System.Collections.Generic.List<PeerAddress>(), ID, com);
			messages = new Messages(ID);
		}

		Thread thread;
		DateTime lastHelloSaid;
		void thread_procedure()
		{
			while (true)
			{
				p.RemoveOldNeighbors();
				if (lastHelloSaid.AddSeconds(Peers.helloNeighborsDelay) <= DateTime.Now)
				{
					p.SayHello();
					lastHelloSaid = DateTime.Now;
				}
				// TODO: Send Neighbors
				Thread.Sleep(1000);
			}
		}

		public void Run()
		{
			lastHelloSaid = DateTime.Now.AddSeconds(-Peers.helloNeighborsDelay);
			thread = new Thread(new ThreadStart(thread_procedure));
			thread.Start();

			while (true)
			{
				Communications.DataReceived? data = com.ReceiveMessage();
				if (data.HasValue)
				{
					Communications.DataReceived d = data.Value;
					TLV[] tlvs = messages.UnpackTLVs(d.data);
					if (tlvs == null)
						continue;
					foreach (TLV t in tlvs)
					{
						switch (t.type)
						{
							case TLV.Type.Hello:
								p.TreatHello(d.peer, t);
								break;
							// TODO: in particular GoAway and Neighbors
						}
					}
				}
			}
		}
	}
}
