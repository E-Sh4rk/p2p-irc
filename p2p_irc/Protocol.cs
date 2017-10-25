using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace p2p_irc
{
	public class Protocol
	{
		ulong ID;
		Random r;

		Communications com;
		Messages messages;
		TLV_utils tlv_utils;

		Peers p;
		Chat c;

		public Protocol()
		{
			r = new Random();
			ID = (ulong)(r.NextDouble() * ulong.MaxValue);
			com = new Communications();
			messages = new Messages();
			tlv_utils = new TLV_utils(ID);
			p = new Peers(new System.Collections.Generic.List<PeerAddress>(), com, tlv_utils, messages);
			c = new Chat(com, tlv_utils, messages);
		}

		Thread thread;
		Stopwatch lastHelloSaid;
		Stopwatch lastNeighborsSaid;
		void thread_procedure()
		{
			while (true)
			{
				p.RemoveOldNeighbors();
				if (lastHelloSaid == null || lastHelloSaid.ElapsedMilliseconds >= Peers.helloNeighborsDelay * 1000)
				{
					p.SayHello();
					lastHelloSaid = Stopwatch.StartNew();
				}
				if (lastNeighborsSaid == null || lastNeighborsSaid.ElapsedMilliseconds >= Peers.sendNeighborsDelay * 1000)
				{
					p.SendNeighbors();
					lastNeighborsSaid = Stopwatch.StartNew();
				}
				Thread.Sleep(1000);
			}
		}

		public void Run()
		{
			lastHelloSaid = null;
			lastNeighborsSaid = Stopwatch.StartNew();
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
