using System.Collections.Generic;
using NeoMapleStory.Game.Skill;

namespace NeoMapleStory.Game.Mob
{
    public class MonsterStatusEffect
    {
        private readonly bool m_monsterSkill;
        private readonly ISkill m_skill;
        //private ScheduledFuture<?> cancelTask;
        //private ScheduledFuture<?> poisonSchedule;

        public MonsterStatusEffect(Dictionary<MonsterStatus, int> stati, ISkill skillId, bool monsterSkill)
        {
            this.Stati = new Dictionary<MonsterStatus, int>(stati);
            m_skill = skillId;
            this.m_monsterSkill = monsterSkill;
        }

        public Dictionary<MonsterStatus, int> Stati { get; }

        public void SetValue(MonsterStatus status, int newVal)
        {
            if (Stati.ContainsKey(status))
                Stati[status] = newVal;
            else
                Stati.Add(status, newVal);
        }

        public ISkill GetSkill()
        {
            return m_skill;
        }

        public bool IsMonsterSkill()
        {
            return m_monsterSkill;
        }

        //public ScheduledFuture<?> getCancelTask()
        //{
        //    return cancelTask;
        //}

        //public void setCancelTask(ScheduledFuture<?> cancelTask)
        //{
        //    this.cancelTask = cancelTask;
        //}

        public void RemoveActiveStatus(MonsterStatus stat)
        {
            Stati.Remove(stat);
        }

        //    {
        //    if (poisonSchedule != null)
        //{

        //public void cancelPoisonSchedule()
        //}
        //    this.poisonSchedule = poisonSchedule;
        //{

        //public void setPoisonSchedule(ScheduledFuture<?> poisonSchedule)
        //        poisonSchedule.cancel(false);
        //    }
        //}
    }
}