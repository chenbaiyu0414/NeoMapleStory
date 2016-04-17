using NeoMapleStory.Core.IO;
using System.Drawing;

namespace NeoMapleStory.Game.Movement
{
    public class AbsoluteLifeMovement : AbstractLifeMovement
    {
        public Point PixelsPerSecond { get; set; }
        public short Unk { get; set; }

        public AbsoluteLifeMovement(byte type, Point position, short duration, byte newstate)
            : base(type, position, duration, newstate)
        {

        }

        public override void Serialize(OutPacket packet)
        {
            packet.WriteByte(Type);
            packet.WriteShort((short)Position.X);
            packet.WriteShort((short)Position.Y);
            packet.WriteShort((short)PixelsPerSecond.X);
            packet.WriteShort((short)PixelsPerSecond.Y);
            packet.WriteShort(Unk);
            packet.WriteByte(Newstate);
            packet.WriteShort(Duration);
        }
    }
}
