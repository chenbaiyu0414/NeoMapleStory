using System.Drawing;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Game.Movement
{
    public class ArasMovement : AbstractLifeMovement
    {
        public ArasMovement(byte type, Point position, short unk, byte newstate)
            : base(type, position, unk, newstate)
        {
        }

        public short Unk { get; set; }

        public override void Serialize(OutPacket lew)
        {
            lew.WriteByte(Type);
            lew.WriteByte(Newstate);
            lew.WriteShort(Duration);
        }
    }
}