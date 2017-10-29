using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace p2p_irc
{
    static class Utils
    {
        public static ushort ToUInt16(byte[] b, int offset)
        {
            short s = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, offset));
            unchecked { return (ushort)s; }; // Unchecked is for disabling overflow checking at runtime
        }
        public static uint ToUInt32(byte[] b, int offset)
        {
            int i = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(b, offset));
            unchecked{ return (uint)i; }; // Unchecked is for disabling overflow checking at runtime
        }
        public static int ToInt32(byte[] b, int offset)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(b, offset));
        }
        public static ulong ToUInt64(byte[] b, int offset)
        {
            long l = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(b, offset));
            unchecked { return (ulong)l; }; // Unchecked is for disabling overflow checking at runtime
        }

        public static byte[] GetBytes(ushort s)
        {
            unchecked
            {
                short s2 = (short)s;
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(s2));
            };
        }
        public static byte[] GetBytes(uint i)
        {
            unchecked
            {
                int i2 = (int)i;
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i2));
            };
        }
        public static byte[] GetBytes(int i)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
        }
        public static byte[] GetBytes(ulong l)
        {
            unchecked
            {
                long l2 = (long)l;
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(l2));
            };
        }
    }
}
