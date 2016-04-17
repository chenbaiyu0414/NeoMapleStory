namespace NeoMapleStory.Game.Life
{
    public class BanishInfo
    {
        public int MapId { get; private set; }
        public string Portal { get; private set; }
        public string Msg { get; private set; }

        public BanishInfo(string msg, int map, string portal)
        {
            Msg = msg;
            MapId = map;
            Portal = portal;
        }
    }
}
