using NeoMapleStory.Core;

namespace NeoMapleStory.Game.World
{
    public class PlayerCoolDownValueHolder
    {
        public int SkillId { get; private set; }
        public long StartTime { get; private set; }
        public long Duration { get; private set; }
        private readonly int _id;

        public PlayerCoolDownValueHolder(int skillId, long startTime, long duration)
        {
            SkillId = skillId;
            StartTime = startTime;
            Duration = duration;
            _id = (int)(Randomizer.NextDouble() * 100);
        }

        public override int GetHashCode() => 1 * 31 + _id;

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
            PlayerCoolDownValueHolder other = (PlayerCoolDownValueHolder)obj;
            if (_id != other._id)
            {
                return false;
            }
            return true;
        }
    }
}
