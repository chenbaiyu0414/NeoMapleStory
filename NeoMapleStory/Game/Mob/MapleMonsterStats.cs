using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Life;

namespace NeoMapleStory.Game.Mob
{
    public class MapleMonsterStats
    {
        private readonly Dictionary<string, int> m_mAnimationTimes = new Dictionary<string, int>();

        private readonly Dictionary<Element, ElementalEffectiveness> m_mResistance =
            new Dictionary<Element, ElementalEffectiveness>();

        private readonly List<Tuple<byte, byte>> m_mSkills = new List<Tuple<byte, byte>>();
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
        public bool IsMobile => m_mAnimationTimes.ContainsKey("move") || m_mAnimationTimes.ContainsKey("fly");

        public void SetAnimationTime(string name, int delay)
        {
            m_mAnimationTimes.Add(name, delay);
        }

        public int GetAnimationTime(string name)
        {
            int ret;
            if (!m_mAnimationTimes.TryGetValue(name, out ret))
            {
                return 500;
            }
            return ret;
        }

        public void SetEffectiveness(Element e, ElementalEffectiveness ee)
        {
            m_mResistance.Add(e, ee);
        }

        public void RemoveEffectiveness(Element e)
        {
            m_mResistance.Remove(e);
        }

        public ElementalEffectiveness GetEffectiveness(Element e)
        {
            ElementalEffectiveness elementalEffectiveness;

            if (m_mResistance.TryGetValue(e, out elementalEffectiveness))
                return ElementalEffectiveness.Normal;
            return elementalEffectiveness;
        }

        public void SetSkills(List<Tuple<byte, byte>> skills) => m_mSkills.AddRange(skills);

        public List<Tuple<byte, byte>> GetSkills() => m_mSkills;

        public int GetSkillsCount() => m_mSkills.Count;

        public bool HasSkill(int skillId, int level)
        {
            foreach (var skill in m_mSkills)
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