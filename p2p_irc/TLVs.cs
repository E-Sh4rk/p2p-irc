using System;
using System.Collections.Generic;

namespace p2p_irc
{
	public struct TLV
	{
		public enum Type
		{
			Pad1 = 0,
			PadN = 1,
			Hello = 2,
			Neighbour = 3,
			Data = 4,
			Ack = 5,
			GoAway = 6,
			Warning = 7
		}
		public Type type;
		public byte[] body;
	}
	public class TLVs
	{
		public TLVs() { }

		// Read the TLV at the given position, and update the pointer offset. Return null if it is a Pad or an incorrect/unknown TLV.
		public TLV? Read(byte[] buffer, ref int offset)
		{
			if (buffer == null)
				return null;
			try
			{
				byte type = buffer[offset];
				offset++;
				if (type == (byte)TLV.Type.Pad1)
					return null;
				byte length = buffer[offset];
				offset++;
				byte[] body = new byte[length];
				Array.Copy(buffer, offset, body, 0, length);
				offset += length;

				if (!Enum.IsDefined(typeof(TLV.Type), type))
					return null;
				TLV t = new TLV();
				t.type = (TLV.Type)type;
				if (t.type == TLV.Type.PadN)
					return null;
				t.body = body;
				return t;
			}
			catch
			{
				Console.WriteLine("[ERROR] Invalid TLV !");
				// We put the pointer to the end of the buffer to avoid reading incorrect TLVs again.
				offset = buffer.Length;
			}
			return null;
		}

		public TLV[] ReadAll(byte[] buffer)
		{
			if (buffer == null)
				return null;
			List<TLV> tlvs = new List<TLV>();
			int offset = 0;
			while (offset < buffer.Length)
			{
				TLV? tlv = Read(buffer, ref offset);
				if (tlv.HasValue)
					tlvs.Add(tlv.Value);
			}
			return tlvs.ToArray();
		}

		// TODO: 1024 length max
		public byte[] Write(TLV tlv)
		{
			try
			{
				byte[] res = new byte[tlv.body.Length + 2];
				res[0] = (byte)tlv.type;
				res[1] = (byte)tlv.body.Length;
				Array.Copy(tlv.body, 0, res, 2, tlv.body.Length);
				return res;
			}
			catch { return null; }
		}

		public byte[] WriteAll(TLV[] tlvs)
		{
			int total_len = 0;
			List<byte[]> bs = new List<byte[]>();
			foreach (TLV t in tlvs)
			{
				byte[] b = Write(t);
				if (b == null)
					continue;
				total_len += b.Length;
				bs.Add(b);
			}
			byte[] res = new byte[total_len];
			int current = 0;
			foreach (byte[] b in bs)
			{
				Array.Copy(b, 0, res, current, b.Length);
				current += b.Length;
			}
			return res;
		}
	}
}
