using System;
using System.Collections.Generic;

namespace p2p_irc
{
    public class TLV
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
        const int max_tlv_size = Messages.max_message_size - Messages.MessageHeader.body_offset;

        public TLVs() { }

        // Read the TLV at the given position, and update the pointer offset. Return null if it is a Pad or an incorrect/unknown TLV.
        public TLV Read(byte[] buffer, ref int offset)
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

                if (!Enum.IsDefined(typeof(TLV.Type), (int)type))
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
                Utils.Debug("[ERROR] Invalid TLV !");
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
                TLV tlv = Read(buffer, ref offset);
                if (tlv != null)
                    tlvs.Add(tlv);
            }
            return tlvs.ToArray();
        }

        public byte[] Write(TLV tlv)
        {
            try
            {
                byte[] res = new byte[tlv.body.Length + 2];
                if (res.Length > max_tlv_size)
                {
                    Utils.Debug("[ERROR] TLV too big !");
                    return null;
                }
                res[0] = (byte)tlv.type;
                res[1] = (byte)tlv.body.Length;
                Array.Copy(tlv.body, 0, res, 2, tlv.body.Length);
                return res;
            }
            catch { return null; }
        }

        public byte[][] WriteAll(TLV[] tlvs)
        {
            if (tlvs == null)
                return null;
            List<byte[]> bs = new List<byte[]>();
            List<int> packets_size = new List<int>();
            int current_packet_size = 0;
            foreach (TLV t in tlvs)
            {
                byte[] b = Write(t);
                if (b == null)
                    continue;
                if (current_packet_size + b.Length <= max_tlv_size)
                    current_packet_size += b.Length;
                else
                {
                    packets_size.Add(current_packet_size);
                    current_packet_size = b.Length;
                }
                bs.Add(b);
            }
            if (current_packet_size > 0)
                packets_size.Add(current_packet_size);

            byte[][] res = new byte[packets_size.Count][];
            int current_offset = 0;
            int current_array = 0;
            foreach (byte[] b in bs)
            {
                if (current_offset >= packets_size[current_array])
                {
                    current_offset = 0;
                    current_array++;
                }
                if (res[current_array] == null)
                    res[current_array] = new byte[packets_size[current_array]];
                Array.Copy(b, 0, res[current_array], current_offset, b.Length);
                current_offset += b.Length;
            }
            return res;
        }
    }
}
