using NeoMapleStory.Core.IO;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public class MapleMapEffect
    {
        private readonly string _mMsg;
        private readonly int _mItemId;
        private readonly bool _mActive = true;

        public MapleMapEffect(string msg, int itemId)
        {
            _mMsg = msg;
            _mItemId = itemId;
        }

        public OutPacket CreateDestroyData()
        {
            return PacketCreator.RemoveMapEffect();
        }

        public OutPacket CreateStartData()
        {
            return  PacketCreator.StartMapEffect(_mMsg, _mItemId, _mActive);
        }

        public void SendStartData(MapleClient client)
        {
            client.Send(CreateStartData());
        }
    }
}
