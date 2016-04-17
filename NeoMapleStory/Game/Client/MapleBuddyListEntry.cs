namespace NeoMapleStory.Game.Client
{
    public class MapleBuddyListEntry
    {
        public string CharacterName { get; private set; }
        public string Group { get; set; }
        public int CharacterId { get; private set; }
        public int ChannelId { get; set; }
        public bool Visible { get; set; }
        public bool IsOnline
        {
            get { return ChannelId >= 0; }
            set { IsOnline = false; ChannelId = -1; }
        }

        public MapleBuddyListEntry(string name, int characterId, int channel, bool visible)
        {
            CharacterName = name;
            Group = "群未定";
            CharacterId = characterId;
            ChannelId = channel;
            Visible = visible;
        }

        public MapleBuddyListEntry(string name, string group, int characterId, int channel, bool visible)
        {
            CharacterName = name;
            Group = group;
            CharacterId = characterId;
            ChannelId = channel;
            Visible = visible;
        }

        public override int GetHashCode()
        {
            return 1 * 31 + CharacterId;
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
            MapleBuddyListEntry other = (MapleBuddyListEntry)obj;
            if (CharacterId != other.CharacterId)
            {
                return false;
            }
            return true;
        }
    }
}
