using System.Drawing;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Game.Movement
{
    public class ChangeEquipSpecialAwesome : ILifeMovementFragment
    {
        private readonly int _mWui;

        public ChangeEquipSpecialAwesome(int wui)
        {
            _mWui = wui;
        }

        public Point Position { get; } = Point.Empty;
       

        public void Serialize(OutPacket packet)
        {
            packet.WriteByte(10);
            packet.WriteByte((byte)_mWui);
        }
    }
}
