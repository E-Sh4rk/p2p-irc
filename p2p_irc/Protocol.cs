using System;
using System.Net.Sockets;

namespace p2p_irc
{
	public class Protocol
	{
		ulong ID;
		Random r;
		Peers p;
		UdpClient socket;

		public Protocol()
		{
			r = new Random();
			ID = (ulong)(r.NextDouble() * ulong.MaxValue);
			socket = new UdpClient(AddressFamily.InterNetworkV6); // No need to bind the socket...
			// For .NET framework < 4.5 : https://blogs.msdn.microsoft.com/webdev/2013/01/08/dual-mode-sockets-never-create-an-ipv4-socket-again/
			socket.Client.DualMode = true;
			p = new Peers(new System.Collections.Generic.List<PeerAddress>(), ID, new Communications(socket));
		}
	}
}
