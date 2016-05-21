using System.Collections.Generic;
using System.Linq;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.World
{
    public class MapleParty
    {
        private readonly List<MaplePartyCharacter> m_mMembers = new List<MaplePartyCharacter>();

        public MapleParty(int id, MaplePartyCharacter chrfor)
        {
            Leader = chrfor;
            m_mMembers.Add(Leader);
            PartyId = id;
        }

        public MaplePartyCharacter Leader { get; set; }
        public int PartyId { get; set; }
        public int Cp { get; set; }
        public int Team { get; set; }
        public int TotalCp { get; set; }
        public MapleParty CpqEnemy { get; set; } = null;

        public bool ContainsMember(MaplePartyCharacter member)
        {
            return m_mMembers.Contains(member);
        }

        public void AddMember(MaplePartyCharacter member)
        {
            m_mMembers.Add(member);
        }

        public void RemoveMember(MaplePartyCharacter member)
        {
            m_mMembers.Remove(member);
        }

        public void UpdateMember(MaplePartyCharacter member)
        {
            for (var i = 0; i < m_mMembers.Count; i++)
            {
                var chr = m_mMembers[i];
                if (chr.Equals(member))
                {
                    m_mMembers[i] = member;
                }
            }
        }

        public MaplePartyCharacter GetMemberById(int id) => m_mMembers.FirstOrDefault(x => x.ChannelId == id);


        public List<MaplePartyCharacter> GetMembers() => m_mMembers;


        public List<MapleCharacter> GetPartyMembers(MapleParty party)
        {
            if (party == null)
            {
                return null;
            }

            var chars = new List<MapleCharacter>();

            MasterServer.Instance.ChannelServers.ForEach(server =>
            {
                server.GetPartyMembers(party).ForEach(chr =>
                {
                    if (chr != null)
                        chars.Add(chr);
                });
            });
            return chars;
        }

        public List<MapleCharacter> GetPartyMembers()
        {
            return GetPartyMembers(this);
        }

        public override int GetHashCode()
        {
            return 1*31 + PartyId;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this != obj)
            {
                return false;
            }
            var other = (MapleParty) obj;
            if (PartyId != other.PartyId)
            {
                return false;
            }
            return true;
        }
    }
}