using NeoMapleStory.Core.Encryption;
using SuperSocket.Common;
using SuperSocket.Facility.Protocol;
using System;
using System.Linq;

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
            int result = MapleCipher.GetPacketLength(header.CloneRange(offset, length));
            return result;
        }

        protected override PacketRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {           
            PacketRequestInfo packet = new PacketRequestInfo();
            packet.Header = header.ToArray();
            packet.Data = bodyBuffer.CloneRange(offset, length);
            return packet;
        }
    }
}
