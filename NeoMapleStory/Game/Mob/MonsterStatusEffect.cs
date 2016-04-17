using System.Collections.Generic;
using NeoMapleStory.Game.Skill;

namespace NeoMapleStory.Game.Mob
{
    public class MonsterStatusEffect
    {
        public Dictionary<MonsterStatus, int> stati { get; private set; }
        private ISkill skill;
        private bool monsterSkill;
        //private ScheduledFuture<?> cancelTask;
        //private ScheduledFuture<?> poisonSchedule;

        public MonsterStatusEffect(Dictionary<MonsterStatus, int> stati, ISkill skillId, bool monsterSkill)
        {
            this.stati = new Dictionary<MonsterStatus, int>(stati);
            this.skill = skillId;
            this.monsterSkill = monsterSkill;
        }

        public void setValue(MonsterStatus status, int newVal)
        {
            if (stati.ContainsKey(status))
                stati[status] = newVal;
            else
                stati.Add(status, newVal);
        }

        public ISkill getSkill()
        {
            return skill;
        }

        public bool isMonsterSkill()
        {
            return monsterSkill;
        }

        //public ScheduledFuture<?> getCancelTask()
        //{
        //    return cancelTask;
        //}

        //public void setCancelTask(ScheduledFuture<?> cancelTask)
        //{
        //    this.cancelTask = cancelTask;
        //}

        public void removeActiveStatus(MonsterStatus stat)
        {
            stati.Remove(stat);
        }

        //public void setPoisonSchedule(ScheduledFuture<?> poisonSchedule)
        //{
        //    this.poisonSchedule = poisonSchedule;
        //}

        //public void cancelPoisonSchedule()
        //{
        //    if (poisonSchedule != null)
        //    {
        //        poisonSchedule.cancel(false);
        //    }
        //}
    }
}
