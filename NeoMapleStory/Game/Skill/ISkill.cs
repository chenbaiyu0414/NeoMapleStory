using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Life;

namespace NeoMapleStory.Game.Skill
{
    public interface ISkill
    {
        int SkillId { get; }

        byte MaxLevel { get; }

        int AnimationTime { get; }

        bool IsFourthJob { get; }

        Element Element { get; }

        bool IsBeginnerSkill { get; }

        bool HasCharge { get; }

        MapleStatEffect GetEffect(int level);

        bool CanBeLearned(MapleJob job);
    }
}