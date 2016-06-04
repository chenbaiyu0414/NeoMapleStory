using System;
using System.Collections.Generic;
using System.Linq;
using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Quest;
using NeoMapleStory.Game.Script.Event;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Game.World;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Mob
{
    public class MapleMonster : AbstractLoadedMapleLife
    {
        private readonly List<MonsterStatusEffect> m_activeEffects = new List<MonsterStatusEffect>();

        private readonly List<IAttackerEntry> m_attackers = new List<IAttackerEntry>();

        private readonly WeakReference<MapleCharacter> m_controller = new WeakReference<MapleCharacter>(null);

        private bool m_controllerHasAggro;
        private bool m_controllerKnowsAboutAggro;
        private MapleCharacter m_highestDamageChar;
        private readonly Dictionary<Tuple<int, int>, int> m_skillsUsedTimes = new Dictionary<Tuple<int, int>, int>();

        private readonly Dictionary<MonsterStatus, MonsterStatusEffect> m_stati =
            new Dictionary<MonsterStatus, MonsterStatusEffect>();


        private readonly List<Tuple<int, int>> m_usedSkills = new List<Tuple<int, int>>();

        public List<MonsterKilled.MonsterKilledEvent> Listeners = new List<MonsterKilled.MonsterKilledEvent>();


        public MapleMonster(int id, MapleMonsterStats stats) : base(id)
        {
            InitWithStats(stats);
        }

        public MapleMonster(MapleMonster monster) : base(monster)
        {
            InitWithStats(monster.Stats);
        }

        public MapleMonsterStats Stats { get; set; }
        public MapleMonsterStats OverrideStats { get; set; }

        public MapleMap Map { get; set; }

        public bool IsBoss => Stats.IsBoss || IsHt || IsPb;

        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHp => OverrideStats?.Hp ?? Stats.Hp;
        public int MaxMp => OverrideStats?.Mp ?? Stats.Mp;

        private bool IsHt => Id == 8810018;

        private bool IsPb => Id >= 8820010 && Id <= 8820014;

        public bool IsAlive => Hp > 0;

        public bool IsFake { get; set; }

        public bool IsHpLock { get; set; } = false;

        public OutPacket BossHpBarPacket
            => PacketCreator.ShowBossHp(Id, Hp, MaxHp, Stats.TagColor, Stats.TagBgColor);

        public EventInstanceManager EventInstanceManager { get; set; }

        public bool HasBossHpBar => (IsBoss && Stats.TagColor > 0) || IsHt || IsPb;
        public List<MonsterStatus> MonsterBuffs { get; } = new List<MonsterStatus>();

        public bool IsMoveLock { get; set; } = false;

        public bool ControllerHasAggro
        {
            get { return !IsFake && m_controllerHasAggro; }
            set
            {
                if (!IsFake) m_controllerHasAggro = value;
            }
        }


        public bool ControllerKnowsAboutAggro
        {
            get { return !IsFake && m_controllerKnowsAboutAggro; }
            set
            {
                if (!IsFake) m_controllerKnowsAboutAggro = value;
            }
        }

        public int VenomMultiplier { get; set; }

        private void InitWithStats(MapleMonsterStats stats)
        {
            Stance = 5;
            Stats = stats;
            Hp = stats.Hp;
            Mp = stats.Mp;
        }

        public override void SendDestroyData(MapleClient client)
        {
            client.Send(PacketCreator.KillMonster(ObjectId, false));
        }

        public override void SendSpawnData(MapleClient client)
        {
            if (!IsAlive)
                return;

            client.Send(IsFake ? PacketCreator.SpawnFakeMonster(this, 0) : PacketCreator.SpawnMonster(this, false));

            if (m_stati.Any())
            {
                m_activeEffects.ForEach(
                    mse =>
                    {
                        client.Send(PacketCreator.ApplyMonsterStatus(ObjectId, mse.Stati, mse.GetSkill().SkillId, false,
                            0));
                    });
            }

            if (HasBossHpBar)
            {
                client.Send(BossHpBarPacket);
            }
        }

        public MapleCharacter GetController()
        {
            MapleCharacter target;
            m_controller.TryGetTarget(out target);
            return target;
        }

        public void SetController(MapleCharacter chr)
        {
            m_controller.SetTarget(chr);
        }

        public void SwitchController(MapleCharacter newController, bool immediateAggro)
        {
            var controllers = GetController();
            if (controllers == newController)
            {
                return;
            }
            if (controllers != null)
            {
                controllers.StopControllingMonster(this);
                controllers.Client.Send(PacketCreator.StopControllingMonster(ObjectId));
            }
            newController.ControlMonster(this, immediateAggro);
            SetController(newController);
            if (immediateAggro)
            {
                ControllerHasAggro = true;
            }
            ControllerKnowsAboutAggro = false;
        }

        public bool CanUseSkill(MobSkill toUse)
        {
            //if (toUse == null)
            //{
            //    return false;
            //}
            //if (!usedSkills.stream().noneMatch((skill)-> (skill.getLeft() == toUse.getSkillId() && skill.getRight() == toUse.getSkillLevel())))
            //{
            //    return false;
            //}
            //if (toUse.limit > 0)
            //{
            //    if (this.skillsUsed.containsKey(new Pair<>(toUse.getSkillId(), toUse.getSkillLevel())))
            //    {
            //        int times = this.skillsUsed.get(new Pair<>(toUse.getSkillId(), toUse.getSkillLevel()));
            //        if (times >= toUse.getLimit())
            //        {
            //            return false;
            //        }
            //    }
            //}
            //if (toUse.skillId == 200)
            //{
            //    List<IMapleMapObject> mmo = Map.Mapobjects;
            //    int i = 0;
            //    i = mmo.stream().filter((mo)-> (mo.getType() == MapleMapObjectType.MONSTER)).map((_item)-> 1).reduce(i, Integer::sum);
            //    if (i > 100)
            //    {
            //        return false;
            //    }
            //}
            //int percHpLeft = (Hp / maxhp) * 100;
            //return toUse.getHP() >= percHpLeft;
            return false;
        }

        public void UsedSkill(int skillId, int level, int cooltime)
        {
            var skillTuple = new Tuple<int, int>(skillId, level);

            m_usedSkills.Add(skillTuple);

            if (m_skillsUsedTimes.ContainsKey(skillTuple))
            {
                var times = m_skillsUsedTimes[skillTuple] + 1;
                m_skillsUsedTimes.Remove(skillTuple);

                if (m_skillsUsedTimes.ContainsKey(skillTuple))
                    m_skillsUsedTimes[skillTuple] = times;
                else
                    m_skillsUsedTimes.Add(skillTuple, times);
            }
            else
            {
                if (m_skillsUsedTimes.ContainsKey(skillTuple))
                    m_skillsUsedTimes[skillTuple] = 1;
                else
                    m_skillsUsedTimes.Add(skillTuple, 1);
            }

            TimerManager.Instance.RunOnceTask(() => { ClearSkill(skillId, level); }, cooltime);
        }

        public void ClearSkill(int skillId, int level)
        {
            var index = -1;
            foreach (var skill in m_usedSkills.Where(skill => skill.Item1 == skillId && skill.Item2 == level))
            {
                index = m_usedSkills.IndexOf(skill);
                break;
            }
            if (index != -1)
            {
                m_usedSkills.RemoveAt(index);
            }
        }

        public bool IsAttackedBy(MapleCharacter chr)
        {
            //return attackers.Any((aentry)=> (aentry.Contains(chr)));
            return true;
        }

        public ElementalEffectiveness GetEffectiveness(Element e)
        {
            if (m_activeEffects.Any() && m_stati.ContainsKey(MonsterStatus.Doom))
            {
                return ElementalEffectiveness.Normal; // like blue snails
            }
            return Stats.GetEffectiveness(e);
        }

        public void Damage(MapleCharacter from, int damage, bool updateAttackTime)
        {
            IAttackerEntry attacker;

            if (from.Party != null)
            {
                attacker = new PartyAttackerEntry(from.Party.PartyId, from.Client.ChannelServer, this);
            }
            else
            {
                attacker = new SingleAttackerEntry(from, from.Client.ChannelServer, this);
            }

            var replaced = false;
            foreach (var aentry in m_attackers)
            {
                if (aentry.Equals(attacker))
                {
                    attacker = aentry;
                    replaced = true;
                    break;
                }
            }
            if (!replaced)
            {
                m_attackers.Add(attacker);
            }

            var rDamage = Math.Max(0, Math.Min(damage, Hp));
            if (IsHpLock)
            {
                rDamage = 0;
            }
            attacker.AddDamage(from, rDamage, updateAttackTime);
            Hp -= rDamage;

            var remhppercentage = (byte)Math.Ceiling(Hp * 100.0 / MaxHp);
            if (remhppercentage < 1)
            {
                remhppercentage = 1;
            }
            var okTime = DateTime.Now.GetTimeMilliseconds() - 4000;

            if (HasBossHpBar)
            {
                //from.Map.BroadcastMessage(makeBossHPBarPacket(), Position);
            }
            else if (!IsBoss)
            {
                foreach (var mattacker in m_attackers)
                {
                    foreach (var cattacker in mattacker.GetAttackers())
                    {
                        // current attacker is on the map of the monster
                        if (cattacker.Attacker.Map.MapId == from.Map.MapId)
                        {
                            if (cattacker.LastAttackTime >= okTime)
                            {
                                cattacker.Attacker.Client.Send(PacketCreator.ShowMonsterHp(ObjectId, remhppercentage));
                            }
                        }
                    }
                }
            }
        }

        public int GetDrop(MapleCharacter killer)
        {
            var mi = MapleMonsterInformationProvider.Instance;
            var lastAssigned = -1;
            var minChance = 1;
            var dl = mi.RetrieveDropChances(Id);
            foreach (var d in dl)
            {
                if (d.Chance > minChance)
                {
                    minChance = d.Chance;
                }
            }
            foreach (var d in dl)
            {
                d.AssignedRangeStart = lastAssigned + 1;
                d.AssignedRangeLength = (int)Math.Ceiling(1.0 / d.Chance * minChance);
                lastAssigned += d.AssignedRangeLength;
            }
            var c = (int)(Randomizer.NextDouble() * minChance);
            foreach (var d in dl)
            {
                var itemid = d.ItemId;
                if ((c >= d.AssignedRangeStart) && (c < d.AssignedRangeStart + d.AssignedRangeLength))
                {
                    if (d.QuestId != 0)
                    {
                        if (killer.GetQuest(MapleQuest.GetInstance(d.QuestId)).Status == MapleQuestStatusType.Started)
                        {
                            return itemid;
                        }
                    }
                    else
                    {
                        return itemid;
                    }
                }
            }
            return -1;
        }

        public int GetMaxDrops(MapleCharacter chr)
        {
            var cserv = chr.Client.ChannelServer;
            int maxDrops;
            if (IsPqMonster())
            {
                maxDrops = 1;
                //PQ Monsters always drop a max of 1 item (pass) - I think? MonsterCarnival monsters don't count
            }
            else if (Stats.IsExplosive)
            {
                maxDrops = 10 * cserv.BossDropRate;
            }
            else if (IsBoss && !Stats.IsExplosive)
            {
                maxDrops = 7 * cserv.BossDropRate;
            }
            else
            {
                maxDrops = 4 * cserv.DropRate;
                if (m_stati.ContainsKey(MonsterStatus.Taunt))
                {
                    var alterDrops = m_stati[MonsterStatus.Taunt].Stati[MonsterStatus.Taunt];
                    maxDrops *= 1 + alterDrops / 100;
                }
            }
            return maxDrops;
        }

        public bool IsPqMonster()
        {
            return (Id >= 9300000 && Id <= 9300003) || (Id >= 9300005 && Id <= 9300010) ||
                   (Id >= 9300012 && Id <= 9300017) || (Id >= 9300169 && Id <= 9300171);
        }

        public void Heal(int hp, int mp)
        {
            var hp2Heal = Hp + hp;
            var mp2Heal = Mp + mp;

            if (hp2Heal >= MaxHp)
            {
                hp2Heal = MaxHp;
            }
            if (mp2Heal >= MaxMp)
            {
                mp2Heal = MaxMp;
            }

            if (!IsHpLock)
                Hp = hp2Heal;
            if (mp2Heal >= 0)
                Mp = mp2Heal;

            Map.BroadcastMessage(PacketCreator.HealMonster(ObjectId, hp));
        }

        public bool ApplyStatus(MapleCharacter from, MonsterStatusEffect status, bool poison, long duration)
        {
            return ApplyStatus(from, status, poison, duration, false);
        }

        public bool ApplyStatus(MapleCharacter from, MonsterStatusEffect status, bool poison, long duration, bool venom)
        {
            switch (Stats.GetEffectiveness(status.GetSkill().Element).Value)
            {
                case 1: //IMMUNE:
                case 2: //STRONG:        
                    return false;
                case 0: //NORMAL:
                case 3: //WEAK:
                    break;
                default:
                    throw new Exception(
                        $"Unknown elemental effectiveness:{Stats.GetEffectiveness(status.GetSkill().Element)}");
            }

            // compos don't have an elemental (they have 2 - so we have to hack here...)
            ElementalEffectiveness effectiveness = null;
            switch (status.GetSkill().SkillId)
            {
                case 2111006:
                    effectiveness = Stats.GetEffectiveness(Element.Poison);
                    if (effectiveness == ElementalEffectiveness.Immune || effectiveness == ElementalEffectiveness.Strong)
                    {
                        return false;
                    }
                    break;
                case 2211006:
                    effectiveness = Stats.GetEffectiveness(Element.Ice);
                    if (effectiveness == ElementalEffectiveness.Strong || effectiveness == ElementalEffectiveness.Strong)
                    {
                        return false;
                    }
                    break;
                case 4120005:
                case 4220005:
                    effectiveness = Stats.GetEffectiveness(Element.Poison);
                    if (effectiveness == ElementalEffectiveness.Weak)
                    {
                        return false;
                    }
                    break;
            }

            if (poison && Hp <= 1)
            {
                return false;
            }

            if (IsBoss && !status.Stati.ContainsKey(MonsterStatus.Speed))
            {
                return false;
            }

            status.Stati.Keys.ToList().ForEach(stat =>
            {
                MonsterStatusEffect oldEffect;
                if (m_stati.TryGetValue(stat, out oldEffect))
                {
                    oldEffect.RemoveActiveStatus(stat);
                    if (!oldEffect.Stati.Any())
                    {
                        //oldEffect.getCancelTask().cancel(false);
                        //oldEffect.cancelPoisonSchedule();
                        m_activeEffects.Remove(oldEffect);
                    }
                }
            });

            var timerManager = TimerManager.Instance;
            Action cancelTask = () =>
            {
                if (IsAlive)
                {
                    var packet = PacketCreator.CancelMonsterStatus(ObjectId, status.Stati);
                    Map.BroadcastMessage(packet, Position);
                    if (GetController() != null && !GetController().VisibleMapObjects.Contains(this))
                    {
                        GetController().Client.Send(packet);
                    }
                }
                try
                {
                    m_activeEffects.Remove(status);
                    status.Stati.Keys.ToList().ForEach(stat => { m_stati.Remove(stat); });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                VenomMultiplier = 0;
                //status.cancelPoisonSchedule();
            };
            if (!Map.HasEvent)
            {
                if (poison)
                {
                    var poisonLevel = from.GetSkillLevel(status.GetSkill());
                    var poisonDamage = Math.Min(short.MaxValue, (int)(MaxHp / (70.0 - poisonLevel) + 0.999));
                    status.SetValue(MonsterStatus.Poison, poisonDamage);
                    //status.setPoisonSchedule(timerManager.register(new PoisonTask(poisonDamage, from, status, cancelTask, false), 1000, 1000));
                }
                else if (venom)
                {
                    if (from.Job == MapleJob.Nightlord || from.Job == MapleJob.Shadower)
                    {
                        var poisonLevel = 0;
                        var matk = 0;
                        if (from.Job == MapleJob.Nightlord)
                        {
                            poisonLevel = from.GetSkillLevel(SkillFactory.GetSkill(4120005));
                            if (poisonLevel <= 0)
                            {
                                return false;
                            }
                            matk = SkillFactory.GetSkill(4120005).GetEffect(poisonLevel).Matk;
                        }
                        else if (from.Job == MapleJob.Shadower)
                        {
                            poisonLevel = from.GetSkillLevel(SkillFactory.GetSkill(4220005));
                            if (poisonLevel <= 0)
                            {
                                return false;
                            }
                            matk = SkillFactory.GetSkill(4220005).GetEffect(poisonLevel).Matk;
                        }
                        else
                        {
                            return false;
                        }

                        int luk = from.Luk;
                        var maxDmg = (int)Math.Ceiling(Math.Min(short.MaxValue, 0.2 * luk * matk));
                        var minDmg = (int)Math.Ceiling(Math.Min(short.MaxValue, 0.1 * luk * matk));
                        var gap = maxDmg - minDmg;
                        if (gap == 0)
                        {
                            gap = 1;
                        }
                        var poisonDamage = 0;
                        for (var i = 0; i < VenomMultiplier; i++)
                        {
                            poisonDamage = poisonDamage + Randomizer.Next(gap) + minDmg;
                        }
                        poisonDamage = Math.Min(short.MaxValue, poisonDamage);
                        status.SetValue(MonsterStatus.Poison, poisonDamage);
                        //status.setPoisonSchedule(timerManager.register(new PoisonTask(poisonDamage, from, status, cancelTask, false), 1000, 1000));
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (status.GetSkill().SkillId == 4111003)
                {
                    // shadow web
                    var webDamage = (int)(MaxHp / 50.0 + 0.999);
                    // actually shadow web works different but similar...
                    //status.setPoisonSchedule(timerManager.schedule(new PoisonTask(webDamage, from, status, cancelTask, true), 3500));
                }

                foreach (var stat in status.Stati.Keys)
                {
                    if (m_stati.ContainsKey(stat))
                        m_stati[stat] = status;
                    else
                        m_stati.Add(stat, status);
                }

                m_activeEffects.Add(status);

                var animationTime = status.GetSkill().AnimationTime;
                var packet = PacketCreator.ApplyMonsterStatus(ObjectId, status.Stati, status.GetSkill().SkillId, false,
                    0);
                Map.BroadcastMessage(packet, Position);
                if (GetController() != null && !GetController().VisibleMapObjects.Contains(this))
                {
                    GetController().Client.Send(packet);
                }
                //ScheduledFuture <?> schedule = timerManager.schedule(cancelTask, duration + animationTime);
                //status.setCancelTask(schedule);
            }
            return true;
        }

        public MapleCharacter KillBy(MapleCharacter killer)
        {
            long totalBaseExpL;
            // update exp
            //if (killer.inTutorialMap())
            //{
            //    totalBaseExpL = Stats.Exp*killer.Client.ChannelServer.ExpRate; /** killer.hasEXPCard();*/
            //}
            //else
            //{
            totalBaseExpL = Stats.Exp * killer.Client.ChannelServer.ExpRate;
            /** killer.getClient().getPlayer().hasEXPCard();*/
            //}

            var totalBaseExp = (int)Math.Min(int.MaxValue, totalBaseExpL);
            IAttackerEntry highest = null;
            var highdamage = 0;
            foreach (var attackEntry in m_attackers)
            {
                if (attackEntry.GetDamage() > highdamage)
                {
                    highest = attackEntry;
                    highdamage = attackEntry.GetDamage();
                }
            }

            foreach (var attackEntry in m_attackers)
            {
                var baseExp = (int)Math.Ceiling(totalBaseExp * ((double)attackEntry.GetDamage() / MaxHp));
                attackEntry.KilledMob(killer.Map, baseExp, attackEntry == highest);
            }

            if (GetController() != null)
            {
                // this can/should only happen when a hidden gm attacks the monster
                GetController().Client.Send(PacketCreator.StopControllingMonster(ObjectId));
                GetController().StopControllingMonster(this);
            }
            if (IsBoss)
            {
                //killer.finishAchievement(6);
            }

            var toSpawn = Stats.Revives;

            var canSpawn = true;

            if (EventInstanceManager != null)
            {
                if (EventInstanceManager.Name.IndexOf("BossQuest", 0, StringComparison.Ordinal) != -1)
                {
                    canSpawn = false;
                }
            }

            if (toSpawn != null && canSpawn)
            {
                var reviveMap = killer.Map;

                TimerManager.Instance.RunOnceTask(() =>
                {
                    foreach (var mid in toSpawn)
                    {
                        var mob = MapleLifeFactory.GetMonster(mid);
                        EventInstanceManager?.RegisterMonster(mob);
                        mob.Position = Position;
                        //if (dropdisabled)
                        //    mob.dropdisabled;
                        reviveMap.SpawnRevives(mob);
                    }
                }, Stats.GetAnimationTime("die1"));
            }
            if (EventInstanceManager != null)
            {
                //EventInstanceManager.unregisterMonster(this);
            }
            foreach (var listener in Listeners)
            {
                var arg = new MonsterKilledEventArgs { Monster = this, HighestDamageChar = m_highestDamageChar };
                listener(this, arg);
            }
            var ret = m_highestDamageChar;
            m_highestDamageChar = null; // may not keep hard references to chars outside of PlayerStorage or MapleMap
            return ret;
        }

        public void ApplyMonsterBuff(MonsterStatus status, int x, int skillId, int duration, MobSkill skill)
        {
            var timerManager = TimerManager.Instance;

            var applyPacket = PacketCreator.ApplyMonsterStatus(ObjectId,
                new Dictionary<MonsterStatus, int> { { status, x } }, skillId, true, 0, skill);

            Map.BroadcastMessage(applyPacket, Position);
            if (GetController() != null && !GetController().VisibleMapObjects.Contains(this))
            {
                GetController().Client.Send(applyPacket);
            }

            timerManager.RunOnceTask(() =>
            {
                if (IsAlive)
                {
                    var packet = PacketCreator.CancelMonsterStatus(ObjectId,
                        new Dictionary<MonsterStatus, int> { { status, x } });
                    Map.BroadcastMessage(packet, Position);
                    if (GetController() != null && !GetController().VisibleMapObjects.Contains(this))
                    {
                        GetController().Client.Send(packet);
                    }
                    MonsterBuffs.Remove(status);
                }
            }, duration);
            MonsterBuffs.Add(status);
        }

        public void GiveExpToCharacter(MapleCharacter attacker, int exp, bool highestDamage, int numExpSharers)
        {
            if (Id == 9300027)
            {
                exp = 1;
            }
            if (highestDamage)
            {
                //if (eventInstance != null)
                //{
                //    eventInstance.monsterKilled(attacker, this);
                //}
                m_highestDamageChar = attacker;
            }
            if (attacker.Hp > 0)
            {
                var personalExp = exp;
                if (exp > 0)
                {
                    if (m_stati.ContainsKey(MonsterStatus.Taunt))
                    {
                        var alterExp = m_stati[MonsterStatus.Taunt].Stati[MonsterStatus.Taunt];
                        personalExp *= (int)(1.0 + alterExp / 100.0);
                    }
                    var holySymbol = attacker.GetBuffedValue(MapleBuffStat.HolySymbol);
                    if (holySymbol != null)
                    {
                        if (numExpSharers == 1)
                        {
                            personalExp *= (int)(1.0 + holySymbol.Value / 500.0);
                        }
                        else
                        {
                            personalExp *= (int)(1.0 + holySymbol.Value / 100.0);
                        }
                    }
                }
                if (exp < 0)
                {
                    personalExp = int.MaxValue;
                }
                personalExp /= attacker.Diseases.Contains(MapleDisease.Curse) ? 2 : 1;
                attacker.GainExp(personalExp, true, false, highestDamage);

                try
                {
                    attacker.MobKilled(Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


        public override MapleMapObjectType GetType() => MapleMapObjectType.Monster;


        public class MonsterKilledEventArgs : EventArgs
        {
            public MapleMonster Monster { get; set; }
            public MapleCharacter HighestDamageChar { get; set; }
        }

        public class MonsterKilled
        {
            public delegate void MonsterKilledEvent(object sender, MonsterKilledEventArgs e);

            public event MonsterKilledEvent CustomerEvent;

            public void OnCustomerEvent(MonsterKilledEventArgs e)
            {
                CustomerEvent?.Invoke(this, e);
            }
        }


        private class AttackingMapleCharacter
        {
            public AttackingMapleCharacter(MapleCharacter attacker, long lastAttackTime)
            {
                Attacker = attacker;
                LastAttackTime = lastAttackTime;
            }

            public MapleCharacter Attacker { get; }
            public long LastAttackTime { get; }
        }

        private interface IAttackerEntry
        {
            List<AttackingMapleCharacter> GetAttackers();

            void AddDamage(MapleCharacter from, int damage, bool updateAttackTime);

            int GetDamage();

            bool Contains(MapleCharacter chr);

            void KilledMob(MapleMap map, int baseExp, bool mostDamage);
        }

        private class SingleAttackerEntry : IAttackerEntry
        {
            private readonly int m_chrid;
            private readonly ChannelServer m_cserv;

            private int m_damage;
            private long m_lastAttackTime;
            private readonly MapleMonster m_monster;

            public SingleAttackerEntry(MapleCharacter from, ChannelServer cserv, MapleMonster monster)
            {
                m_chrid = from.Id;
                m_cserv = cserv;
                m_monster = monster;
            }


            public void AddDamage(MapleCharacter from, int damage, bool updateAttackTime)
            {
                if (m_chrid == from.Id)
                {
                    m_damage += damage;
                }
                else
                {
                    throw new ArgumentException("Not the attacker of this entry");
                }
                if (updateAttackTime)
                {
                    m_lastAttackTime = DateTime.Now.GetTimeMilliseconds();
                }
            }


            public List<AttackingMapleCharacter> GetAttackers()
            {
                var chr = m_cserv.Characters.FirstOrDefault(x => x.Id == m_chrid);
                return chr != null
                    ? new List<AttackingMapleCharacter> { new AttackingMapleCharacter(chr, m_lastAttackTime) }
                    : new List<AttackingMapleCharacter>();
            }


            public bool Contains(MapleCharacter chr)
            {
                return m_chrid == chr.Id;
            }


            public int GetDamage()
            {
                return m_damage;
            }


            public void KilledMob(MapleMap map, int baseExp, bool mostDamage)
            {
                var chr = m_cserv.Characters.FirstOrDefault(x => x.Id == m_chrid);
                if (chr != null && chr.Map.MapId == map.MapId)
                {
                    m_monster.GiveExpToCharacter(chr, baseExp, mostDamage, 1);
                }
            }
        }

        private class OnePartyAttacker
        {

            public MapleParty LastKnownParty { get; set; }
            public int Damage { get; set; }
            public long LastAttackTime { get; set; }

            public OnePartyAttacker(MapleParty lastKnownParty, int damage)
            {
                LastKnownParty = lastKnownParty;
                Damage = damage;
                LastAttackTime = DateTime.Now.GetTimeMilliseconds();
            }
        }

        private class PartyAttackerEntry : IAttackerEntry
        {

            private int m_totDamage;
            private Dictionary<int, OnePartyAttacker> m_attackers;
            private int m_partyId;
            private ChannelServer m_cserv;
            private MapleMonster m_monster;

            public PartyAttackerEntry(int partyid, ChannelServer cserv, MapleMonster monster)
            {
                this.m_partyId = partyid;
                this.m_cserv = cserv;
                m_attackers = new Dictionary<int, OnePartyAttacker>(6);
                m_monster = monster;
            }


            private Dictionary<MapleCharacter, OnePartyAttacker> ResolveAttackers()
            {
                var ret = new Dictionary<MapleCharacter, OnePartyAttacker>(m_attackers.Count);
                m_attackers.ToList().ForEach((aentry) =>
                {
                    MapleCharacter chr = m_cserv.Characters.FirstOrDefault(x => x.Id == aentry.Key);
                    if (chr != null)
                    {
                        if (ret.ContainsKey(chr))
                            ret[chr] = aentry.Value;
                        else
                            ret.Add(chr, aentry.Value);
                    }
                });
                return ret;
            }

            public List<AttackingMapleCharacter> GetAttackers()
            {
                List<AttackingMapleCharacter> ret = new List<AttackingMapleCharacter>(m_attackers.Count);
                m_attackers.ToList().ForEach((entry) =>
                {
                    MapleCharacter chr = m_cserv.Characters.FirstOrDefault(x => x.Id == entry.Key);
                    if (chr != null)
                    {
                        ret.Add(new AttackingMapleCharacter(chr, entry.Value.LastAttackTime));
                    }
                });
                return ret;
            }

            public void AddDamage(MapleCharacter from, int damage, bool updateAttackTime)
            {
                OnePartyAttacker oldPartyAttacker;
                if (m_attackers.TryGetValue(from.Id, out oldPartyAttacker))
                {
                    oldPartyAttacker.Damage += damage;
                    oldPartyAttacker.LastKnownParty = from.Party;
                    if (updateAttackTime)
                    {
                        oldPartyAttacker.LastAttackTime = DateTime.Now.GetTimeMilliseconds();
                    }
                }
                else
                {
                    // TODO actually this causes wrong behaviour when the party changes between attacks
                    // only the last setup will get exp - but otherwise we'd have to store the full party
                    // constellation for every attack/everytime it changes, might be wanted/needed in the
                    // future but not now
                    OnePartyAttacker onePartyAttacker = new OnePartyAttacker(from.Party, damage);

                    if (m_attackers.ContainsKey(from.Id))
                        m_attackers[from.Id] = onePartyAttacker;
                    else
                        m_attackers.Add(from.Id, onePartyAttacker);

                    if (!updateAttackTime)
                    {
                        onePartyAttacker.LastAttackTime = 0;
                    }
                }
                m_totDamage += damage;
            }

            public int GetDamage() => m_totDamage;

            public bool Contains(MapleCharacter chr) => m_attackers.ContainsKey(chr.Id);

            public void KilledMob(MapleMap map, int baseExp, bool mostDamage)
            {
                Dictionary<MapleCharacter, OnePartyAttacker> attackers = ResolveAttackers();

                MapleCharacter highest = null;
                int highestDamage = 0;

                Dictionary<MapleCharacter, int> expMap = new Dictionary<MapleCharacter, int>(6);
                foreach (var attacker in attackers)
                {
                    MapleParty party = attacker.Value.LastKnownParty;
                    double averagePartyLevel = 0;

                    List<MapleCharacter> expApplicable = new List<MapleCharacter>();
                    foreach (var partychar in party.GetMembers())
                    {
                        if (attacker.Key.Level - partychar.Level <= 5 || m_monster.Stats.Level - partychar.Level <= 5)
                        {
                            MapleCharacter pchr = m_cserv.Characters.FirstOrDefault(x => x.Name == partychar.CharacterName);
                            if (pchr == null) continue;
                            if (!pchr.IsAlive || pchr.Map != map) continue;
                            expApplicable.Add(pchr);
                            averagePartyLevel += pchr.Level;
                        }
                    }
                    double expBonus = 1.0;
                    if (expApplicable.Count > 1)
                    {
                        expBonus = 1.10 + 0.05 * expApplicable.Count;
                        averagePartyLevel /= expApplicable.Count;
                    }

                    int iDamage = attacker.Value.Damage;
                    if (iDamage > highestDamage)
                    {
                        highest = attacker.Key;
                        highestDamage = iDamage;
                    }
                    double innerBaseExp = baseExp * ((double)iDamage / m_totDamage);
                    double expFraction = innerBaseExp * expBonus / (expApplicable.Count + 1);

                    foreach (var expReceiver in expApplicable)
                    {
                        int oexp;
                        int iexp = !expMap.TryGetValue(expReceiver, out oexp) ? 0 : oexp;
                        double expWeight = expReceiver == attacker.Key ? 2.0 : 1.0;
                        double levelMod = expReceiver.Level / averagePartyLevel;
                        if (levelMod > 1.0 || m_attackers.ContainsKey(expReceiver.Id))
                        {
                            levelMod = 1.0;
                        }
                        iexp += (int)Math.Round(expFraction * expWeight * levelMod);
                        if (expMap.ContainsKey(expReceiver))
                            expMap[expReceiver] = iexp;
                        else
                            expMap.Add(expReceiver, iexp);
                    }
                }
                // FUCK we are done -.-
                foreach (var expReceiver in expMap)
                {
                    bool white = mostDamage && expReceiver.Key == highest;
                    m_monster.GiveExpToCharacter(expReceiver.Key, expReceiver.Value, white, expMap.Count);
                }
            }
        }
    }
}