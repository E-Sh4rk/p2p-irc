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

		const int minSymetricNeighbors = 8;

		Dictionary<PeerAddress, PeerInfo> neighborsTable = new Dictionary<PeerAddress, PeerInfo>();
		List<PeerAddress> potentialNeighbors;

		public Peers(List<PeerAddress> potentialNeighbors)
		{
			this.potentialNeighbors = potentialNeighbors;
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
	}
}
