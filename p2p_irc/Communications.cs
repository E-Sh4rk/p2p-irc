using System;
using System.Net;
using System.Net.Sockets;

namespace p2p_irc
{
	public class Communications
	{
		UdpClient socket;
		public Communications()
		{
			socket = new UdpClient(AddressFamily.InterNetworkV6); // No need to bind the socket...
			// For .NET framework < 4.5 : https://blogs.msdn.microsoft.com/webdev/2013/01/08/dual-mode-sockets-never-create-an-ipv4-socket-again/
			socket.Client.DualMode = true;
		}
		public void SendMessage(PeerAddress pa, byte[] msg)
		{
			IPEndPoint ep = new IPEndPoint(pa.ip, pa.port);
			socket.Send(msg, msg.Length, ep);
		}
		public EndPoint ReceiveMessage(byte[] buffer)
		{
			// TODO
			return null;
		}
	}
}
