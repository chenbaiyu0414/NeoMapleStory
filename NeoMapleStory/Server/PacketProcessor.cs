using System;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Server
{
    public delegate void PacketHandler(MapleClient c, InPacket p);


    public sealed class PacketProcessor
    {
        private readonly PacketHandler[] m_mHandlers;

        public PacketProcessor(string label)
        {
            Label = label;
            m_mHandlers = new PacketHandler[0xFFFF];
        }

        public string Label { get; private set; }

        public PacketHandler this[short opcode] => m_mHandlers[opcode];

        public void AppendHandler(short opcode, PacketHandler handler)
        {
            if (m_mHandlers[opcode] != null)
                throw new InvalidOperationException("已经注册过某个Opcode!");

            m_mHandlers[opcode] = handler;
        }
    }
}