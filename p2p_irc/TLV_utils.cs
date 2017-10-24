using System;
using System.Diagnostics;

namespace p2p_irc
{
	public class TLV_utils
	{
		ulong ID;

		public TLV_utils(ulong ID)
		{
			this.ID = ID;
		}

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
	}
}
