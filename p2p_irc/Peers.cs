using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace p2p_irc
{
	public struct PeerAddress
	{
		public IPAddress ip;
		public int port;
	}

	public struct PeerInfo
	{
		public long ID;
		public DateTime lastHello;
		public DateTime lastHelloLong;
	}

	public class Peers
	{
		const int recentDelay = 60 * 2; // Durée max pour qu'un voisin soit considéré comme récent
		const int symetricDelay = 60 * 2; // Durée max pour qu'un voisin soit considéré comme symétrique

		const int searchNeighborsThreshold = 8;
		const int helloNeighborsDelay = 30;

		Dictionary<PeerAddress, PeerInfo> neighborsTable = new Dictionary<PeerAddress, PeerInfo>();
		List<PeerAddress> potentialNeighbors;

		TLV_utils tlv_utils;
		Communications com;
		Messages messages;

		public Peers(List<PeerAddress> potentialNeighbors, ulong ID, Communications com)
		{
			this.potentialNeighbors = potentialNeighbors;
			this.com = com;
			messages = new Messages(ID);
			tlv_utils = new TLV_utils(ID);
		}

		public bool IsRecentNeighbor(PeerAddress a)
		{
			try
			{
				if (neighborsTable[a].lastHello.AddSeconds(recentDelay) >= DateTime.Now)
					return true;
				return false;
			}
			catch { return false; }
		}

		public bool IsSymetricNeighbor(PeerAddress a)
		{
			try
			{
				if (neighborsTable[a].lastHelloLong.AddSeconds(symetricDelay) >= DateTime.Now)
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

		public PeerAddress[] GetRecentsNeighbors()
		{
			List<PeerAddress> sn = new List<PeerAddress>();
			foreach (PeerAddress a in neighborsTable.Keys)
			{
				if (IsRecentNeighbor(a))
					sn.Add(a);
			}
			return sn.ToArray();
		}

		public void SayHello()
		{
			// Short hello if not enough neighbours
			PeerAddress[] sym = GetSymetricsNeighbors();
			if (sym.Length < searchNeighborsThreshold)
			{
				foreach (PeerAddress pot in potentialNeighbors)
				{
					if (!IsSymetricNeighbor(pot))
						com.SendMessage(pot, messages.PackTLV(tlv_utils.shortHello()));
				}
			}
			// Long hello
			// TODO
		}
	}
}
