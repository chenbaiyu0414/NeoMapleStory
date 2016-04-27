using NeoMapleStory.Game.Life;
using System;
using System.Collections.Generic;

namespace NeoMapleStory.Game.Mob
{
     public class MapleMonsterStats
    {
        public int Exp { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int Level { get; set; }
        public int RemoveAfter { get; set; }
        public int DropPeriod { get; set; }
        public bool IsBoss { get; set; }
        public bool IsUndead { get; set; }
        public bool IsFfaLoot { get; set; }
        public string Name { get; set; }
        public bool IsFirstAttack { get; set; }
        public int BuffToGive { get; set; }
        public bool IsExplosive { get; set; }
        public BanishInfo Banish { get; set; }
        public List<int> Revives { get; set; } = new List<int>();
        public byte TagColor { get; set; }
        public byte TagBgColor { get; set; }
        public bool IsMobile => _mAnimationTimes.ContainsKey("move") || _mAnimationTimes.ContainsKey("fly");

        private readonly List<Tuple<byte, byte>> _mSkills = new List<Tuple<byte, byte>>();
        private readonly Dictionary<string, int> _mAnimationTimes = new Dictionary<string, int>();
        private readonly Dictionary<Element, ElementalEffectiveness> _mResistance = new Dictionary<Element, ElementalEffectiveness>();

        public void SetAnimationTime(string name, int delay)
        {
            _mAnimationTimes.Add(name, delay);
        }

        public int GetAnimationTime(string name)
        {
            int ret;
            if (_mAnimationTimes.TryGetValue(name, out ret))
            {
                return 500;
            }
            return ret;
        }

        public void SetEffectiveness(Element e, ElementalEffectiveness ee)
        {
            _mResistance.Add(e, ee);
        }

        public void RemoveEffectiveness(Element e)
        {
            _mResistance.Remove(e);
        }

        public ElementalEffectiveness GetEffectiveness(Element e)
        {
            ElementalEffectiveness elementalEffectiveness;

            if (_mResistance.TryGetValue(e, out elementalEffectiveness))
                return ElementalEffectiveness.Normal;
            return elementalEffectiveness;
        }

        public void SetSkills(List<Tuple<byte, byte>> skills) => _mSkills.AddRange(skills);

        public List<Tuple<byte, byte>> GetSkills()=> _mSkills;

        public int GetSkillsCount()=> _mSkills.Count;

        public bool HasSkill(int skillId, int level)
        {
            foreach (var skill in _mSkills)
            {
                if (skill.Item1 == skillId && skill.Item2 == level)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
