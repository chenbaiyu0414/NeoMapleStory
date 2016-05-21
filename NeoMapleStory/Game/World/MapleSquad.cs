using System.Collections.Generic;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Packet;

namespace NeoMapleStory.Game.World
{
    public enum MapleSquadType
    {
        Zakum = 0,
        Horntail = 1,
        Unknown = 2,
        Bossquest = 3
    }

    public class MapleSquad
    {
        private readonly List<MapleCharacter> m_bannedMembers = new List<MapleCharacter>();
        private int m_ch;

        public MapleSquad(int ch, MapleCharacter leader)
        {
            Leader = leader;
            Status = 1;
            Members.Add(leader);
            m_ch = ch;
        }

        public MapleCharacter Leader { get; }
        public int Status { get; set; }

        public List<MapleCharacter> Members { get; } = new List<MapleCharacter>();

        public bool ContainsMember(MapleCharacter member) => Members.Exists(x => x.Id == member.Id);

        public bool IsBanned(MapleCharacter member) => m_bannedMembers.Exists(x => x.Id == member.Id);

        public bool AddMember(MapleCharacter member)
        {
            if (IsBanned(member))
                return false;

            Members.Add(member);
            Leader.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText,
                $"{member.Name} 加入了远征队"));
            return true;
        }

        public void BanMember(MapleCharacter member, bool ban)
        {
            var index = Members.FindIndex(x => x.Id == member.Id);
            if (index > -1)
                Members.RemoveAt(index);

            if (ban)
                m_bannedMembers.Add(member);
        }

        public void Clear()
        {
            Members.Clear();
            m_bannedMembers.Clear();
        }
    }
}