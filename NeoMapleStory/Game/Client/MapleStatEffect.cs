using NeoMapleStory.Core;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Skill;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace NeoMapleStory.Game.Client
{
     public class MapleStatEffect
    {
        private short _watk, _matk, _wdef, _mdef, _acc, _avoid, _hands, _speed, _jump;


        private short _hp, _mp;
        private double _hpR, _mpR;
        private short _mpCon, _hpCon;
        private int _duration;
        private bool _overTime;
        private int _sourceid;
        private int _moveTo;
        private bool _skill;
        private List<Tuple<MapleBuffStat, int>> _statups;
        private Dictionary<MonsterStatus, int> _monsterStatus;
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        private double _prop;
        private int _itemCon, _itemConNo;
        private int _fixDamage;
        private int _damage, _attackCount, _bulletCount, _bulletConsume;
        private Point _lt, _rb;
        private int _mobCount;
        private int _moneyCon;
        private int _cooldown;
        private bool _isMorph;
        private int _morphId;
        private List<MapleDisease> _cureDebuffs;
        private int _mastery, _range;
        private string _remark;
        private object _ret;

        public MapleStatEffect()
        {
        }

        public string GetRemark()
        {
            return _remark;
        }

        public static MapleStatEffect LoadSkillEffectFromData(IMapleData source, int skillid, bool overtime, string lvl)
        => LoadFromData(source, skillid, true, overtime, "Level " + lvl);


        public static MapleStatEffect LoadItemEffectFromData(IMapleData source, int itemid)
 => LoadFromData(source, itemid, false, false, "");
        

        private static void AddBuffStatTupleToListIfNotZero(List<Tuple<MapleBuffStat, int>> list, MapleBuffStat buffstat, int val)
        {
            if (val!= 0)
            {
                list.Add(Tuple.Create(buffstat, val));
            }
        }

        private static MapleStatEffect LoadFromData(IMapleData source, int sourceid, bool skill, bool overTime, string remarrk)
        {
            MapleStatEffect ret = new MapleStatEffect();
            ret._duration = MapleDataTool.ConvertToInt("time", source, -1);
            ret._hp = (short)MapleDataTool.GetInt("hp", source, 0);
            ret._hpR = MapleDataTool.GetInt("hpR", source, 0) / 100.0;
            ret._mp = (short)MapleDataTool.GetInt("mp", source, 0);
            ret._mpR = MapleDataTool.GetInt("mpR", source, 0) / 100.0;
            ret._mpCon = (short)MapleDataTool.GetInt("mpCon", source, 0);
            ret._hpCon = (short)MapleDataTool.GetInt("hpCon", source, 0);
            int iprop = MapleDataTool.GetInt("prop", source, 100);
            ret._prop = iprop / 100.0;
            ret._attackCount = MapleDataTool.GetInt("attackCount", source, 1);
            ret._mobCount = MapleDataTool.GetInt("mobCount", source, 1);
            ret._cooldown = MapleDataTool.GetInt("cooltime", source, 0);
            ret._morphId = MapleDataTool.GetInt("morph", source, 0);
            ret._isMorph = ret._morphId > 0 ? true : false;
            ret._remark = remarrk;
            ret._sourceid = sourceid;
            ret._skill = skill;

            if (!ret._skill && ret._duration > -1)
            {
                ret._overTime = true;
            }
            else {
                ret._duration *= 1000; // items have their times stored in ms, of course
                ret._overTime = overTime;
            }

            List<Tuple<MapleBuffStat, int>> statups = new List<Tuple<MapleBuffStat, int>>();

            ret._watk = (short)MapleDataTool.GetInt("pad", source, 0);
            ret._wdef = (short)MapleDataTool.GetInt("pdd", source, 0);
            ret._matk = (short)MapleDataTool.GetInt("mad", source, 0);
            ret._mdef = (short)MapleDataTool.GetInt("mdd", source, 0);
            ret._acc = (short)MapleDataTool.ConvertToInt("acc", source, 0);
            ret._avoid = (short)MapleDataTool.GetInt("eva", source, 0);
            ret._speed = (short)MapleDataTool.GetInt("speed", source, 0);
            ret._jump = (short)MapleDataTool.GetInt("jump", source, 0);
            if (ret._overTime && ret.GetSummonMovementType() == null)
            {
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Watk, ret._watk);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Wdef, ret._wdef);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Matk, ret._matk);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Mdef, ret._mdef);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Acc, ret._acc);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Avoid, ret._avoid);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Speed, ret._speed);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Jump, ret._jump);
                AddBuffStatTupleToListIfNotZero(statups, MapleBuffStat.Morph, ret._morphId);
            }

            IMapleData ltd = source.GetChildByPath("lt");
            if (ltd != null)
            {
                ret._lt = (Point)ltd.Data;
                ret._rb = (Point)source.GetChildByPath("rb").Data;
            }

            int x = MapleDataTool.GetInt("x", source, 0);
            ret.X = x;
            ret.Y = MapleDataTool.GetInt("y", source, 0);
            ret.Z = MapleDataTool.GetInt("z", source, 0);
            ret._damage = MapleDataTool.ConvertToInt("damage", source, 100);
            ret._bulletCount = MapleDataTool.ConvertToInt("bulletCount", source, 1);
            ret._bulletConsume = MapleDataTool.ConvertToInt("bulletConsume", source, 0);
            ret._moneyCon = MapleDataTool.ConvertToInt("moneyCon", source, 0);

            ret._itemCon = MapleDataTool.GetInt("itemCon", source, 0);
            ret._itemConNo = MapleDataTool.GetInt("itemConNo", source, 0);
            ret._fixDamage = MapleDataTool.GetInt("fixdamage", source, 0);

            ret._moveTo = MapleDataTool.GetInt("moveTo", source, -1);

            ret._mastery = MapleDataTool.ConvertToInt("mastery", source, 0);
            ret._range = MapleDataTool.ConvertToInt("range", source, 0);

            List<MapleDisease> localCureDebuffs = new List<MapleDisease>();

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
            ret._cureDebuffs = localCureDebuffs;

            Dictionary<MonsterStatus, int> monsterStatus = new Dictionary<MonsterStatus, int>();

            if (skill)
            { // 出租的,因为我们不能从datafile……
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
                        ret._duration = 60 * 120 * 1000;
                        ret._overTime = true;
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
                    case 21120002://战神之舞
                    case 13101003: //精灵使者-无形箭
                        statups.Add(Tuple.Create(MapleBuffStat.Soularrow, x));
                        break;
                    case 2311002: //时空门
                        statups.Add(new Tuple<MapleBuffStat, int>(MapleBuffStat.Soularrow, x));
                        break;
                    case 1211003:
                    case 1211004:
                    case 1211005:
                    case 1221003://圣灵之剑
                    case 11101002://终极剑
                    case 15111006://闪光击
                    case 1221004://圣灵之锤
                    case 1211006: // 寒冰钝器
                    case 21111005://冰雪矛
                    case 1211007:

                    case 1211008:
                    case 15101006:
                        statups.Add(Tuple.Create(MapleBuffStat.WkCharge, x));
                        break;
                    case 21120007://战神之盾
                        statups.Add(Tuple.Create(MapleBuffStat.MagicGuard, x));
                        break;
                    case 21111001://灵巧击退MAGIC_GUARD
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, x));
                        break;
                    case 21100005://连环吸血
                        statups.Add(Tuple.Create(MapleBuffStat.Infinity, x));
                        break;
                    case 21101003://抗压
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
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, (int)ret._wdef));
                        goto case 1121010;
                    case 1121010: // enrage
                        statups.Add(Tuple.Create(MapleBuffStat.Watk, (int)ret._watk));
                        break;
                    case 1301006: // iron will
                        statups.Add(Tuple.Create(MapleBuffStat.Mdef, (int)ret._mdef));
                        goto case 1001003;
                    case 1001003: // iron body
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, (int)ret._wdef));
                        break;
                    case 2001003: // magic armor
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, (int)ret._wdef));
                        break;
                    case 2101001: // meditation
                    case 2201001: // meditation
                        statups.Add(Tuple.Create(MapleBuffStat.Matk, (int)ret._matk));
                        break;
                    case 4101004: // 轻功
                    case 4201003: // 轻功
                    case 9001001: // gm轻功
                        statups.Add(Tuple.Create(MapleBuffStat.Speed, (int)ret._speed));
                        statups.Add(Tuple.Create(MapleBuffStat.Jump, (int)ret._jump));
                        break;
                    case 2301004: //祝福 
                        statups.Add(Tuple.Create(MapleBuffStat.Wdef, (int)ret._wdef));
                        statups.Add(Tuple.Create(MapleBuffStat.Mdef, (int)ret._mdef));
                        goto case 3001003;
                    case 3001003: //二连射
                        statups.Add(Tuple.Create(MapleBuffStat.Acc, (int)ret._acc));
                        statups.Add(Tuple.Create(MapleBuffStat.Avoid, (int)ret._avoid));
                        break;
                    case 9001003: //GM祝福
                        statups.Add(Tuple.Create(MapleBuffStat.Matk, (int)ret._matk));
                        goto case 9001003;
                    case 3121008: // 集中精力
                        statups.Add(Tuple.Create(MapleBuffStat.Watk, (int)ret._watk));
                        break;
                    case 5001005://疾驰
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
                        ret._hpR = -x / 100.0;
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
                    case 21110000://属性暴击
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
                    case 0000012://精灵的祝福
                    case 21120004://防守策略
                    case 21120009://(隐藏) 战神之舞- 双重重击
                    case 21120010://(隐藏) 战神之舞 - 三重重击
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
                        monsterStatus.Add(MonsterStatus.Watk, ret.X);
                        monsterStatus.Add(MonsterStatus.Wdef, ret.Y);
                        break;
                    case 1201006: // threaten
                        monsterStatus.Add(MonsterStatus.Watk, ret.X);
                        monsterStatus.Add(MonsterStatus.Wdef, ret.Y);
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
                        monsterStatus.Add(MonsterStatus.Stun, 1);
                        break;
                    case 4121003:
                    case 4221003:
                        monsterStatus.Add(MonsterStatus.Taunt, ret.X);
                        monsterStatus.Add(MonsterStatus.Mdef, ret.X);
                        monsterStatus.Add(MonsterStatus.Wdef, ret.X);
                        break;
                    case 4121004: // Ninja ambush
                    case 4221004://忍者伏击
                                 //int damage = 2 * (c.GetPlayer().GetStr() + c.GetPlayer().GetLuk()) * (ret.damage / 100);
                        monsterStatus.Add(MonsterStatus.NinjaAmbush, 1);
                        break;
                    case 2201004: // 冰冻术
                    case 20001013:
                    case 2211002: // ice strike
                    case 5221003:

                    case 3211003: // blizzard
                    case 2211006: // il elemental compo
                    case 2221007: // 落霜冰破
                    case 21120006://星辰
                    case 5211005: // Ice Splitter
                    case 2121006: // Paralyze
                        monsterStatus.Add(MonsterStatus.Freeze, 1);
                        ret._duration *= 2; // 冰冻的时间
                        break;
                    case 2121003: // fire demon
                    case 2221003: // ice demon
                        monsterStatus.Add(MonsterStatus.Poison, 1);
                        monsterStatus.Add(MonsterStatus.Freeze, 1);
                        break;
                    case 2101003: // fp slow
                    case 2201003: // il slow
                        monsterStatus.Add(MonsterStatus.Speed, ret.X);
                        break;
                    case 2101005: // poison breath
                    case 2111006: // fp elemental compo
                        monsterStatus.Add(MonsterStatus.Poison, 1);
                        break;
                    case 2311005:
                        monsterStatus.Add(MonsterStatus.Doom, 1);
                        break;
                    case 3111005: // golden hawk
                    case 3211005: // golden eagle
                    case 13111004:
                        statups.Add(Tuple.Create(MapleBuffStat.Summon, 1));
                        monsterStatus.Add(MonsterStatus.Stun, 1);
                        break;
                    case 2121005: // elquines
                    case 3221005: // frostprey
                        statups.Add(Tuple.Create(MapleBuffStat.Summon, 1));
                        monsterStatus.Add(MonsterStatus.Freeze, 1);
                        break;
                    case 2111004: // fp seal
                    case 2211004: // il seal
                    case 12111002:
                        monsterStatus.Add(MonsterStatus.Seal, 1);
                        break;
                    case 4111003: // shadow web
                    case 14111001:
                        monsterStatus.Add(MonsterStatus.ShadowWeb, 1);
                        break;
                    case 3121007: // Hamstring
                        statups.Add(Tuple.Create(MapleBuffStat.Hamstring, x));
                        monsterStatus.Add(MonsterStatus.Speed, x);
                        break;
                    case 3221006: // Blind
                        statups.Add(Tuple.Create(MapleBuffStat.Blind, x));
                        monsterStatus.Add(MonsterStatus.Acc, x);
                        break;
                    case 5221009:
                        monsterStatus.Add(MonsterStatus.Hypnotized, 1);
                        break;
                    default:
                        break;
                }
            }

            if (ret._isMorph && !ret.IsPirateMorph())
            {
                statups.Add(Tuple.Create(MapleBuffStat.Morph, ret._morphId));
            }

            ret._monsterStatus = monsterStatus;

            statups.TrimExcess();
            ret._statups = statups;

            return ret;
        }

        //public void applyPassive(MapleCharacter applyto, IMapleMapObject obj, int attack)
        //{
        //    if (makeChanceResult())
        //    {
        //        switch (sourceid)
        //        {
        //            // MP eater
        //            case 2100000:
        //            case 2200000:
        //            case 2300000:
        //                if (obj == null || obj.GetType() != MapleMapObjectType.MONSTER)
        //                {
        //                    return;
        //                }
        //                MapleMonster mob = (MapleMonster)obj;
        //                // x is absorb percentage
        //                if (!mob.IsBoss)
        //                {
        //                    int absorbMp = Math.Min((int)(mob.GetMaxMp() * (x / 100.0)), mob.GetMp());
        //                    if (absorbMp > 0)
        //                    {
        //                        mob.setMp(mob.GetMp() - absorbMp);
        //                        applyto.AddMP(absorbMp);
        //                        applyto.GetClient().GetSession().write(MaplePacketCreator.showOwnBuffEffect(sourceid, 1));
        //                        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.showBuffeffect(applyto.GetId(), sourceid, 1, (byte)3), false);
        //                    }
        //                }
        //                break;
        //        }
        //    }
        //}

        //public bool applyTo(MapleCharacter chr)
        //{
        //    return applyTo(chr, chr, true, null);
        //}

        //public bool applyTo(MapleCharacter chr, Point pos)
        //{
        //    return applyTo(chr, chr, true, pos);
        //}

        //private bool applyTo(MapleCharacter applyfrom, MapleCharacter applyto, bool primary, Point? pos)
        //{
        //    int hpchange = calcHPChange(applyfrom, primary);
        //    int mpchange = calcMPChange(applyfrom, primary);

        //    if (primary)
        //    {
        //        if (itemConNo != 0)
        //        {
        //            MapleInventoryType type = MapleItemInformationProvider.Instance.GetInventoryType(itemCon);
        //            MapleInventoryManipulator.removeById(applyto.Client, type, itemCon, itemConNo, false, true);
        //        }
        //    }
        //    if (cureDebuffs.Any())
        //    {
        //        foreach (MapleDisease debuff in cureDebuffs)
        //        {
        //            applyfrom.dispelDebuff(debuff);
        //        }
        //    }
        //    List<Tuple<MapleStat, int>> hpmpupdate = new List<Tuple<MapleStat, int>>(2);
        //    if (!primary && isResurrection())
        //    {
        //        hpchange = applyto.maxhp;
        //        applyto.Stance = 0;
        //    }
        //    if (isDispel() && makeChanceResult())
        //    {
        //        applyto.dispelDebuffs();
        //    }
        //    if (isHeroWill())
        //    {
        //        applyto.cancelAllDebuffs();
        //    }
        //    if (hpchange != 0)
        //    {
        //        if (hpchange < 0 && (-hpchange) > applyto.hp)
        //        {
        //            return false;
        //        }
        //        int newHp = applyto.hp + hpchange;
        //        if (newHp < 1)
        //        {
        //            newHp = 1;
        //        }
        //        applyto.hp = (short)newHp;
        //        hpmpupdate.Add(new Tuple<MapleStat, int>(MapleStat.HP, applyto.hp));
        //    }
        //    if (mpchange != 0)
        //    {
        //        if (mpchange < 0 && (-mpchange) > applyto.mp)
        //        {
        //            return false;
        //        }
        //        applyto.mp += (short)mpchange;
        //        hpmpupdate.Add(new Tuple<MapleStat, int>(MapleStat.MP, applyto.mp));
        //    }

        //    //applyto.GetClient().GetSession().write(MaplePacketCreator.updatePlayerStats(hpmpupdate, true));

        //    if (moveTo != -1)
        //    {
        //        MapleMap target = null;
        //        bool nearest = false;
        //        if (moveTo == 999999999)
        //        {
        //            nearest = true;
        //            if (applyto.Map.returnMapId != 999999999)
        //            {
        //                target = applyto.Map.ReturnMap;
        //            }
        //        }
        //        else {
        //            target = applyto.Client.ChannelServer.MapFactory.GetMap(moveTo);
        //            int targetMapId = target.MapID / 10000000;
        //            int charMapId = applyto.MapID / 10000000;
        //            if (targetMapId != 60 && charMapId != 61)
        //            {
        //                if (targetMapId != 21 && charMapId != 20)
        //                {
        //                    if (targetMapId != 12 && charMapId != 10)
        //                    {
        //                        if (targetMapId != 10 && charMapId != 12)
        //                        {
        //                            if (targetMapId != charMapId)
        //                            {
        //                                Console.WriteLine("人物 {0} 尝试回到一个非法的位置 ({1}->{2})", applyto.CharacterName, applyto.MapID, target.MapID);
        //                                applyto.Client.Close();
        //                                return false;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (target == applyto.Map || nearest && applyto.Map.town)
        //        {
        //            return false;
        //        }
        //    }
        //    if (isShadowClaw())
        //    {
        //        MapleInventory use = applyto.Inventorys[MapleInventoryType.USE.Value];
        //        MapleItemInformationProvider mii = MapleItemInformationProvider.Instance;
        //        int projectile = 0;
        //        for (int i = 0; i < 255; i++)
        //        { // impose order...
        //            var item = use.Inventory[(sbyte)i];
        //            if (item != null)
        //            {
        //                bool isStar = mii.IsThrowingStar(item.ItemID);
        //                if (isStar && item.Quantity >= 200)
        //                {
        //                    projectile = item.ItemID;
        //                    break;
        //                }
        //            }
        //        }
        //        if (projectile == 0)
        //        {
        //            return false;
        //        }
        //        else {
        //            MapleInventoryManipulator.removeById(applyto.Client, MapleInventoryType.USE, projectile, 200, false, true);
        //        }
        //    }
        //    if (overTime)
        //    {
        //        applyBuffEffect(applyfrom, applyto, primary);
        //    }
        //    if (primary && (overTime || isHeal()))
        //    {
        //        applyBuff(applyfrom);
        //    }
        //    if (primary && isMonsterBuff())
        //    {
        //        applyMonsterBuff(applyfrom);
        //    }

        //    SummonMovementType? summonMovementType = getSummonMovementType();
        //    if (summonMovementType.HasValue && pos != null)
        //    {
        //        MapleSummon tosummon = new MapleSummon(applyfrom, sourceid, pos.Value, summonMovementType.Value);
        //        if (!tosummon.isPuppet())
        //        {
        //            applyfrom.GetCheatTracker().resetSummonAttack();
        //        }
        //        applyfrom.Map.spawnSummon(tosummon);
        //        applyfrom.GetSummons().Add(sourceid, tosummon);
        //        tosummon.hp += x;
        //        if (isBeholder())
        //        {
        //            tosummon.hp++;
        //        }
        //    }
        //    // Magic Door
        //    if (isMagicDoor())
        //    {
        //        Point doorPosition = new Point(applyto.Position.X, applyto.Position.Y);
        //        MapleDoor door = new MapleDoor(applyto, doorPosition);
        //        applyto.GetMap().spawnDoor(door);
        //        applyto.AddDoor(door);
        //        door = new MapleDoor(door);
        //        applyto.AddDoor(door);
        //        door.GetTown().spawnDoor(door);
        //        if (applyto.GetParty() != null)
        //        {
        //            applyto.silentPartyUpdate();
        //        }
        //        applyto.disableDoor();
        //    }
        //    else if (isMist())
        //    {
        //        Rectangle bounds = calculateBoundingBox(applyfrom.GetPosition(), applyfrom.isFacingLeft());
        //        MapleMist mist = new MapleMist(bounds, applyfrom, this);
        //        applyfrom.GetMap().spawnMist(mist, getDuration(), false);
        //    }
        //    if (isTimeLeap())
        //    {
        //        for (PlayerCoolDownValueHolder i : applyto.GetAllCooldowns())
        //        {
        //            if (i.skillId != 5121010)
        //            {
        //                applyto.removeCooldown(i.skillId);
        //            }
        //        }
        //    }
        //    if (isHide())
        //    {
        //        if (applyto.isHidden())
        //        {
        //            applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.removePlayerFromMap(applyto.GetId()), false);
        //            applyto.GetClient().GetSession().write(MaplePacketCreator.giveGmHide(true));
        //        }
        //        else {
        //            applyto.GetClient().GetSession().write(MaplePacketCreator.giveGmHide(false));
        //            applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.spawnPlayerMapobject(applyto), false);
        //            for (MaplePet pet : applyto.GetPets())
        //            {
        //                if (pet != null)
        //                {
        //                    applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.showPet(applyto, pet, false, false), false);
        //                }
        //            }
        //        }
        //    }
        //    return true;
        //}

        //public bool applyReturnScroll(MapleCharacter applyto)
        //{
        //    if (moveTo != -1)
        //    {
        //        if (applyto.GetMap().GetReturnMapId() != applyto.GetMapId())
        //        {
        //            MapleMap target;
        //            if (moveTo == 999999999)
        //            {
        //                target = applyto.GetMap().GetReturnMap();
        //            }
        //            else {
        //                target = ChannelServer.GetInstance(applyto.GetClient().GetChannel()).GetMapFactory().GetMap(moveTo);
        //                if (target.GetId() / 10000000 != 60 && applyto.GetMapId() / 10000000 != 61)
        //                {
        //                    if (target.GetId() / 10000000 != 21 && applyto.GetMapId() / 10000000 != 20)
        //                    {
        //                        if (target.GetId() / 10000000 != applyto.GetMapId() / 10000000)
        //                        {
        //                            return false;
        //                        }
        //                    }
        //                }
        //            }
        //            applyto.changeMap(target, target.GetPortal(0));
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private void applyBuff(MapleCharacter applyfrom)
        //{
        //    if (isPartyBuff() && (applyfrom.GetParty() != null || isGMBuff()))
        //    {
        //        Rectangle bounds = calculateBoundingBox(applyfrom.Position, applyfrom.IsFacingLeft);
        //        List<IMapleMapObject> affecteds = applyfrom.GetMap().GetMapObjectsInRect(bounds, Arrays.asList(MapleMapObjectType.PLAYER));
        //        List<MapleCharacter> affectedp = new List<MapleCharacter>(affecteds.Count);
        //        foreach (IMapleMapObject affectedmo in affecteds)
        //        {
        //            MapleCharacter affected = (MapleCharacter)affectedmo;
        //            //this is new and weird...
        //            if (affected != null && isHeal() && affected != applyfrom && affected.GetParty() == applyfrom.GetParty() && affected.isAlive())
        //            {
        //                int expadd = (int)((calcHPChange(applyfrom, true) / 10) * (applyfrom.GetClient().GetChannelServer().GetExpRate() + ((Math.random() * 10) + 30)) * (Math.floor(Math.random() * (applyfrom.GetSkillLevel(SkillFactory.GetSkill(2301002))) / 100) * (applyfrom.GetLevel() / 30)));
        //                if (affected.GetHp() < affected.GetMaxHp() - affected.GetMaxHp() / 20)
        //                {
        //                    applyfrom.gainExp(expadd, true, false, false);
        //                }
        //            }
        //            if (affected != applyfrom && (isGMBuff() || applyfrom.GetParty().equals(affected.GetParty())))
        //            {
        //                bool isRessurection = isResurrection();
        //                if ((isRessurection && !affected.isAlive()) || (!isRessurection && affected.isAlive()))
        //                {
        //                    affectedp.Add(affected);
        //                }
        //                if (isTimeLeap())
        //                {
        //                    for (PlayerCoolDownValueHolder i : affected.GetAllCooldowns())
        //                    {
        //                        if (i.skillId != 5121010)
        //                        {
        //                            affected.removeCooldown(i.skillId);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        for (MapleCharacter affected : affectedp)
        //        {
        //            // TODO actually heal (and others) shouldn't recalculate everything
        //            // for heal this is an actual bug since heal hp is decreased with the number
        //            // of affected players
        //            applyTo(applyfrom, affected, false, null);
        //            affected.GetClient().GetSession().write(MaplePacketCreator.showOwnBuffEffect(sourceid, 2));
        //            affected.GetMap().broadcastMessage(affected, MaplePacketCreator.showBuffeffect(affected.GetId(), sourceid, 2, (byte)3), false);
        //        }
        //    }
        //}

        //private void applyMonsterBuff(MapleCharacter applyfrom)
        //{
        //    Rectangle bounds = calculateBoundingBox(applyfrom.GetPosition(), applyfrom.isFacingLeft());
        //    List<MapleMapObject> affected = applyfrom.GetMap().GetMapObjectsInRect(bounds, Arrays.asList(MapleMapObjectType.MONSTER));
        //    ISkill skill_ = SkillFactory.GetSkill(sourceid);
        //    int i = 0;
        //    for (MapleMapObject mo : affected)
        //    {
        //        MapleMonster monster = (MapleMonster)mo;
        //        if (makeChanceResult())
        //        {
        //            monster.applyStatus(applyfrom, new MonsterStatusEffect(getMonsterStati(), skill_, false), isPoison(), getDuration());
        //        }
        //        i++;
        //        if (i >= mobCount)
        //        {
        //            break;
        //        }
        //    }
        //}

        //private Rectangle calculateBoundingBox(Point posFrom, bool facingLeft)
        //{
        //    Point mylt;
        //    Point myrb;
        //    if (facingLeft)
        //    {
        //        mylt = new Point(lt.X + posFrom.X, lt.Y + posFrom.Y);
        //        myrb = new Point(rb.X + posFrom.X, rb.Y + posFrom.Y);
        //    }
        //    else {
        //        myrb = new Point(lt.X * -1 + posFrom.X, rb.Y + posFrom.Y);
        //        mylt = new Point(rb.X * -1 + posFrom.X, lt.Y + posFrom.Y);
        //    }
        //    Rectangle bounds = new Rectangle(mylt.X, mylt.Y, myrb.X - mylt.X, myrb.Y - mylt.Y);
        //    return bounds;
        //}

        //public void silentApplyBuff(MapleCharacter chr, long starttime)
        //{
        //    int localDuration = duration;
        //    localDuration = alchemistModifyVal(chr, localDuration, false);
        //    CancelEffectAction cancelAction = new CancelEffectAction(chr, this, starttime);

        //    var schedule = TimerManager.Instance.Schedule(cancelAction, starttime + localDuration - DateTime.Now.GetTimeMilliseconds());
        //    chr.registerEffect(this, starttime, schedule);

        //    SummonMovementType? summonMovementType = getSummonMovementType();
        //    if (summonMovementType.HasValue)
        //    {
        //        MapleSummon tosummon = new MapleSummon(chr, sourceid, chr.Position, summonMovementType.Value);
        //        if (!tosummon.isPuppet())
        //        {
        //            chr.GetCheatTracker().resetSummonAttack();
        //            chr.GetSummons().Add(sourceid, tosummon);
        //            tosummon.hp += x;
        //        }
        //    }
        //}

        //private void applyBuffEffect(MapleCharacter applyfrom, MapleCharacter applyto, bool primary)
        //{
        //    if (!isMonsterRiding())
        //    {
        //        applyto.cancelEffect(this, true, -1);
        //    }
        //    List<Tuple<MapleBuffStat, int>> localstatups = statups;
        //    int localDuration = duration;
        //    int localsourceid = sourceid;
        //    if (isMonsterRiding())
        //    {
        //        int ridingLevel = 0; // mount id
        //        IMapleItem mount = applyfrom.Inventorys[MapleInventoryType.EQUIPPED.Value].Inventory[-18];
        //        if (mount != null)
        //        {
        //            ridingLevel = mount.ItemID;
        //        }
        //        localDuration = sourceid;
        //        localsourceid = ridingLevel;
        //        localstatups = new List<Tuple<MapleBuffStat, int>>(1);
        //        localstatups.Add(Tuple.Create(MapleBuffStat.MONSTER_RIDING, 0));
        //    }
        //    if (isBattleShip())
        //    {
        //        int ridingLevel = 1932000;
        //        localDuration = sourceid;
        //        localsourceid = ridingLevel;
        //        localstatups = new List<Tuple<MapleBuffStat, int>>(1);
        //        localstatups.Add(Tuple.Create(MapleBuffStat.MONSTER_RIDING, 0));
        //    }
        //    if (primary)
        //    {
        //        localDuration = alchemistModifyVal(applyfrom, localDuration, false);
        //    }
        //    if (localstatups.Any())
        //    {
        //        if (isDash())
        //        {
        //            applyto.GetClient().GetSession().write(MaplePacketCreator.giveDash(statups, localDuration / 1000));
        //        }
        //        else {
        //            applyto.GetClient().GetSession().write(MaplePacketCreator.giveBuff(applyto, (skill ? localsourceid : -localsourceid), localDuration, localstatups));
        //        }
        //    }
        //    if (localstatups.Any())
        //    {
        //        if (isDash())
        //        {
        //            applyto.GetClient().GetSession().write(MaplePacketCreator.giveDash(localstatups, localDuration / 1000));
        //        }
        //        else if (isInfusion())
        //        {
        //            applyto.GetClient().GetSession().write(MaplePacketCreator.giveInfusion(localDuration / 1000, x));
        //        }
        //        else {
        //            applyto.GetClient().GetSession().write(MaplePacketCreator.giveBuff(applyto, (skill ? localsourceid : -localsourceid), localDuration, localstatups));
        //        }
        //    }
        //    if (isMonsterRiding())
        //    {
        //        int ridingLevel = 0;
        //        IMapleItem mount = applyfrom.Inventorys[MapleInventoryType.EQUIPPED.Value].Inventory[-18];
        //        if (mount != null)
        //        {
        //            ridingLevel = mount.ItemID;
        //        }
        //        List<Tuple<MapleBuffStat, int>> stat = Collections.singletonList(Tuple.Create(MapleBuffStat.MONSTER_RIDING, 1));
        //        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.showMonsterRiding(applyto.CharacterID, stat, ridingLevel, sourceid), false);
        //        localDuration = duration;
        //    }
        //    if (isBattleShip())
        //    {
        //        int ridingLevel = 1932000;
        //        List<Tuple<MapleBuffStat, int>> stat = Collections.singletonList(Tuple.Create(MapleBuffStat.MONSTER_RIDING, 1));
        //        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.showMonsterRiding(applyto.CharacterID, stat, ridingLevel, sourceid), false);
        //        localDuration = duration;
        //    }
        //    if (isDs())
        //    {
        //        List<Tuple<MapleBuffStat, int>> dsstat = Collections.singletonList(Tuple.Create(MapleBuffStat.DARKSIGHT, 0));
        //        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.giveForeignBuff(applyto, dsstat, this), false);
        //    }
        //    if (isCombo())
        //    {
        //        List<Tuple<MapleBuffStat, int>> stat = Collections.singletonList(Tuple.Create(MapleBuffStat.COMBO, 1));
        //        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.giveForeignBuff(applyto, stat, this), false);
        //    }
        //    if (isShadowPartner())
        //    {
        //        List<Tuple<MapleBuffStat, int>> stat = Collections.singletonList(Tuple.Create(MapleBuffStat.SHADOWPARTNER, 0));
        //        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.giveForeignBuff(applyto, stat, this), false);
        //    }
        //    if (isSoulArrow())
        //    {
        //        List<Tuple<MapleBuffStat, int>> stat = Collections.singletonList(Tuple.Create(MapleBuffStat.SOULARROW, 0));
        //        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.giveForeignBuff(applyto, stat, this), false);
        //    }
        //    if (isEnrage())
        //    {
        //        applyto.handleOrbconsume();
        //    }
        //    if (isMorph)
        //    {
        //        List<Tuple<MapleBuffStat, int>> stat = Collections.singletonList(Tuple.Create(MapleBuffStat.MORPH, morphId));
        //        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.giveForeignBuff(applyto, stat, this), false);
        //    }
        //    if (isPirateMorph())
        //    {
        //        List<Tuple<MapleBuffStat, int>> stat = new ArrayList<Tuple<MapleBuffStat, int>>();
        //        stat.Add(Tuple.Create(MapleBuffStat.SPEED, speed)));
        //        stat.Add(Tuple.Create(MapleBuffStat.MORPH, morphId)));
        //        applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.giveForeignBuff(applyto, stat, this), false);
        //    }
        //    if (isTimeLeap())
        //    {
        //        for (PlayerCoolDownValueHolder i : applyto.GetAllCooldowns())
        //        {
        //            if (i.skillId != 5121010)
        //            {
        //                applyto.removeCooldown(i.skillId);
        //            }
        //        }
        //    }
        //    if (localstatups.size() > 0)
        //    {
        //        long starttime = System.currentTimeMillis();
        //        CancelEffectAction cancelAction = new CancelEffectAction(applyto, this, starttime);
        //        ScheduledFuture <?> schedule = TimerManager.GetInstance().schedule(cancelAction, localDuration);
        //        applyto.registerEffect(this, starttime, schedule);
        //    }
        //    if (primary && !isHide())
        //    {
        //        if (isDash())
        //        {
        //            applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.showDashEffecttoOthers(applyto.GetId(), localstatups, localDuration / 1000), false);
        //        }
        //        else if (isInfusion())
        //        {
        //            applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.giveForeignInfusion(applyto.GetId(), x, localDuration / 1000), false);
        //        }
        //        else {
        //            applyto.GetMap().broadcastMessage(applyto, MaplePacketCreator.showBuffeffect(applyto.GetId(), sourceid, 1, (byte)3), false);
        //        }
        //    }
        //}

        private int CalcHpChange(MapleCharacter applyfrom, bool primary)
        {
            int hpchange = 0;
            if (_hp != 0)
            {
                if (!_skill)
                {
                    if (primary)
                    {
                        hpchange += AlchemistModifyVal(applyfrom, _hp, true);
                    }
                    else {
                        hpchange += _hp;
                    }
                }
                else { // assumption: this is heal
                    hpchange += MakeHealHp(_hp / 100.0, applyfrom.Magic, 3, 5);
                }
            }
            if (_hpR != 0)
            {
                hpchange += (int)(applyfrom.Localmaxhp * _hpR);
                //applyfrom.checkBerserk();
            }
            if (primary)
            {
                if (_hpCon != 0)
                {
                    hpchange -= _hpCon;
                }
            }
            if (IsChakra())
            {
                hpchange += MakeHealHp(Y / 100.0, applyfrom.Luk, 2.3, 3.5);
            }
            if (IsPirateMpRecovery())
            {
                hpchange -= (int)(Y / 100.0 * applyfrom.Localmaxhp);
            }
            return hpchange;
        }

        private int MakeHealHp(double rate, double stat, double lowerfactor, double upperfactor)
        {
            int maxHeal = (int)(stat * upperfactor * rate);
            int minHeal = (int)(stat * lowerfactor * rate);
            return (int)(Randomizer.NextDouble() * (maxHeal - minHeal + 1) + minHeal);
        }

        private int CalcMpChange(MapleCharacter applyfrom, bool primary)
        {
            int mpchange = 0;
            if (_mp != 0)
            {
                if (primary)
                {
                    mpchange += AlchemistModifyVal(applyfrom, _mp, true);
                }
                else {
                    mpchange += _mp;
                }
            }
            if (_mpR != 0)
            {
                mpchange += (int)(applyfrom.Localmaxmp * _mpR);
            }
            if (primary)
            {
                if (_mpCon != 0)
                {
                    double mod = 1.0;
                    bool isAFpMage = applyfrom.Job== MapleJob.FpMage;
                    if (isAFpMage || applyfrom.Job== MapleJob.IlMage)
                    {
                        ISkill amp;
                        if (isAFpMage)
                        {
                            amp = SkillFactory.GetSkill(2110001);
                        }
                        else {
                            amp = SkillFactory.GetSkill(2210001);
                        }
                        int ampLevel = applyfrom.getSkillLevel(amp);
                        if (ampLevel > 0)
                        {
                            MapleStatEffect ampStat = amp.GetEffect(ampLevel);
                            mod = ampStat.X / 100.0;
                        }
                    }
                    mpchange -= (int)(_mpCon * mod);
                    if (applyfrom.GetBuffedValue(MapleBuffStat.Infinity) != null)
                    {
                        mpchange = 0;
                    }
                }
            }
            if (IsPirateMpRecovery())
            {
                mpchange += (int)(Y * X / 10000.0 * applyfrom.Localmaxhp);
            }
            return mpchange;
        }

        private int AlchemistModifyVal(MapleCharacter chr, int val, bool withX)
        {
            if (!_skill && (chr.Job==MapleJob.Hermit || chr.Job==MapleJob.Nightlord))
            {
                MapleStatEffect alchemistEffect = GetAlchemistEffect(chr);
                if (alchemistEffect != null)
                {
                    return (int)(val * ((withX ? alchemistEffect.X : alchemistEffect.Y) / 100.0));
                }
            }
            return val;
        }

        private MapleStatEffect GetAlchemistEffect(MapleCharacter chr)
        {
            ISkill alchemist = SkillFactory.GetSkill(4110000);
            int alchemistLevel = chr.getSkillLevel(alchemist);
            if (alchemistLevel == 0)
            {
                return null;
            }
            return alchemist.GetEffect(alchemistLevel);
        }

        public void SetSourceId(int newid)
        {
            _sourceid = newid;
        }

        private bool IsGmBuff()
        {
            switch (_sourceid)
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
            if (!_skill)
            {
                return false;
            }
            switch (_sourceid)
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
            if ((_lt == null) || (_rb == null))
            {
                return false;
            }

            return ((_sourceid < 1211003) || (_sourceid > 1211008)) && (_sourceid != 1221003) && (_sourceid != 1221004);
        }

        public bool IsHeal()
        {
            return _sourceid == 2301002 || _sourceid == 9001000;
        }

        public bool IsResurrection()
        {
            return _sourceid == 9001005 || _sourceid == 2321006;
        }

        public bool IsTimeLeap()
        {
            return _sourceid == 5121010;
        }

        public bool IsInfusion()
        {
            return false;
        }

        public bool IsOverTime()
        {
            return _overTime;
        }

        public List<Tuple<MapleBuffStat, int>> GetStatups()
        {
            return _statups;
        }

        public bool SameSource(MapleStatEffect effect)
        {
            return _sourceid == effect._sourceid && _skill == effect._skill;
        }

        public bool IsHide()
        {
            return _skill && _sourceid == 9001004;
        }

        public bool IsDragonBlood()
        {
            return _skill && _sourceid == 1311008;
        }

        public bool IsBerserk()
        {
            return _skill && _sourceid == 1320006;
        }

        private bool IsDs()
        {
            return _skill && _sourceid == 4001003;
        }

        private bool IsCombo()
        {
            return (_skill && _sourceid == 1111002) || (_skill && _sourceid == 11111001);
        }

        private bool IsEnrage()
        {
            return _skill && _sourceid == 1121010;
        }

        public bool IsBeholder()
        {
            return _skill && _sourceid == 1321007;
        }

        private bool IsShadowPartner()
        {
            return _skill && _sourceid == 4111002;
        }

        private bool IsChakra()
        {
            return _skill && _sourceid == 4211001;
        }

        private bool IsPirateMpRecovery()
        {
            return _skill && _sourceid == 5101005;
        }

        public bool IsMonsterRiding()
        {
            return _skill && (_sourceid % 20000000 == 1004 || _sourceid == 5221006);
        }

        private bool IsBattleShip()
        {
            return _skill && _sourceid == 5221006;
        }

        /* public bool isMagicDoor() {
             return skill && sourceid == 2311002;
         }*/
        public bool IsMagicDoor()
        {
            return _skill && (_sourceid == 2311002); //时空门
        }
        public bool IsMesoGuard()
        {
            return _skill && _sourceid == 4211005;
        }

        public bool IsCharge()
        {
            return _skill && _sourceid >= 1211003 && _sourceid <= 1211008;
        }

        public bool IsPoison()
        {
            return _skill && (_sourceid == 2111003 || _sourceid == 2101005 || _sourceid == 2111006);
        }

        private bool IsMist()
        {
            return _skill && (_sourceid == 2111003 || _sourceid == 4221006); // poison mist and smokescreen
        }

        private bool IsSoulArrow()
        {
            return _skill && (_sourceid == 3101004 || _sourceid == 3201004 || _sourceid == 13101003); // bow and crossbow
        }

        private bool IsShadowClaw()
        {
            return _skill && _sourceid == 4121006;
        }

        private bool IsDispel()
        {
            return _skill && (_sourceid == 2311001 || _sourceid == 9001000);
        }

        private bool IsHeroWill()
        {
            return _skill && (_sourceid == 1121011 || _sourceid == 1221012 || _sourceid == 1321010 || _sourceid == 2121008 || _sourceid == 2221008 || _sourceid == 2321009 || _sourceid == 3121009 || _sourceid == 3221008 || _sourceid == 4121009 || _sourceid == 4221008 || _sourceid == 5121008 || _sourceid == 5221010);
        }
        public bool IsComboMove()
        {
            return _skill && ((_sourceid == 21100004) || (_sourceid == 21100005) || (_sourceid == 21110003) || (_sourceid == 21110004) || (_sourceid == 21120006) || (_sourceid == 21120007));
        }


        private bool IsDash()
        {
            return _skill && (_sourceid == 5001005);
        }

        public bool IsPirateMorph()
        {
            return _skill && (_sourceid == 5111005 || _sourceid == 5121003);
        }


        public SummonMovementType? GetSummonMovementType()
        {
            if (!_skill)
            {
                return null;
            }
            switch (_sourceid)
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
                case 11001004://魂精灵
                case 12001004://炎精灵
                case 13001004://风精灵
                case 14001005://夜精灵
                case 15001004://雷精灵
                case 12111004://火魔兽
                    return SummonMovementType.Follow;
            }
            return null;
        }

        public bool IsSkill()
        {
            return _skill;
        }

        public int GetSourceId()
        {
            return _sourceid;
        }

        public double GetIProp()
        {
            return _prop * 100;
        }

        public int GetMastery()
        {
            return _mastery;
        }
        public int GetRange()
        {
            return _range;
        }

        public int GetFixedDamage()
        {
            return _fixDamage;
        }

        public string GetBuffString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("WATK: ");
            sb.Append(_watk);
            sb.Append(", ");
            sb.Append("WDEF: ");
            sb.Append(_wdef);
            sb.Append(", ");
            sb.Append("MATK: ");
            sb.Append(_matk);
            sb.Append(", ");
            sb.Append("MDEF: ");
            sb.Append(_mdef);
            sb.Append(", ");
            sb.Append("ACC: ");
            sb.Append(_acc);
            sb.Append(", ");
            sb.Append("AVOID: ");
            sb.Append(_avoid);
            sb.Append(", ");
            sb.Append("SPEED: ");
            sb.Append(_speed);
            sb.Append(", ");
            sb.Append("JUMP: ");
            sb.Append(_jump);
            sb.Append(".");

            return sb.ToString();
        }

        /**
         * 
         * @return true if the effect should happen based on it's probablity, false otherwise
         */
        public bool MakeChanceResult()
        {
            return _prop == 1.0 || Randomizer.NextDouble() < _prop;
        }
        //     public class CancelEffectAction 
        //    {

        //    private MapleStatEffect effect;
        //    private WeakReference<MapleCharacter> target;
        //    private long startTime;

        //    public CancelEffectAction(MapleCharacter target, MapleStatEffect effect, long startTime)
        //    {
        //        this.effect = effect;
        //        this.target = new WeakReference<MapleCharacter>(target);
        //        this.startTime = startTime;
        //    }

        //    public void run()
        //    {
        //        MapleCharacter realTarget = target.Get();
        //        if (realTarget != null)
        //        {
        //            if (realTarget.inCS() || realTarget.inMTS())
        //            {
        //                realTarget.AddToCancelBuffPackets(effect, startTime);
        //                return;
        //            }
        //            realTarget.cancelEffect(effect, false, startTime);
        //        }
        //    }
        //}
    }
}

