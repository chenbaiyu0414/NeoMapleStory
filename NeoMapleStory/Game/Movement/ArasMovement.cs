using NeoMapleStory.Core.IO;
using System.Drawing;

namespace NeoMapleStory.Game.Movement
{
    public class ArasMovement:AbstractLifeMovement
    {
        public short Unk { get; set; }

        public ArasMovement(byte type, Point position, short unk, byte newstate)
        :base(type, position, unk, newstate)
        { 
        }

        public override void Serialize(OutPacket lew)
        {
            lew.WriteByte(Type);
            lew.WriteByte(Newstate);
            lew.WriteShort(Duration);
        }
    }
}
