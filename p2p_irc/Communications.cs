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
		public Communications(int? port)
		{
			// For .NET framework < 4.5 : https://blogs.msdn.microsoft.com/webdev/2013/01/08/dual-mode-sockets-never-create-an-ipv4-socket-again/
			if (port.HasValue)
			{
				// Create a new socket and bind it to the port
				socket = new UdpClient(/*port.Value, */AddressFamily.InterNetworkV6);
				socket.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, port.Value));
				// socket.Client.DualMode = true; // TODO: Fail... Why?
			}
			else
			{
				socket = new UdpClient(AddressFamily.InterNetworkV6); // Create a new socket without binding it
				socket.Client.DualMode = true;
			}
		}
		public void SendMessage(PeerAddress pa, byte[] msg)
		{
			try
			{
				IPEndPoint ep = new IPEndPoint(pa.ip.MapToIPv6(), pa.port);
				socket.Send(msg, msg.Length, ep);
			}
			catch { Console.WriteLine("[ERROR] Error while sending datagram."); }
		}
		public DataReceived? ReceiveMessage()
		{
			try
			{
				DataReceived res = new DataReceived();
				IPEndPoint endpoint = new IPEndPoint(IPAddress.IPv6Any, 0);
				res.data = socket.Receive(ref endpoint);
				PeerAddress pa = new PeerAddress();
				pa.ip = endpoint.Address.MapToIPv6();
				pa.port = endpoint.Port;
				res.peer = pa;
				return res;
			}
			catch { Console.WriteLine("[ERROR] Error while receiving datagram."); }
			return null;
		}
		public bool IsSelf(PeerAddress pa)
		{
			IPEndPoint ep = (IPEndPoint)socket.Client.LocalEndPoint;
			if (!pa.ip.MapToIPv6().Equals(ep.Address.MapToIPv6()))
				return false;
			if (pa.port != ep.Port)
				return false;
			return true;
		}
	}
}
