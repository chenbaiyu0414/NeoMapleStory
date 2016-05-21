using System.Drawing;
using NeoMapleStory.Server;

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