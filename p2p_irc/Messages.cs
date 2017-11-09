using System;
using System.Diagnostics;

namespace p2p_irc
{
	public class Messages
	{
		const byte magic = 93;
		const byte version = 0;
        public const int max_message_size = 1024;

		TLVs tlv_reader;

		public class MessageHeader
		{
			public byte magic;
			public byte version;
			public ushort body_length;
			public const int body_offset = 4; // Not in the header of the message, but useful
		}

		public Messages()
		{
			tlv_reader = new TLVs();
		}

		MessageHeader ReadHeader(byte[] buffer) // Can throw an exception!
		{
			MessageHeader h = new MessageHeader();
			h.magic = buffer[0];
			h.version = buffer[1];
			h.body_length = Utils.ToUInt16(buffer, 2);
			return h;
		}

		// Unpack and return the message body. Return null if the message is invalid.
		public byte[] UnpackBody(byte[] buffer)
		{
			try
			{
				MessageHeader h = ReadHeader(buffer);
				if (h.magic != magic)
					return null;
				if (h.version != version)
					return null;
				byte[] body = new byte[h.body_length];
				Array.Copy(buffer, MessageHeader.body_offset, body, 0, h.body_length);
				return body;
			}
			catch (IndexOutOfRangeException e)
			{ Console.WriteLine("[ERROR] Message not complete !"); }
			catch (ArgumentException e)
			{ Console.WriteLine("[ERROR] Message not complete !"); }
			catch { Console.WriteLine("[ERROR] Incorrect message !"); }
			return null;
		}

		// Unpack every TLVs of a message
		public TLV[] UnpackTLVs(byte[] buffer)
		{
			return tlv_reader.ReadAll(UnpackBody(buffer));
		}

		byte[] WriteHeader(MessageHeader h)
		{
			byte[] res = new byte[MessageHeader.body_offset + h.body_length];
			res[0] = magic;
			res[1] = version;
			byte[] length_b = Utils.GetBytes(h.body_length);
			if (length_b.Length != 2)
				Debug.Assert(false);
			Array.Copy(length_b, 0, res, 2, length_b.Length);
			return res;
		}

		public byte[] PackBody(byte[] body)
		{
			try
			{
				MessageHeader h = new MessageHeader();
				h.magic = magic;
				h.version = version;
				h.body_length = (ushort)body.Length;
				byte[] res = WriteHeader(h);
                if (res.Length > max_message_size)
                {
                    Console.WriteLine("[ERROR] Message too big !");
                    return null;
                }
                Array.Copy(body, 0, res, MessageHeader.body_offset, h.body_length);
				return res;
			}
			catch { Console.WriteLine("[ERROR] Error while packing body !"); }
			return null;
		}

		public byte[] PackTLV(TLV t)
		{
			return PackBody(tlv_reader.Write(t));
		}

		public byte[][] PackTLVs(TLV[] ts)
		{
            try
            {
                byte[][] res = tlv_reader.WriteAll(ts);
                for (int i = 0; i < res.Length; i++)
                    res[i] = PackBody(res[i]);
                return res;
            } catch { return null; }
		}
	}
}
