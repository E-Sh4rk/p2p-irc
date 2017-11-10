using System;
using System.Net;
using System.Net.Sockets;

namespace p2p_irc
{
	public class Communications
	{
		public class DataReceived
		{
			public PeerAddress peer;
			public byte[] data;
		}

        private const int MaxUDPSize = 0x10000;
        Socket socket;
		public Communications(int? port)
		{
            // For .NET framework < 4.5 : https://blogs.msdn.microsoft.com/webdev/2013/01/08/dual-mode-sockets-never-create-an-ipv4-socket-again/
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.DualMode = true;
            if (port.HasValue)
                socket.Bind(new IPEndPoint(IPAddress.IPv6Any, port.Value));
            socket.ReceiveTimeout = 100;
            socket.SendTimeout = 100; // Should not be used in UDP... but anyway.
            socket.Blocking = false; // If a packet can't be send immedialty, we prefer to drop it rather than blocking the thread...
		}

        public void Close()
        {
            socket.Close();
            socket = null;
        }
		public void SendMessage(PeerAddress pa, byte[] msg)
		{
			try
			{
				IPEndPoint ep = new IPEndPoint(pa.ip.MapToIPv6(), pa.port);
                socket.SendTo(msg, msg.Length, SocketFlags.None, ep);
            }
			catch { Utils.Debug("[ERROR] Error while sending datagram."); }
		}
		public DataReceived ReceiveMessage()
		{
            try
            {
                EndPoint endpoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                byte[] buffer = new byte[MaxUDPSize];
                int read = socket.ReceiveFrom(buffer, MaxUDPSize, SocketFlags.None, ref endpoint);

                PeerAddress pa = new PeerAddress();
                pa.ip = ((IPEndPoint)endpoint).Address.MapToIPv6();
                pa.port = ((IPEndPoint)endpoint).Port;
                DataReceived res = new DataReceived();
                res.peer = pa;
                res.data = new byte[read];
                Array.Copy(buffer, res.data, read);

                return res;
            }
            catch (SocketException) { } // Timeout
            catch { Utils.Debug("[ERROR] Error while receiving datagram."); }
			return null;
		}
		public bool IsSelf(PeerAddress pa)
		{
			IPEndPoint ep = (IPEndPoint)socket.LocalEndPoint;
			if (!pa.ip.MapToIPv6().Equals(ep.Address.MapToIPv6()))
				return false;
			if (pa.port != ep.Port)
				return false;
			return true;
		}
	}
}
