using System.Drawing;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public abstract class AbstractMapleMapObject : IMapleMapObject
    {
        public int ObjectId { get; set; } = 0;

        public Point Position { get; set; }

        public abstract void SendDestroyData(MapleClient client);
        public abstract void SendSpawnData(MapleClient client);
        public new abstract MapleMapObjectType GetType();
    }
}