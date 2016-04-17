using NeoMapleStory.Server;
using System.Drawing;

namespace NeoMapleStory.Game.Map
{
    public interface IMapleMapObject
    {
        int ObjectId { get; set; }

        Point Position { get; set; }

        MapleMapObjectType GetType();

        void SendSpawnData(MapleClient client);

        void SendDestroyData(MapleClient client);
    }
}
