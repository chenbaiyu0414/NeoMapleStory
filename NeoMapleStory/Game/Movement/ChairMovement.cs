using NeoMapleStory.Core.IO;
using System.Drawing;

namespace NeoMapleStory.Game.Movement
{
    public class ChairMovement : AbstractLifeMovement
    {
        public int Unk { get; set; }

        public ChairMovement(byte type, Point position, short duration, byte newstate)
        : base(type, position, duration, newstate)
        {
        }

        public override void Serialize(OutPacket packet)
        {
            packet.WriteByte(Type);
            packet.WriteShort((short)Position.X);
            packet.WriteShort((short)Position.Y);
            packet.WriteShort((short)Unk);
            packet.WriteByte(Newstate);
            packet.WriteShort(Duration);
        }
    }
}
