using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Game.World;
using NeoMapleStory.Packet;

namespace NeoMapleStory.Game.Client
{
    public class MapleStatEffect
    {
        public int AttackCount;
        private int m_bulletCount, m_bulletConsume;
        public int Cooldown;
        private List<MapleDisease> m_cureDebuffs;
        public int Duration;
        private int m_fixDamage;


        private short m_hp, m_mp;
        private double m_hpR, m_mpR;
        private bool m_isMorph;
        private int m_itemCon, m_itemConNo;
        private Point m_lt, m_rb;
        private int m_mastery, m_range;
        private int m_mobCount;
        private int m_moneyCon;
        public Dictionary<MonsterStatus, int> MonsterStatus;
        private int m_morphId;
        private int m_moveTo;
        public short MpCon, HpCon;
        private bool m_overTime;
        private double m_prop;
        private string m_remark;
        private object m_ret;
        private bool m_skill;
        private int m_sourceid;
        private List<Tuple<MapleBuffStat, int>> m_statups;
        public short Watk, Matk, Wdef, Mdef, Acc, Avoid, Hands, Speed, Jump;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        public int Damage { get; set; }

        public string GetRemark()
        {
            return m_remark;
        }

        public static MapleStatEffect LoadSkillEffectFromData(IMapleData source, int skillid, bool overtime, string lvl)
            => LoadFromData(source, skillid, true, overtime, "Level " + lvl);


        public static MapleStatEffect LoadItemEffectFromData(IMapleData source, int itemid)
            => LoadFromData(source, itemid, false, false, "");


        private static void AddBuffStatTupleToListIfNotZero(List<Tuple<MapleBuffStat, int>> list, MapleBuffStat buffstat,
            int val)
        {
            if (val != 0)
            {
                list.Add(Tuple.Create(buffstat, val));
            }
        }

        private static MapleStatEffect LoadFromData(IMapleData source, int sourceid, bool skill, bool overTime,
            string remarrk)
        {
            var ret = new MapleStatEffect();
            ret.Duration = MapleDataTool.ConvertToInt("time", source, -1);
            ret.m_hp = (short) MapleDataTool.GetInt("hp", source, 0);
            ret.m_hpR = MapleDataTool.GetInt("hpR", source, 0)/100.0;
            ret.m_mp = (short) MapleDataTool.GetInt("mp", source, 0);
            ret.m_mpR = MapleDataTool.GetInt("mpR", source, 0)/100.0;
            ret.MpCon = (short) MapleDataTool.GetInt("mpCon", source, 0);
            ret.HpCon = (short) MapleDataTool.GetInt("hpCon", source, 0);
            var iprop = MapleDataTool.GetInt("prop", source, 100);
            ret.m_prop = iprop/100.0;
            ret.AttackCount = MapleDataTool.GetInt("attackCount", source, 1);
            ret.m_mobCount = MapleDataTool.GetInt("mobCount", source, 1);
            ret.Cooldown = MapleDataTool.GetInt("cooltime", source, 0);
            ret.m_morphId = MapleDataTool.GetInt("morph", source, 0);
            ret.m_isMorph = ret.m_morphId > 0 ? true : false;
            ret.m_remark = remarrk;
            ret.m_sourceid = sourceid;
            ret.m_skill = skill;

            if (!ret.m_skill && ret.Duration > -1)
            {
                ret.m_overTime = true;
            }
            else
            {
                ret.Duration *= 1000; // items have their times stored in ms, of course
                ret.m_overTime = overTime;
            }

            var statups = new List<Tuple<MapleBuffStat, int>>();

            ret.Watk = (short) MapleDataTool.GetInt("pad", source, 0);
            ret.Wdef = (short) MapleDataTool.GetInt("pdd", source, 0);
            ret.Matk = (short) MapleDataTool.GetInt("mad", source, 0);
            ret.Mdef = (short) MapleDataTool.GetInt("mdd", source, 0);
            ret.Acc = (short) MapleDataTool.ConvertToInt("acc", source, 0);
            ret.Avoid = (short) MapleDataTool.GetInt("eva", source, 0);
            ret.Speed = (short) MapleDataTool.GetInt("speed", source, 0);
            ret.Jump = (short) MapleDataTool.GetInt("jump", source, 0);
            if (ret.m_overTime && ret.GetSummonMovementType() == null)
            {
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Watk, ret.Watk);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Wdef, ret.Wdef);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Matk, ret.Matk);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Mdef, ret.Mdef);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Acc, ret.Acc);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Avoid, ret.Avoid);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Speed, ret.Speed);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Jump, ret.Jump);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Morph, ret.m_morphId);
            }

            var ltd = source.GetChildByPath("lt");
            if (ltd != null)
            {
                ret.m_lt = (Point) ltd.Data;
                ret.m_rb = (Point) source.GetChildByPath("rb").Data;
            }

            var x = MapleDataTool.GetInt("x", source, 0);
            ret.X = x;
            ret.Y = MapleDataTool.GetInt("y", source, 0);
            ret.Z = MapleDataTool.GetInt("z", source, 0);
            ret.Damage = MapleDataTool.ConvertToInt("damage", source, 100);
            ret.m_bulletCount = MapleDataTool.ConvertToInt("bulletCount", source, 1);
            ret.m_bulletConsume = MapleDataTool.ConvertToInt("bulletConsume", source, 0);
            ret.m_moneyCon = MapleDataTool.ConvertToInt("moneyCon", source, 0);

            ret.m_itemCon = MapleDataTool.GetInt("itemCon", source, 0);
            ret.m_itemConNo = MapleDataTool.GetInt("itemConNo", source, 0);
            ret.m_fixDamage = MapleDataTool.GetInt("fixdamage", source, 0);

            ret.m_moveTo = MapleDataTool.GetInt("moveTo", source, -1);

            ret.m_mastery = MapleDataTool.ConvertToInt("mastery", source, 0);
            ret.m_range = MapleDataTool.ConvertToInt("range", source, 0);

            var localCureDebuffs = new List<MapleDisease>();

            if (MapleDataTool.GetInt("poison", source, 0) > 0)
            {
                localCureDebuffs.Add(MapleDisease.Poison);
            }
            if (MapleDataTool.GetInt("seal", source, 0) > 0)
            {
                localCureDebuffs.Add(MapleDisease.Seal);
            }
            if (MapleDataTool.GetInt("darkness", source, 0) > 0)
            {
                localCureDebuffs.Add(MapleDisease.Darkness);
            }
            if (MapleDataTool.GetInt("weakness", source, 0) > 0)
            {
                localCureDebuffs.Add(MapleDisease.Weaken);
            }
            if (MapleDataTool.GetInt("curse", source, 0) > 0)
            {
                localCureDebuffs.Add(MapleDisease.Curse);
            }
            ret.m_cureDebuffs = localCureDebuffs;

            var monsterStatus = new Dictionary<MonsterStatus, int>();

            if (skill)
            {
                // 出租的,因为我们不能从datafile……
                switch (sourceid)
                {
                    case 2001002: // 魔法守卫
                    case 12001001:
                        statups.Add(Tuple.Create(MapleBuffStat.MagicGuard, x));
                        break;
                    case 2301003: // 无敌
                        statups.Add(Tuple.Create(MapleBuffStat.Invincible, x));
                        break;
                    case 9001004: // 隐藏
                        ret.Duration = 60*120*1000;
                        ret.m_overTime = true;
                        goto case 14001003;
                        //夜行者 NIGHT_KNIGHT
                    case 14001003: // 隐身
                        statups.Add(Tuple.Create(MapleBuffStat.Darksight, x));
                        break;
                    case 4001003: // darksight
                        statups.Add(Tuple.Create(MapleBuffStat.Darksight, x));
                        break;
                    case 4211003: // pickpocket
                        statups.Add(Tuple.Create(MapleBuffStat.Pickpocket, x));
                        break;
                    case 4211005: // mesoguard
                        statups.Add(Tuple.Create(MapleBuffStat.Mesoguard, x));
                        break;
                    case 4111001: // mesoup
                        statups.Add(Tuple.Create(MapleBuffStat.Mesoup, x));
                        break;
                    case 4111002: // 影分身
                    case 14111000:
                        statups.Add(Tuple.Create(MapleBuffStat.Shadowpartner, x));
                        break;
                    case 3101004: // 灵魂的箭头
                    case 3201004:
                    case 21120002: //战神之舞
                    case 13101003: //精灵使者-无形箭
                        statups.Add(Tuple.Create(MapleBuffStat.Soularrow, x));
                        break;
                    case 2311002: //时空门
                        statups.Add(new Tuple<MapleBuffStat, int>(MapleBuffStat.Soularrow, x));
                        break;
                    case 1211003:
                    case 1211004:
                    case 1211005:
                    case 1221003: //圣灵之剑
                    case 11101002: //终极剑
                    case 15111006: //闪光击
                    case 1221004: //圣灵之锤
                    case 1211006: // 寒冰钝器
                    case 21111005: //冰雪矛
                    case 1211007:

                    case 1211008:
                    case 15101006:
                        statups.Add(Tuple.Create(MapleBuffStat.WkCharge, x));
                        break;
                    case 21120007: //战神之盾
                        statups.Add(Tuple.Create(MapleBuffStat.MagicGuard, x));
                        break;
                    case 21111001: //灵巧击退MAGIC_GUARD
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, x));
                        break;
                    case 21100005: //连环吸血
                        statups.Add(Tuple.Create(MapleBuffStat.Infinity, x));
                        break;
                    case 21101003: //抗压
                        statups.Add(Tuple.Create(MapleBuffStat.Powerguard, x));
                        break;
                    /*case 21111005: //冰雪矛
                        statups.Add(Tuple.Create(MapleBuffStat.WK_CHARGE,x));
                        ret.duration *= 2; //冰雪矛冰冻时间为2秒
                        //monsterStatus.Add(MonsterStatus.SPEED, ret.x));
                                            monsterStatus.Add(MonsterStatus.FREEZE, 1));
                        break;*/
                    case 1101004:
                    case 1101005: // booster
                    case 1201004:
                    case 1201005:
                    case 1301004:
                    case 1301005: //快速矛
                    case 2111005: // spell booster, do these work the same? :做这些工作的助推器,法术一样吗?
                    case 2211005:
                    case 3101002:
                    case 3201002:
                    case 4101003:
                    case 4201002:
                    case 5101006:
                    case 5201003:
                    case 11101001: //魂骑士-快速剑
                    case 12101004: //炎骑士-魔法狂暴
                    case 13101001: //精灵使者-快速箭
                    case 14101002:
                    case 15101002:
                    case 21001003:
                        statups.Add(Tuple.Create(MapleBuffStat.Booster, x));
                        break;
                    case 5121009:
                        statups.Add(Tuple.Create(MapleBuffStat.SpeedInfusion, x));
                        goto case 15111005;
                    case 15111005:
                        statups.Add(Tuple.Create(MapleBuffStat.SpeedInfusion, x));
                        break;
                    case 1101006: // 愤怒
                    case 11101003: // 愤怒之火
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, (int) ret.Wdef));
                        goto case 1121010;
                    case 1121010: // enrage
                        statups.Add(Tuple.Create(MapleBuffStat.Watk, (int) ret.Watk));
                        break;
                    case 1301006: // iron will
                        statups.Add(Tuple.Create(MapleBuffStat.Mdef, (int) ret.Mdef));
                        goto case 1001003;
                    case 1001003: // iron body
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, (int) ret.Wdef));
                        break;
                    case 2001003: // magic armor
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, (int) ret.Wdef));
                        break;
                    case 2101001: // meditation
                    case 2201001: // meditation
                        statups.Add(Tuple.Create(MapleBuffStat.Matk, (int) ret.Matk));
                        break;
                    case 4101004: // 轻功
                    case 4201003: // 轻功
                    case 9001001: // gm轻功
                        statups.Add(Tuple.Create(MapleBuffStat.Speed, (int) ret.Speed));
                        statups.Add(Tuple.Create(MapleBuffStat.Jump, (int) ret.Jump));
                        break;
                    case 2301004: //祝福 
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, (int) ret.Wdef));
                        statups.Add(Tuple.Create(MapleBuffStat.Mdef, (int) ret.Mdef));
                        goto case 3001003;
                    case 3001003: //二连射
                        statups.Add(Tuple.Create(MapleBuffStat.Acc, (int) ret.Acc));
                        statups.Add(Tuple.Create(MapleBuffStat.Avoid, (int) ret.Avoid));
                        break;
                    case 9001003: //GM祝福
                        statups.Add(Tuple.Create(MapleBuffStat.Matk, (int) ret.Matk));
                        goto case 9001003;
                    case 3121008: // 集中精力
                        statups.Add(Tuple.Create(MapleBuffStat.Watk, (int) ret.Watk));
                        break;
                    case 5001005: //疾驰
                        statups.Add(Tuple.Create(MapleBuffStat.Dash, x));
                        statups.Add(Tuple.Create(MapleBuffStat.Jump, ret.Y));
                        goto case 1101007;
                    case 1101007: //伤害反击
                    case 1201007:
                    case 21100003:
                        statups.Add(Tuple.Create(MapleBuffStat.Powerguard, x));
                        break;
                    case 1301007:
                    case 9001008:
                        statups.Add(Tuple.Create(MapleBuffStat.Hyperbodyhp, x));
                        statups.Add(Tuple.Create(MapleBuffStat.Hyperbodymp, ret.Y));
                        break;
                    case 1001: // recovery
                    case 10001001:
                    case 20001001:
                        statups.Add(Tuple.Create(MapleBuffStat.Recovery, x));
                        break;
                    case 1111002: // combo
                    case 11111001:
                        statups.Add(Tuple.Create(MapleBuffStat.Combo, 1));
                        break;
                    case 1011:
                    case 20001011:
                        statups.Add(Tuple.Create(MapleBuffStat.BerserkFury, 1));
                        break;
                    case 1004: // monster riding
                    case 10001004:
                    case 20001004:
                    case 5221006: // 4th Job - Pirate riding
                    case 5221008:
                        statups.Add(Tuple.Create(MapleBuffStat.MonsterRiding, 1));
                        break;
                    case 1311006: //dragon roar
                        ret.m_hpR = -x/100.0;
                        break;
                    case 1311008: // dragon blood
                        statups.Add(Tuple.Create(MapleBuffStat.Dragonblood, 1));
                        break;
                    case 1121000: // maple warrior, all classes
                    case 1221000:
                    case 1321000:
                    case 2121000:
                    case 2221000:
                    case 2321000:
                    case 3121000:
                    case 3221000:
                    case 4121000:
                    case 4221000:
                    case 5121000:
                    case 5221000:
                    case 21121000:
                        statups.Add(Tuple.Create(MapleBuffStat.MapleWarrior, 1));
                        break;
                    case 3121002: // sharp eyes bowmaster
                    case 3221002: // sharp eyes marksmen
                        statups.Add(Tuple.Create(MapleBuffStat.SharpEyes, ret.X << 8 | ret.Y));
                        break;
                    case 1321007: //Beholder
                    case 2221005: // ifrit
                    case 2311006: // summon dragon
                    case 2321003: // bahamut
                    case 3121006: // phoenix
                    case 5211001: // Pirate octopus summon
                    case 5211002: // Pirate bird summon
                    case 5220002: // wrath of the octopi
                    case 11001004:
                    case 12001004:
                    case 13001004:
                    case 14001005:
                    case 15001004:
                    case 12111004:
                        statups.Add(Tuple.Create(MapleBuffStat.Summon, 1));
                        break;
                    case 2311003: //神圣祈祷
                    case 21110000: //属性暴击
                    case 9001002: //GM圣化之力
                        statups.Add(Tuple.Create(MapleBuffStat.HolySymbol, x));
                        break;
                    case 4121006: // 暗器伤人
                        statups.Add(Tuple.Create(MapleBuffStat.ShadowClaw, 0));
                        break;
                    case 2121004:
                    case 2221004:
                    case 2321004: // Infinity
                        statups.Add(Tuple.Create(MapleBuffStat.Infinity, x));
                        break;
                    case 1121002:
                    case 1221002:
                    case 0000012: //精灵的祝福
                    case 21120004: //防守策略
                    case 21120009: //(隐藏) 战神之舞- 双重重击
                    case 21120010: //(隐藏) 战神之舞 - 三重重击
                    case 1321002: // Stance
                    case 21121003: //战神的意志
                        statups.Add(Tuple.Create(MapleBuffStat.Stance, iprop));
                        break;
                    case 1005: // Echo of Hero
                        statups.Add(Tuple.Create(MapleBuffStat.EchoOfHero, ret.X));
                        break;
                    case 2121002: // mana reflection
                    case 2221002:
                    case 2321002:
                        statups.Add(Tuple.Create(MapleBuffStat.ManaReflection, 1));
                        break;
                    case 2321005: // holy shield
                        statups.Add(Tuple.Create(MapleBuffStat.HolyShield, x));
                        break;
                    case 3111002: // puppet ranger
                    case 3211002: // puppet sniper
                        statups.Add(Tuple.Create(MapleBuffStat.Puppet, 1));
                        break;

                    // -----------------------------飓风把! ----------------------------- //

                    case 4001002: //混乱
                        monsterStatus.Add(Mob.MonsterStatus.Watk, ret.X);
                        monsterStatus.Add(Mob.MonsterStatus.Wdef, ret.Y);
                        break;
                    case 1201006: // threaten
                        monsterStatus.Add(Mob.MonsterStatus.Watk, ret.X);
                        monsterStatus.Add(Mob.MonsterStatus.Wdef, ret.Y);
                        break;
                    case 1211002: // charged blow
                    case 1111008: // shout
                    case 4211002: // assaulter
                    case 3101005: // arrow bomb
                    case 1111005: // coma: sword
                    case 1111006: // coma: axe
                    case 4221007: // boomerang step
                    case 20001005:
                    case 5101002: // Backspin Blow
                    case 5101003: // Double Uppercut
                    case 5121004: // Demolition
                    case 14101006:
                    case 21110004:
                    case 21100004:
                    case 5121005: // Snatch
                    case 5121007: // Barrage
                    case 5201004: // pirate blank shot 
                    case 11111003:
                        monsterStatus.Add(Mob.MonsterStatus.Stun, 1);
                        break;
                    case 4121003:
                    case 4221003:
                        monsterStatus.Add(Mob.MonsterStatus.Taunt, ret.X);
                        monsterStatus.Add(Mob.MonsterStatus.Mdef, ret.X);
                        monsterStatus.Add(Mob.MonsterStatus.Wdef, ret.X);
                        break;
                    case 4121004: // Ninja ambush
                    case 4221004: //忍者伏击
                        //int damage = 2 * (c.GetPlayer().GetStr() + c.GetPlayer().GetLuk()) * (ret.damage / 100);
                        monsterStatus.Add(Mob.MonsterStatus.NinjaAmbush, 1);
                        break;
                    case 2201004: // 冰冻术
                    case 20001013:
                    case 2211002: // ice strike
                    case 5221003:

                    case 3211003: // blizzard
                    case 2211006: // il elemental compo
                    case 2221007: // 落霜冰破
                    case 21120006: //星辰
                    case 5211005: // Ice Splitter
                    case 2121006: // Paralyze
                        monsterStatus.Add(Mob.MonsterStatus.Freeze, 1);
                        ret.Duration *= 2; // 冰冻的时间
                        break;
                    case 2121003: // fire demon
                    case 2221003: // ice demon
                        monsterStatus.Add(Mob.MonsterStatus.Poison, 1);
                        monsterStatus.Add(Mob.MonsterStatus.Freeze, 1);
                        break;
                    case 2101003: // fp slow
                    case 2201003: // il slow
                        monsterStatus.Add(Mob.MonsterStatus.Speed, ret.X);
                        break;
                    case 2101005: // poison breath
                    case 2111006: // fp elemental compo
                        monsterStatus.Add(Mob.MonsterStatus.Poison, 1);
                        break;
                    case 2311005:
                        monsterStatus.Add(Mob.MonsterStatus.Doom, 1);
                        break;
                    case 3111005: // golden hawk
                    case 3211005: // golden eagle
                    case 13111004:
                        statups.Add(Tuple.Create(MapleBuffStat.Summon, 1));
                        monsterStatus.Add(Mob.MonsterStatus.Stun, 1);
                        break;
                    case 2121005: // elquines
                    case 3221005: // frostprey
                        statups.Add(Tuple.Create(MapleBuffStat.Summon, 1));
                        monsterStatus.Add(Mob.MonsterStatus.Freeze, 1);
                        break;
                    case 2111004: // fp seal
                    case 2211004: // il seal
                    case 12111002:
                        monsterStatus.Add(Mob.MonsterStatus.Seal, 1);
                        break;
                    case 4111003: // shadow web
                    case 14111001:
                        monsterStatus.Add(Mob.MonsterStatus.ShadowWeb, 1);
                        break;
                    case 3121007: // Hamstring
                        statups.Add(Tuple.Create(MapleBuffStat.Hamstring, x));
                        monsterStatus.Add(Mob.MonsterStatus.Speed, x);
                        break;
                    case 3221006: // Blind
                        statups.Add(Tuple.Create(MapleBuffStat.Blind, x));
                        monsterStatus.Add(Mob.MonsterStatus.Acc, x);
                        break;
                    case 5221009:
                        monsterStatus.Add(Mob.MonsterStatus.Hypnotized, 1);
                        break;
                    default:
                        break;
                }
            }

            if (ret.m_isMorph && !ret.IsPirateMorph())
            {
                statups.Add(Tuple.Create(MapleBuffStat.Morph, ret.m_morphId));
            }

            ret.MonsterStatus = monsterStatus;

            statups.TrimExcess();
            ret.m_statups = statups;

            return ret;
        }

        public void ApplyPassive(MapleCharacter applyto, IMapleMapObject obj, int attack)
        {
            if (MakeChanceResult())
            {
                switch (m_sourceid)
                {
                    // MP eater
                    case 2100000:
                    case 2200000:
                    case 2300000:
                        if (obj == null || obj.GetType() != MapleMapObjectType.Monster)
                        {
                            return;
                        }
                        var mob = (MapleMonster) obj;
                        // x is absorb percentage
                        if (!mob.IsBoss)
                        {
                            var absorbMp = Math.Min((int) (mob.MaxMp*(X/100.0)), mob.Mp);
                            if (absorbMp > 0)
                            {
                                mob.Mp -= absorbMp;
                                applyto.Mp += (short) absorbMp;
                                applyto.Client.Send(PacketCreator.ShowOwnBuffEffect(m_sourceid, 1));
                                applyto.Map.BroadcastMessage(applyto,
                                    PacketCreator.ShowBuffeffect(applyto.Id, m_sourceid, 1, 3), false);
                            }
                        }
                        break;
                }
            }
        }

        public bool ApplyTo(MapleCharacter chr)
        {
            return ApplyTo(chr, chr, true, null);
        }

        public bool ApplyTo(MapleCharacter chr, Point pos)
        {
            return ApplyTo(chr, chr, true, pos);
        }

        private bool ApplyTo(MapleCharacter applyfrom, MapleCharacter applyto, bool primary, Point? pos)
        {
            var hpchange = CalcHpChange(applyfrom, primary);
            var mpchange = CalcMpChange(applyfrom, primary);

            if (primary)
            {
                if (m_itemConNo != 0)
                {
                    var type = MapleItemInformationProvider.Instance.GetInventoryType(m_itemCon);
                    MapleInventoryManipulator.RemoveById(applyto.Client, type, m_itemCon, m_itemConNo, false, true);
                }
            }
            if (m_cureDebuffs.Any())
            {
                foreach (var debuff in m_cureDebuffs)
                {
                    applyfrom.DispelDebuff(debuff);
                }
            }
            var hpmpupdate = new List<Tuple<MapleStat, int>>(2);
            if (!primary && IsResurrection())
            {
                hpchange = applyto.MaxHp;
                applyto.Stance = 0;
            }
            if (IsDispel() && MakeChanceResult())
            {
                applyto.DispelDebuffs();
            }
            if (IsHeroWill())
            {
                applyto.CancelAllDebuffs();
            }
            if (hpchange != 0)
            {
                if (hpchange < 0 && -hpchange > applyto.Hp)
                {
                    return false;
                }
                var newHp = applyto.Hp + hpchange;
                if (newHp < 1)
                {
                    newHp = 1;
                }
                applyto.Hp = (short) newHp;
                hpmpupdate.Add(new Tuple<MapleStat, int>(MapleStat.Hp, applyto.Hp));
            }
            if (mpchange != 0)
            {
                if (mpchange < 0 && -mpchange > applyto.Mp)
                {
                    return false;
                }
                applyto.Mp += (short) mpchange;
                hpmpupdate.Add(new Tuple<MapleStat, int>(MapleStat.Mp, applyto.Mp));
            }

            applyto.Client.Send(PacketCreator.UpdatePlayerStats(hpmpupdate, true));

            if (m_moveTo != -1)
            {
                MapleMap target = null;
                var nearest = false;
                if (m_moveTo == 999999999)
                {
                    nearest = true;
                    if (applyto.Map.ReturnMapId != 999999999)
                    {
                        target = applyto.Map.ReturnMap;
                    }
                }
                else
                {
                    target = applyto.Client.ChannelServer.MapFactory.GetMap(m_moveTo);
                    var targetMapId = target.MapId/10000000;
                    var charMapId = applyto.Map.MapId/10000000;
                    if (targetMapId != 60 && charMapId != 61)
                    {
                        if (targetMapId != 21 && charMapId != 20)
                        {
                            if (targetMapId != 12 && charMapId != 10)
                            {
                                if (targetMapId != 10 && charMapId != 12)
                                {
                                    if (targetMapId != charMapId)
                                    {
                                        Console.WriteLine("人物 {0} 尝试回到一个非法的位置 ({1}->{2})", applyto.Name,
                                            applyto.Map.MapId, target.MapId);
                                        applyto.Client.Close();
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                if (target == applyto.Map || nearest && applyto.Map.Town)
                {
                    return false;
                }
            }
            if (IsShadowClaw())
            {
                var use = applyto.Inventorys[MapleInventoryType.Use.Value];
                var mii = MapleItemInformationProvider.Instance;
                var projectile = 0;
                for (var i = 0; i < 255; i++)
                {
                    // impose order...
                    IMapleItem item;
                    if (use.Inventory.TryGetValue(1, out item))
                    {
                        var isStar = mii.IsThrowingStar(item.ItemId);
                        if (isStar && item.Quantity >= 200)
                        {
                            projectile = item.ItemId;
                            break;
                        }
                    }
                }
                if (projectile == 0)
                {
                    return false;
                }
                MapleInventoryManipulator.RemoveById(applyto.Client, MapleInventoryType.Use, projectile, 200, false,true);
            }
            if (m_overTime)
            {
                ApplyBuffEffect(applyfrom, applyto, primary);
            }
            if (primary && (m_overTime || IsHeal()))
            {
                ApplyBuff(applyfrom);
            }
            if (primary && IsMonsterBuff())
            {
                ApplyMonsterBuff(applyfrom);
            }

            var summonMovementType = GetSummonMovementType();
            if (summonMovementType.HasValue && pos != null)
            {
                var tosummon = new MapleSummon(applyfrom, m_sourceid, pos.Value, summonMovementType.Value);
                if (!tosummon.IsPuppet())
                {
                    applyfrom.AntiCheatTracker.ResetSummonAttack();
                }
                applyfrom.Map.SpawnSummon(tosummon);
                applyfrom.Summons.Add(m_sourceid, tosummon);
                tosummon.Hp += X;
                if (IsBeholder())
                {
                    tosummon.Hp++;
                }
            }
            // Magic Door
            if (IsMagicDoor())
            {
                //Point doorPosition = new Point(applyto.Position.X, applyto.Position.Y);
                //MapleDoor door = new MapleDoor(applyto, doorPosition);
                //applyto.Map.SpawnDoor(door);
                //applyto.AddDoor(door);
                //door = new MapleDoor(door);
                //applyto.AddDoor(door);
                //door.Town.spawnDoor(door);
                //if (applyto.Party != null)
                //{
                //    applyto.SilentPartyUpdate();
                //}
                //applyto.disableDoor();
            }
            else if (IsMist())
            {
                //Rectangle bounds = CalculateBoundingBox(applyfrom.Position, applyfrom.IsFacingLeft);
                //MapleMist mist = new MapleMist(bounds, applyfrom, this);
                //applyfrom.Map.spawnMist(mist, _duration, false);
            }
            if (IsTimeLeap())
            {
                foreach (var i in applyto.GetAllCooldowns())
                {
                    if (i.SkillId != 5121010)
                    {
                        applyto.RemoveCooldown(i.SkillId);
                    }
                }
            }
            if (IsHide())
            {
                if (applyto.IsHidden)
                {
                    applyto.Map.BroadcastMessage(applyto, PacketCreator.RemovePlayerFromMap(applyto.Id), false);
                    applyto.Client.Send(PacketCreator.GiveGmHide(true));
                }
                else
                {
                    applyto.Client.Send(PacketCreator.GiveGmHide(false));
                    applyto.Map.BroadcastMessage(applyto, PacketCreator.SpawnPlayerMapobject(applyto), false);
                    foreach (var pet in applyto.Pets)
                    {
                        if (pet != null)
                        {
                            applyto.Map.BroadcastMessage(applyto, PacketCreator.ShowPet(applyto, pet, false), false);
                        }
                    }
                }
            }
            return true;
        }

        public bool ApplyReturnScroll(MapleCharacter applyto)
        {
            if (m_moveTo != -1)
            {
                if (applyto.Map.ReturnMapId != applyto.Map.MapId)
                {
                    MapleMap target;
                    if (m_moveTo == 999999999)
                    {
                        target = applyto.Map.ReturnMap;
                    }
                    else
                    {
                        target = applyto.Client.ChannelServer.MapFactory.GetMap(m_moveTo);
                        if (target.MapId/10000000 != 60 && applyto.Id/10000000 != 61)
                        {
                            if (target.MapId/10000000 != 21 && applyto.Map.MapId/10000000 != 20)
                            {
                                if (target.MapId/10000000 != applyto.Map.MapId/10000000)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    applyto.ChangeMap(target, target.GetPortal(0));
                    return true;
                }
            }
            return false;
        }

        private void ApplyBuff(MapleCharacter applyfrom)
        {
            if (IsPartyBuff() && (applyfrom.Party != null || IsGmBuff()))
            {
                var bounds = CalculateBoundingBox(applyfrom.Position, applyfrom.IsFacingLeft);
                var affecteds = applyfrom.Map.GetMapObjectsInRect(bounds,
                    new List<MapleMapObjectType> {MapleMapObjectType.Player});

                var affectedp = new List<MapleCharacter>(affecteds.Count);
                foreach (var affectedmo in affecteds)
                {
                    var affected = (MapleCharacter) affectedmo;
                    //this is new and weird...
                    if (affected != null && IsHeal() && affected != applyfrom && affected.Party == applyfrom.Party &&
                        affected.IsAlive)
                    {
                        var expadd =
                            (int)
                                (CalcHpChange(applyfrom, true)/10*
                                 (applyfrom.Client.ChannelServer.ExpRate + (Randomizer.NextDouble()*10 + 30))*
                                 (Math.Floor(Randomizer.NextDouble()*
                                             applyfrom.GetSkillLevel(SkillFactory.GetSkill(2301002))/100)*
                                  (applyfrom.Level/30)));
                        if (affected.Hp < affected.MaxHp - affected.MaxHp/20)
                        {
                            applyfrom.GainExp(expadd, true, false, false);
                        }
                    }
                    if (affected != applyfrom && (IsGmBuff() || applyfrom.Party.Equals(affected?.Party)))
                    {
                        var isRessurection = IsResurrection();
                        if ((isRessurection && !affected.IsAlive) || (!isRessurection && affected.IsAlive))
                        {
                            affectedp.Add(affected);
                        }
                        if (IsTimeLeap())
                        {
                            foreach (var i in affected.GetAllCooldowns())
                            {
                                if (i.SkillId != 5121010)
                                {
                                    affected.RemoveCooldown(i.SkillId);
                                }
                            }
                        }
                    }
                }
                foreach (var affected in affectedp)
                {
                    // TODO actually heal (and others) shouldn't recalculate everything
                    // for heal this is an actual bug since heal hp is decreased with the number
                    // of affected players
                    ApplyTo(applyfrom, affected, false, null);
                    affected.Client.Send(PacketCreator.ShowOwnBuffEffect(m_sourceid, 2));
                    affected.Map.BroadcastMessage(affected, PacketCreator.ShowBuffeffect(affected.Id, m_sourceid, 2, 3),
                        false);
                }
            }
        }

        private void ApplyMonsterBuff(MapleCharacter applyfrom)
        {
            var bounds = CalculateBoundingBox(applyfrom.Position, applyfrom.IsFacingLeft);
            var affected = applyfrom.Map.GetMapObjectsInRect(bounds,
                new List<MapleMapObjectType> {MapleMapObjectType.Monster});
            var skill = SkillFactory.GetSkill(m_sourceid);
            var i = 0;
            foreach (var mo in affected)
            {
                var monster = (MapleMonster) mo;
                if (MakeChanceResult())
                {
                    monster.ApplyStatus(applyfrom, new MonsterStatusEffect(MonsterStatus, skill, false), IsPoison(),
                        Duration);
                }
                i++;
                if (i >= m_mobCount)
                {
                    break;
                }
            }
        }

        private Rectangle CalculateBoundingBox(Point posFrom, bool facingLeft)
        {
            Point mylt;
            Point myrb;
            if (facingLeft)
            {
                mylt = new Point(m_lt.X + posFrom.X, m_lt.Y + posFrom.Y);
                myrb = new Point(m_rb.X + posFrom.X, m_rb.Y + posFrom.Y);
            }
            else
            {
                myrb = new Point(m_lt.X*-1 + posFrom.X, m_rb.Y + posFrom.Y);
                mylt = new Point(m_rb.X*-1 + posFrom.X, m_lt.Y + posFrom.Y);
            }
            var bounds = new Rectangle(mylt.X, mylt.Y, myrb.X - mylt.X, myrb.Y - mylt.Y);
            return bounds;
        }

        public void SilentApplyBuff(MapleCharacter chr, int starttime)
        {
            var localDuration = Duration;
            localDuration = AlchemistModifyVal(chr, localDuration, false);
            var cancelAction = new CancelEffectAction(chr, this, starttime);

            var schedule = TimerManager.Instance.RunOnceTask(cancelAction.Run, starttime + localDuration);
            chr.RegisterEffect(this, starttime, schedule);

            var summonMovementType = GetSummonMovementType();
            if (summonMovementType.HasValue)
            {
                var tosummon = new MapleSummon(chr, m_sourceid, chr.Position, summonMovementType.Value);
                if (!tosummon.IsPuppet())
                {
                    chr.AntiCheatTracker.ResetSummonAttack();
                    chr.Summons.Add(m_sourceid, tosummon);
                    tosummon.Hp += X;
                }
            }
        }

        private void ApplyBuffEffect(MapleCharacter applyfrom, MapleCharacter applyto, bool primary)
        {
            if (!IsMonsterRiding())
            {
                applyto.CancelEffect(this, true, -1);
            }
            List<Tuple<MapleBuffStat, int>> localstatups = m_statups;
            int localDuration = Duration;
            int localsourceid = m_sourceid;
            if (IsMonsterRiding())
            {
                int ridingLevel = 0; // mount id
                IMapleItem mount; 
                if (applyfrom.Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(18,out mount))
                {
                    ridingLevel = mount.ItemId;
                }
                localDuration = m_sourceid;
                localsourceid = ridingLevel;
                localstatups = new List<Tuple<MapleBuffStat, int>>(1);
                localstatups.Add(Tuple.Create(MapleBuffStat.MonsterRiding, 0));
            }
            if (IsBattleShip())
            {
                int ridingLevel = 1932000;
                localDuration = m_sourceid;
                localsourceid = ridingLevel;
                localstatups = new List<Tuple<MapleBuffStat, int>>(1);
                localstatups.Add(Tuple.Create(MapleBuffStat.MonsterRiding, 0));
            }
            if (primary)
            {
                localDuration = AlchemistModifyVal(applyfrom, localDuration, false);
            }
            if (localstatups.Any())
            {
                applyto.Client.Send(IsDash()
                    ? PacketCreator.GiveDash(m_statups, localDuration/1000)
                    : PacketCreator.GiveBuff(applyto, m_skill ? localsourceid : -localsourceid, localDuration, localstatups));
            }
            if (localstatups.Any())
            {
                if (IsDash())
                {
                    applyto.Client.Send(PacketCreator.GiveDash(localstatups, localDuration / 1000));
                }
                else if (IsInfusion())
                {
                    applyto.Client.Send(PacketCreator.GiveInfusion((short)(localDuration / 1000), (short)X));
                }
                else
                {
                    applyto.Client.Send(PacketCreator.GiveBuff(applyto, (m_skill ? localsourceid : -localsourceid), localDuration, localstatups));
                }
            }
            if (IsMonsterRiding())
            {
                int ridingLevel = 0;
                IMapleItem mount;
                if (applyfrom.Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(18, out mount))
                {
                    ridingLevel = mount.ItemId;
                }
                List<Tuple<MapleBuffStat, int>> stat = new List<Tuple<MapleBuffStat, int>> { Tuple.Create(MapleBuffStat.MonsterRiding, 1)};
                applyto.Map.BroadcastMessage(applyto, PacketCreator.ShowMonsterRiding(applyto.Id, stat, ridingLevel, m_sourceid), false);
                localDuration = Duration;
            }
            if (IsBattleShip())
            {
                int ridingLevel = 1932000;
                List<Tuple<MapleBuffStat, int>> stat = new List<Tuple<MapleBuffStat, int>> { Tuple.Create(MapleBuffStat.MonsterRiding, 1) };
                applyto.Map.BroadcastMessage(applyto, PacketCreator.ShowMonsterRiding(applyto.Id, stat, ridingLevel, m_sourceid), false);
                localDuration = Duration;
            }
            if (IsDs())
            {
                List<Tuple<MapleBuffStat, int>> dsstat = new List<Tuple<MapleBuffStat, int>> { (Tuple.Create(MapleBuffStat.Darksight, 0)) };
                applyto.Map.BroadcastMessage(applyto, PacketCreator.GiveForeignBuff(applyto, dsstat, this), false);
            }
            if (IsCombo())
            {
                List<Tuple<MapleBuffStat, int>> stat = new List<Tuple<MapleBuffStat, int>> { Tuple.Create(MapleBuffStat.Combo, 1)};
                applyto.Map.BroadcastMessage(applyto, PacketCreator.GiveForeignBuff(applyto, stat, this), false);
            }
            if (IsShadowPartner())
            {
                List<Tuple<MapleBuffStat, int>> stat = new List<Tuple<MapleBuffStat, int>> { Tuple.Create(MapleBuffStat.Shadowpartner, 0)};
                applyto.Map.BroadcastMessage(applyto, PacketCreator.GiveForeignBuff(applyto, stat, this), false);
            }
            if (IsSoulArrow())
            {
                List<Tuple<MapleBuffStat, int>> stat = new List<Tuple<MapleBuffStat, int>> { Tuple.Create(MapleBuffStat.Soularrow, 0)};
                applyto.Map.BroadcastMessage(applyto, PacketCreator.GiveForeignBuff(applyto, stat, this), false);
            }
            if (IsEnrage())
            {
                applyto.HandleOrbconsume();
            }
            if (m_isMorph)
            {
                List<Tuple<MapleBuffStat, int>> stat = new List<Tuple<MapleBuffStat, int>> { Tuple.Create(MapleBuffStat.Morph, m_morphId) };
                applyto.Map.BroadcastMessage(applyto, PacketCreator.GiveForeignBuff(applyto, stat, this), false);
            }
            if (IsPirateMorph())
            {
                List<Tuple<MapleBuffStat, int>> stat = new List<Tuple<MapleBuffStat, int>>
                {
                    Tuple.Create(MapleBuffStat.Speed, (int) Speed),
                    Tuple.Create(MapleBuffStat.Morph, m_morphId)
                };
                applyto.Map.BroadcastMessage(applyto, PacketCreator.GiveForeignBuff(applyto, stat, this), false);
            }
            if (IsTimeLeap())
            {
                foreach (var i in applyto.GetAllCooldowns())
                {
                    if (i.SkillId != 5121010)
                    {
                        applyto.RemoveCooldown(i.SkillId);
                    }
                }
            }
            if (localstatups.Any())
            {
                long starttime = DateTime.Now.GetTimeMilliseconds();
                CancelEffectAction cancelAction = new CancelEffectAction(applyto, this, starttime);
                var schedule = TimerManager.Instance.RunOnceTask(()=>cancelAction.Run(), localDuration);
                applyto.RegisterEffect(this, starttime, schedule);
            }
            if (primary && !IsHide())
            {
                if (IsDash())
                {
                    applyto.Map.BroadcastMessage(applyto, PacketCreator.ShowDashEffecttoOthers(applyto.Id, localstatups,(short) (localDuration / 1000)), false);
                }
                else if (IsInfusion())
                {
                    applyto.Map.BroadcastMessage(applyto, PacketCreator.GiveForeignInfusion(applyto.Id, X, localDuration / 1000), false);
                }
                else
                {
                    applyto.Map.BroadcastMessage(applyto, PacketCreator.ShowBuffeffect(applyto.Id, m_sourceid, 1, (byte)3), false);
                }
            }
        }

        public bool isMorph()
        {
            return m_morphId > 0;
        }

        private int CalcHpChange(MapleCharacter applyfrom, bool primary)
        {
            var hpchange = 0;
            if (m_hp != 0)
            {
                if (!m_skill)
                {
                    if (primary)
                    {
                        hpchange += AlchemistModifyVal(applyfrom, m_hp, true);
                    }
                    else
                    {
                        hpchange += m_hp;
                    }
                }
                else
                {
                    // assumption: this is heal
                    hpchange += MakeHealHp(m_hp/100.0, applyfrom.Magic, 3, 5);
                }
            }
            if (m_hpR != 0)
            {
                hpchange += (int) (applyfrom.Localmaxhp*m_hpR);
                applyfrom.CheckBerserk();
            }
            if (primary)
            {
                if (HpCon != 0)
                {
                    hpchange -= HpCon;
                }
            }
            if (IsChakra())
            {
                hpchange += MakeHealHp(Y/100.0, applyfrom.Luk, 2.3, 3.5);
            }
            if (IsPirateMpRecovery())
            {
                hpchange -= (int) (Y/100.0*applyfrom.Localmaxhp);
            }
            return hpchange;
        }

        private int MakeHealHp(double rate, double stat, double lowerfactor, double upperfactor)
        {
            var maxHeal = (int) (stat*upperfactor*rate);
            var minHeal = (int) (stat*lowerfactor*rate);
            return (int) (Randomizer.NextDouble()*(maxHeal - minHeal + 1) + minHeal);
        }

        private int CalcMpChange(MapleCharacter applyfrom, bool primary)
        {
            var mpchange = 0;
            if (m_mp != 0)
            {
                if (primary)
                {
                    mpchange += AlchemistModifyVal(applyfrom, m_mp, true);
                }
                else
                {
                    mpchange += m_mp;
                }
            }
            if (m_mpR != 0)
            {
                mpchange += (int) (applyfrom.Localmaxmp*m_mpR);
            }
            if (primary)
            {
                if (MpCon != 0)
                {
                    var mod = 1.0;
                    var isAFpMage = applyfrom.Job == MapleJob.FpMage;
                    if (isAFpMage || applyfrom.Job == MapleJob.IlMage)
                    {
                        ISkill amp;
                        if (isAFpMage)
                        {
                            amp = SkillFactory.GetSkill(2110001);
                        }
                        else
                        {
                            amp = SkillFactory.GetSkill(2210001);
                        }
                        var ampLevel = applyfrom.GetSkillLevel(amp);
                        if (ampLevel > 0)
                        {
                            var ampStat = amp.GetEffect(ampLevel);
                            mod = ampStat.X/100.0;
                        }
                    }
                    mpchange -= (int) (MpCon*mod);
                    if (applyfrom.GetBuffedValue(MapleBuffStat.Infinity) != null)
                    {
                        mpchange = 0;
                    }
                }
            }
            if (IsPirateMpRecovery())
            {
                mpchange += (int) (Y*X/10000.0*applyfrom.Localmaxhp);
            }
            return mpchange;
        }

        private int AlchemistModifyVal(MapleCharacter chr, int val, bool withX)
        {
            if (!m_skill && (chr.Job == MapleJob.Hermit || chr.Job == MapleJob.Nightlord))
            {
                var alchemistEffect = GetAlchemistEffect(chr);
                if (alchemistEffect != null)
                {
                    return (int) (val*((withX ? alchemistEffect.X : alchemistEffect.Y)/100.0));
                }
            }
            return val;
        }

        private MapleStatEffect GetAlchemistEffect(MapleCharacter chr)
        {
            var alchemist = SkillFactory.GetSkill(4110000);
            var alchemistLevel = chr.GetSkillLevel(alchemist);
            if (alchemistLevel == 0)
            {
                return null;
            }
            return alchemist.GetEffect(alchemistLevel);
        }

        public void SetSourceId(int newid)
        {
            m_sourceid = newid;
        }

        private bool IsGmBuff()
        {
            switch (m_sourceid)
            {
                case 1005: // echo of hero acts like a gm buff
                case 9001000:
                case 9001001:
                case 9001002:
                case 9001003:
                case 9001005:
                case 9001008:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsMonsterBuff()
        {
            if (!m_skill)
            {
                return false;
            }
            switch (m_sourceid)
            {
                case 1201006: // threaten
                case 2101003: // fp slow
                case 2201003: // il slow
                case 2211004: // il seal
                case 2111004: // fp seal
                case 2311005: // doom
                case 4111003: // shadow web
                case 4121004: // Ninja ambush
                case 4421004: // Ninja ambush
                    return true;
            }
            return false;
        }

        private bool IsPartyBuff()
        {
            if ((m_lt == null) || (m_rb == null))
            {
                return false;
            }

            return ((m_sourceid < 1211003) || (m_sourceid > 1211008)) && (m_sourceid != 1221003) && (m_sourceid != 1221004);
        }

        public bool IsHeal()
        {
            return m_sourceid == 2301002 || m_sourceid == 9001000;
        }

        public bool IsResurrection()
        {
            return m_sourceid == 9001005 || m_sourceid == 2321006;
        }

        public bool IsTimeLeap()
        {
            return m_sourceid == 5121010;
        }

        public bool IsInfusion()
        {
            return false;
        }

        public bool IsOverTime()
        {
            return m_overTime;
        }

        public List<Tuple<MapleBuffStat, int>> GetStatups()
        {
            return m_statups;
        }

        public bool SameSource(MapleStatEffect effect)
        {
            return m_sourceid == effect.m_sourceid && m_skill == effect.m_skill;
        }

        public bool IsHide()
        {
            return m_skill && m_sourceid == 9001004;
        }

        public bool IsDragonBlood()
        {
            return m_skill && m_sourceid == 1311008;
        }

        public bool IsBerserk()
        {
            return m_skill && m_sourceid == 1320006;
        }

        private bool IsDs()
        {
            return m_skill && m_sourceid == 4001003;
        }

        private bool IsCombo()
        {
            return (m_skill && m_sourceid == 1111002) || (m_skill && m_sourceid == 11111001);
        }

        private bool IsEnrage()
        {
            return m_skill && m_sourceid == 1121010;
        }

        public bool IsBeholder()
        {
            return m_skill && m_sourceid == 1321007;
        }

        private bool IsShadowPartner()
        {
            return m_skill && m_sourceid == 4111002;
        }

        private bool IsChakra()
        {
            return m_skill && m_sourceid == 4211001;
        }

        private bool IsPirateMpRecovery()
        {
            return m_skill && m_sourceid == 5101005;
        }

        public bool IsMonsterRiding()
        {
            return m_skill && (m_sourceid%20000000 == 1004 || m_sourceid == 5221006);
        }

        private bool IsBattleShip()
        {
            return m_skill && m_sourceid == 5221006;
        }

        /* public bool isMagicDoor() {
             return skill && sourceid == 2311002;
         }*/

        public bool IsMagicDoor()
        {
            return m_skill && (m_sourceid == 2311002); //时空门
        }

        public bool IsMesoGuard()
        {
            return m_skill && m_sourceid == 4211005;
        }

        public bool IsCharge()
        {
            return m_skill && m_sourceid >= 1211003 && m_sourceid <= 1211008;
        }

        public bool IsPoison()
        {
            return m_skill && (m_sourceid == 2111003 || m_sourceid == 2101005 || m_sourceid == 2111006);
        }

        private bool IsMist()
        {
            return m_skill && (m_sourceid == 2111003 || m_sourceid == 4221006); // poison mist and smokescreen
        }

        private bool IsSoulArrow()
        {
            return m_skill && (m_sourceid == 3101004 || m_sourceid == 3201004 || m_sourceid == 13101003);
                // bow and crossbow
        }

        private bool IsShadowClaw()
        {
            return m_skill && m_sourceid == 4121006;
        }

        private bool IsDispel()
        {
            return m_skill && (m_sourceid == 2311001 || m_sourceid == 9001000);
        }

        private bool IsHeroWill()
        {
            return m_skill &&
                   (m_sourceid == 1121011 || m_sourceid == 1221012 || m_sourceid == 1321010 || m_sourceid == 2121008 ||
                    m_sourceid == 2221008 || m_sourceid == 2321009 || m_sourceid == 3121009 || m_sourceid == 3221008 ||
                    m_sourceid == 4121009 || m_sourceid == 4221008 || m_sourceid == 5121008 || m_sourceid == 5221010);
        }

        public bool IsComboMove()
        {
            return m_skill &&
                   ((m_sourceid == 21100004) || (m_sourceid == 21100005) || (m_sourceid == 21110003) ||
                    (m_sourceid == 21110004) || (m_sourceid == 21120006) || (m_sourceid == 21120007));
        }


        private bool IsDash()
        {
            return m_skill && (m_sourceid == 5001005);
        }

        public bool IsPirateMorph()
        {
            return m_skill && (m_sourceid == 5111005 || m_sourceid == 5121003);
        }


        public SummonMovementType? GetSummonMovementType()
        {
            if (!m_skill)
            {
                return null;
            }
            switch (m_sourceid)
            {
                case 3211002: // puppet sniper
                case 3111002: // puppet ranger
                case 5211001: // octopus - pirate
                case 5220002: // advanced octopus - pirate
                    return SummonMovementType.Stationary;
                case 3211005: // golden eagle
                case 3111005: // golden hawk
                case 2311006: // summon dragon
                case 3221005: // frostprey
                case 3121006: // phoenix
                case 5211002: // bird - pirate
                    return SummonMovementType.CircleFollow;
                case 1321007: // 灵魂助力
                case 2121005: // 冰破魔兽
                case 2221005: // 火魔兽
                case 5221010:
                case 21121008:
                case 2321003: // 强化圣龙

                    break;
                case 11001004: //魂精灵
                case 12001004: //炎精灵
                case 13001004: //风精灵
                case 14001005: //夜精灵
                case 15001004: //雷精灵
                case 12111004: //火魔兽
                    return SummonMovementType.Follow;
            }
            return null;
        }

        public bool IsSkill()
        {
            return m_skill;
        }

        public int GetSourceId()
        {
            return m_sourceid;
        }

        public double GetIProp()
        {
            return m_prop*100;
        }

        public int GetMastery()
        {
            return m_mastery;
        }

        public int GetRange()
        {
            return m_range;
        }

        public int GetFixedDamage()
        {
            return m_fixDamage;
        }

        public string GetBuffString()
        {
            var sb = new StringBuilder();
            sb.Append("WATK: ");
            sb.Append(Watk);
            sb.Append(", ");
            sb.Append("WDEF: ");
            sb.Append(Wdef);
            sb.Append(", ");
            sb.Append("MATK: ");
            sb.Append(Matk);
            sb.Append(", ");
            sb.Append("MDEF: ");
            sb.Append(Mdef);
            sb.Append(", ");
            sb.Append("ACC: ");
            sb.Append(Acc);
            sb.Append(", ");
            sb.Append("AVOID: ");
            sb.Append(Avoid);
            sb.Append(", ");
            sb.Append("SPEED: ");
            sb.Append(Speed);
            sb.Append(", ");
            sb.Append("JUMP: ");
            sb.Append(Jump);
            sb.Append(".");

            return sb.ToString();
        }

        /**
         * 
         * @return true if the effect should happen based on it's probablity, false otherwise
         */

        public bool MakeChanceResult()
        {
            return Math.Abs(m_prop - 1.0) < 0.000001 || Randomizer.NextDouble() < m_prop;
        }

        public class CancelEffectAction
        {
            private readonly MapleStatEffect m_effect;
            private readonly long m_startTime;
            private readonly WeakReference<MapleCharacter> m_target;

            public CancelEffectAction(MapleCharacter target, MapleStatEffect effect, long startTime)
            {
                this.m_effect = effect;
                this.m_target = new WeakReference<MapleCharacter>(target);
                this.m_startTime = startTime;
            }

            public void Run()
            {
                MapleCharacter realTarget;
                if (m_target.TryGetTarget(out realTarget))
                {
                    //if (realTarget.inCS() || realTarget.inMTS())
                    //{
                    //    realTarget.AddToCancelBuffPackets(effect, startTime);
                    //    return;
                    //}
                    realTarget.CancelEffect(m_effect, false, m_startTime);
                }
            }
        }
    }
}