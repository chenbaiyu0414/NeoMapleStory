namespace NeoMapleStory.Game.Life
{
    public class BanishInfo
    {
        public BanishInfo(string msg, int map, string portal)
        {
            Msg = msg;
            MapId = map;
            Portal = portal;
        }

        public int MapId { get; private set; }
        public string Portal { get; private set; }
        public string Msg { get; private set; }
    }
}