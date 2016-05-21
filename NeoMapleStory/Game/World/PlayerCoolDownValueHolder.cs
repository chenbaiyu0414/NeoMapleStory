using NeoMapleStory.Core;

namespace NeoMapleStory.Game.World
{
    public class PlayerCoolDownValueHolder
    {
        private readonly int m_id;

        public PlayerCoolDownValueHolder(int skillId, long startTime, long duration)
        {
            SkillId = skillId;
            StartTime = startTime;
            Duration = duration;
            m_id = (int) (Randomizer.NextDouble()*100);
        }

        public int SkillId { get; private set; }
        public long StartTime { get; private set; }
        public long Duration { get; private set; }

        public override int GetHashCode() => 1*31 + m_id;

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
            var other = (PlayerCoolDownValueHolder) obj;
            if (m_id != other.m_id)
            {
                return false;
            }
            return true;
        }
    }
}