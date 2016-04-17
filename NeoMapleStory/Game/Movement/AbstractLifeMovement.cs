using System.Drawing;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Game.Movement
{
    public abstract class AbstractLifeMovement : ILifeMovement
    {
        public short Duration { get; private set; }

        public byte Newstate { get; private set; }

        public Point Position { get; private set; }

        public byte Type { get; private set; }

        public AbstractLifeMovement(byte type, Point position, short duration, byte newstate)
        {
            Type = type;
            Position = position;
            Duration = duration;
            Newstate = newstate;
        }

        public abstract void Serialize(OutPacket packet);

    }
}
