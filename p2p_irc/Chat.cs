﻿using System;
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
		public int nextAttemptMillisecond;
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

		int ComputeNextFloodAttempt(int numberAttempts)
		{
			int res = (int)(Math.Pow(2.0, numberAttempts - 1)*1000.0);
			return res + r.Next(res);
		}
		MessageFloodInfo InitNewFloodInfo()
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
			return mii;
		}

		public void TreatTLV(PeerAddress a, TLV tlv)
		{
			try
			{
				TLV_utils.DataMessage? dm;
				switch (tlv.type)
				{
					case TLV.Type.Data:
						dm = tlv_utils.getDataMessage(tlv);
						if (dm.HasValue && peers.IsSymetricNeighbor(a))
						{
							MessageIdentifier mid = new MessageIdentifier();
							mid.nonce = dm.Value.nonce;
							mid.sender = dm.Value.sender;

							// Adding message to the flooding list...
							if (!recent_messages.ContainsKey(mid))
							{
								MessageFloodInfo mii = InitNewFloodInfo();
								recent_messages[mid] = mii;
								new_message_action(mid.sender, dm.Value.msg);
							}
							// Ack & Remove from flooding list
							com.SendMessage(a, messages.PackTLV(tlv_utils.ack(mid.sender, mid.nonce)));
							try { recent_messages[mid].neighbors.Remove(a); } catch { }
						}
						break;
					case TLV.Type.Ack:
						dm = tlv_utils.getAckMessage(tlv);
						if (dm.HasValue && peers.IsSymetricNeighbor(a))
						{
							MessageIdentifier mid = new MessageIdentifier();
							mid.nonce = dm.Value.nonce;
							mid.sender = dm.Value.sender;
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

		public void Flood()
		{
			// TODO (+ dont forget to delete non-symetric neighbors from the list)
		}

		public void SendMessage(string msg)
		{
			MessageIdentifier mid = new MessageIdentifier();
			mid.sender = tlv_utils.getSelfID();
			mid.nonce = tlv_utils.nextDataNonce();
			MessageFloodInfo mii = InitNewFloodInfo();
			recent_messages[mid] = mii;
		}
	}
}
