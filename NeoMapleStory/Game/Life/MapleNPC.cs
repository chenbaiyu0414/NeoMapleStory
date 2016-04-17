using NeoMapleStory.Game.Map;
using NeoMapleStory.Server;
using NeoMapleStory.Packet;
using NeoMapleStory.Game.Shop;

namespace NeoMapleStory.Game.Life
{
    public class MapleNpc : AbstractLoadedMapleLife
    {
        private readonly MapleNpcStats _mStats;
        public bool IsCustom { get; set; } = false;
        public string Name => _mStats.NpcName;

        public MapleNpc(int id, MapleNpcStats stats)
            : base(id)
        {
            _mStats = stats;
        }

        public bool HasShop()
        {
            return MapleShopFactory.Instance.GetShopForNpc(Id) != null;
        }

        public void SendShop(MapleClient c)
        {
            MapleShopFactory.Instance.GetShopForNpc(Id).SendShop(c);
        }

        public override MapleMapObjectType GetType() => MapleMapObjectType.Npc;

        public override void SendDestroyData(MapleClient client)
        {
            client.Send(PacketCreator.RemoveNpc(ObjectId));
        }

        public override void SendSpawnData(MapleClient client)
        {
            if (Name.Contains("Maple TV"))
            {
                return;
            }
            if (Id >= 9010011 && Id <= 9010013)
            {
                client.Send(PacketCreator.SpawnNpcRequestController(this, false));
            }
            else {
                client.Send(PacketCreator.SpawnNpc(this));
                client.Send(PacketCreator.SpawnNpcRequestController(this, true));
            }
        }
    }
}
