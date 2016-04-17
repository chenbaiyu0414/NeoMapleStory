using NeoMapleStory.Core.IO;
using System.Drawing;

namespace NeoMapleStory.Game.Movement
{
    public class JumpDownMovement : AbstractLifeMovement
    {
        public Point PixelsPerSecond { get; set; }
        public int Unk { get; set; }
        public int Fh { get; set; }

        public JumpDownMovement(byte type, Point position, short duration, byte newstate)
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
            packet.WriteShort((short)Unk);
            packet.WriteShort((short)Fh);
            packet.WriteByte(Newstate);
            packet.WriteShort(Duration);
        }
    }
}
