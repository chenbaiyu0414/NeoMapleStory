using NeoMapleStory.Core.IO;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public class MapleMapEffect
    {
        private readonly bool m_mActive = true;
        private readonly int m_mItemId;
        private readonly string m_mMsg;

        public MapleMapEffect(string msg, int itemId)
        {
            m_mMsg = msg;
            m_mItemId = itemId;
        }

        public OutPacket CreateDestroyData()
        {
            return PacketCreator.RemoveMapEffect();
        }

        public OutPacket CreateStartData()
        {
            return PacketCreator.StartMapEffect(m_mMsg, m_mItemId, m_mActive);
        }

        public void SendStartData(MapleClient client)
        {
            client.Send(CreateStartData());
        }
    }
}