using System;
using System.Net;
using System.Net.Sockets;

namespace p2p_irc
{
	public class Communications
	{
		public struct DataReceived
		{
			public PeerAddress peer;
			public byte[] data;
		}

		UdpClient socket;
		public Communications()
		{
			socket = new UdpClient(AddressFamily.InterNetworkV6); // No need to bind the socket...
			// For .NET framework < 4.5 : https://blogs.msdn.microsoft.com/webdev/2013/01/08/dual-mode-sockets-never-create-an-ipv4-socket-again/
			socket.Client.DualMode = true;
		}
		public void SendMessage(PeerAddress pa, byte[] msg)
		{
			try
			{
				IPEndPoint ep = new IPEndPoint(pa.ip, pa.port);
				socket.Send(msg, msg.Length, ep);
			}
			catch { Console.WriteLine("[ERROR] Error while sending datagram."); }
		}
		public DataReceived? ReceiveMessage()
		{
			try
			{
				DataReceived res = new DataReceived();
				IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
				res.data = socket.Receive(ref endpoint);
				PeerAddress pa = new PeerAddress();
				pa.ip = endpoint.Address;
				pa.port = endpoint.Port;
				res.peer = pa;
				return res;
			}
			catch { Console.WriteLine("[ERROR] Error while receiving datagram."); }
			return null;
		}
	}
}
