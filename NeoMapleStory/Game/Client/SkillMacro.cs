namespace NeoMapleStory.Game.Client
{
    public class SkillMacro
    {
        public int MacroId { get; set; }
        public int SkillId1 { get; set; }
        public int SkillId2 { get; set; }
        public int SkillId3 { get; set; }
        public string MacroName { get; set; }
        public int Shout { get; set; }
        public int Position { get; set; }

        public SkillMacro(int skill1, int skill2, int skill3, string name, int shout, int position)
        {
            SkillId1 = skill1;
            SkillId2 = skill2;
            SkillId3 = skill3;
            MacroName = name;
            Shout = shout;
            Position = position;
        }
    }
}
