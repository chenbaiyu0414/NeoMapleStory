using System.Drawing;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Game.Movement
{
    public abstract class AbstractLifeMovement : ILifeMovement
    {
        public AbstractLifeMovement(byte type, Point position, short duration, byte newstate)
        {
            Type = type;
            Position = position;
            Duration = duration;
            Newstate = newstate;
        }

        public short Duration { get; }

        public byte Newstate { get; }

        public Point Position { get; }

        public byte Type { get; }

        public abstract void Serialize(OutPacket packet);
    }
}