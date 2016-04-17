namespace NeoMapleStory.Game.Skill
{
    public class SkillEntry
    {
        public int SkilLevel { get; private set; }
        public int MasterLevel { get; private set; }

        public SkillEntry(int skillevel, int masterlevel)
        {
            SkilLevel = skillevel;
            MasterLevel = masterlevel;
        }

        public override string ToString() => $"{SkilLevel}:{ MasterLevel}";
    }
}
