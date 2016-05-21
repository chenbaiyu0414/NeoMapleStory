using System.Drawing;
using NeoMapleStory.Core.IO;

namespace NeoMapleStory.Game.Movement
{
    public class ChangeEquipSpecialAwesome : ILifeMovementFragment
    {
        private readonly int m_mWui;

        public ChangeEquipSpecialAwesome(int wui)
        {
            m_mWui = wui;
        }

        public Point Position { get; } = Point.Empty;


        public void Serialize(OutPacket packet)
        {
            packet.WriteByte(10);
            packet.WriteByte((byte) m_mWui);
        }
    }
}