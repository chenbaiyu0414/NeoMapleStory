using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Life;

namespace NeoMapleStory.Game.Skill
{
    public interface ISkill
    {
        int SkillId { get; }

        MapleStatEffect GetEffect(int level);

        int MaxLevel { get; }

        int AnimationTime { get; }

        bool IsFourthJob { get; }

        bool CanBeLearned(MapleJob job);

        Element Element { get; }

        bool IsBeginnerSkill { get; }

        bool HasCharge { get; }
    }
}
