using Quartz;

namespace NeoMapleStory.Game.World
{
    public class MapleCoolDownValueHolder
    {
        public int SkillId { get; private set; }
        public long StartTime { get; private set; }
        public long Duration { get; private set; }
        public TriggerKey TriggerKey { get; private set; }

        public MapleCoolDownValueHolder(int skillId, long startTime, long length, TriggerKey triggerKey)
        {
            SkillId = skillId;
            StartTime = startTime;
            Duration = length;
            TriggerKey = triggerKey;
        }
    }
}
