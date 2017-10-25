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
		DateTime lastNeighborsSaid;
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
				if (lastNeighborsSaid.AddSeconds(Peers.sendNeighborsDelay) <= DateTime.Now)
				{
					p.SendNeighbors();
					lastNeighborsSaid = DateTime.Now;
				}
				Thread.Sleep(1000);
			}
		}

		public void Run()
		{
			lastHelloSaid = DateTime.Now.AddSeconds(-Peers.helloNeighborsDelay);
			lastNeighborsSaid = DateTime.Now;
			thread = new Thread(new ThreadStart(thread_procedure));
			thread.Start();

			while (true)
			{
				Communications.DataReceived? data = com.ReceiveMessage();
				if (data.HasValue)
				{
					Communications.DataReceived d = data.Value;
					if (com.IsSelf(d.peer))
						continue;
					TLV[] tlvs = messages.UnpackTLVs(d.data);
					if (tlvs == null)
						continue;
					foreach (TLV t in tlvs)
					{
						switch (t.type)
						{
							case TLV.Type.Hello:
							case TLV.Type.GoAway:
							case TLV.Type.Neighbour:
								p.TreatTLV(d.peer, t);
								break;
						}
					}
				}
			}
		}
	}
}
