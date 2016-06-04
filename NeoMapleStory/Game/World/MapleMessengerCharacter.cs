using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.World
{
    public class MapleMessengerCharacter
    {


        public string CharacterName { get; }
        public int CharacterId { get; private set; }
        public int ChannelId { get; private set; }
        public bool IsOnline { get; set; }
        public int Position { get; set; }

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
            CharacterName = null;
        }

        public static bool operator ==(MapleMessengerCharacter left, MapleMessengerCharacter right)
        {
            if (left?.CharacterName == null || right?.CharacterName == null)
                return false;
            return left.CharacterName == right.CharacterName;
        }

        public static bool operator !=(MapleMessengerCharacter left, MapleMessengerCharacter right)
        {
            return !(left == right);
        }

        protected bool Equals(MapleMessengerCharacter other)
        {
            return string.Equals(CharacterName, other.CharacterName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MapleMessengerCharacter) obj);
        }

        public override int GetHashCode()
        {
            return CharacterName.GetHashCode();
        }
    }
}