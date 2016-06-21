using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Client.Message
{
    public class ServernoticeMapleClientMessageCallback : IMessageCallback
    {
        private readonly MapleClient m_client;
        private readonly int m_mode;

        public ServernoticeMapleClientMessageCallback(MapleClient c): this(c.Player.IsGm ? 6 : 5, c)
        {
        }

        public ServernoticeMapleClientMessageCallback(int mode, MapleClient client)
        {
            m_client = client;
            m_mode = mode;
        }

        public void DropMessage(string message)
        {
            m_client.Send(PacketCreator.ServerNotice((PacketCreator.ServerMessageType)m_mode, message));
        }
    }
}
