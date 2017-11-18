using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Diagnostics;

namespace p2p_irc
{
    public struct PeerAddress
    {
        public IPAddress ip;
        public int port;
    }

    public class PeerInfo
    {
        public ulong ID;
        public Stopwatch lastHello;
        public Stopwatch lastHelloLong;
    }

    public class Peers
    {
        public const int recentDelay = 60 * 2; // Durée max pour qu'un voisin soit considéré comme récent
        public const int symetricDelay = 60 * 2; // Durée max pour qu'un voisin soit considéré comme symétrique

        const int maxPotentialNeighborsNumber = 25;

        public const int searchNeighborsThreshold = 8;
        public const int helloNeighborsDelay = 30;
        public const int sendNeighborsDelay = 60;

        Dictionary<PeerAddress, PeerInfo> neighborsTable;
        Dictionary<PeerAddress, int> potentialNeighbors; // Associate to a potential neighbor the number of unsucessful tries.

        TLV_utils tlv_utils;
        Communications com;
        Messages messages;

        public Peers(PeerAddress[] potentialNeighbors, Communications com, TLV_utils tlv_utils, Messages messages)
        {
            this.potentialNeighbors = new Dictionary<PeerAddress, int>();
            for (int i = 0; i < potentialNeighbors.Length; i++)
            {
                PeerAddress addr = potentialNeighbors[i];
                // To be sure that initial addresses are IPv6
                addr.ip = addr.ip.MapToIPv6();
                this.potentialNeighbors[addr] = -1; // We associate -1 : we want to never delete inital potential neighbors from the list.
            }

            this.com = com;
            this.messages = messages;
            this.tlv_utils = tlv_utils;
            neighborsTable = new Dictionary<PeerAddress, PeerInfo>();
        }

        public void TreatTLV(PeerAddress a, TLV tlv)
        {
            try
            {
                switch (tlv.type)
                {
                    case TLV.Type.Hello:
                        ulong? src_ID = tlv_utils.getHelloSource(tlv);
                        if (src_ID.HasValue)
                        {
                            PeerInfo pi;
                            if (neighborsTable.ContainsKey(a))
                                pi = neighborsTable[a];
                            else
                            {
                                pi = new PeerInfo();
                                pi.lastHelloLong = null;
                                // For a first contact, we reply immediatly to avoid a 30sec delay for establishing a symetric relation
                                com.SendMessage(a, messages.PackTLV(tlv_utils.longHello(src_ID.Value)));
                            }
                            pi.ID = src_ID.Value;
                            pi.lastHello = Stopwatch.StartNew();
                            if (tlv_utils.isValidLongHello(tlv))
                                pi.lastHelloLong = Stopwatch.StartNew();
                            neighborsTable[a] = pi;
                            potentialNeighbors[a] = 0;
                        }
                        break;
                    case TLV.Type.GoAway:
                        byte? reason = tlv_utils.getGoAwayCode(tlv);
                        if (reason.HasValue)
                        {
                            try { neighborsTable.Remove(a); } catch { }
                        }
                        break;
                    case TLV.Type.Neighbour:
                        PeerAddress? pa = tlv_utils.getNeighbourAddress(tlv);
                        if (pa.HasValue)
                        {
                            if (!com.IsSelf(pa.Value))
                                potentialNeighbors[pa.Value] = 0;
                        }
                        break;
                }
            }
            catch { }
        }

        public bool IsSymetricNeighbor(PeerAddress a)
        {
            try
            {
                if (neighborsTable[a].lastHelloLong != null)
                    if (neighborsTable[a].lastHelloLong.ElapsedMilliseconds <= symetricDelay * 1000)
                        return true;
                return false;
            }
            catch { return false; }
        }

        public PeerAddress[] GetNeighbors()
        {
            return neighborsTable.Keys.ToArray();
        }

        public PeerAddress[] GetSymetricsNeighbors()
        {
            List<PeerAddress> sn = new List<PeerAddress>();
            foreach (PeerAddress a in neighborsTable.Keys)
            {
                if (IsSymetricNeighbor(a))
                    sn.Add(a);
            }
            return sn.ToArray();
        }

        public void Goodbye(PeerAddress n, byte reason)
        {
            com.SendMessage(n, messages.PackTLV(tlv_utils.goAway(reason, "")));
            neighborsTable.Remove(n);
        }

        bool IsRecentNeighbor(PeerAddress a)
        {
            try
            {
                if (neighborsTable[a].lastHello != null)
                    if (neighborsTable[a].lastHello.ElapsedMilliseconds <= recentDelay * 1000)
                        return true;
                return false;
            }
            catch { return false; }
        }
        bool RemoveOldestPotentialNeighbor()
        {
            int max = -1;
            PeerAddress? addr_max = null;
            foreach (PeerAddress addr in potentialNeighbors.Keys)
            {
                if (potentialNeighbors[addr] > max)
                {
                    max = potentialNeighbors[addr];
                    addr_max = addr;
                }
            }
            if (addr_max.HasValue)
            {
                potentialNeighbors.Remove(addr_max.Value);
                return true;
            }
            return false;
        }
        public void RemoveOldNeighbors()
        {
            PeerAddress[] ns = GetNeighbors();
            foreach (PeerAddress n in ns)
            {
                if (!IsRecentNeighbor(n))
                    Goodbye(n,2);
            }
            // If the list of potential neighbors is too big, we also purge it
            while (potentialNeighbors.Count > maxPotentialNeighborsNumber)
            {
                if (!RemoveOldestPotentialNeighbor())
                    break;
            }
        }

        public void SayHello()
        {
            // Short hello if not enough neighbours
            if (GetSymetricsNeighbors().Length < searchNeighborsThreshold)
            {
                PeerAddress[] ps = potentialNeighbors.Keys.ToArray();
                foreach (PeerAddress p in ps)
                {
                    if (!neighborsTable.ContainsKey(p))
                    {
                        com.SendMessage(p, messages.PackTLV(tlv_utils.shortHello()));
                        if (potentialNeighbors[p] >= 0 && potentialNeighbors[p] < int.MaxValue)
                            potentialNeighbors[p]++;
                    }
                }
            }
            // Long hello
            foreach (PeerAddress p in GetNeighbors())
            {
                ulong dest_ID = neighborsTable[p].ID;
                com.SendMessage(p, messages.PackTLV(tlv_utils.longHello(dest_ID)));
            }
        }

        public void SendNeighbors()
        {
            PeerAddress[] sym = GetSymetricsNeighbors();
            foreach (PeerAddress p in GetNeighbors())
            {
                List<TLV> tlvs = new List<TLV>();
                foreach (PeerAddress pa in sym)
                {
                    if (!pa.Equals(p))
                        tlvs.Add(tlv_utils.neighbour(pa));
                }

                byte[][] msgs = messages.PackTLVs(tlvs.ToArray());
                foreach (byte[] msg in msgs)
                    com.SendMessage(p, msg);
            }
        }

        public void GoodByeEveryone()
        {
            PeerAddress[] neighbors = GetNeighbors();
            foreach (PeerAddress p in neighbors)
            {
                Goodbye(p, 1);
            }
        }
    }
}
