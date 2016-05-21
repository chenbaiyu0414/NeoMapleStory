using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.World
{
    public class MapleMessengerCharacter
    {
        public MapleMessengerCharacter(MapleCharacter maplechar)
        {
            CharacterName = maplechar.Name;
            ChannelId = maplechar.Client.ChannelId;
            CharacterId = maplechar.Id;
            IsOnline = true;
            Position = 0;
        }

        public MapleMessengerCharacter(MapleCharacter maplechar, int position)
        {
            CharacterName = maplechar.Name;
            ChannelId = maplechar.Client.ChannelId;
            CharacterId = maplechar.Id;
            IsOnline = true;
            Position = position;
        }

        public MapleMessengerCharacter()
        {
            CharacterName = "";
        }

        public string CharacterName { get; }
        public int CharacterId { get; private set; }
        public int ChannelId { get; private set; }
        public bool IsOnline { get; set; }
        public int Position { get; set; }

        public override int GetHashCode()
        {
            return 1*31 + CharacterName == null ? 0 : CharacterName.GetHashCode();
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
            var other = (MapleMessengerCharacter) obj;
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