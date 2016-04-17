using NeoMapleStory.Core.IO;
using System;

namespace NeoMapleStory.Server
{
    public delegate void PacketHandler(MapleClient c, InPacket p);

    
    public sealed class PacketProcessor
    {
        public string Label { get; private set; }

        private readonly PacketHandler[] _mHandlers;

        public PacketProcessor(string label)
        {
            Label = label;
            _mHandlers = new PacketHandler[0xFFFF];
        }

        public void AppendHandler(short opcode, PacketHandler handler)
        {
            if (_mHandlers[opcode] != null)
                throw new InvalidOperationException("已经注册过某个Opcode!");

            _mHandlers[opcode] = handler;
        }

        public PacketHandler this[short opcode] => _mHandlers[opcode];
    }
}
