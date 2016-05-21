namespace NeoMapleStory.Game.Skill
{
    public class SkillEntry
    {
        public SkillEntry(int skillevel, int masterlevel)
        {
            SkilLevel = skillevel;
            MasterLevel = masterlevel;
        }

        public int SkilLevel { get; }
        public int MasterLevel { get; }

        public override string ToString() => $"{SkilLevel}:{MasterLevel}";
    }
}