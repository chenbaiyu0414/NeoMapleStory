using System.Drawing;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Game.Movement
{
    public class AbsoluteLifeMovement : AbstractLifeMovement
    {
        public AbsoluteLifeMovement(byte type, Point position, short duration, byte newstate)
            : base(type, position, duration, newstate)
        {
        }

        public Point PixelsPerSecond { get; set; }
        public short Unk { get; set; }

        public override void Serialize(OutPacket packet)
        {
            packet.WriteByte(Type);
            packet.WriteShort((short) Position.X);
            packet.WriteShort((short) Position.Y);
            packet.WriteShort((short) PixelsPerSecond.X);
            packet.WriteShort((short) PixelsPerSecond.Y);
            packet.WriteShort(Unk);
            packet.WriteByte(Newstate);
            packet.WriteShort(Duration);
        }
    }
}