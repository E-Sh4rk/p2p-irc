using System;
using System.Net;
using System.Net.Sockets;

namespace p2p_irc
{
	public class Communications
	{
		UdpClient socket;
		public Communications(UdpClient socket)
		{
			this.socket = socket;
		}
		public void SendMessage(PeerAddress pa, byte[] msg)
		{
			IPEndPoint ep = new IPEndPoint(pa.ip, pa.port);
			socket.Send(msg, msg.Length, ep);
		}
	}
}
