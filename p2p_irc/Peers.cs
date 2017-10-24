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
		public ulong ID;
		public DateTime lastHello;
		public DateTime lastHelloLong;
	}

	public class Peers
	{
		public const int recentDelay = 60 * 2; // Durée max pour qu'un voisin soit considéré comme récent
		public const int symetricDelay = 60 * 2; // Durée max pour qu'un voisin soit considéré comme symétrique

		public const int searchNeighborsThreshold = 8;
		public const int helloNeighborsDelay = 30;

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

		// TODO : Make it thread safe

		public void TreatHello(PeerAddress a, TLV hello)
		{
			ulong? src_ID = tlv_utils.getHelloSource(hello);
			if (src_ID.HasValue)
			{
				PeerInfo pi;
				if (neighborsTable.ContainsKey(a))
					pi = neighborsTable[a];
				else
				{
					pi = new PeerInfo();
					pi.lastHelloLong = DateTime.Now.AddSeconds(-symetricDelay);
				}
				pi.ID = src_ID.Value;
				pi.lastHello = DateTime.Now;
				if (tlv_utils.isValidLongHello(hello))
					pi.lastHelloLong = DateTime.Now;
				neighborsTable[a] = pi;
			}
		}

		public bool IsSymetricNeighbor(PeerAddress a)
		{
			try
			{
				if (neighborsTable[a].lastHelloLong.AddSeconds(symetricDelay) > DateTime.Now)
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

		bool IsRecentNeighbor(PeerAddress a)
		{
			try
			{
				if (neighborsTable[a].lastHello.AddSeconds(recentDelay) > DateTime.Now)
					return true;
				return false;
			}
			catch { return false; }
		}
		public void RemoveOldNeighbors()
		{
			PeerAddress[] ns = GetNeighbors();
			foreach (PeerAddress n in ns)
			{
				if (!IsRecentNeighbor(n))
				{
					com.SendMessage(n,messages.PackTLV(tlv_utils.goAway(2, "")));
					neighborsTable.Remove(n);
				}
			}
		}

		public void SayHello()
		{
			// Short hello if not enough neighbours
			if (GetSymetricsNeighbors().Length < searchNeighborsThreshold)
			{
				foreach (PeerAddress p in potentialNeighbors)
				{
					if (!IsSymetricNeighbor(p))
						com.SendMessage(p, messages.PackTLV(tlv_utils.shortHello()));
				}
			}
			// Long hello
			foreach (PeerAddress p in GetNeighbors())
			{
				ulong dest_ID = neighborsTable[p].ID;
				com.SendMessage(p, messages.PackTLV(tlv_utils.longHello(dest_ID)));
			}
		}
	}
}
