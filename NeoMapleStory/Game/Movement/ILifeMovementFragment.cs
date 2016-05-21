using System.Drawing;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Game.Movement
{
    public interface ILifeMovementFragment
    {
        Point Position { get; }
        void Serialize(OutPacket packet);
    }
}