using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace p2p_irc
{
	public class TLV_utils
	{
		ulong ID;

		public TLV_utils(ulong ID)
		{
			this.ID = ID;
		}

		// ----- HELLO -----
		public TLV shortHello()
		{
			TLV t = new TLV();
			t.type = TLV.Type.Hello;
			t.body = BitConverter.GetBytes(ID);
			Debug.Assert(t.body.Length == 8);
			return t;
		}
		public TLV longHello(ulong destID)
		{
			TLV t = new TLV();
			t.type = TLV.Type.Hello;
			byte[] src = BitConverter.GetBytes(ID);
			byte[] dest = BitConverter.GetBytes(destID);
			t.body = new byte[src.Length + dest.Length];
			Array.Copy(src, t.body, src.Length);
			Array.Copy(dest, 0, t.body, src.Length, dest.Length);
			Debug.Assert(t.body.Length == 16);
			return t;
		}
		public ulong? getHelloSource(TLV tlv)
		{
			try
			{
				if (tlv.type != TLV.Type.Hello)
					return null;
				if (tlv.body.Length != 8 && tlv.body.Length != 16)
					return null;
				return BitConverter.ToUInt64(tlv.body, 0);
			}
			catch { return null; }
		}
		public bool isValidLongHello(TLV tlv)
		{
			try
			{
				if (tlv.type != TLV.Type.Hello)
					return false;
				if (tlv.body.Length != 16)
					return false;
				return BitConverter.ToUInt64(tlv.body, 8) == ID;
			}
			catch { return false; }
		}

		// ----- GO_AWAY -----
		public TLV goAway(byte code, string message)
		{
			TLV t = new TLV();
			t.type = TLV.Type.GoAway;
			byte[] msg = Encoding.UTF8.GetBytes(message);
			t.body = new byte[msg.Length + 1];
			t.body[0] = code;
			Array.Copy(msg, 0, t.body, 1, msg.Length);
			return t;
		}
		public byte? getGoAwayCode(TLV tlv)
		{
			try
			{
				try
				{
					string msg = Encoding.UTF8.GetString(tlv.body, 1, tlv.body.Length - 1);
					if (!String.IsNullOrEmpty(msg))
						Console.WriteLine("[GO_AWAY] " + msg);
				} catch { Console.WriteLine("[GO_AWAY] Invalid message."); }
				return tlv.body[0];
			}
			catch { return null; }
		}

		// ----- NEIGHBOUR -----
		public TLV neighbour(PeerAddress pa)
		{
			TLV t = new TLV();
			t.type = TLV.Type.Neighbour;
			byte[] ip = pa.ip.MapToIPv6().GetAddressBytes();
			t.body = new byte[ip.Length + 4];
			Array.Copy(ip, t.body, ip.Length);
			Array.Copy(BitConverter.GetBytes(pa.port), 0, t.body, t.body.Length-4, 4);
			Debug.Assert(t.body.Length == 20);
			return t;
		}
		public PeerAddress? getNeighbourAddress(TLV tlv)
		{
			try
			{
				if (tlv.type != TLV.Type.Neighbour)
					return null;
				PeerAddress pa = new PeerAddress();
				byte[] byte_addr = new byte[tlv.body.Length - 4];
				Array.Copy(tlv.body, byte_addr, byte_addr.Length);
				pa.ip = new IPAddress(byte_addr).MapToIPv6();
				pa.port = BitConverter.ToInt32(tlv.body, tlv.body.Length - 4);
				return pa;
			}
			catch { return null; }
		}
	}
}
