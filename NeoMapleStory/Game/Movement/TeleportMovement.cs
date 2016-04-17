using NeoMapleStory.Core.IO;
using System.Drawing;

namespace NeoMapleStory.Game.Movement
{
    public class TeleportMovement : AbsoluteLifeMovement
    {
        public TeleportMovement(byte type, Point position, byte newstate)
            : base(type, position, 0, newstate)
        {

        }

        public new void Serialize(OutPacket packet)
        {
            packet.WriteByte(Type);
            packet.WriteShort((short)Position.X);
            packet.WriteShort((short)Position.Y);
            packet.WriteShort((short)PixelsPerSecond.X);
            packet.WriteShort((short)PixelsPerSecond.Y);
            packet.WriteByte(Newstate);
        }
    }
}
