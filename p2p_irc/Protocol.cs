using System;
namespace p2p_irc
{
	public class Protocol
	{
		ulong ID;
		Random r;
		Peers p;

		public Protocol()
		{
			r = new Random();
			ID = (ulong)(r.NextDouble() * ulong.MaxValue);
			p = new Peers(new System.Collections.Generic.List<PeerAddress>());
		}
	}
}
