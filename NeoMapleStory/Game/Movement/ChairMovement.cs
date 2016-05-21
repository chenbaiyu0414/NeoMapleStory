using System.Drawing;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Game.Movement
{
    public class ChairMovement : AbstractLifeMovement
    {
        public ChairMovement(byte type, Point position, short duration, byte newstate)
            : base(type, position, duration, newstate)
        {
        }

        public int Unk { get; set; }

        public override void Serialize(OutPacket packet)
        {
            packet.WriteByte(Type);
            packet.WriteShort((short) Position.X);
            packet.WriteShort((short) Position.Y);
            packet.WriteShort((short) Unk);
            packet.WriteByte(Newstate);
            packet.WriteShort(Duration);
        }
    }
}