namespace NeoMapleStory.Game.Skill
{
    public class SkillEntry
    {
        public SkillEntry(byte skillevel, byte masterlevel)
        {
            SkilLevel = skillevel;
            MasterLevel = masterlevel;
        }

        public byte SkilLevel { get; }
        public byte MasterLevel { get; }

        public override string ToString() => $"{SkilLevel}:{MasterLevel}";
    }
}