using NeoMapleStory.Game.Client;
using NeoMapleStory.Server;
using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.World
{
    public class MapleParty
    {
        public MaplePartyCharacter Leader { get; set; }     
        public int PartyId { get; set; }
        public int Cp { get; set; }
        public int Team { get; set; }
        public int TotalCp { get; set; }
        public MapleParty CpqEnemy { get; set; } = null;

        private readonly List<MaplePartyCharacter> _mMembers = new List<MaplePartyCharacter>();

        public MapleParty(int id, MaplePartyCharacter chrfor)
        {
            Leader = chrfor;
            _mMembers.Add(Leader);
            PartyId = id;
        }

        public bool ContainsMember(MaplePartyCharacter member)
        {
            return _mMembers.Contains(member);
        }

        public void AddMember(MaplePartyCharacter member)
        {
            _mMembers.Add(member);
        }

        public void RemoveMember(MaplePartyCharacter member)
        {
            _mMembers.Remove(member);
        }

        public void UpdateMember(MaplePartyCharacter member)
        {
            for (int i = 0; i < _mMembers.Count; i++)
            {
                MaplePartyCharacter chr = _mMembers[i];
                if (chr.Equals(member))
                {
                    _mMembers[i] = member;
                }
            }
        }

        public MaplePartyCharacter GetMemberById(int id)=> _mMembers.FirstOrDefault(x => x.ChannelId == id);


        public List<MaplePartyCharacter> GetMembers() => _mMembers;


        public List<MapleCharacter> GetPartyMembers(MapleParty party)
        {
            if (party == null)
            {
                return null;
            }

            List<MapleCharacter> chars = new List<MapleCharacter>();

            MasterServer.Instance.ChannelServers.ForEach(server => {
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
            return 1 * 31 + PartyId;
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
            MapleParty other = (MapleParty)obj;
            if (PartyId != other.PartyId)
            {
                return false;
            }
            return true;
        }
    }
}
