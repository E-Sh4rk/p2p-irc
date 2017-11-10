using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace p2p_irc
{
	public class TLV_utils
	{
		ulong ID;
		uint lastNonce;

		public TLV_utils(ulong ID)
		{
			this.ID = ID;
			this.lastNonce = 0;
		}

		// ----- HELLO -----
		public TLV shortHello()
		{
			TLV t = new TLV();
			t.type = TLV.Type.Hello;
			t.body = Utils.GetBytes(ID);
			Debug.Assert(t.body.Length == 8);
			return t;
		}
		public TLV longHello(ulong destID)
		{
			TLV t = new TLV();
			t.type = TLV.Type.Hello;
			byte[] src = Utils.GetBytes(ID);
			byte[] dest = Utils.GetBytes(destID);
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
				return Utils.ToUInt64(tlv.body, 0);
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
				return Utils.ToUInt64(tlv.body, 8) == ID;
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
				if (tlv.type != TLV.Type.GoAway)
					return null;
				try
				{
					string msg = Encoding.UTF8.GetString(tlv.body, 1, tlv.body.Length - 1);
					if (!String.IsNullOrEmpty(msg))
						Utils.Debug("[GO_AWAY] " + msg);
				} catch { Utils.Debug("[ERROR] GoAway invalid message."); }
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
			Array.Copy(Utils.GetBytes(pa.port), 0, t.body, t.body.Length-4, 4);
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
				pa.port = Utils.ToInt32(tlv.body, tlv.body.Length - 4);
				return pa;
			}
			catch { return null; }
		}

		// ----- DATA -----
		public class DataMessage
		{
			public ulong sender;
			public uint nonce;
			public string msg;
		}
		public uint nextDataNonce()
		{
			lastNonce++;
			return lastNonce;
		}
		public ulong getSelfID()
		{
			return ID;
		}
		public TLV data(ulong sender, uint nonce, string message)
		{
			byte[] msg_b = Encoding.UTF8.GetBytes(message);
			byte[] ID_b = Utils.GetBytes(sender);
			byte[] nonce_b = Utils.GetBytes(nonce);
			TLV tlv = new TLV();
			tlv.type = TLV.Type.Data;
			tlv.body = new byte[msg_b.Length + ID_b.Length + nonce_b.Length];
			Array.Copy(ID_b,tlv.body,ID_b.Length);
			Array.Copy(nonce_b, 0, tlv.body, ID_b.Length, nonce_b.Length);
			Array.Copy(msg_b, 0, tlv.body, ID_b.Length+nonce_b.Length, msg_b.Length);
			Debug.Assert(tlv.body.Length == msg_b.Length + 12);
			return tlv;
		}
		public DataMessage getDataMessage(TLV tlv)
		{
			try
			{
				if (tlv.type != TLV.Type.Data)
					return null;
				DataMessage dm = new DataMessage();
				dm.sender = Utils.ToUInt64(tlv.body, 0);
				dm.nonce = Utils.ToUInt32(tlv.body, 8);
				dm.msg = Encoding.UTF8.GetString(tlv.body, 12, tlv.body.Length - 12);
				return dm;
			}
			catch { return null; }
		}

		// ----- ACK -----
		public TLV ack(ulong sender, uint nonce)
		{
			byte[] ID_b = Utils.GetBytes(sender);
			byte[] nonce_b = Utils.GetBytes(nonce);
			TLV tlv = new TLV();
			tlv.type = TLV.Type.Ack;
			tlv.body = new byte[ID_b.Length + nonce_b.Length];
			Array.Copy(ID_b, tlv.body, ID_b.Length);
			Array.Copy(nonce_b, 0, tlv.body, ID_b.Length, nonce_b.Length);
			Debug.Assert(tlv.body.Length == 12);
			return tlv;
		}
		public DataMessage getAckMessage(TLV tlv)
		{
			try
			{
				if (tlv.type != TLV.Type.Ack)
					return null;
				if (tlv.body.Length != 12)
					return null;
				DataMessage dm = new DataMessage();
				dm.sender = Utils.ToUInt64(tlv.body, 0);
				dm.nonce = Utils.ToUInt32(tlv.body, 8);
				dm.msg = null;
				return dm;
			}
			catch { return null; }
		}

		// ----- WARNING -----
		public TLV warning(string message)
		{
			TLV t = new TLV();
			t.type = TLV.Type.Warning;
			t.body = Encoding.UTF8.GetBytes(message);
			return t;
		}
		public string getWarningMessage(TLV tlv)
		{
			try
			{
				if (tlv.type != TLV.Type.Warning)
					return null;
				return Encoding.UTF8.GetString(tlv.body);
			}
			catch { return null; }
		}
	}
}
