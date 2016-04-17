using SuperSocket.SocketBase.Protocol;

namespace NeoMapleStory.Server
{

    public sealed class PacketRequestInfo : IRequestInfo
    {
        public string Key { get; set; }

        public byte[] Header { get; set; }

        public byte[] Data { get; set; }
    }
}
