namespace NeoMapleStory.Game.World
{
    public class MapleCoolDownValueHolder
    {
        public int SkillId { get; private set; }
        public long StartTime { get; private set; }
        public long Duration { get; private set; }
        public string JobName { get; private set; }

        public MapleCoolDownValueHolder(int skillId, long startTime, long length, string jobname)
        {
            SkillId = skillId;
            StartTime = startTime;
            Duration = length;
            JobName = jobname;
        }
    }
}
