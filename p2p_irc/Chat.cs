using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace p2p_irc
{
	public struct MessageIdentifier
	{
		public ulong sender;
		public uint nonce;
	}

	public struct NeighborInondationInfo
	{
		public PeerAddress neighbor;
		public int numberAttempts;
	}

	public struct MessageInondationInfo
	{
		public Stopwatch timeElapsed;
		public List<NeighborInondationInfo> neighbors;
	}

	public class Chat
	{
		TLV_utils tlv_utils;
		Communications com;
		Messages messages;

		Dictionary<MessageIdentifier, MessageInondationInfo> inondationInfos;

		public Chat(Communications com, TLV_utils tlv_utils, Messages messages)
		{
			this.com = com;
			this.messages = messages;
			this.tlv_utils = tlv_utils;
			inondationInfos = new Dictionary<MessageIdentifier, MessageInondationInfo>();
		}
	}
}
