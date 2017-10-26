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

	public struct NeighborFloodInfo
	{
		public Stopwatch lastAttempt;
		public int numberAttempts;
	}

	public struct MessageFloodInfo
	{
		public Stopwatch timeElapsed;
		public Dictionary<PeerAddress, NeighborFloodInfo> neighbors;
	}

	public class Chat
	{
		public const int recentMessageDelay = 60 * 5; // Durée max pendant laquelle on continue à innonder

		TLV_utils tlv_utils;
		Communications com;
		Messages messages;
		Peers peers;

		Dictionary<MessageIdentifier, MessageFloodInfo> recent_messages;
		public delegate void NewMessage(ulong id, string msg);
		NewMessage new_message_action;

		public Chat(Communications com, TLV_utils tlv_utils, Messages messages, Peers peers, NewMessage new_message_action)
		{
			this.com = com;
			this.messages = messages;
			this.tlv_utils = tlv_utils;
			this.peers = peers;
			this.new_message_action = new_message_action;
			recent_messages = new Dictionary<MessageIdentifier, MessageFloodInfo>();
		}

		public void TreatTLV(PeerAddress a, TLV tlv)
		{
			try
			{
				switch (tlv.type)
				{
					case TLV.Type.Data:
						TLV_utils.DataMessage? dm = tlv_utils.getDataMessage(tlv);
						if (dm.HasValue && peers.IsSymetricNeighbor(a))
						{
							MessageIdentifier mid = new MessageIdentifier();
							mid.nonce = dm.Value.nonce;
							mid.sender = dm.Value.sender;

							// Adding message to the flooding list...
							if (!recent_messages.ContainsKey(mid))
							{
								Dictionary<PeerAddress, NeighborFloodInfo> neighbors = new Dictionary<PeerAddress, NeighborFloodInfo>();
								foreach (PeerAddress p in peers.GetSymetricsNeighbors())
								{
									NeighborFloodInfo nii = new NeighborFloodInfo();
									nii.numberAttempts = 0;
									nii.lastAttempt = Stopwatch.StartNew();
									neighbors[p] = nii;
								}
								MessageFloodInfo mii = new MessageFloodInfo();
								mii.neighbors = neighbors;
								mii.timeElapsed = Stopwatch.StartNew();
								recent_messages[mid] = mii;

								new_message_action(mid.sender, dm.Value.msg);
							}
							// Ack & Remove from flooding list
							com.SendMessage(a, messages.PackTLV(tlv_utils.ack(mid.sender, mid.nonce)));
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
					try { recent_messages.Remove(m); } catch { }
			}
		}
	}
}
