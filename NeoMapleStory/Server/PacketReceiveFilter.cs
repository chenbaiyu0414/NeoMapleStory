using System;
using System.Linq;
using NeoMapleStory.Core.Encryption;
using SuperSocket.Common;
using SuperSocket.Facility.Protocol;

namespace NeoMapleStory.Server
{
    internal sealed class PacketReceiveFilter : FixedHeaderReceiveFilter<PacketRequestInfo>
    {
        public PacketReceiveFilter()
            : base(4)
        {
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            var result = MapleCipher.GetPacketLength(header.CloneRange(offset, length));
            return result;
        }

        protected override PacketRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset,
            int length)
        {
            var packet = new PacketRequestInfo();
            packet.Header = header.ToArray();
            packet.Data = bodyBuffer.CloneRange(offset, length);
            return packet;
        }
    }
}