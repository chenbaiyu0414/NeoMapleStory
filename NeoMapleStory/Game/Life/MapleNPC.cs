using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Shop;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Life
{
    public class MapleNpc : AbstractLoadedMapleLife
    {
        private readonly MapleNpcStats m_mStats;

        public MapleNpc(int id, MapleNpcStats stats)
            : base(id)
        {
            m_mStats = stats;
        }

        public bool IsCustom { get; set; } = false;
        public string Name => m_mStats.NpcName;

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
            else
            {
                client.Send(PacketCreator.SpawnNpc(this));
                client.Send(PacketCreator.SpawnNpcRequestController(this, true));
            }
        }
    }
}