using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoMapleStory.Game.Life
{
    public class MobAttackInfo
    {
        public int MobId { get; private set; }
        public int AttackId { get; private set; }
        public bool IsDeadlyAttack { get; set; }
        public int MpBurn { get; set; }
        public byte DiseaseSkill { get; set; }
        public byte DiseaseLevel { get; set; }
        public int MpCon { get; set; }

        public MobAttackInfo(int mobId, int attackId)
        {
            MobId = mobId;
            AttackId = attackId;
        }
    }
}
