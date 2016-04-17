using NeoMapleStory.Core;
using NeoMapleStory.Core.TimeManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using FluentScheduler;

namespace NeoMapleStory.Game.Client.AntiCheat
{
     public class CheatTracker
    {
        public static Dictionary<CheatingOffense, CheatingOffenseEntry> Offenses { get; } =
            new Dictionary<CheatingOffense, CheatingOffenseEntry>();

        private static WeakReference _chr;
        private long _regenHpSince;
        private long _regenMpSince;
        private int _numHpRegens;
        private int _numMpRegens;
        private int _numSequentialAttacks;
        private long _lastAttackTime;
        private long _lastDamage;
        private long _takingDamageSince;
        private int _numSequentialDamage;
        private long _lastDamageTakenTime;
        private int _numSequentialSummonAttack;
        private long _summonSummonTime;
        private int _numSameDamage;
        private long _attackingSince;
        private long _lastFjTime;
        private int _infiniteFjCount;
        private Point _lastMonsterMove;
        private int _monsterMoveCount;
        private int _attacksWithoutHit;
        private readonly string[] _lastText = {"", "", ""};
        private int _mobsOwned;
        private bool _ispickupComplete = true;
        private static string _invalidationTaskToken;

        public CheatTracker(MapleCharacter chr)
        {
            _chr = new WeakReference(chr);
            _invalidationTaskToken = TimerManager.Instance.RegisterJob<InvalidationTask>(60);
            _takingDamageSince = _attackingSince = _regenMpSince = _regenHpSince = DateTime.Now.GetTimeMilliseconds();
        }



        public bool CheckAttack(int skillId)
        {
            _numSequentialAttacks++;

            long oldLastAttackTime = _lastAttackTime;
            _lastAttackTime = DateTime.Now.GetTimeMilliseconds();
            long attackTime = _lastAttackTime - _attackingSince;
            if (_numSequentialAttacks > 3)
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
                if (attackTime/divisor < _numSequentialAttacks)
                {
                    registerOffense(CheatingOffense.Fastattack);
                    return false;
                }
            }
            if (_lastAttackTime - oldLastAttackTime > 1500)
            {
                _attackingSince = _lastAttackTime;
                _numSequentialAttacks = 0;
            }
            return true;
        }

        public void CheckTakeDamage()
        {
            _numSequentialDamage++;
            long oldLastDamageTakenTime = _lastDamageTakenTime;
            _lastDamageTakenTime = DateTime.Now.GetTimeMilliseconds();

            long timeBetweenDamage = _lastDamageTakenTime - _takingDamageSince;

            if (timeBetweenDamage/500 < _numSequentialDamage)
            {
                registerOffense(CheatingOffense.FastTakeDamage);
            }

            if (_lastDamageTakenTime - oldLastDamageTakenTime > 4500)
            {
                _takingDamageSince = _lastDamageTakenTime;
                _numSequentialDamage = 0;
            }
        }

        public int CheckDamage(long dmg)
        {
            if (dmg > 1 && _lastDamage == dmg)
            {
                _numSameDamage++;
            }
            else
            {
                _lastDamage = dmg;
                _numSameDamage = 0;
            }
            return _numSameDamage;
        }

        public bool CheckHpLoss()
        {
            if (((MapleCharacter) _chr.Target).GmLevel == 0)
            {
                if (_mobsOwned >= 6)
                {
                    registerOffense(CheatingOffense.AttackWithoutGettingHit);
                    AutobanManager.Instance.Autoban(((MapleCharacter) _chr.Target).Client, "无敌自动封号");
                }
            }
            if (((MapleCharacter) _chr.Target).Hp >= ((MapleCharacter) _chr.Target).MaxHp)
            {
                _mobsOwned++;
            }
            else
            {
                _mobsOwned = 0;
            }
            return false;
        }

        public void CheckMoveMonster(Point pos)
        {
            if (pos.Equals(_lastMonsterMove))
            {
                _monsterMoveCount++;
                if (_monsterMoveCount > 9)
                {
                    //15
                    registerOffense(CheatingOffense.MoveMonsters);
                    AutobanManager.Instance.Autoban(((MapleCharacter) _chr.Target).Client, "吸怪自动封号1");
                }
            }
            else
            {
                _lastMonsterMove = pos;
                _monsterMoveCount = 1;
            }
        }

        public void CheckFj()
        {
            long oldLastFjTime = _lastFjTime;
            _lastFjTime = DateTime.Now.GetTimeMilliseconds();
            if (_lastFjTime - oldLastFjTime > 200)
            {
                _infiniteFjCount = 0;
            }
            else
            {
                _infiniteFjCount++;
            }
            if (_infiniteFjCount > 10 && ((MapleCharacter) _chr.Target).GmLevel == 0)
            {
                AutobanManager.Instance.Autoban(((MapleCharacter) _chr.Target).Client, "异常移动");
            }
        }

        public bool TextSpam(string text)
        {
            if (((MapleCharacter) _chr.Target).GmLevel == 0)
            {

                string lowerStr = text.ToLower();
                if (_lastText[0].ToLower() == lowerStr && _lastText[1].ToLower() == lowerStr &&
                    _lastText[2].ToLower() == lowerStr)
                {
                    return true;
                }
                else if (_lastText[2].ToLower() != lowerStr && _lastText[1].ToLower() == lowerStr &&
                         _lastText[0].ToLower() == lowerStr)
                {
                    _lastText[2] = text;
                }
                else if (_lastText[1].ToLower() != lowerStr && _lastText[0].ToLower() == lowerStr)
                {
                    _lastText[1] = text;
                }
                else if (_lastText[0].ToLower() != lowerStr)
                {
                    _lastText[0] = text;
                }
            }
            return false;
        }

        public bool CheckHpRegen()
        {
            _numHpRegens++;
            if ((DateTime.Now.GetTimeMilliseconds() - _regenHpSince)/10000 < _numHpRegens)
            {
                registerOffense(CheatingOffense.FastHpRegen);
                return false;
            }
            return true;
        }

        public void ResetHpRegen()
        {
            _regenHpSince = DateTime.Now.GetTimeMilliseconds();
            _numHpRegens = 0;
        }

        public bool CheckMpRegen()
        {
            _numMpRegens++;
            long allowedRegens = (DateTime.Now.GetTimeMilliseconds() - _regenMpSince)/10000;
            if (allowedRegens < _numMpRegens)
            {
                registerOffense(CheatingOffense.FastMpRegen);
                return false;
            }
            return true;
        }

        public void ResetMpRegen()
        {
            _regenMpSince = DateTime.Now.GetTimeMilliseconds();
            _numMpRegens = 0;
        }

        public void ResetSummonAttack()
        {
            _summonSummonTime = DateTime.Now.GetTimeMilliseconds();
            _numSequentialSummonAttack = 0;
        }

        public bool CheckSummonAttack()
        {
            _numSequentialSummonAttack++;
            //estimated
            long allowedAttacks = (DateTime.Now.GetTimeMilliseconds() - _summonSummonTime)/2000 + 1;
            if (allowedAttacks < _numSequentialAttacks)
            {
                registerOffense(CheatingOffense.FastSummonAttack);
                return false;
            }
            return true;
        }

        public void CheckPickupAgain()
        {
            if (_ispickupComplete)
            {
                _ispickupComplete = false;
            }
            else
            {
                registerOffense(CheatingOffense.Tubi);
            }
        }

        public void PickupComplete() => _ispickupComplete = false;

        public int GetAttacksWithoutHit() => _attacksWithoutHit;

        public void SetAttacksWithoutHit(int attacksWithoutHit) => this._attacksWithoutHit = attacksWithoutHit;

        public void registerOffense(CheatingOffense offense)
        {
            registerOffense(offense, null);
        }

        public void registerOffense(CheatingOffense offense, string param)
        {
            MapleCharacter chrhardref = (MapleCharacter) _chr.Target;
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
            int ret = 0;
            CheatingOffenseEntry[] offensesCopy = new CheatingOffenseEntry[Offenses.Count];
            lock (Offenses)
            {
                Offenses.Values.CopyTo(offensesCopy, 0);
            }
            foreach (CheatingOffenseEntry entry in offensesCopy)
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
            StringBuilder ret = new StringBuilder();
            List<CheatingOffenseEntry> offenseList = new List<CheatingOffenseEntry>();
            lock (Offenses)
            {
                offenseList.AddRange(Offenses.Values.Where(entry => !entry.IsExpired()));
            }
            offenseList.Sort((o1, o2) =>
            {
                int thisVal = o1.Points;
                int anotherVal = o2.Points;
                return thisVal < anotherVal ? 1 : (thisVal == anotherVal ? 0 : -1);
            });

            int to = Math.Min(offenseList.Count, 4);
            for (int x = 0; x < to; x++)
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

        public static void Dispose()
        {
            TimerManager.Instance.CancelJob(_invalidationTaskToken);

        }

        private class InvalidationTask :IJob
        {
            public void Execute()
            {
                 CheatingOffenseEntry[] offensesCopy = new CheatingOffenseEntry[Offenses.Count];
                lock (Offenses)
                {
                    Offenses.Values.CopyTo(offensesCopy, 0);

                }
                foreach (var offense in offensesCopy.Where(offense => offense.IsExpired()))
                {
                    ExpireEntry(offense);
                }

                if (!_chr.IsAlive)
                {
                    Dispose();
                }
            }    
        }
    }
}

