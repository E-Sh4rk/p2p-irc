using System;
using System.Diagnostics;
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
	}
}
