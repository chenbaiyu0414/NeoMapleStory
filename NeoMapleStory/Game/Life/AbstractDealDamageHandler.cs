using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NeoMapleStory.Core;
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

namespace NeoMapleStory.Game.Life
{
    public class AbstractDealDamageHandler
    {
        public static AttackInfo ParseDamage(MapleCharacter c, InPacket p, bool ranged)
        {
            var ret = new AttackInfo();
            p.ReadByte();
            p.Skip(8);
            ret.NumAttackedAndDamage = p.ReadByte();
            p.Skip(8);
            ret.NumAttacked = RightMove(ret.NumAttackedAndDamage, 4) & 0xF;
            ret.NumDamage = ret.NumAttackedAndDamage & 0xF;
            ret.AllDamage = new List<Tuple<int, List<int>>>();
            ret.Skill = p.ReadInt();
            p.Skip(8);

            if ((ret.Skill == 2121001) || (ret.Skill == 2221001) || (ret.Skill == 2321001) || (ret.Skill == 5201002) ||
                (ret.Skill == 14111006) || (ret.Skill == 5101004) || (ret.Skill == 15101003))
                ret.Charge = p.ReadInt();
            else
            {
                ret.Charge = 0;
            }

            if (ret.Skill == 1221011)
                ret.IsHh = true;

            p.ReadInt();
            ret.AresCombo = p.ReadByte(); //记录目前的Combo点数
            var sourceid = ret.Skill; //以下�?能为Combo专用�?�?
            if ((sourceid == 21100004) || (sourceid == 21100005) || (sourceid == 21110003) || (sourceid == 21110004) ||
                (sourceid == 21120006) || (sourceid == 21120007))
            {
                //c.setCombo(1);
            }
            ret.Pos = p.ReadByte(); //动作
            ret.Stance = p.ReadByte(); //姿势

            if (ret.Skill == 4211006)
            {
                //return parseMesoExplosion(lea, ret);
            }

            if (ranged)
            {
                p.ReadByte();
                ret.Speed = p.ReadByte();
                p.ReadByte();
                ret.Direction = p.ReadByte();
                p.Skip(7);
                if ((ret.Skill == 3121004) || (ret.Skill == 3221001) || (ret.Skill == 5221004) ||
                    (ret.Skill == 13111002))
                    p.Skip(4);
            }
            else
            {
                p.ReadByte();
                ret.Speed = p.ReadByte();
                p.Skip(4);
            }

            for (var i = 0; i < ret.NumAttacked; ++i)
            {
                var oid = p.ReadInt();

                p.Skip(14);

                var allDamageNumbers = new List<int>();
                for (var j = 0; j < ret.NumDamage; ++j)
                {
                    var damage = p.ReadInt();

                    MapleStatEffect effect = null;
                    if (ret.Skill != 0)
                        effect =
                            SkillFactory.GetSkill(ret.Skill)
                                .GetEffect(c.GetSkillLevel(SkillFactory.GetSkill(ret.Skill)));

                    if ((damage != 0) && (effect != null) && (effect.GetFixedDamage() != 0))
                        damage = effect.GetFixedDamage();

                    allDamageNumbers.Add(damage);
                }
                if (ret.Skill != 5221004)
                    p.Skip(4);

                ret.AllDamage.Add(Tuple.Create(oid, allDamageNumbers));
            }

            return ret;
        }

        private static int RightMove(int value, int pos)
        {
            if (pos != 0) //移动 0 位时直接返回原值
            {
                var mask = 0x7fffffff; // int.MaxValue = 0x7FFFFFFF 整数最大值
                value >>= 1; //无符号整数最高位不表示正负但操作数还是有符号的，有符号数右移1位，正数时高位补0，负数时高位补1
                value &= mask; //和整数最大值进行逻辑与运算，运算后的结果为忽略表示正负值的最高位
                value >>= pos - 1; //逻辑运算后的值无符号，对无符号的值直接做右移运算，计算剩下的位
            }
            return value;
        }

        public static void ApplyAttack(AttackInfo attack, MapleCharacter player, int maxDamagePerMonster,
            int attackCount)
        {
            //应用攻击
            player.AntiCheatTracker.ResetHpRegen();
            //player.resetAfkTimer();
            player.AntiCheatTracker.CheckAttack(attack.Skill);

            ISkill theSkill = null;
            MapleStatEffect attackEffect = null;
            if (attack.Skill != 0)
            {
                theSkill = SkillFactory.GetSkill(attack.Skill);
                attackEffect = attack.GetAttackEffect(player, theSkill);
                if (attackEffect == null)
                {
                    AutobanManager.Instance.Autoban(player.Client, $"使用了没有的技能ID:{attack.Skill}");
                }
                else if (attack.Skill != 2301002)
                {
                    if (player.IsAlive)
                    {
                        attackEffect.ApplyTo(player);
                    }
                    else
                    {
                        player.Client.Send(PacketCreator.EnableActions());
                    }
                }
            }

            if (!player.IsAlive)
            {
                player.AntiCheatTracker.RegisterOffense(CheatingOffense.AttackingWhileDead);
                return;
            }

            if (attackCount != attack.NumDamage && attack.Skill != 4211006 && attack.NumDamage != attackCount*2)
            {
                player.AntiCheatTracker.RegisterOffense(CheatingOffense.MismatchingBulletcount,
                    attack.NumDamage + "/" + attackCount);
                return;
            }

            var totDamage = 0;
            var map = player.Map;

            if (attack.Skill == 4211006)
            {
                // meso explosion
                long delay = 0;
                foreach (var oned in attack.AllDamage)
                {
                    var mapobject = map.Mapobjects[oned.Item1];
                    if (mapobject != null && mapobject.GetType() == MapleMapObjectType.Item)
                    {
                        var mapitem = (MapleMapItem) mapobject;
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
                                    map.RemoveMapObject(mapitem);
                                    map.BroadcastMessage(PacketCreator.RemoveItemFromMap(mapitem.ObjectId, 4, 0),
                                        mapitem.Position);
                                    mapitem.IsPickedUp = true;
                                }, delay);
                                delay += 100;
                            }
                        }
                        else if (mapitem.Money == 0)
                        {
                            player.AntiCheatTracker.RegisterOffense(CheatingOffense.EtcExplosion);
                            return;
                        }
                    }
                    else if (mapobject != null && mapobject.GetType() != MapleMapObjectType.Monster)
                    {
                        player.AntiCheatTracker.RegisterOffense(CheatingOffense.ExplodingNonexistant);
                        return; // etc explosion, exploding nonexistant things, etc.
                    }
                }
            }

            foreach (var oned in attack.AllDamage)
            {
                var monster = map.GetMonsterByOid(oned.Item1);

                if (monster != null)
                {
                    var totDamageToOneMonster = 0;
                    foreach (var eachd in oned.Item2)
                    {
                        totDamageToOneMonster += eachd;
                    }
                    totDamage += totDamageToOneMonster;


                    player.CheckMonsterAggro(monster);
                    if (totDamageToOneMonster > attack.NumDamage + 1)
                    {
                        var dmgCheck = player.AntiCheatTracker.CheckDamage(totDamageToOneMonster);
                        if (dmgCheck > 5 && totDamageToOneMonster < 99999 && monster.Id < 9500317 &&
                            monster.Id > 9500319)
                        {
                            player.AntiCheatTracker
                                .RegisterOffense(CheatingOffense.SameDamage,
                                    dmgCheck + " times: " + totDamageToOneMonster);
                        }
                    }
                    // �?测单次攻击�?�，这里不会�?!
                    if (player.IsGm || player.Job == MapleJob.Ares && player.Level <= 10)
                    {
                        //log.info("这里不进行操�?");
                    }
                    else
                    {
                        if (player.Level < 10)
                        {
                            if (totDamageToOneMonster > 10000)
                            {
                                AutobanManager.Instance.BroadcastMessage(player.Client,
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
                                AutobanManager.Instance.BroadcastMessage(player.Client,
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
                                AutobanManager.Instance.BroadcastMessage(player.Client,
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
                                AutobanManager.Instance.BroadcastMessage(player.Client,
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
                                AutobanManager.Instance.BroadcastMessage(player.Client,
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
                                AutobanManager.Instance.BroadcastMessage(player.Client,
                                    player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " +
                                    player.Level + ")");
                                //player.ban(player.Name + " 被系统封�?.(异常攻击伤害�?: " + totDamageToOneMonster + " 当前等级 " + player.Level + " (IP: " + player.Client.getSession().getRemoteAddress().toString().split(":")[0] + ")");
                                return;
                            }
                        }
                    }

                    CheckHighDamage(player, monster, attack, theSkill, attackEffect, totDamageToOneMonster,
                        maxDamagePerMonster);
                    var distance = player.Position.DistanceSquare(monster.Position);
                    if (distance > 400000.0)
                    {
                        // 600^2, 550 is approximatly the range of ultis
                        player.AntiCheatTracker.RegisterOffense(CheatingOffense.AttackFarawayMonster,
                            Math.Sqrt(distance).ToString(CultureInfo.InvariantCulture));
                    } //遥远的�?�物袭击

                    if (attack.Skill == 5111004)
                    {
                        // 能量转换
                        var edrain = SkillFactory.GetSkill(5111004);
                        var gainhp = (int)
                            (totDamage*(double) edrain.GetEffect(player.GetSkillLevel(edrain)).X/100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp/2));
                        player.Hp += (short) gainhp;
                    }
                    else if (attack.Skill == 15100004)
                    {
                        //光�?�拳
                        var edrain = SkillFactory.GetSkill(15100004);
                        var gainhp = (int)
                            (totDamage*(double) edrain.GetEffect(player.GetSkillLevel(edrain)).X/100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp/2));
                        player.Hp += (short) gainhp;
                    }

                    if (!monster.ControllerHasAggro)
                    {
                        if (monster.GetController() == player)
                        {
                            monster.ControllerHasAggro = true;
                        }
                        else
                        {
                            monster.SwitchController(player, true);
                        }
                    }
                    if (attack.Skill == 2301002 && !monster.Stats.IsUndead)
                    {
                        player.AntiCheatTracker.RegisterOffense(CheatingOffense.HealAttackingUndead); //医治攻击亡灵
                        return;
                    }
                    // Pickpocket
                    if ((attack.Skill == 4001334 || attack.Skill == 4201005 || attack.Skill == 0 ||
                         attack.Skill == 4211002 || attack.Skill == 4211004) &&
                        player.GetBuffedValue(MapleBuffStat.Pickpocket) != null)
                    {
                        HandlePickPocket(player, monster, oned);
                    }
                    if (attack.Skill == 21100005)
                    {
                        // 生命吸收21100005
                        var drain = SkillFactory.GetSkill(21100005);
                        var gainhp =
                            (int)
                                (totDamageToOneMonster*
                                 (double) drain.GetEffect(player.GetSkillLevel(drain)).X/100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp/2));
                        player.Hp += (short) gainhp;
                    }
                    if (attack.Skill == 4101005)
                    {
                        // 生命吸收21100005
                        var drain = SkillFactory.GetSkill(4101005);
                        var gainhp =
                            (int)
                                (totDamageToOneMonster*
                                 (double) drain.GetEffect(player.GetSkillLevel(drain)).X/100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp/2));
                        player.Hp += (short) gainhp;
                    }
                    if (attack.Skill == 14101006)
                    {
                        // 吸血
                        var drain = SkillFactory.GetSkill(14101006);
                        var gainhp =
                            (int)
                                (totDamageToOneMonster*
                                 (double) drain.GetEffect(player.GetSkillLevel(drain)).X/100.0);
                        gainhp = Math.Min(monster.MaxHp, Math.Min(gainhp, player.MaxHp/2));
                        player.Hp += (short) gainhp;
                    }
                    if (player.GetBuffedValue(MapleBuffStat.Hamstring) != null)
                    {
                        var hamstring = SkillFactory.GetSkill(3121007); //降低速度的击�?�?
                        if (hamstring.GetEffect(player.GetSkillLevel(hamstring)).MakeChanceResult())
                        {
                            var monsterStatusEffect =
                                new MonsterStatusEffect(
                                    new Dictionary<MonsterStatus, int>
                                    {
                                        {
                                            MonsterStatus.Speed,
                                            hamstring.GetEffect(player.GetSkillLevel(hamstring)).X
                                        }
                                    }, hamstring, false);
                            monster.ApplyStatus(player, monsterStatusEffect, false,
                                hamstring.GetEffect(player.GetSkillLevel(hamstring)).Y*1000);
                        }
                    }

                    if (player.GetBuffedValue(MapleBuffStat.Blind) != null)
                    {
                        //刺眼�?
                        var blind = SkillFactory.GetSkill(3221006);
                        if (blind.GetEffect(player.GetSkillLevel(blind)).MakeChanceResult())
                        {
                            var monsterStatusEffect =
                                new MonsterStatusEffect(
                                    new Dictionary<MonsterStatus, int>
                                    {
                                        {
                                            MonsterStatus.Acc,
                                            blind.GetEffect(player.GetSkillLevel(blind)).X
                                        }
                                    }
                                    , blind, false);
                            monster.ApplyStatus(player, monsterStatusEffect, false,
                                blind.GetEffect(player.GetSkillLevel(blind)).Y*1000);
                        }
                    }

                    if (player.Job == MapleJob.Whiteknight)
                    {
                        int[] charges = {1211005, 1211006}; //寒冰钝器
                        foreach (var charge in charges)
                        {
                            var chargeSkill = SkillFactory.GetSkill(charge);

                            if (player.IsBuffFrom(MapleBuffStat.WkCharge, chargeSkill))
                            {
                                var iceEffectiveness = monster.GetEffectiveness(Element.Ice);
                                if (totDamageToOneMonster > 0 && iceEffectiveness == ElementalEffectiveness.Normal ||
                                    iceEffectiveness == ElementalEffectiveness.Weak)
                                {
                                    var chargeEffect =
                                        chargeSkill.GetEffect(player.GetSkillLevel(chargeSkill));
                                    var monsterStatusEffect =
                                        new MonsterStatusEffect(
                                            new Dictionary<MonsterStatus, int> {{MonsterStatus.Freeze, 1}},
                                            chargeSkill, false);
                                    monster.ApplyStatus(player, monsterStatusEffect, false, chargeEffect.Y*2000);
                                }
                                break;
                            }
                        }
                    }
                    var venomNl = SkillFactory.GetSkill(4120005); //武器用毒�?
                    if (player.GetSkillLevel(venomNl) <= 0)
                    {
                        venomNl = SkillFactory.GetSkill(14110004); //武器用毒�?
                    }
                    var venomShadower = SkillFactory.GetSkill(4220005);
                    if (player.GetSkillLevel(venomNl) > 0)
                    {
                        var venomEffect = venomNl.GetEffect(player.GetSkillLevel(venomNl));
                        for (var i = 0; i < attackCount; i++)
                        {
                            if (venomEffect.MakeChanceResult())
                            {
                                if (monster.VenomMultiplier < 3)
                                {
                                    monster.VenomMultiplier += 1;
                                    var monsterStatusEffect =
                                        new MonsterStatusEffect(
                                            new Dictionary<MonsterStatus, int> {{MonsterStatus.Poison, 1}},
                                            venomNl, false);
                                    monster.ApplyStatus(player, monsterStatusEffect, false, venomEffect.Duration,
                                        true);
                                }
                            }
                        }
                    }
                    else if (player.GetSkillLevel(venomShadower) > 0)
                    {
                        var venomEffect = venomShadower.GetEffect(player.GetSkillLevel(venomShadower));
                        for (var i = 0; i < attackCount; i++)
                        {
                            if (venomEffect.MakeChanceResult())
                            {
                                if (monster.VenomMultiplier < 3)
                                {
                                    monster.VenomMultiplier += 1;
                                    var monsterStatusEffect =
                                        new MonsterStatusEffect(
                                            new Dictionary<MonsterStatus, int> {{MonsterStatus.Poison, 1}},
                                            venomShadower, false);
                                    monster.ApplyStatus(player, monsterStatusEffect, false, venomEffect.Duration,
                                        true);
                                }
                            }
                        }
                    }
                    if (totDamageToOneMonster > 0 && attackEffect != null && attackEffect.MonsterStatus.Any())
                    {
                        if (attackEffect.MakeChanceResult())
                        {
                            var monsterStatusEffect =
                                new MonsterStatusEffect(attackEffect.MonsterStatus, theSkill, false);
                            monster.ApplyStatus(player, monsterStatusEffect, attackEffect.IsPoison(),
                                attackEffect.Duration);
                        }
                    }
                    if (attack.IsHh && !monster.IsBoss)
                    {
                        map.DamageMonster(player, monster, monster.Hp - 1);
                    }
                    else if (attack.IsHh && monster.IsBoss)
                    {
                        var weaponItem = player.Inventorys[MapleInventoryType.Equipped.Value].Inventory[0xF5]; //装备
                        MapleItemInformationProvider.Instance.GetWeaponType(weaponItem.ItemId);
                    }
                    else
                    {
                        map.DamageMonster(player, monster, totDamageToOneMonster);
                    }
                }
            }
            if (totDamage > 1)
            {
                player.AntiCheatTracker.SetAttacksWithoutHit(player.AntiCheatTracker.GetAttacksWithoutHit() + 1);
                int offenseLimit;
                if (attack.Skill != 3121004)
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
                        .RegisterOffense(CheatingOffense.AttackWithoutGettingHit,
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
            var pickpocket = SkillFactory.GetSkill(4211003);
            var delay = 0;
            var maxmeso = player.GetBuffedValue(MapleBuffStat.Pickpocket);
            var reqdamage = 20000;
            var monsterPosition = monster.Position;

            if (maxmeso == null) return;

            foreach (var eachd in oned.Item2)
            {
                if (pickpocket.GetEffect(player.GetSkillLevel(pickpocket)).MakeChanceResult())
                {
                    var perc = eachd/(double) reqdamage;
                    var todrop = Math.Min((int) Math.Max(perc*maxmeso.Value, 1), maxmeso.Value);
                    var tdmap = player.Map;
                    var tdpos = new Point((int) (monsterPosition.X + Randomizer.NextDouble()*100 - 50),
                        monsterPosition.Y);
                    var tdmob = monster;
                    var tdchar = player;
                    TimerManager.Instance.RunOnceTask(
                        () => { tdmap.SpawnMesoDrop(todrop, tdpos, tdmob, tdchar, false); }, delay);
                    delay += 1000;
                }
            }
        }

        private static void CheckHighDamage(MapleCharacter player, MapleMonster monster, AttackInfo attack,
            ISkill theSkill, MapleStatEffect attackEffect, int damageToMonster, int maximumDamageToMonster)
        {
            //检查高攻击伤害
            int elementalMaxDamagePerMonster;
            var element = Element.Neutral;
            if (theSkill != null)
            {
                element = theSkill.Element;
                var skillId = theSkill.SkillId;
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
                var chargeSkillId = player.GetBuffSource(MapleBuffStat.WkCharge);
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
                var chargeSkill = SkillFactory.GetSkill(chargeSkillId);
                maximumDamageToMonster *= (int) (chargeSkill.GetEffect(player.GetSkillLevel(chargeSkill)).Damage/100.0);
            }
            if (element != Element.Neutral)
            {
                double elementalEffect;
                if (attack.Skill == 3211003 || attack.Skill == 3111003)
                {
                    //烈火箭和寒冰�?
                    elementalEffect = attackEffect.X/200.0;
                }
                else
                {
                    elementalEffect = 0.5;
                }

                switch (monster.GetEffectiveness(element).Value)
                {
                    case 1: //immue
                        elementalMaxDamagePerMonster = 1;
                        break;
                    case 0: //normal
                        elementalMaxDamagePerMonster = maximumDamageToMonster;
                        break;
                    case 3: //weak              
                        elementalMaxDamagePerMonster = (int) (maximumDamageToMonster*(1.0 + elementalEffect));
                        break;
                    case 2: //strong
                        elementalMaxDamagePerMonster = (int) (maximumDamageToMonster*(1.0 - elementalEffect));
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
                player.AntiCheatTracker.RegisterOffense(CheatingOffense.HighDamage); //高伤�?
                if (attack.Skill != 1009 && attack.Skill != 10001009 && attack.Skill != 20001009)
                {
                    // * 3 until implementation of lagsafe pingchecks for buff expiration
                    if (damageToMonster <= elementalMaxDamagePerMonster*4) return;

                    if (player.IsGm || player.Job == MapleJob.Ares && player.Level <= 10)
                    {
                        //log.info("这里不进行操�?");
                    }
                    else
                    {
                        if (player.Level < 70)
                        {
                            AutobanManager.Instance.BroadcastMessage(player.Client,
                                $" {player.Name} 被系统封号 封号原因:伤害异常({damageToMonster}) 当前等级:{player.Level}");
                            //player.ban(player.getName() + " 被系统封�?.(异常攻击伤害�?: " + damageToMonster + " 当前等级 " + player.getLevel() + " ElementalMaxDamage: " + elementalMaxDamagePerMonster * 4 + " (IP: " + player.getClient().getSession().getRemoteAddress().toString().split(":")[0] + ")");
                        }
                    }
                }
                else
                {
                    var maxDamage = (int) Math.Floor(monster.MaxHp*0.3);
                    if (damageToMonster > 500000)
                    {
                        AutobanManager.Instance.Autoban(player.Client,
                            damageToMonster +
                            $"伤害异常 等级: {player.Level} 攻击力: {player.Watk} 技能ID: {attack.Skill} 攻击怪物ID: {monster.Id} 造成最大伤害: {maxDamage}");
                    }
                }
            }
        }

        public class AttackInfo
        {
            public List<Tuple<int, List<int>>> AllDamage;
            public bool IsHh;

            public int NumAttacked, NumDamage;
            public int Skill, Direction, Charge, AresCombo;
            public byte Speed = 4;
            public byte Stance, Pos, NumAttackedAndDamage;

            public MapleStatEffect GetAttackEffect(MapleCharacter chr, ISkill theSkill)
            {
                var mySkill = theSkill ?? SkillFactory.GetSkill(Skill);
                var skillLevel = chr.GetSkillLevel(mySkill);
                if (mySkill.SkillId == 1009 || mySkill.SkillId == 10001009)
                {
                    skillLevel = 1;
                }
                return skillLevel == 0 ? null : mySkill.GetEffect(skillLevel);
            }

            public MapleStatEffect GetAttackEffect(MapleCharacter chr)
            {
                return GetAttackEffect(chr, null);
            }
        }
    }
}