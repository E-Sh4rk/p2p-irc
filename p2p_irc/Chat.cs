using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace p2p_irc
{
    public struct MessageIdentifier
    {
        public ulong sender;
        public uint nonce;
    }

    public class NeighborFloodInfo
    {
        public Stopwatch lastAttempt;
        public int nextAttemptMillisecond;
        public int numberAttempts;
    }

    public class MessageFloodInfo
    {
        public Stopwatch timeElapsed;
        public string msg;
        public Dictionary<PeerAddress, NeighborFloodInfo> neighbors;
    }

    public class Chat
    {
        public const int recentMessageDelay = 60 * 5; // Durée pendant laquelle on garde un message en memoire
        public const int max_flood_tries_number = 5;

        TLV_utils tlv_utils;
        Communications com;
        Messages messages;
        Peers peers;

        Random r;
        Dictionary<MessageIdentifier, MessageFloodInfo> recent_messages;
        public delegate void NewMessage(ulong id, string msg);
        NewMessage new_message_action;

        public Chat(Communications com, TLV_utils tlv_utils, Messages messages, Peers peers, NewMessage new_message_action)
        {
            r = new Random();
            this.com = com;
            this.messages = messages;
            this.tlv_utils = tlv_utils;
            this.peers = peers;
            this.new_message_action = new_message_action;
            recent_messages = new Dictionary<MessageIdentifier, MessageFloodInfo>();
        }

        public void DeleteAll()
        {
            recent_messages.Clear();
        }

        int ComputeNextFloodAttempt(int numberAttempts)
        {
            int res = (int)(Math.Pow(2.0, numberAttempts - 1) * 1000.0);
            return res + r.Next(res);
        }
        MessageFloodInfo InitNewFloodInfo(string msg)
        {
            Dictionary<PeerAddress, NeighborFloodInfo> neighbors = new Dictionary<PeerAddress, NeighborFloodInfo>();
            foreach (PeerAddress p in peers.GetSymetricsNeighbors())
            {
                NeighborFloodInfo nii = new NeighborFloodInfo();
                nii.numberAttempts = 0;
                nii.nextAttemptMillisecond = ComputeNextFloodAttempt(nii.numberAttempts);
                nii.lastAttempt = Stopwatch.StartNew();
                neighbors[p] = nii;
            }
            MessageFloodInfo mii = new MessageFloodInfo();
            mii.neighbors = neighbors;
            mii.timeElapsed = Stopwatch.StartNew();
            mii.msg = msg;
            return mii;
        }

        public void TreatTLV(PeerAddress a, TLV tlv)
        {
            try
            {
                TLV_utils.DataMessage dm;
                switch (tlv.type)
                {
                    case TLV.Type.Data:
                        dm = tlv_utils.getDataMessage(tlv);
                        if (dm != null && peers.IsSymetricNeighbor(a))
                        {
                            MessageIdentifier mid = new MessageIdentifier();
                            mid.nonce = dm.nonce;
                            mid.sender = dm.sender;

                            // Adding message to the flooding list...
                            if (!recent_messages.ContainsKey(mid))
                            {
                                MessageFloodInfo mii = InitNewFloodInfo(dm.msg);
                                recent_messages[mid] = mii;
                                new_message_action(mid.sender, dm.msg);
                            }
                            // Ack & Remove from flooding list
                            com.SendMessage(a, messages.PackTLV(tlv_utils.ack(mid.sender, mid.nonce)));
                            try { recent_messages[mid].neighbors.Remove(a); } catch { }
                        }
                        break;
                    case TLV.Type.Ack:
                        dm = tlv_utils.getAckMessage(tlv);
                        if (dm != null && peers.IsSymetricNeighbor(a))
                        {
                            MessageIdentifier mid = new MessageIdentifier();
                            mid.nonce = dm.nonce;
                            mid.sender = dm.sender;
                            try { recent_messages[mid].neighbors.Remove(a); } catch { }
                        }
                        break;
                }
            }
            catch { }
        }

        bool IsRecentMessage(MessageIdentifier m)
        {
            try
            {
                if (recent_messages[m].timeElapsed != null)
                    if (recent_messages[m].timeElapsed.ElapsedMilliseconds <= recentMessageDelay * 1000)
                        return true;
                return false;
            }
            catch { return false; }
        }
        public void RemoveOldMessages()
        {
            MessageIdentifier[] recent_mes = recent_messages.Keys.ToArray();
            foreach (MessageIdentifier m in recent_mes)
            {
                if (!IsRecentMessage(m))
                    recent_messages.Remove(m);
            }
        }

        public void Flood()
        {
            foreach (MessageIdentifier m in recent_messages.Keys)
            {
                PeerAddress[] pas = recent_messages[m].neighbors.Keys.ToArray();
                foreach (PeerAddress pa in pas)
                {
                    if (!peers.IsSymetricNeighbor(pa))
                        recent_messages[m].neighbors.Remove(pa);
                    else
                    {
                        NeighborFloodInfo nfi = recent_messages[m].neighbors[pa];
                        if (nfi.lastAttempt.ElapsedMilliseconds >= nfi.nextAttemptMillisecond)
                        {
                            nfi.numberAttempts++;
                            nfi.lastAttempt = Stopwatch.StartNew();
                            nfi.nextAttemptMillisecond = ComputeNextFloodAttempt(nfi.numberAttempts);
                            com.SendMessage(pa, messages.PackTLV(tlv_utils.data(m.sender, m.nonce, recent_messages[m].msg)));
                            if (nfi.numberAttempts >= max_flood_tries_number)
                            {
                                recent_messages[m].neighbors.Remove(pa);
                                peers.Goodbye(pa, 2);
                            }
                        }
                    }
                }
            }
        }

        public void SendMessage(string msg)
        {
            MessageIdentifier mid = new MessageIdentifier();
            mid.sender = tlv_utils.getSelfID();
            mid.nonce = tlv_utils.nextDataNonce();
            // Send the message only if it is not too big
            if (messages.PackTLV(tlv_utils.data(mid.sender, mid.nonce, msg)) != null)
            {
                MessageFloodInfo mii = InitNewFloodInfo(msg);
                recent_messages[mid] = mii;
                new_message_action(mid.sender, msg);
            }
        }
    }
}
