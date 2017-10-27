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

	public struct PeerInfo
	{
		public ulong ID;
		public Stopwatch lastHello;
		public Stopwatch lastHelloLong;
	}

	public class Peers
	{
		public const int recentDelay = 60 * 2; // Durée max pour qu'un voisin soit considéré comme récent
		public const int symetricDelay = 60 * 2; // Durée max pour qu'un voisin soit considéré comme symétrique

		public const int searchNeighborsThreshold = 8;
		public const int helloNeighborsDelay = 30;
		public const int sendNeighborsDelay = 60;

		Dictionary<PeerAddress, PeerInfo> neighborsTable;
		List<PeerAddress> potentialNeighbors;

		TLV_utils tlv_utils;
		Communications com;
		Messages messages;

		public Peers(List<PeerAddress> potentialNeighbors, Communications com, TLV_utils tlv_utils, Messages messages)
		{
			this.potentialNeighbors = potentialNeighbors;
			this.com = com;
			this.messages = messages;
			this.tlv_utils = tlv_utils;
			neighborsTable = new Dictionary<PeerAddress, PeerInfo>();
		}

		// TODO : Delete some potential neighbors sometimes?

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
							}
							pi.ID = src_ID.Value;
							pi.lastHello = Stopwatch.StartNew();
							if (tlv_utils.isValidLongHello(tlv))
								pi.lastHelloLong = Stopwatch.StartNew();
							neighborsTable[a] = pi;
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
							if (!potentialNeighbors.Contains(pa.Value) && !com.IsSelf(pa.Value))
								potentialNeighbors.Add(pa.Value);
						}
						break;
				}
			} catch { }
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

				byte[] msg = messages.PackTLVs(tlvs.ToArray());
				com.SendMessage(p, msg);
			}
		}
	}
}
