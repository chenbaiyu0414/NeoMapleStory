using NeoMapleStory.Core.IO;
using System.Drawing;

namespace NeoMapleStory.Game.Movement
{
    public interface ILifeMovementFragment
    {
        void Serialize(OutPacket packet);
        Point Position { get; }
    }
}
