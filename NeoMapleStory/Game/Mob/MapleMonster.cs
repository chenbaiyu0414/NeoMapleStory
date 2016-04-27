using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Script.Event;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using System.Linq;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Skill;

namespace NeoMapleStory.Game.Mob
{
    public class MapleMonster : AbstractLoadedMapleLife
    {
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

        public OutPacket BossHPBarPacket
            => PacketCreator.showBossHP(Id, Hp, MaxHp, Stats.TagColor, Stats.TagBgColor);

        public EventInstanceManager EventInstanceManager { get; set; }

        public bool HasBossHPBar => (IsBoss && Stats.TagColor > 0) || IsHt || IsPb;

        public List<MonsterKilled.MonsterKilledEvent> listeners = new List<MonsterKilled.MonsterKilledEvent>();

        private WeakReference<MapleCharacter> controller = new WeakReference<MapleCharacter>(null);

        private Dictionary<MonsterStatus, MonsterStatusEffect> stati =
            new Dictionary<MonsterStatus, MonsterStatusEffect>();

        private List<MonsterStatusEffect> activeEffects = new List<MonsterStatusEffect>();
        public List<MonsterStatus> MonsterBuffs { get; } = new List<MonsterStatus>();


        private List<Tuple<int, int>> usedSkills = new List<Tuple<int, int>>();
        private Dictionary<Tuple<int, int>, int> skillsUsedTimes = new Dictionary<Tuple<int, int>, int>();

        private List<AttackerEntry> attackers = new List<AttackerEntry>();

        public bool IsMoveLock { get; set; } = false;

        private bool _controllerHasAggro;
        private bool _controllerKnowsAboutAggro;
        private MapleCharacter highestDamageChar;

        public bool ControllerHasAggro
        {
            get { return !IsFake && _controllerHasAggro; }
            set
            {
                if (!IsFake) _controllerHasAggro = value;
            }
        }


        public bool ControllerKnowsAboutAggro
        {
            get { return !IsFake && _controllerKnowsAboutAggro; }
            set
            {
                if (!IsFake) _controllerKnowsAboutAggro = value;
            }
        }


        public MapleMonster(int id, MapleMonsterStats stats) : base(id)
        {
            InitWithStats(stats);
        }

        public MapleMonster(MapleMonster monster) : base(monster)
        {
            InitWithStats(monster.Stats);
        }

        private void InitWithStats(MapleMonsterStats stats)
        {
            Stance = 5;
            Stats = stats;
            Hp = stats.Hp;
            Mp = stats.Mp;
        }

        public override void SendDestroyData(MapleClient client)
        {
            client.Send(PacketCreator.killMonster(ObjectId, false));
        }

        public override void SendSpawnData(MapleClient client)
        {
            if (!IsAlive)
                return;

            client.Send(IsFake ? PacketCreator.spawnFakeMonster(this, 0) : PacketCreator.spawnMonster(this, false));

            if (stati.Any())
            {
                activeEffects.ForEach((mse) =>
                {
                    client.Send(PacketCreator.applyMonsterStatus(ObjectId, mse.stati, mse.getSkill().SkillId, false, 0));
                });
            }

            if (HasBossHPBar)
            {
                client.Send(BossHPBarPacket);
            }
        }

        public MapleCharacter GetController()
        {
            MapleCharacter target = null;
            controller.TryGetTarget(out target);
            return target;
        }

        public void SetController(MapleCharacter chr)
        {
            controller.SetTarget(chr);
        }

        public void switchController(MapleCharacter newController, bool immediateAggro)
        {
            MapleCharacter controllers = GetController();
            if (controllers == newController)
            {
                return;
            }
            if (controllers != null)
            {
                controllers.stopControllingMonster(this);
                controllers.Client.Send(PacketCreator.stopControllingMonster(ObjectId));
            }
            newController.controlMonster(this, immediateAggro);
            SetController(newController);
            if (immediateAggro)
            {
                ControllerHasAggro = true;
            }
            ControllerKnowsAboutAggro = false;
        }

        public bool canUseSkill(MobSkill toUse)
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

        public void usedSkill(int skillId, int level, int cooltime)
        {
            var skillTuple = new Tuple<int, int>(skillId, level);

            usedSkills.Add(skillTuple);

            if (skillsUsedTimes.ContainsKey(skillTuple))
            {
                int times = skillsUsedTimes[skillTuple] + 1;
                skillsUsedTimes.Remove(skillTuple);

                if (skillsUsedTimes.ContainsKey(skillTuple))
                    skillsUsedTimes[skillTuple] = times;
                else
                    skillsUsedTimes.Add(skillTuple, times);
            }
            else
            {
                if (skillsUsedTimes.ContainsKey(skillTuple))
                    skillsUsedTimes[skillTuple] = 1;
                else
                    skillsUsedTimes.Add(skillTuple, 1);
            }

            TimerManager.Instance.ScheduleJob(() =>
            {
                clearSkill(skillId, level);
            }, cooltime);
        }

        public void clearSkill(int skillId, int level)
        {
            int index = -1;
            foreach (var skill in usedSkills.Where(skill => skill.Item1 == skillId && skill.Item2 == level))
            {
                index = usedSkills.IndexOf(skill);
                break;
            }
            if (index != -1)
            {
                usedSkills.RemoveAt(index);
            }
        }

        public bool isAttackedBy(MapleCharacter chr)
        {
            //return attackers.Any((aentry)=> (aentry.Contains(chr)));
            return true;
        }

        public int VenomMultiplier { get; set; } = 0;

        public ElementalEffectiveness getEffectiveness(Element e)
        {
            if (activeEffects.Any() && stati.ContainsKey(MonsterStatus.Doom))
            {
                return ElementalEffectiveness.Normal; // like blue snails
            }
            return Stats.GetEffectiveness(e);
        }

        public void damage(MapleCharacter from, int damage, bool updateAttackTime)
        {
            AttackerEntry attacker = null;

            //if (from.Party != null)
            //{
            //    attacker = new PartyAttackerEntry(from.getParty().getId(), from.getClient().getChannelServer());
            //}
            //else
            //{
            attacker = new SingleAttackerEntry(from, from.Client.ChannelServer, this);
            //}

            bool replaced = false;
            foreach (AttackerEntry aentry in attackers)
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
                attackers.Add(attacker);
            }

            int rDamage = Math.Max(0, Math.Min(damage, Hp));
            if (IsHpLock)
            {
                rDamage = 0;
            }
            attacker.addDamage(from, rDamage, updateAttackTime);
            Hp -= rDamage;

            byte remhppercentage = (byte)Math.Ceiling(Hp * 100.0 / MaxHp);
            if (remhppercentage < 1)
            {
                remhppercentage = 1;
            }
            long okTime = DateTime.Now.GetTimeMilliseconds() - 4000;

            if (HasBossHPBar)
            {
                //from.Map.BroadcastMessage(makeBossHPBarPacket(), Position);
            }
            else if (!IsBoss)
            {
                foreach (AttackerEntry mattacker in attackers)
                {
                    foreach (AttackingMapleCharacter cattacker in mattacker.getAttackers())
                    {
                        // current attacker is on the map of the monster
                        if (cattacker.Attacker.Map.MapId == from.Map.MapId)
                        {
                            if (cattacker.LastAttackTime >= okTime)
                            {
                                cattacker.Attacker.Client.Send(PacketCreator.showMonsterHP(ObjectId, remhppercentage));
                            }
                        }
                    }
                }
            }
        }

        public void heal(int hp, int mp)
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

            Map.BroadcastMessage(PacketCreator.healMonster(ObjectId, hp));
        }

        public bool applyStatus(MapleCharacter from, MonsterStatusEffect status, bool poison, long duration)
        {
            return applyStatus(from, status, poison, duration, false);
        }

        public bool applyStatus(MapleCharacter from, MonsterStatusEffect status, bool poison, long duration, bool venom)
        {
            switch (Stats.GetEffectiveness(status.getSkill().Element).Value)
            {
                case 1://IMMUNE:
                case 2://STRONG:        
                    return false;
                case 0://NORMAL:
                case 3://WEAK:
                    break;
                default:
                    throw new Exception(
                        $"Unknown elemental effectiveness:{Stats.GetEffectiveness(status.getSkill().Element)}");
            }

            // compos don't have an elemental (they have 2 - so we have to hack here...)
            ElementalEffectiveness effectiveness = null;
            switch (status.getSkill().SkillId)
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

            if (IsBoss && !(status.stati.ContainsKey(MonsterStatus.Speed)))
            {
                return false;
            }

            status.stati.Keys.ToList().ForEach(stat =>
           {
               MonsterStatusEffect oldEffect;
               if (stati.TryGetValue(stat, out oldEffect))
               {
                   oldEffect.removeActiveStatus(stat);
                   if (!oldEffect.stati.Any())
                   {
                        //oldEffect.getCancelTask().cancel(false);
                        //oldEffect.cancelPoisonSchedule();
                        activeEffects.Remove(oldEffect);
                   }
               }
           });

            TimerManager timerManager = TimerManager.Instance;
            Action cancelTask = () =>
            {
                if (IsAlive)
                {
                    OutPacket packet = PacketCreator.cancelMonsterStatus(ObjectId, status.stati);
                    Map.BroadcastMessage(packet, Position);
                    if (GetController() != null && !GetController().VisibleMapObjects.Contains(this))
                    {
                        GetController().Client.Send(packet);
                    }
                }
                try
                {
                    activeEffects.Remove(status);
                    status.stati.Keys.ToList().ForEach(stat =>
                    {
                        stati.Remove(stat);
                    });
                }
                catch (Exception e)
                {
                    throw e;
                }
                VenomMultiplier = 0;
                //status.cancelPoisonSchedule();
            };
            if (!Map.HasEvent)
            {
                if (poison)
                {
                    int poisonLevel = from.getSkillLevel(status.getSkill());
                    int poisonDamage = Math.Min(short.MaxValue, (int)(MaxHp / (70.0 - poisonLevel) + 0.999));
                    status.setValue(MonsterStatus.Poison, poisonDamage);
                    //status.setPoisonSchedule(timerManager.register(new PoisonTask(poisonDamage, from, status, cancelTask, false), 1000, 1000));
                }
                else if (venom)
                {
                    if (from.Job == MapleJob.Nightlord || from.Job == MapleJob.Shadower)
                    {
                        int poisonLevel = 0;
                        int matk = 0;
                        if (from.Job == MapleJob.Nightlord)
                        {
                            poisonLevel = from.getSkillLevel(SkillFactory.GetSkill(4120005));
                            if (poisonLevel <= 0)
                            {
                                return false;
                            }
                            matk = SkillFactory.GetSkill(4120005).GetEffect(poisonLevel)._matk;
                        }
                        else if (from.Job == MapleJob.Shadower)
                        {
                            poisonLevel = from.getSkillLevel(SkillFactory.GetSkill(4220005));
                            if (poisonLevel <= 0)
                            {
                                return false;
                            }
                            matk = SkillFactory.GetSkill(4220005).GetEffect(poisonLevel)._matk;
                        }
                        else
                        {
                            return false;
                        }

                        int luk = from.Luk;
                        int maxDmg = (int)Math.Ceiling(Math.Min(short.MaxValue, 0.2 * luk * matk));
                        int minDmg = (int)Math.Ceiling(Math.Min(short.MaxValue, 0.1 * luk * matk));
                        int gap = maxDmg - minDmg;
                        if (gap == 0)
                        {
                            gap = 1;
                        }
                        int poisonDamage = 0;
                        for (int i = 0; i < VenomMultiplier; i++)
                        {
                            poisonDamage = poisonDamage + Randomizer.Next(gap) + minDmg;
                        }
                        poisonDamage = Math.Min(short.MaxValue, poisonDamage);
                        status.setValue(MonsterStatus.Poison, poisonDamage);
                        //status.setPoisonSchedule(timerManager.register(new PoisonTask(poisonDamage, from, status, cancelTask, false), 1000, 1000));
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (status.getSkill().SkillId == 4111003)
                {
                    // shadow web
                    int webDamage = (int)(MaxHp / 50.0 + 0.999);
                    // actually shadow web works different but similar...
                    //status.setPoisonSchedule(timerManager.schedule(new PoisonTask(webDamage, from, status, cancelTask, true), 3500));
                }

                foreach (var stat in status.stati.Keys)
                {
                    if (stati.ContainsKey(stat))
                        stati[stat] = status;
                    else
                        stati.Add(stat, status);
                }

                activeEffects.Add(status);

                int animationTime = status.getSkill().AnimationTime;
                OutPacket packet = PacketCreator.applyMonsterStatus(ObjectId, status.stati, status.getSkill().SkillId, false, 0);
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

        public MapleCharacter killBy(MapleCharacter killer)
        {
            long totalBaseExpL;
            // update exp
            //if (killer.inTutorialMap())
            //{
            //    totalBaseExpL = Stats.Exp*killer.Client.ChannelServer.ExpRate; /** killer.hasEXPCard();*/
            //}
            //else
            //{
                totalBaseExpL = Stats.Exp*killer.Client.ChannelServer.ExpRate;
                    /** killer.getClient().getPlayer().hasEXPCard();*/
            //}

            int totalBaseExp = (int)Math.Min(int.MaxValue, totalBaseExpL);
            AttackerEntry highest = null;
            int highdamage = 0;
            foreach (var attackEntry in attackers)
            {
                if (attackEntry.getDamage() > highdamage)
                {
                    highest = attackEntry;
                    highdamage = attackEntry.getDamage();
                }
            }

            foreach (AttackerEntry attackEntry in attackers)
            {
                int baseExp = (int)Math.Ceiling(totalBaseExp * ((double)attackEntry.getDamage() / MaxHp));
                attackEntry.killedMob(killer.Map, baseExp, attackEntry == highest);
            }

            if (this.GetController() != null)
            { 
                // this can/should only happen when a hidden gm attacks the monster
                GetController().Client.Send(PacketCreator.stopControllingMonster(this.ObjectId));
                GetController().stopControllingMonster(this);
            }
            if (this.IsBoss)
            {
                //killer.finishAchievement(6);
            }

           //List<int > toSpawn = this.getRevives();

           // bool canSpawn = true;
        
            //if (EventInstanceManager != null)
            //{
            //    if (eventInstance.getName().indexOf("BossQuest", 0) != -1)
            //    {
            //        canSpawn = false;
            //    }
            //}

            //if (toSpawn != null && canSpawn)
            //{
            //    final MapleMap reviveMap = killer.getMap();

            //    TimerManager.getInstance().schedule(()-> {
            //        toSpawn.stream().map((mid)->MapleLifeFactory.getMonster(mid)).map((mob)-> {
            //            if (eventInstance != null)
            //            {
            //                eventInstance.registerMonster(mob);
            //            }
            //            return mob;
            //        }).map((mob)-> {
            //            mob.setPosition(getPosition());
            //            return mob;
            //        }).map((mob)-> {
            //            if (dropsDisabled())
            //            {
            //                mob.disableDrops();
            //            }
            //            return mob;
            //        }).forEach((mob)-> {
            //            reviveMap.spawnRevives(mob);
            //        });
            //    }, this.getAnimationTime("die1"));
            //}
            //if (eventInstance != null)
            //{
            //    eventInstance.unregisterMonster(this);
            //}
            foreach (var listener in listeners)
            {
                var arg = new MonsterKilledEventArgs {monster = this, highestDamageChar = highestDamageChar};
                listener(this, arg);
            }
            MapleCharacter ret = highestDamageChar;
            highestDamageChar = null; // may not keep hard references to chars outside of PlayerStorage or MapleMap
            return ret;
        }

        public void applyMonsterBuff(MonsterStatus status, int x, int skillId, int duration, MobSkill skill)
        {
            TimerManager timerManager = TimerManager.Instance;

            var applyPacket = PacketCreator.applyMonsterStatus(ObjectId,
                new Dictionary<MonsterStatus, int> { { status, x } }, skillId, true, 0, skill);

            Map.BroadcastMessage(applyPacket, Position);
            if (GetController() != null && !GetController().VisibleMapObjects.Contains(this))
            {
                GetController().Client.Send(applyPacket);
            }


            timerManager.ScheduleJob(() =>
            {
                if (IsAlive)
                {
                    OutPacket packet = PacketCreator.cancelMonsterStatus(ObjectId,
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

        public void giveExpToCharacter(MapleCharacter attacker, int exp, bool highestDamage, int numExpSharers)
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
                highestDamageChar = attacker;
            }
            if (attacker.Hp > 0)
            {
                int personalExp = exp;
                if (exp > 0)
                {
                    if (stati.ContainsKey(MonsterStatus.Taunt))
                    {
                        int alterExp = stati[MonsterStatus.Taunt].stati[MonsterStatus.Taunt];
                        personalExp *= (int)(1.0 + alterExp / 100.0);
                    }
                    int? holySymbol = attacker.GetBuffedValue(MapleBuffStat.HolySymbol);
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
                personalExp /= attacker._diseases.Contains(MapleDisease.Curse) ? 2 : 1;
                attacker.gainExp(personalExp, true, false, highestDamage);

                try
                {
                    attacker.mobKilled(Id);
                }
                catch (Exception e)
                {
                    //log.info("Quest Bug", npe);
                }
            }
        }


        public override MapleMapObjectType GetType() => MapleMapObjectType.Monster;


        public class MonsterKilledEventArgs : EventArgs
        {
            public MapleMonster monster { get; set; }
            public MapleCharacter highestDamageChar { get; set; }
        }

        public class MonsterKilled
        {
            public delegate void MonsterKilledEvent(object sender, MonsterKilledEventArgs e);

            public event MonsterKilledEvent CustomerEvent;

            public void OnCustomerEvent(MonsterKilledEventArgs e)
            {
                if (CustomerEvent != null)
                    CustomerEvent(this, e);
            }
        }


        private class AttackingMapleCharacter
        {

            public MapleCharacter Attacker { get; private set; }
            public long LastAttackTime { get; set; }

            public AttackingMapleCharacter(MapleCharacter attacker, long lastAttackTime)
            {
                Attacker = attacker;
                LastAttackTime = lastAttackTime;
            }
        }

        private interface AttackerEntry
        {

            List<AttackingMapleCharacter> getAttackers();

            void addDamage(MapleCharacter from, int damage, bool updateAttackTime);

            int getDamage();

            bool contains(MapleCharacter chr);

            void killedMob(MapleMap map, int baseExp, bool mostDamage);
        }

        private class SingleAttackerEntry : AttackerEntry
        {

            private int damage;
            private int chrid;
            private long lastAttackTime;
            private ChannelServer cserv;
            private MapleMonster monster;
            public SingleAttackerEntry(MapleCharacter from, ChannelServer cserv, MapleMonster monster)
            {
                this.chrid = from.Id;
                this.cserv = cserv;
                this.monster = monster;
            }


            public void addDamage(MapleCharacter from, int damage, bool updateAttackTime)
            {
                if (chrid == from.Id)
                {
                    this.damage += damage;
                }
                else
                {
                    throw new ArgumentException("Not the attacker of this entry");
                }
                if (updateAttackTime)
                {
                    lastAttackTime = DateTime.Now.GetTimeMilliseconds();
                }
            }


            public List<AttackingMapleCharacter> getAttackers()
            {
                MapleCharacter chr = cserv.Characters.FirstOrDefault(x => x.Id == chrid);
                return chr != null
                    ? new List<AttackingMapleCharacter> { new AttackingMapleCharacter(chr, lastAttackTime) }
                    : new List<AttackingMapleCharacter>();
            }


            public bool contains(MapleCharacter chr)
            {
                return chrid == chr.Id;
            }


            public int getDamage()
            {
                return damage;
            }


            public void killedMob(MapleMap map, int baseExp, bool mostDamage)
            {
                MapleCharacter chr = cserv.Characters.FirstOrDefault(x => x.Id == chrid);
                if (chr != null && chr.Map.MapId == map.MapId)
                {
                    monster.giveExpToCharacter(chr, baseExp, mostDamage, 1);
                }
            }

            public override int GetHashCode()
            {
                return chrid;
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }

                SingleAttackerEntry other = (SingleAttackerEntry) obj;
                return chrid == other?.chrid;
            }
        }

    }
}
