using SuperSocket.SocketBase.Protocol;

namespace NeoMapleStory.Server
{
    public sealed class PacketRequestInfo : IRequestInfo
    {
        public byte[] Header { get; set; }

        public byte[] Data { get; set; }
        public string Key { get; set; }
    }
}