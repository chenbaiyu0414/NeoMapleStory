using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Client.AntiCheat;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Packet;
using NeoMapleStory.Core;

namespace NeoMapleStory.Game.Life
{
    public class AbstractDealDamageHandler
    {
        public class AttackInfo
        {

            public int numAttacked, numDamage;
            public int skill, direction, charge, aresCombo;
            public byte stance, pos, numAttackedAndDamage;
            public List<Tuple<int, List<int>>> allDamage;
            public bool isHH = false;
            public byte speed = 4;

            public MapleStatEffect getAttackEffect(MapleCharacter chr, ISkill theSkill)
            {
                ISkill mySkill = theSkill ?? SkillFactory.GetSkill(skill);
                int skillLevel = chr.getSkillLevel(mySkill);
                if (mySkill.SkillId == 1009 || mySkill.SkillId == 10001009)
                {
                    skillLevel = 1;
                }
                return skillLevel == 0 ? null : mySkill.GetEffect(skillLevel);
            }

            public MapleStatEffect getAttackEffect(MapleCharacter chr)
            {
                return getAttackEffect(chr, null);
            }
        }

        public static AttackInfo parseDamage(MapleCharacter c, InPacket p, bool ranged)
        {
            AttackInfo ret = new AttackInfo();
            p.ReadByte();
            p.Skip(8);
            ret.numAttackedAndDamage = p.ReadByte();
            p.Skip(8);
            ret.numAttacked = rightMove(ret.numAttackedAndDamage, 4) & 0xF;
            ret.numDamage = (ret.numAttackedAndDamage & 0xF);
            ret.allDamage = new List<Tuple<int, List<int>>>();
            ret.skill = p.ReadInt();
            p.Skip(8);

            if ((ret.skill == 2121001) || (ret.skill == 2221001) || (ret.skill == 2321001) || (ret.skill == 5201002) ||
                (ret.skill == 14111006) || (ret.skill == 5101004) || (ret.skill == 15101003))
                ret.charge = p.ReadInt();
            else
            {
                ret.charge = 0;
            }

            if (ret.skill == 1221011)
                ret.isHH = true;

            p.ReadInt();
            ret.aresCombo = p.ReadByte(); //记录目前的Combo点数
            int sourceid = ret.skill; //以下�?能为Combo专用�?�?
            if ((sourceid == 21100004) || (sourceid == 21100005) || (sourceid == 21110003) || (sourceid == 21110004) ||
                (sourceid == 21120006) || (sourceid == 21120007))
            {
                //c.setCombo(1);
            }
            ret.pos = p.ReadByte(); //动作
            ret.stance = p.ReadByte(); //姿势

            if (ret.skill == 4211006)
            {
                //return parseMesoExplosion(lea, ret);
            }

            if (ranged)
            {
                p.ReadByte();
                ret.speed = p.ReadByte();
                p.ReadByte();
                ret.direction = p.ReadByte();
                p.Skip(7);
                if ((ret.skill == 3121004) || (ret.skill == 3221001) || (ret.skill == 5221004) ||
                    (ret.skill == 13111002))
                    p.Skip(4);
            }
            else
            {
                p.ReadByte();
                ret.speed = p.ReadByte();
                p.Skip(4);
            }

            for (int i = 0; i < ret.numAttacked; ++i)
            {
                int oid = p.ReadInt();

                p.Skip(14);

                var allDamageNumbers = new List<int>();
                for (int j = 0; j < ret.numDamage; ++j)
                {
                    int damage = p.ReadInt();

                    MapleStatEffect effect = null;
                    if (ret.skill != 0)
                        effect =
                            SkillFactory.GetSkill(ret.skill)
                                .GetEffect(c.getSkillLevel(SkillFactory.GetSkill(ret.skill)));

                    if ((damage != 0) && (effect != null) && (effect.GetFixedDamage() != 0))
                        damage = effect.GetFixedDamage();

                    allDamageNumbers.Add(damage);
                }
                if (ret.skill != 5221004)
                    p.Skip(4);

                ret.allDamage.Add(Tuple.Create(oid, allDamageNumbers));
            }

            return ret;
        }

        static int rightMove(int value, int pos)
        {
            if (pos != 0) //移动 0 位时直接返回原值
            {
                int mask = 0x7fffffff; // int.MaxValue = 0x7FFFFFFF 整数最大值
                value >>= 1; //无符号整数最高位不表示正负但操作数还是有符号的，有符号数右移1位，正数时高位补0，负数时高位补1
                value &= mask; //和整数最大值进行逻辑与运算，运算后的结果为忽略表示正负值的最高位
                value >>= pos - 1; //逻辑运算后的值无符号，对无符号的值直接做右移运算，计算剩下的位
            }
            return value;
        }

        public static void applyAttack(AttackInfo attack, MapleCharacter player, int maxDamagePerMonster, int attackCount)
        {
            //应用攻击
            player.AntiCheatTracker.ResetHpRegen();
            //player.resetAfkTimer();
            player.AntiCheatTracker.CheckAttack(attack.skill);

            ISkill theSkill = null;
            MapleStatEffect attackEffect = null;
            if (attack.skill != 0)
            {
                theSkill = SkillFactory.GetSkill(attack.skill);
                attackEffect = attack.getAttackEffect(player, theSkill);
                if (attackEffect == null)
                {
                    AutobanManager.Instance.Autoban(player.Client, $"使用了没有的技能ID:{attack.skill}");
                } else if (attack.skill != 2301002)
                {
                    if (player.IsAlive)
                    {
                        attackEffect.applyTo(player);
                    }
                    else
                    {
                        player.Client.Send(PacketCreator.EnableActions());
                    }
                }
            }

            if (!player.IsAlive)
            {
                player.AntiCheatTracker.registerOffense(CheatingOffense.AttackingWhileDead);
                return;
            }

            if (attackCount != attack.numDamage && attack.skill != 4211006 && attack.numDamage != attackCount * 2)
            {
                player.AntiCheatTracker.registerOffense(CheatingOffense.MismatchingBulletcount, attack.numDamage + "/" + attackCount);
                return;
            }

            int totDamage = 0;
            MapleMap map = player.Map;

            if (attack.skill == 4211006)
            {
                // meso explosion
                long delay = 0;
                foreach (var oned in attack.allDamage)
                {
                    var mapobject = map.Mapobjects[oned.Item1];
                    if (mapobject != null && mapobject.GetType() == MapleMapObjectType.Item)
                    {
                        MapleMapItem mapitem = (MapleMapItem)mapobject;
                        if (mapitem.Money > 0)
                        {
                            lock (mapitem)
                            {
                                if (mapitem.IsPickedUp)
                                {
                                    return;
                                }
                                TimerManager.Instance.RunOnceTask(() =>
                                {
                                    map.removeMapObject(mapitem);
                                    map.BroadcastMessage(PacketCreator.RemoveItemFromMap(mapitem.ObjectId, 4, 0), mapitem.Position);
                                    mapitem.IsPickedUp = (true);
                                }, delay);
                                delay += 100;
                            }
                        }
                        else if (mapitem.Money == 0)
                        {
                            player.AntiCheatTracker.registerOffense(CheatingOffense.EtcExplosion);
                            return;
                        }
                    }
                    else if (mapobject != null && mapobject.GetType() != MapleMapObjectType.Monster)
                    {
                        player.AntiCheatTracker.registerOffense(CheatingOffense.ExplodingNonexistant);
                        return; // etc explosion, exploding nonexistant things, etc.
                    }
                }
            }

            foreach (var oned in attack.allDamage)
            {
                MapleMonster monster = map.getMonsterByOid(oned.Item1);

                if (monster != null)
                {
                    int totDamageToOneMonster = 0;
                    foreach (var eachd in oned.Item2)
                    {
                        totDamageToOneMonster += eachd;
                    }
                    totDamage += totDamageToOneMonster;


                    player.checkMonsterAggro(monster);
                    if (totDamageToOneMonster > attack.numDamage + 1)
                    {
                        int dmgCheck = player.AntiCheatTracker.CheckDamage(totDamageToOneMonster);
                        if (dmgCheck > 5 && totDamageToOneMonster < 99999 && monster.Id < 9500317 && monster.Id > 9500319)
                        {
                            player.AntiCheatTracker
                                .registerOffense(CheatingOffense.SameDamage,
                                    dmgCheck + " times: " + totDamageToOneMonster);
                        }
                    }
                    // �?测单次攻击�?�，这里不会�?!
                    if (player.IsGm || player.Job == (MapleJob.Ares) && player.Level <= 10)
                    {
                        //log.info("这里不进行操�?");
                    }
                    else
                    {
                        if (player.Level < 10)
                        {
                            if (totDamageToOneMonster > 10000)
                            {
                                AutobanManager.Instance.broadcastMessage(player.Client,
                                    player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " +
                                    player.Level + ")");
                                //player.ban(player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " + player.Level + " (IP: " + player.Client.getSession().getRemoteAddress().toString().split(":")[0] + ")");
                                return;
                            }
                        }
                        if (player.Level < 20)
                        {
                            if (totDamageToOneMonster > 20000)
                            {
                                AutobanManager.Instance.broadcastMessage(player.Client,
                                    player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " +
                                    player.Level + ")");
                                //player.ban(player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " + player.Level + " (IP: " + player.Client.getSession().getRemoteAddress().toString().split(":")[0] + ")");
                                return;
                            }
                        }
                        if (player.Level < 30)
                        {
                            if (totDamageToOneMonster > 30000)
                            {
                                AutobanManager.Instance.broadcastMessage(player.Client,
                                    player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " +
                                    player.Level + ")");
                                //player.ban(player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " + player.Level + " (IP: " + player.Client.getSession().getRemoteAddress().toString().split(":")[0] + ")");
                                return;
                            }
                        }
                        if (player.Level < 50)
                        {
                            if (totDamageToOneMonster > 50000)
                            {
                                AutobanManager.Instance.broadcastMessage(player.Client,
                                    player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " +
                                    player.Level + ")");
                                //player.ban(player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " + player.Level + " (IP: " + player.Client.getSession().getRemoteAddress().toString().split(":")[0] + ")");
                                return;
                            }
                        }
                        if (player.Level < 70)
                        {
                            if (totDamageToOneMonster > 150000)
                            {
                                AutobanManager.Instance.broadcastMessage(player.Client,
                                    player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " +
                                    player.Level + ")");
                                //player.ban(player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " + player.Level + " (IP: " + player.Client.getSession().getRemoteAddress().toString().split(":")[0] + ")");
                                return;
                            }
                        }
                        if (player.Level < 150)
                        {
                            if (totDamageToOneMonster > 350000)
                            {
                                AutobanManager.Instance.broadcastMessage(player.Client,
                                    player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " +
                                    player.Level + ")");
                                //player.ban(player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " + player.Level + " (IP: " + player.Client.getSession().getRemoteAddress().toString().split(":")[0] + ")");
                                return;
                            }
                        }
                    }

                    CheckHighDamage(player, monster, attack, theSkill, attackEffect, totDamageToOneMonster,
                        maxDamagePerMonster);
                    double distance = player.Position.DistanceSquare(monster.Position);
                    if (distance > 400000.0)
                    {
                        // 600^2, 550 is approximatly the range of ultis
                        player.AntiCheatTracker.registerOffense(CheatingOffense.AttackFarawayMonster,
                            Math.Sqrt(distance).ToString(CultureInfo.InvariantCulture));
                    } //遥远的�?�物袭击

                    if (attack.skill == 5111004)
                    {
                        // 能量转换
                        ISkill edrain = SkillFactory.GetSkill(5111004);
                        var gainhp = (int)
                            (totDamage * (double)edrain.GetEffect(player.getSkillLevel(edrain)).X / 100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp / 2));
                        player.Hp += (short)gainhp;
                    }
                    else if (attack.skill == 15100004)
                    {
                        //光�?�拳
                        ISkill edrain = SkillFactory.GetSkill(15100004);
                        var gainhp = (int)
                            (totDamage * (double)edrain.GetEffect(player.getSkillLevel(edrain)).X / 100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp / 2));
                        player.Hp += (short)gainhp;
                    }

                    if (!monster.ControllerHasAggro)
                    {
                        if (monster.GetController() == player)
                        {
                            monster.ControllerHasAggro = (true);
                        }
                        else
                        {
                            monster.switchController(player, true);
                        }
                    }
                    if (attack.skill == 2301002 && !monster.Stats.IsUndead)
                    {
                        player.AntiCheatTracker.registerOffense(CheatingOffense.HealAttackingUndead); //医治攻击亡灵
                        return;
                    }
                    // Pickpocket
                    if ((attack.skill == 4001334 || attack.skill == 4201005 || attack.skill == 0 ||
                         attack.skill == 4211002 || attack.skill == 4211004) &&
                        player.GetBuffedValue(MapleBuffStat.Pickpocket) != null)
                    {
                        HandlePickPocket(player, monster, oned);
                    }
                    if (attack.skill == 21100005)
                    {
                        // 生命吸收21100005
                        ISkill drain = SkillFactory.GetSkill(21100005);
                        int gainhp =
                            (int)
                                (totDamageToOneMonster *
                                 (double)drain.GetEffect(player.getSkillLevel(drain)).X / 100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp / 2));
                        player.Hp += (short)(gainhp);
                    }
                    if (attack.skill == 4101005)
                    {
                        // 生命吸收21100005
                        ISkill drain = SkillFactory.GetSkill(4101005);
                        int gainhp =
                            (int)
                                (totDamageToOneMonster *
                                 (double)drain.GetEffect(player.getSkillLevel(drain)).X / 100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp / 2));
                        player.Hp += (short)(gainhp);
                    }
                    if (attack.skill == 14101006)
                    {
                        // 吸血
                        ISkill drain = SkillFactory.GetSkill(14101006);
                        int gainhp =
                            (int)
                                (totDamageToOneMonster *
                                 (double)drain.GetEffect(player.getSkillLevel(drain)).X / 100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp / 2));
                        player.Hp += (short)(gainhp);
                    }
                    if (player.GetBuffedValue(MapleBuffStat.Hamstring) != null)
                    {
                        ISkill hamstring = SkillFactory.GetSkill(3121007); //降低速度的击�?�?
                        if (hamstring.GetEffect(player.getSkillLevel(hamstring)).MakeChanceResult())
                        {
                            MonsterStatusEffect monsterStatusEffect =
                                new MonsterStatusEffect(
                                    new Dictionary<MonsterStatus, int>
                                    {
                                        {
                                            MonsterStatus.Speed,
                                            hamstring.GetEffect(player.getSkillLevel(hamstring)).X
                                        }
                                    }, hamstring, false);
                            monster.applyStatus(player, monsterStatusEffect, false,
                                hamstring.GetEffect(player.getSkillLevel(hamstring)).Y * 1000);
                        }
                    }

                    if (player.GetBuffedValue(MapleBuffStat.Blind) != null)
                    {
                        //刺眼�?
                        ISkill blind = SkillFactory.GetSkill(3221006);
                        if (blind.GetEffect(player.getSkillLevel(blind)).MakeChanceResult())
                        {
                            MonsterStatusEffect monsterStatusEffect =
                                new MonsterStatusEffect(
                                    new Dictionary<MonsterStatus, int>
                                    {
                                        {
                                            MonsterStatus.Acc,
                                            blind.GetEffect(player.getSkillLevel(blind)).X
                                        }
                                    }
                                    , blind, false);
                            monster.applyStatus(player, monsterStatusEffect, false,
                                blind.GetEffect(player.getSkillLevel(blind)).Y * 1000);
                        }
                    }

                    if (player.Job == (MapleJob.Whiteknight))
                    {
                        int[] charges = { 1211005, 1211006 }; //寒冰钝器
                        foreach (int charge in charges)
                        {
                            ISkill chargeSkill = SkillFactory.GetSkill(charge);

                            if (player.isBuffFrom(MapleBuffStat.WkCharge, chargeSkill))
                            {
                                ElementalEffectiveness iceEffectiveness = monster.getEffectiveness(Element.Ice);
                                if (totDamageToOneMonster > 0 && iceEffectiveness == ElementalEffectiveness.Normal ||
                                    iceEffectiveness == ElementalEffectiveness.Weak)
                                {
                                    MapleStatEffect chargeEffect =
                                        chargeSkill.GetEffect(player.getSkillLevel(chargeSkill));
                                    MonsterStatusEffect monsterStatusEffect =
                                        new MonsterStatusEffect(
                                            new Dictionary<MonsterStatus, int> { { MonsterStatus.Freeze, 1 } },
                                            chargeSkill, false);
                                    monster.applyStatus(player, monsterStatusEffect, false, chargeEffect.Y * 2000);
                                }
                                break;
                            }
                        }
                    }
                    ISkill venomNl = SkillFactory.GetSkill(4120005); //武器用毒�?
                    if (player.getSkillLevel(venomNl) <= 0)
                    {
                        venomNl = SkillFactory.GetSkill(14110004); //武器用毒�?
                    }
                    ISkill venomShadower = SkillFactory.GetSkill(4220005);
                    if (player.getSkillLevel(venomNl) > 0)
                    {
                        MapleStatEffect venomEffect = venomNl.GetEffect(player.getSkillLevel(venomNl));
                        for (int i = 0; i < attackCount; i++)
                        {
                            if (venomEffect.MakeChanceResult())
                            {
                                if (monster.VenomMultiplier < 3)
                                {
                                    monster.VenomMultiplier += 1;
                                    MonsterStatusEffect monsterStatusEffect =
                                        new MonsterStatusEffect(
                                            new Dictionary<MonsterStatus, int> { { MonsterStatus.Poison, 1 } },
                                            venomNl, false);
                                    monster.applyStatus(player, monsterStatusEffect, false, venomEffect._duration,
                                        true);
                                }
                            }
                        }
                    }
                    else if (player.getSkillLevel(venomShadower) > 0)
                    {
                        MapleStatEffect venomEffect = venomShadower.GetEffect(player.getSkillLevel(venomShadower));
                        for (int i = 0; i < attackCount; i++)
                        {
                            if (venomEffect.MakeChanceResult())
                            {
                                if (monster.VenomMultiplier < 3)
                                {
                                    monster.VenomMultiplier += 1;
                                    MonsterStatusEffect monsterStatusEffect =
                                        new MonsterStatusEffect(new Dictionary<MonsterStatus, int> { { MonsterStatus.Poison, 1 } },
                                            venomShadower, false);
                                    monster.applyStatus(player, monsterStatusEffect, false, venomEffect._duration,
                                        true);
                                }
                            }
                        }
                    }
                    if (totDamageToOneMonster > 0 && attackEffect != null && attackEffect._monsterStatus.Any())
                    {
                        if (attackEffect.MakeChanceResult())
                        {
                            MonsterStatusEffect monsterStatusEffect =
                                new MonsterStatusEffect(attackEffect._monsterStatus, theSkill, false);
                            monster.applyStatus(player, monsterStatusEffect, attackEffect.IsPoison(),
                                attackEffect._duration);
                        }
                    }
                    if (attack.isHH && !monster.IsBoss)
                    {
                        map.damageMonster(player, monster, monster.Hp - 1);
                    }
                    else if (attack.isHH && monster.IsBoss)
                    {                       
                        var weaponItem = player.Inventorys[MapleInventoryType.Equipped.Value].Inventory[11]; //装备
                        MapleItemInformationProvider.Instance.GetWeaponType(weaponItem.ItemId);
                    }
                    else
                    {
                        map.damageMonster(player, monster, totDamageToOneMonster);
                    }
                }
            }
            if (totDamage > 1)
            {
                player.AntiCheatTracker.SetAttacksWithoutHit(player.AntiCheatTracker.GetAttacksWithoutHit() + 1);
                int offenseLimit;
                if (attack.skill != 3121004)
                {
                    //暴风箭雨
                    offenseLimit = 100;
                }
                else
                {
                    offenseLimit = 300;
                }
                if (player.AntiCheatTracker.GetAttacksWithoutHit() > offenseLimit)
                {
                    player.AntiCheatTracker
                        .registerOffense(CheatingOffense.AttackWithoutGettingHit,
                            player.AntiCheatTracker.GetAttacksWithoutHit().ToString());
                }
                //没有受到撞击攻击
                //if (player.hasEnergyCharge())
                //{
                //    player.increaseEnergyCharge(attack.numAttacked);
                //}
            }
        }

        private static void HandlePickPocket(MapleCharacter player, MapleMonster monster, Tuple<int, List<int>> oned)
        {
            //金钱炸弹
            ISkill pickpocket = SkillFactory.GetSkill(4211003);
            int delay = 0;
            int? maxmeso = player.GetBuffedValue(MapleBuffStat.Pickpocket);
            int reqdamage = 20000;
            Point monsterPosition = monster.Position;

            if (maxmeso == null) return;

            foreach (int eachd in oned.Item2)
            {
                if (pickpocket.GetEffect(player.getSkillLevel(pickpocket)).MakeChanceResult())
                {
                    double perc = eachd / (double)reqdamage;
                    int todrop = Math.Min((int)Math.Max(perc * maxmeso.Value, 1), maxmeso.Value);
                    MapleMap tdmap = player.Map;
                    Point tdpos = new Point((int)(monsterPosition.X + Randomizer.NextDouble() * 100 - 50), monsterPosition.Y);
                    MapleMonster tdmob = monster;
                    MapleCharacter tdchar = player;
                    TimerManager.Instance.RunOnceTask(() =>
                    {
                        tdmap.spawnMesoDrop(todrop, tdpos, tdmob, tdchar, false);
                    }, delay);
                    delay += 1000;
                }
            }
        }

        private static void CheckHighDamage(MapleCharacter player, MapleMonster monster, AttackInfo attack, ISkill theSkill, MapleStatEffect attackEffect, int damageToMonster, int maximumDamageToMonster)
        {
            //检查高攻击伤害
            int elementalMaxDamagePerMonster;
            Element element = Element.Neutral;
            if (theSkill != null)
            {
                element = theSkill.Element;
                int skillId = theSkill.SkillId;
                if (skillId == 3221007)
                {
                    maximumDamageToMonster = 99999;
                }
                else if (skillId == 4221001)
                {
                    maximumDamageToMonster = 400000;
                }
            }
            if (player.GetBuffedValue(MapleBuffStat.WkCharge) != null)
            {
                int chargeSkillId = player.getBuffSource(MapleBuffStat.WkCharge);
                switch (chargeSkillId)
                {
                    case 1211003:
                    case 1211004:
                        element = Element.Fire;
                        break;
                    case 1211005:
                    case 1211006:
                        element = Element.Ice;
                        break;
                    case 1211007:
                    case 1211008:
                        element = Element.Lighting;
                        break;
                    case 1221003:
                    case 1221004:
                        element = Element.Holy;
                        break;
                }
                ISkill chargeSkill = SkillFactory.GetSkill(chargeSkillId);
                maximumDamageToMonster *= (int)(chargeSkill.GetEffect(player.getSkillLevel(chargeSkill))._damage / 100.0);
            }
            if (element != Element.Neutral)
            {
                double elementalEffect;
                if (attack.skill == 3211003 || attack.skill == 3111003)
                {
                    //烈火箭和寒冰�?
                    elementalEffect = attackEffect.X / 200.0;
                }
                else
                {
                    elementalEffect = 0.5;
                }

                switch (monster.getEffectiveness(element).Value)
                {
                    case 1: //immue
                        elementalMaxDamagePerMonster = 1;
                        break;
                    case 0: //normal
                        elementalMaxDamagePerMonster = maximumDamageToMonster;
                        break;
                    case 3: //weak              
                        elementalMaxDamagePerMonster = (int)(maximumDamageToMonster * (1.0 + elementalEffect));
                        break;
                    case 2: //strong
                        elementalMaxDamagePerMonster = (int)(maximumDamageToMonster * (1.0 - elementalEffect));
                        break;
                    default:
                        throw new Exception("Effectiveness不正确");
                }
            }
            else
            {
                elementalMaxDamagePerMonster = maximumDamageToMonster;
            }

            if (damageToMonster > elementalMaxDamagePerMonster)
            {
                player.AntiCheatTracker.registerOffense(CheatingOffense.HighDamage);//高伤�?
                if (attack.skill != 1009 && attack.skill != 10001009 && attack.skill != 20001009)
                {
                    // * 3 until implementation of lagsafe pingchecks for buff expiration
                    if (damageToMonster <= elementalMaxDamagePerMonster * 4) return;

                    if (player.IsGm || player.Job == MapleJob.Ares && player.Level <= 10)
                    {
                        //log.info("这里不进行操�?");
                    }
                    else
                    {
                        if (player.Level < 70)
                        {
                            AutobanManager.Instance.broadcastMessage(player.Client,
                                $" {player.Name} 被系统封号 封号原因:伤害异常({damageToMonster}) 当前等级:{player.Level}");
                            //player.ban(player.getName() + " 被系统封�?.(异常攻击伤害�?: " + damageToMonster + " 当前等级 " + player.getLevel() + " ElementalMaxDamage: " + elementalMaxDamagePerMonster * 4 + " (IP: " + player.getClient().getSession().getRemoteAddress().toString().split(":")[0] + ")");
                        }
                    }
                }
                else
                {
                    int maxDamage = (int)Math.Floor(monster.MaxHp * 0.3);
                    if (damageToMonster > 500000)
                    {
                        AutobanManager.Instance.Autoban(player.Client, damageToMonster + $"伤害异常 等级: { player.Level } 攻击力: { player.Watk } 技能ID: { attack.skill } 攻击怪物ID: { monster.Id } 造成最大伤害: { maxDamage }");
                    }
                }
            }
        }
    }
}
