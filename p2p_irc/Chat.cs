using System;
namespace p2p_irc
{
	public class Chat
	{
		TLV_utils tlv_utils;
		Communications com;
		Messages messages;

		public Chat(Communications com, TLV_utils tlv_utils, Messages messages)
		{
			this.com = com;
			this.messages = messages;
			this.tlv_utils = tlv_utils;
		}
	}
}
