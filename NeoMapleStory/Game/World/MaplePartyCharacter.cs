using System.Drawing;
using System.Linq;
using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.World
{
    public  class MaplePartyCharacter
    {
        public string CharacterName { get; private set; }
        public int CharacterId { get; private set; }
        public int Level { get; private set; }
        public int ChannelId { get; private set; }
        public int JobId { get; private set; }
        public int MapId { get; private set; }
        public bool Gender { get; private set; }
        public bool IsMarried { get; private set; }
        public int DoorTown { get; private set; } = 999999999;
        public int DoorTarget { get; private set; } = 999999999;
        public Point DoorPosition { get; private set; } = new Point(0, 0);
        public bool IsOnline { get; set; }

        private MapleCharacter _player;

        public MaplePartyCharacter(MapleCharacter maplechar)
        {
            CharacterName = maplechar.Name;
            Level = maplechar.Level;
            ChannelId = maplechar.Client.ChannelId;
            CharacterId = maplechar.Id;
            JobId = maplechar.Job.JobId;
            MapId = maplechar.Map.MapId;
            IsOnline = true;
            Gender = maplechar.Gender;
            IsMarried = maplechar.IsMarried;
            if (maplechar.Doors.Any())
            {
                DoorTown = maplechar.Doors[0].Town.MapId;
                DoorTarget = maplechar.Doors[0].TargetMap.MapId;
                DoorPosition = maplechar.Doors[0].TargetMapPosition;
            }
        }

        public MaplePartyCharacter()
        {
            CharacterName = "";
        }

        //public MapleCharacter getPlayer()
        //{
        //    //return ChannelServer.getInstance(ChannelID).getPlayerStorage().getCharacterById(CharacterID);
        //}

        public override int GetHashCode()
        {
            return 31 * 1 + CharacterName == null ? 0 : CharacterName.GetHashCode();
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
            MaplePartyCharacter other = (MaplePartyCharacter)obj;
            if (CharacterName == null)
            {
                if (other.CharacterName != null)
                {
                    return false;
                }
            }
            else if (!CharacterName.Equals(other.CharacterName))
            {
                return false;
            }
            return true;
        }
    }
}
