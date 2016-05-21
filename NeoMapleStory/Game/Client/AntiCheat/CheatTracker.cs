using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NeoMapleStory.Core;
using Quartz;

namespace NeoMapleStory.Game.Client.AntiCheat
{
    public class CheatTracker
    {
        private static WeakReference m_chr;
        private static TriggerKey m_invalidationTaskToken;
        private readonly string[] m_lastText = {"", "", ""};
        private long m_attackingSince;
        private int m_attacksWithoutHit;
        private int m_infiniteFjCount;
        private bool m_ispickupComplete = true;
        private long m_lastAttackTime;
        private long m_lastDamage;
        private long m_lastDamageTakenTime;
        private long m_lastFjTime;
        private Point m_lastMonsterMove;
        private int m_mobsOwned;
        private int m_monsterMoveCount;
        private int m_numHpRegens;
        private int m_numMpRegens;
        private int m_numSameDamage;
        private int m_numSequentialAttacks;
        private int m_numSequentialDamage;
        private int m_numSequentialSummonAttack;
        private long m_regenHpSince;
        private long m_regenMpSince;
        private long m_summonSummonTime;
        private long m_takingDamageSince;

        public CheatTracker(MapleCharacter chr)
        {
            m_chr = new WeakReference(chr);
            m_invalidationTaskToken = TimerManager.Instance.RepeatTask<InvalidationTask>(60*1000);
            m_takingDamageSince = m_attackingSince = m_regenMpSince = m_regenHpSince = DateTime.Now.GetTimeMilliseconds();
        }

        public static Dictionary<CheatingOffense, CheatingOffenseEntry> Offenses { get; } =
            new Dictionary<CheatingOffense, CheatingOffenseEntry>();


        public bool CheckAttack(int skillId)
        {
            m_numSequentialAttacks++;

            var oldLastAttackTime = m_lastAttackTime;
            m_lastAttackTime = DateTime.Now.GetTimeMilliseconds();
            var attackTime = m_lastAttackTime - m_attackingSince;
            if (m_numSequentialAttacks > 3)
            {
                int divisor;
                if (skillId == 3121004 || skillId == 5221004 || skillId == 13111002)
                {
                    // hurricane
                    divisor = 10;
                }
                else
                {
                    divisor = 300;
                }
                if (attackTime/divisor < m_numSequentialAttacks)
                {
                    RegisterOffense(CheatingOffense.Fastattack);
                    return false;
                }
            }
            if (m_lastAttackTime - oldLastAttackTime > 1500)
            {
                m_attackingSince = m_lastAttackTime;
                m_numSequentialAttacks = 0;
            }
            return true;
        }

        public void CheckTakeDamage()
        {
            m_numSequentialDamage++;
            var oldLastDamageTakenTime = m_lastDamageTakenTime;
            m_lastDamageTakenTime = DateTime.Now.GetTimeMilliseconds();

            var timeBetweenDamage = m_lastDamageTakenTime - m_takingDamageSince;

            if (timeBetweenDamage/500 < m_numSequentialDamage)
            {
                RegisterOffense(CheatingOffense.FastTakeDamage);
            }

            if (m_lastDamageTakenTime - oldLastDamageTakenTime > 4500)
            {
                m_takingDamageSince = m_lastDamageTakenTime;
                m_numSequentialDamage = 0;
            }
        }

        public int CheckDamage(long dmg)
        {
            if (dmg > 1 && m_lastDamage == dmg)
            {
                m_numSameDamage++;
            }
            else
            {
                m_lastDamage = dmg;
                m_numSameDamage = 0;
            }
            return m_numSameDamage;
        }

        public bool CheckHpLoss()
        {
            if (((MapleCharacter) m_chr.Target).GmLevel == 0)
            {
                if (m_mobsOwned >= 6)
                {
                    RegisterOffense(CheatingOffense.AttackWithoutGettingHit);
                    AutobanManager.Instance.Autoban(((MapleCharacter) m_chr.Target).Client, "无敌自动封号");
                }
            }
            if (((MapleCharacter) m_chr.Target).Hp >= ((MapleCharacter) m_chr.Target).MaxHp)
            {
                m_mobsOwned++;
            }
            else
            {
                m_mobsOwned = 0;
            }
            return false;
        }

        public void CheckMoveMonster(Point pos)
        {
            if (pos.Equals(m_lastMonsterMove))
            {
                m_monsterMoveCount++;
                if (m_monsterMoveCount > 9)
                {
                    //15
                    RegisterOffense(CheatingOffense.MoveMonsters);
                    AutobanManager.Instance.Autoban(((MapleCharacter) m_chr.Target).Client, "吸怪自动封号1");
                }
            }
            else
            {
                m_lastMonsterMove = pos;
                m_monsterMoveCount = 1;
            }
        }

        public void CheckFj()
        {
            var oldLastFjTime = m_lastFjTime;
            m_lastFjTime = DateTime.Now.GetTimeMilliseconds();
            if (m_lastFjTime - oldLastFjTime > 200)
            {
                m_infiniteFjCount = 0;
            }
            else
            {
                m_infiniteFjCount++;
            }
            if (m_infiniteFjCount > 10 && ((MapleCharacter) m_chr.Target).GmLevel == 0)
            {
                AutobanManager.Instance.Autoban(((MapleCharacter) m_chr.Target).Client, "异常移动");
            }
        }

        public bool TextSpam(string text)
        {
            if (((MapleCharacter) m_chr.Target).GmLevel == 0)
            {
                var lowerStr = text.ToLower();
                if (m_lastText[0].ToLower() == lowerStr && m_lastText[1].ToLower() == lowerStr &&
                    m_lastText[2].ToLower() == lowerStr)
                {
                    return true;
                }
                if (m_lastText[2].ToLower() != lowerStr && m_lastText[1].ToLower() == lowerStr &&
                    m_lastText[0].ToLower() == lowerStr)
                {
                    m_lastText[2] = text;
                }
                else if (m_lastText[1].ToLower() != lowerStr && m_lastText[0].ToLower() == lowerStr)
                {
                    m_lastText[1] = text;
                }
                else if (m_lastText[0].ToLower() != lowerStr)
                {
                    m_lastText[0] = text;
                }
            }
            return false;
        }

        public bool CheckHpRegen()
        {
            m_numHpRegens++;
            if ((DateTime.Now.GetTimeMilliseconds() - m_regenHpSince)/10000 < m_numHpRegens)
            {
                RegisterOffense(CheatingOffense.FastHpRegen);
                return false;
            }
            return true;
        }

        public void ResetHpRegen()
        {
            m_regenHpSince = DateTime.Now.GetTimeMilliseconds();
            m_numHpRegens = 0;
        }

        public bool CheckMpRegen()
        {
            m_numMpRegens++;
            var allowedRegens = (DateTime.Now.GetTimeMilliseconds() - m_regenMpSince)/10000;
            if (allowedRegens < m_numMpRegens)
            {
                RegisterOffense(CheatingOffense.FastMpRegen);
                return false;
            }
            return true;
        }

        public void ResetMpRegen()
        {
            m_regenMpSince = DateTime.Now.GetTimeMilliseconds();
            m_numMpRegens = 0;
        }

        public void ResetSummonAttack()
        {
            m_summonSummonTime = DateTime.Now.GetTimeMilliseconds();
            m_numSequentialSummonAttack = 0;
        }

        public bool CheckSummonAttack()
        {
            m_numSequentialSummonAttack++;
            //estimated
            var allowedAttacks = (DateTime.Now.GetTimeMilliseconds() - m_summonSummonTime)/2000 + 1;
            if (allowedAttacks < m_numSequentialAttacks)
            {
                RegisterOffense(CheatingOffense.FastSummonAttack);
                return false;
            }
            return true;
        }

        public void CheckPickupAgain()
        {
            if (m_ispickupComplete)
            {
                m_ispickupComplete = false;
            }
            else
            {
                RegisterOffense(CheatingOffense.Tubi);
            }
        }

        public void PickupComplete() => m_ispickupComplete = false;

        public int GetAttacksWithoutHit() => m_attacksWithoutHit;

        public void SetAttacksWithoutHit(int attacksWithoutHit) => m_attacksWithoutHit = attacksWithoutHit;

        public void RegisterOffense(CheatingOffense offense)
        {
            RegisterOffense(offense, null);
        }

        public void RegisterOffense(CheatingOffense offense, string param)
        {
            var chrhardref = (MapleCharacter) m_chr.Target;
            if (chrhardref == null || !offense.Enabled)
            {
                return;
            }

            CheatingOffenseEntry entry;
            if (Offenses.TryGetValue(offense, out entry) && entry.IsExpired())
            {
                ExpireEntry(entry);
                entry = null;
            }
            if (entry == null)
            {
                entry = new CheatingOffenseEntry(offense, chrhardref);
            }
            if (param != null)
            {
                entry.Param = param;
            }
            entry.IncrementCount();
            if (offense.ShouldAutoban(entry.Count))
            {
                AutobanManager.Instance.Autoban(chrhardref.Client, nameof(offense));
            }

            if (Offenses.ContainsKey(offense))
                Offenses[offense] = entry;
            else
                Offenses.Add(offense, entry);

            CheatingOffensePersister.Instance.PersistEntry(entry);
        }

        public static void ExpireEntry(CheatingOffenseEntry coe)
        {
            Offenses.Remove(coe.Offense);
        }

        public int GetPoints()
        {
            var ret = 0;
            var offensesCopy = new CheatingOffenseEntry[Offenses.Count];
            lock (Offenses)
            {
                Offenses.Values.CopyTo(offensesCopy, 0);
            }
            foreach (var entry in offensesCopy)
            {
                if (entry.IsExpired())
                {
                    ExpireEntry(entry);
                }
                else
                {
                    ret += entry.Points;
                }
            }
            return ret;
        }

        public string GetSummary()
        {
            var ret = new StringBuilder();
            var offenseList = new List<CheatingOffenseEntry>();
            lock (Offenses)
            {
                offenseList.AddRange(Offenses.Values.Where(entry => !entry.IsExpired()));
            }
            offenseList.Sort((o1, o2) =>
            {
                var thisVal = o1.Points;
                var anotherVal = o2.Points;
                return thisVal < anotherVal ? 1 : (thisVal == anotherVal ? 0 : -1);
            });

            var to = Math.Min(offenseList.Count, 4);
            for (var x = 0; x < to; x++)
            {
                var offense = offenseList[x].Offense;
                ret.Append(nameof(offense));
                ret.Append(": ");
                ret.Append(offenseList[x].Count);
                if (x != to - 1)
                {
                    ret.Append(" ");
                }
            }
            return ret.ToString();
        }

        private class InvalidationTask : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                var offensesCopy = new CheatingOffenseEntry[Offenses.Count];
                lock (Offenses)
                {
                    Offenses.Values.CopyTo(offensesCopy, 0);
                }
                foreach (var offense in offensesCopy.Where(offense => offense.IsExpired()))
                {
                    ExpireEntry(offense);
                }

                if (!m_chr.IsAlive)
                {
                    TimerManager.Instance.CancelTask(m_invalidationTaskToken);
                }
            }
        }
    }
}