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
using NeoMapleStory.Core.TimeManager;

namespace NeoMapleStory.Game.Mob
{
    public class MapleMonster : AbstractLoadedMapleLife
    {
        public MapleMonsterStats Stats { get; set; }
        public MapleMonsterStats OverrideStats { get; set; }
        public MapleMap Map { get; set; }

        public bool IsBoss => Stats.IsBoss || IsHt || IsPb;

        public int MaxHp => OverrideStats?.Hp ?? Stats.Hp;
        public int MaxMp => OverrideStats?.Mp ?? Stats.Mp;

        private bool IsHt => Id == 8810018;

        private bool IsPb => Id >= 8820010 && Id <= 8820014;

        public bool IsAlive => Stats.Hp > 0;

        public bool IsFake { get; set; }

        public bool IsHpLock { get; set; } = false;

        public OutPacket BossHPBarPacket
            => PacketCreator.showBossHP(Id, Stats.Hp, MaxHp, Stats.TagColor, Stats.TagBgColor);

        public EventInstanceManager EventInstanceManager { get; set; }

        public bool HasBossHPBar => (IsBoss && Stats.TagColor > 0) || IsHt || IsPb;

        public List<MonsterKilled.MonsterKilledEvent> listeners = new List<MonsterKilled.MonsterKilledEvent>();

        private WeakReference<MapleCharacter> controller = new WeakReference<MapleCharacter>(null);

        private Dictionary<MonsterStatus, MonsterStatusEffect> stati = new Dictionary<MonsterStatus, MonsterStatusEffect>();
        private List<MonsterStatusEffect> activeEffects = new List<MonsterStatusEffect>();
        public List<MonsterStatus> MonsterBuffs { get; } = new List<MonsterStatus>();


        private List<Tuple<int, int>> usedSkills = new List<Tuple<int, int>>();
        private Dictionary<Tuple<int, int>, int> skillsUsedTimes = new Dictionary<Tuple<int, int>, int>();

        //private List<AttackerEntry> attackers = new List<AttackerEntry>();

        public bool IsMoveLock { get; set; } = false;

        private bool _controllerHasAggro;
        private bool _controllerKnowsAboutAggro;
        public bool ControllerHasAggro
        {
            get { return !IsFake && _controllerHasAggro; }
            set { if (!IsFake) _controllerHasAggro = value; }
        }


        public bool ControllerKnowsAboutAggro
        {
            get { return !IsFake && _controllerKnowsAboutAggro; }
            set { if (!IsFake) _controllerKnowsAboutAggro = value; }
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
            //int percHpLeft = (Stats.Hp / maxhp) * 100;
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

        public void heal(int hp, int mp)
        {
            var hp2Heal = Stats.Hp + hp;
            var mp2Heal = Stats.Mp + mp;

            if (hp2Heal >= MaxHp)
            {
                hp2Heal = MaxHp;
            }
            if (mp2Heal >= MaxMp)
            {
                mp2Heal = MaxMp;
            }

            if (!IsHpLock)
                Stats.Hp = hp2Heal;
            if (mp2Heal >= 0)
                Stats.Mp = mp2Heal;

            Map.BroadcastMessage(PacketCreator.healMonster(ObjectId, hp));
        }

        public void applyMonsterBuff(MonsterStatus status, int x, int skillId, int duration, MobSkill skill)
        {
            TimerManager timerManager = TimerManager.Instance;

            var applyPacket = PacketCreator.applyMonsterStatus(ObjectId,
                new Dictionary<MonsterStatus, int> {{status, x}}, skillId, true, 0, skill);

            Map.BroadcastMessage(applyPacket, Position);
            if (GetController() != null && !GetController().VisibleMapObjects.Contains(this))
            {
                GetController().Client.Send(applyPacket);
            }


            timerManager.ScheduleJob(() =>
            {
                if (IsAlive)
                {
                    OutPacket packet = PacketCreator.cancelMonsterStatus(ObjectId,new Dictionary<MonsterStatus, int> { { status, x } });
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
    }
}
