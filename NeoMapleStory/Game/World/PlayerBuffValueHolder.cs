using NeoMapleStory.Core;
using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.World
{
    public class PlayerBuffValueHolder
    {
        private readonly int m_mId;

        public PlayerBuffValueHolder(long startTime, MapleStatEffect effect)
        {
            StartTime = startTime;
            Effect = effect;
            m_mId = (int) (Randomizer.NextDouble()*100);
        }

        public long StartTime { get; private set; }
        public MapleStatEffect Effect { get; private set; }

        public override int GetHashCode() => 1*31 + m_mId;

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this != obj)
            {
                return false;
            }
            var other = (PlayerBuffValueHolder) obj;
            if (m_mId != other.m_mId)
            {
                return false;
            }
            return true;
        }
    }
}