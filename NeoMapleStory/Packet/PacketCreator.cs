using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Movement;
using NeoMapleStory.Game.Quest;
using NeoMapleStory.Game.Shop;
using NeoMapleStory.Server;
using NeoMapleStory.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;

namespace NeoMapleStory.Packet
{
    public static class PacketCreator
    {
        private static readonly byte[] CharInfoMagic = { 0xFF, 0xC9, 0x9A, 0x3B };
        private static readonly List<Tuple<MapleStat, int>> EmptyStatupdate = new List<Tuple<MapleStat, int>>();
        private static readonly long FinalTime = 3439785600000L;

        public static byte[] Handshake(byte[] sendIv, byte[] recvIv)
        {
            using (OutPacket p = new OutPacket())
            {
                p.WriteShort(0x0D);
                p.WriteShort(ServerSettings.MapleVersion);
                p.WriteZero(2);
                p.WriteBytes(recvIv);
                p.WriteBytes(sendIv);
                p.WriteByte(ServerSettings.MapleLocale);
                return p.ToArray();
            }
        }

        public static void AddInventoryInfo(OutPacket p, MapleCharacter chr)
        {
            p.WriteByte(0x01);
            p.WriteMapleString(chr.Name);
            p.WriteInt(chr.Money.Value); //冒险币
            p.WriteInt(chr.Id);
            p.WriteLong(0);//豆豆
            p.WriteByte(chr.Inventorys[MapleInventoryType.Equip.Value].SlotLimit); // equip slots
            p.WriteByte(chr.Inventorys[MapleInventoryType.Use.Value].SlotLimit); // use slots
            p.WriteByte(chr.Inventorys[MapleInventoryType.Setup.Value].SlotLimit); // set-up slots
            p.WriteByte(chr.Inventorys[MapleInventoryType.Etc.Value].SlotLimit); // etc slots
            p.WriteByte(chr.Inventorys[MapleInventoryType.Cash.Value].SlotLimit); // cash slots
            p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));

            MapleInventory iv = chr.Inventorys[MapleInventoryType.Equipped.Value];
            var equippedC = iv.Inventory.Values;
            List<IMapleItem> equipped = new List<IMapleItem>(equippedC.Count);
            lock (iv)
            {
                equipped.AddRange(equippedC.Where(item => item.Position < 100));
            }

            equipped.Sort();

            foreach (var item in equipped)
            {
                AddItemInfo(p, item);
            }

            p.WriteByte(0x00); // start of equiped cash inventory

            equipped.Clear();

            lock (iv)
            {
                equipped.AddRange(equippedC.Where(item => item.Position > 100));
            }
            equipped.Sort();

            foreach (var item in equipped)
            {
                AddItemInfo(p, item);
            }

            p.WriteByte(0x00); // start of equip inventory
            iv = chr.Inventorys[MapleInventoryType.Equip.Value];
            lock (iv)
            {
                foreach (var item in iv.Inventory.Values)
                {
                    AddItemInfo(p, item);
                }
            }

            p.WriteByte(0x00); // start of use inventory                       
            iv = chr.Inventorys[MapleInventoryType.Use.Value];
            lock (iv)
            {
                foreach (var item in iv.Inventory.Values)
                {
                    AddItemInfo(p, item);
                }
            }

            p.WriteByte(0x00); // start of set-up inventory
            iv = chr.Inventorys[MapleInventoryType.Setup.Value];
            lock (iv)
            {
                foreach (var item in iv.Inventory.Values)
                {
                    AddItemInfo(p, item);
                }
            }

            p.WriteByte(0x00); // start of etc inventory
            iv = chr.Inventorys[MapleInventoryType.Etc.Value];
            lock (iv)
            {
                foreach (var item in iv.Inventory.Values)
                {
                    AddItemInfo(p, item);
                }
            }

            p.WriteByte(0x00); // start of cash inventory
            iv = chr.Inventorys[MapleInventoryType.Cash.Value];
            lock (iv)
            {
                foreach (var item in iv.Inventory.Values)
                {
                    AddItemInfo(p, item);
                }
            }
        }

        public static void AddSkillRecord(OutPacket p, MapleCharacter chr)
        {
            p.WriteByte(0x00); // start of skills
            p.WriteShort((short)chr.Skills.Count);

            foreach (var skill in chr.Skills)
            {
                p.WriteInt(skill.Key.SkillId);
                p.WriteInt(skill.Value.SkilLevel);
                if (skill.Key.IsFourthJob)
                {
                    p.WriteInt(skill.Value.MasterLevel);
                }
            }
            p.WriteShort((short)chr.GetAllCooldowns().Count);
            foreach (var cooling in chr.GetAllCooldowns())
            {
                p.WriteInt(cooling.SkillId);
                int timeLeft = (int)(cooling.Duration + cooling.StartTime - DateTime.Now.GetTimeMilliseconds());
                p.WriteShort((short)(timeLeft / 1000));
            }
        }

        public static void AddQuestRecord(OutPacket p, MapleCharacter chr)
        {
            List<MapleQuestStatus> started = chr.GetStartedQuests();
            p.WriteShort((short)started.Count);
            foreach (var questStatus in started)
            {
                p.WriteShort((short)questStatus.Quest.GetQuestId());
                StringBuilder killStr = new StringBuilder();
                foreach (int kills in questStatus.GetMobKills().Values)
                {
                    killStr.Append(kills.ToString().PadLeft(3, '0'));
                }
                p.WriteMapleString(killStr.ToString());
            }
            List<MapleQuestStatus> completed = chr.GetCompletedQuests();
            p.WriteShort((short)completed.Count);
            foreach (var questStatus in completed)
            {
                p.WriteShort((short)questStatus.Quest.GetQuestId());
                p.WriteLong(DateUtiliy.GetFileTimestamp(questStatus.CompletionTime));
            }
        }

        public static void AddRingInfo(OutPacket p, MapleCharacter chr)
        {
            MapleInventory iv = chr.Inventorys[MapleInventoryType.Equipped.Value];
            List<Item> equipped = new List<Item>(iv.Inventory.Values.Count);
            foreach (var item in iv.Inventory.Values)
            {
                equipped.Add((Item)item);
            }
            equipped.Sort();

            List<IEquip> rings = new List<IEquip>();
            foreach (var item in equipped)
            {
                if (item.ItemId >= 1112800 && item.ItemId <= 1112802 || item.ItemId >= 1112001 && item.ItemId <= 1112003)
                {
                    rings.Add(MapleRing.LoadFromDb(item.ItemId, item.Position, item.UniqueId));
                }
            }

            iv = chr.Inventorys[MapleInventoryType.Equip.Value];
            foreach (var item in iv.Inventory.Values)
            {
                if (item.ItemId >= 1112800 && item.ItemId <= 1112802 || item.ItemId >= 1112001 && item.ItemId <= 1112003)
                {
                    rings.Add(MapleRing.LoadFromDb(item.ItemId, item.Position, item.UniqueId));
                }
            }
            rings.Sort();

            bool frLast = false;
            foreach (var ring in rings)
            {
                if ((ring.ItemId >= 1112800 && ring.ItemId <= 1112802 || ring.ItemId >= 1112001 && ring.ItemId <= 1112003 || ring.ItemId <= 1112804) && rings.IndexOf(ring) == 0)
                {
                    p.WriteShort(0);
                }
                p.WriteShort(0);
                p.WriteShort(1);
                p.WriteInt(ring.PartnerId);
                p.WriteString(ring.PartnerName.PadRight(13, '\0'));
                p.WriteInt(ring.UniqueId);
                p.WriteInt(0);
                p.WriteInt(ring.PartnerUniqueId);
                if (ring.ItemId >= 1112800 && ring.ItemId <= 1112802 || ring.ItemId >= 1112001 && ring.ItemId <= 1112003 || ring.ItemId <= 1112804)
                {
                    //1112804 结婚戒指
                    frLast = true;
                    p.WriteInt(0);
                    p.WriteInt(ring.ItemId);
                    p.WriteShort(0);
                }
                else
                {
                    if (rings.Count > 1)
                    {
                        p.WriteShort(0);
                    }
                    frLast = false;
                }
            }
            if (!frLast)
            {
                p.WriteShort(0);// addMiniGameRecordInfo(mplew, chr); //short amount, int int int int int
                p.WriteShort(0);// addCoupleRecordInfo(mplew, chr); //short amount, foreach amount, encode (0x21 bytes)
                p.WriteShort(0);// addFriendRecordInfo(mplew, chr); //short amount, foreach amount, encode (0x25 bytes)
                p.WriteShort(0);// addMariageRecordInfo(mplew, chr); //short amount, foreach amount, encode (0x30 bytes)
            }
        }

        public static void AddTeleportRockRecord(OutPacket mplew, MapleCharacter chr)
        {
            List<int> maps = chr.GetTRockMaps(0);
            foreach (var map in maps)
            {
                mplew.WriteInt(map);
            }
            for (int i = maps.Count; i < 5; i++)
            {
                mplew.WriteBytes(CharInfoMagic);
            }

            maps = chr.GetTRockMaps(1);
            foreach (var map in maps)
            {
                mplew.WriteInt(map);
            }
            for (int i = maps.Count; i < 10; i++)
            {
                mplew.WriteBytes(CharInfoMagic);
            }
        }

        public static OutPacket SendAutoHpPot(int itemId)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.AutoHpPot))
            {
                p.WriteInt(itemId);
                return p;
            }
        }

        public static OutPacket SendAutoMpPot(int itemId)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.AutoMpPot))
            {
                p.WriteInt(itemId);
                return p;
            }
        }

        public static OutPacket ShowCharCash(MapleCharacter chr)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.CharCash))
            {
                p.WriteInt(chr.Id);
                p.WriteInt(chr.NexonPoint);
                //p.WriteInt(chr.getCSPoints(1));
                return p;
            }
        }

        public static OutPacket WeirdStatUpdate()
        {
            using (OutPacket p = new OutPacket(SendOpcodes.UpdateStats))
            {
                p.WriteByte(0x00);
                p.WriteByte(0x38);
                p.WriteShort(0);
                p.WriteLong(0);
                p.WriteLong(0);
                p.WriteLong(0);
                p.WriteByte(0x00);
                p.WriteByte(0x01);
                return p;
            }
        }

        public static OutPacket DamagePlayer(byte skill, int monsteridfrom, int cid, int damage, int fake, byte direction, bool pgmr, byte pgmr1, bool isPg, int oid, short posX, short posY)
        {
            // 82 00 30 C0 23 00 FF 00 00 00 00 B4 34 03 00 01 00 00 00 00 00 00
            using (var p = new OutPacket(SendOpcodes.DamagePlayer))
            {
                // mplew.writeShort(0x84); // 47 82
                p.WriteInt(cid);
                p.WriteByte(skill);
                p.WriteInt(damage);
                p.WriteInt(monsteridfrom);
                p.WriteByte(direction);
                if (pgmr)
                {
                    p.WriteByte(pgmr1);
                    p.WriteBool(isPg);
                    p.WriteInt(oid);
                    p.WriteByte(6);
                    p.WriteShort(posX);
                    p.WriteShort(posY);
                    p.WriteByte(0);
                }
                else
                {
                    p.WriteShort(0);
                }
                p.WriteInt(damage);
                if (fake > 0)
                {
                    p.WriteInt(fake);
                }
                return p;
            }
        }

        public static OutPacket MusicChange(string song)
        {
            return EnvironmentChange(song, 6);
        }

        public static OutPacket ShowEffect(string effect)
        {
            return EnvironmentChange(effect, 3);
        }

        public static OutPacket PlaySound(string sound)
        {
            return EnvironmentChange(sound, 4);
        }

        public static OutPacket EnvironmentChange(string env, byte mode)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.BossEnv))
            {
                p.WriteByte(mode);
                p.WriteMapleString(env);
                return p;
            }
        }

        public static OutPacket SkillCooldown(int sid, int time)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.Cooldown))
            {
                p.WriteInt(sid);
                p.WriteShort((short)time);
                return p;
            }
        }

        public static OutPacket StartMapEffect(string msg, int itemid, bool active)
        {

            using (OutPacket p = new OutPacket(SendOpcodes.MapEffect))
            {
                p.WriteBool(!active);
                p.WriteInt(itemid);
                if (active)
                {
                    p.WriteMapleString(msg);
                }
                return p;
            }
        }

        public static OutPacket RemoveMapEffect()
        {
            using (OutPacket p = new OutPacket(SendOpcodes.MapEffect))
            {
                p.WriteByte(0x00);
                p.WriteInt(0);
                return p;
            }
        }

        public static OutPacket ShowLevelup(int cid)
        {
            return ShowForeignEffect(cid, 0);
        }

        public static OutPacket ShowJobChange(int cid)
        {
            return ShowForeignEffect(cid, 9);
        }

        public static OutPacket ShowForeignEffect(int cid, int effect)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowForeignEffect))
            {
                p.WriteInt(cid); // charid
                p.WriteByte((byte)effect); // 0 = Level up, 8 = ?, 9 = job change, 10 = Quest Complete
                return p;
            }
        }

        public static OutPacket GetShowMesoGain(int gain, bool inChat = false)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowStatusInfo))
            {
                if (!inChat)
                {
                    p.WriteByte(0x00);
                    p.WriteByte(0x01);
                    p.WriteByte(0x00);
                }
                else {
                    p.WriteByte(0x05);
                }
                p.WriteInt(gain);
                p.WriteShort(0); // inet cafe meso gain ?.o

                return p;
            }
        }

        public static OutPacket GetShowExpGain(int gain, bool inChat, bool white, int k = 0)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowStatusInfo))
            {
                p.WriteByte(0x03); // 3 = exp, 4 = fame, 5 = mesos, 6 = guildpoints
                p.WriteBool(white);
                p.WriteInt(gain);
                p.WriteByte(0);
                p.WriteBool(inChat);
                p.WriteInt(0);
                p.WriteInt(0);
                p.WriteInt(0);
                p.WriteInt(0);
                p.WriteInt(k);
                p.WriteByte(0x00);//网吧
                p.WriteByte(0x00);//网吧
                return p;
            }
        }

        public static OutPacket GetShowFameGain(int gain)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowStatusInfo))
            {
                p.WriteByte(4);
                p.WriteInt(gain);
                return p;
            }
        }

        public static OutPacket GetShowItemGain(int itemId, short quantity, bool inChat = false)
        {
            using (OutPacket mplew = new OutPacket(inChat ? SendOpcodes.ShowItemGainInchat : SendOpcodes.ShowStatusInfo))
            {
                if (inChat)
                {
                    mplew.WriteByte(3);
                    mplew.WriteByte(1);
                    mplew.WriteInt(itemId);
                    mplew.WriteInt(quantity);

                }
                else {
                    mplew.WriteShort(0);
                    mplew.WriteInt(itemId);
                    mplew.WriteInt(quantity);
                    mplew.WriteInt(0);
                    mplew.WriteInt(0);
                }
                return mplew;
            }
        }

        public static OutPacket UpdateSkill(int skillid, int level, int masterlevel)
        {
            // 1E 00 01 01 00 E9 03 00 00 01 00 00 00 00 00 00 00 01
            using (OutPacket p = new OutPacket(SendOpcodes.UpdateSkills))
            {
                p.WriteByte(0x01);
                p.WriteShort(1);
                p.WriteInt(skillid);
                p.WriteInt(level);
                p.WriteInt(masterlevel);
                p.WriteByte(0x01);
                return p;
            }
        }

        public static OutPacket UpdateBuddylist(List<MapleBuddyListEntry> buddylist)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.Buddylist))
            {
                p.WriteByte(7);
                p.WriteByte((byte)buddylist.Count);
                foreach (var buddy in buddylist)
                {
                    if (buddy.Visible)
                    {
                        p.WriteInt(buddy.CharacterId); // cid
                        p.WriteString(buddy.CharacterName.PadRight(13, '\0'));
                        p.WriteByte(0x00);
                        p.WriteInt(buddy.ChannelId);
                        p.WriteString(buddy.Group.PadRight(17, '\0'));
                    }
                }
                if (buddylist.Count > 0)
                    p.WriteZero(buddylist.Count);

                return p;
            }
        }

        public static OutPacket LoadFamily()
        {
            string[] title = { "直接移动到学院成员身边", "直接召唤学院成员", "我的爆率 1.5倍(15分钟)", "我的经验值 1.5倍(15分钟)", "学院成员的团结(30分钟)", "我的爆率 2倍(15分钟)", "我的经验值 2倍(15分钟)", "我的爆率 2倍(30分钟)", "我的经验值 2倍(30分钟)", "我的组队爆率 2倍(30分钟)", "我的组队经验值 2倍(30分钟)" };
            string[] description = { "[对象] 我\n[效果] 直接可以移动到指定的学院成员身边。", "[对象] 学院成员 1名\n[效果] 直接可以召唤指定的学院成员到现在的地图。", "[对象] 我\n[持续效果] 15分钟\n[效果] 打怪爆率增加到 #c1.5倍# \n※ 与爆率活动重叠时失效。", "[对象] 我\n[持续效果] 15分钟\n[效果] 打怪经验值增加到 #c1.5倍# \n※ 与经验值活动重叠时失效。", "[启动条件] 校谱最低层学院成员6名以上在线时\n[持续效果] 30分钟\n[效果] 爆率和经验值增加到 #c2倍# ※ 与爆率、经验值活动重叠时失效。", "[对象] 我\n[持续效果] 15分钟\n[效果] 打怪爆率增加到 #c2倍# \n※ 与爆率活动重叠时失效。", "[对象] 我\n[持续效果] 15分钟\n[效果] 打怪经验值增加到 #c2倍# \n※ 与经验值活动重叠时失效。", "[对象] 我\n[持续效果] 30分钟\n[效果] 打怪爆率增加到 #c2倍# \n※ 与爆率活动重叠时失效。", "[对象] 我\n[持续效果] 30分钟\n[效果] 打怪经验值增加到 #c2倍# \n※ 与经验值活动重叠时失效。", "[对象] 我所属组队\n[持续效果] 30分钟\n[效果] 打怪爆率增加到 #c2倍# \n※ 与爆率活动重叠时失效。", "[对象] 我所属组队\n[持续效果] 30分钟\n[效果] 打怪经验值增加到 #c2倍# \n※ 与经验值活动重叠时失效。" };
            int[] repCost = { 3, 5, 7, 8, 10, 12, 15, 20, 25, 40, 50 };

            using (OutPacket p = new OutPacket(SendOpcodes.LoadFamily))
            {
                p.WriteInt(11);
                for (int i = 0; i < 11; i++)
                {
                    p.WriteByte((byte)(i > 4 ? i % 2 + 1 : i));
                    p.WriteInt(repCost[i] * 100);
                    p.WriteInt(1);
                    p.WriteMapleString(title[i]);
                    p.WriteMapleString(description[i]);
                }
                return p;
            }
        }

        public static OutPacket GetMacros(SkillMacro[] macros)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.SkillMacro))
            {
                int count = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (macros[i] != null)
                    {
                        count++;
                    }
                }
                p.WriteByte((byte)count); // number of macros
                for (int i = 0; i < 5; i++)
                {
                    SkillMacro macro = macros[i];
                    if (macro != null)
                    {
                        p.WriteMapleString(macro.MacroName);
                        p.WriteByte((byte)macro.Shout);
                        p.WriteInt(macro.SkillId1);
                        p.WriteInt(macro.SkillId2);
                        p.WriteInt(macro.SkillId2);
                    }
                }
                return p;
            }
        }

        public static OutPacket ShowNotes(int count)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowNotes))
            {
                p.WriteByte(0x03);
                p.WriteByte((byte)count);
                //for (int i = 0; i < count; i++)
                //{
                //    p.WriteInt(notes.getInt("id"));
                //    p.WriteMapleAsciiString(notes.getString("from"));
                //    p.WriteMapleAsciiString(notes.getString("message"));
                //    p.WriteLong(DateUtil.getFileTimestamp(notes.getLong("timestamp")));
                //    p.WriteByteInt(1);
                //    notes.next();
                //}

                return p;
            }
        }

        public static OutPacket CloseRangeAttack(int cid, int skill, byte stance, byte numAttackedAndDamage, List<Tuple<int, List<int>>> damage,byte speed, byte pos)
        {
            using (var p = new OutPacket(SendOpcodes.CloseRangeAttack))
            {
                if (skill == 4211006)
                {
                    AddMesoExplosion(p, cid, skill, stance, numAttackedAndDamage, 0, damage, speed, pos);
                }
                else
                {
                    AddAttackBody(p, cid, skill, stance, numAttackedAndDamage, 0, damage, speed, pos);
                }
                return p;
            }
        }

        private static void AddAttackBody(OutPacket lew, int cid, int skill, byte stance, byte numAttackedAndDamage, int projectile, List<Tuple<int, List<int>>> damage, byte speed, byte pos)
        {
            lew.WriteInt(cid);
            lew.WriteByte(numAttackedAndDamage);
            lew.WriteByte(0);
            if (skill > 0)
            {
                lew.WriteByte(0xFF); // too low and some skills don't work (?)
                lew.WriteInt(skill);
            }
            else
            {
                lew.WriteByte(0);
            }
            lew.WriteByte(0);
            lew.WriteByte(pos);
            lew.WriteByte(stance);
            lew.WriteByte(speed);
            lew.WriteByte(0);
            lew.WriteInt(projectile);

            foreach (var oned in damage)
            {
                if (oned.Item2 != null)
                {
                    lew.WriteInt(oned.Item1);
                    lew.WriteByte(0xFF);
                    foreach (var eachd in oned.Item2)
                    {
                        lew.WriteInt((int) (skill == 3221007 ? eachd + 0x80000000 : eachd));
                    }
                }
            }
        }

        private static void AddMesoExplosion(OutPacket lew, int cid, int skill, byte stance, byte numAttackedAndDamage, int projectile, List<Tuple<int, List<int>>> damage,byte speed, byte pos)
        {
            // BC 00 90 E5 2F 00 00 5A 1A 3E 41 40 00 00 3F 00 03 0A 00 00 00 00 //078
            lew.WriteInt(cid);
            lew.WriteByte(numAttackedAndDamage);
            lew.WriteByte(0x5A);
            lew.WriteByte(0x1A);
            lew.WriteInt(skill);
            lew.WriteByte(0);
            lew.WriteByte(pos);
            lew.WriteByte(stance);
            lew.WriteByte(speed);
            lew.WriteByte(0x0A);
            lew.WriteInt(projectile);

            foreach (var oned in damage)
            {
                if (oned.Item2 != null)
                {
                    lew.WriteInt(oned.Item1);
                    lew.WriteByte(0xFF);
                    lew.WriteByte((byte)oned.Item2.Count);
                    foreach (var eachd in oned.Item2)
                    {
                        lew.WriteInt(eachd);
                    }
                }
            }

        }

        public static OutPacket RemoveItemFromMap(int oid, byte animation, int cid)
        {
            return RemoveItemFromMap(oid, animation, cid, false, 0);
        }

        public static OutPacket RemoveItemFromMap(int oid, byte animation, int cid, bool pet, byte slot)
        {
            using (var p = new OutPacket(SendOpcodes.RemoveItemFromMap))
            {
                p.WriteByte(animation); // expire
                p.WriteInt(oid);
                if (animation >= 2)
                {
                    p.WriteInt(cid);
                    if (pet)
                    {
                        p.WriteByte(slot);
                    }
                }
                return p;
            }
        }

        public static OutPacket DropMesoFromMapObject(int amount, int itemoid, int dropperoid, int ownerid, Point dropfrom, Point dropto, byte mod)
        {
            return DropItemFromMapObjectInternal(amount, itemoid, dropperoid, ownerid, dropfrom, dropto, mod, true);
        }

        public static OutPacket DropItemFromMapObject(int itemid, int itemoid, int dropperoid, int ownerid, Point dropfrom, Point dropto, byte mod)
        {
            return DropItemFromMapObjectInternal(itemid, itemoid, dropperoid, ownerid, dropfrom, dropto, mod, false);
        }

        public static OutPacket DropItemFromMapObjectInternal(int itemid, int itemoid, int dropperoid, int ownerid, Point dropfrom, Point dropto, byte mod, bool mesos)
        {
            using (var p = new OutPacket(SendOpcodes.DropItemFromMapobject))
            {
                p.WriteByte(mod);
                p.WriteInt(itemoid);
                p.WriteBool(mesos); // 1 = mesos, 0 =item
                p.WriteInt(itemid);
                p.WriteInt(0); // owner charid
                p.WriteByte(0x04);
                p.WriteShort((short)dropto.X);
                p.WriteShort((short)dropto.Y);
                if (mod != 2)
                {
                    p.WriteInt(0);
                    p.WriteShort((short)dropfrom.X);
                    p.WriteShort((short)dropfrom.Y);
                }
                else
                {
                    p.WriteInt(dropperoid);
                }
                p.WriteByte(0);
                if (mod != 2)
                {
                    p.WriteByte(0); //fuck knows
                    p.WriteByte(1); //PET Meso pickup
                }
                if (!mesos)
                {
                    p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
                }

                return p;
            }
        }

        #region 任务

        public static OutPacket StartQuest(MapleCharacter c, short quest)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowStatusInfo))
            {
                p.WriteByte(0x01);
                p.WriteShort(quest);
                p.WriteShort(1);
                p.WriteByte(0x00);
                return p;
            }
        }

        public static OutPacket GetShowQuestCompletion(int id)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowQuestCompletion))
            {
                p.WriteShort((short)id);
                return p;
            }
        }

        public static OutPacket ForfeitQuest(MapleCharacter c, short quest)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowStatusInfo))
            {
                p.WriteByte(0x01);
                p.WriteShort(quest);
                p.WriteShort(0);
                p.WriteByte(0x00);
                p.WriteInt(0);
                p.WriteInt(0);
                return p;
            }
        }

        public static OutPacket CompleteQuest(MapleCharacter c, short quest)
        {

            using (OutPacket p = new OutPacket(SendOpcodes.ShowStatusInfo))
            {
                p.WriteByte(1);
                p.WriteShort(quest);
                p.WriteByte(2);
                p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
                return p;
            }
        }

        public static OutPacket GetChatText(int characterId, string text, bool whiteBg, bool show)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.Chattext))
            {
                p.WriteInt(characterId);
                p.WriteBool(whiteBg);
                p.WriteMapleString(text);
                p.WriteBool(show);
                return p;
            }
        }

        public static OutPacket UpdateQuestInfo(MapleCharacter c, short quest, int npc, byte progress)
        {
            // [A5 00] [08] [69 08] [86 71 0F 00] [00 00 00 00]
            // [C5 00] [08] [38 20] [A9 84 8C 00] [00 00] //Ver076
            // [D2 00] [08] [39 20] [A9 84 8C 00] [00 00] //Ver077


            using (OutPacket p = new OutPacket(SendOpcodes.UpdateQuestInfo))
            {
                p.WriteByte(progress);
                p.WriteShort(quest);
                p.WriteInt(npc);
                p.WriteInt(0);
                return p;
            }
        }

        public static OutPacket UpdateQuest(int quest, string status)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowStatusInfo))
            {
                p.WriteByte(1);
                p.WriteShort((short)quest);
                p.WriteByte(1);
                p.WriteMapleString(status);
                return p;
            }
        }

        public static OutPacket UpdateQuestFinish(short quest, int npc, short nextquest)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.UpdateQuestInfo))
            {
                p.WriteByte(8);
                p.WriteShort(quest);
                p.WriteInt(npc);
                p.WriteShort(nextquest);
                p.WriteShort(0);
                return p;
            }
        }

        #endregion

        #region 服务器消息
        //* 0: [Notice]
        //* 1: Popup
        //* 2: Megaphone
        //* 3: Super Megaphone
        //* 4: Scrolling message at top
        //* 5: Pink Text
        //* 6: Lightblue Text
        //* B: 心脏
        //* C: 白骨

        public enum ServerMessageType : byte
        {
            Notice = 0x00,
            //消息框
            Popup = 0x01,
            //喇叭
            Megaphone = 0x02,
            //全频道喇叭
            SuperMegaphoen = 0x03,
            TopScrollingMessage = 0x04,
            PinkText = 0x05,
            LightBlueText = 0x06,
            Heart = 0x0B,
            Bones = 0x0C
        }

        public static OutPacket ServerMessage(string message)
        {
            return ServerMessage(ServerMessageType.TopScrollingMessage, 0, message, true, false);
        }

        public static OutPacket ServerNotice(ServerMessageType type, string message)
        {
            return ServerMessage(type, 0, message, false, false);
        }

        public static OutPacket ServerNotice(ServerMessageType type, int channelId, string message)
        {
            return ServerMessage(type, channelId, message, false, false);
        }

        public static OutPacket ServerNotice(ServerMessageType type, int channelId, string message, bool smegaEar)
        {
            return ServerMessage(type, channelId, message, false, smegaEar);
        }

        private static OutPacket ServerMessage(ServerMessageType type, int channelId, string message, bool servermessage, bool megaEar)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.Servermessage))
            {
                p.WriteByte((byte)type);

                if (servermessage)
                    p.WriteBool(true);

                p.WriteMapleString(message);

                if (type == ServerMessageType.SuperMegaphoen || type == ServerMessageType.Heart || type == ServerMessageType.Bones)
                {
                    p.WriteByte((byte)channelId); // channel
                    p.WriteBool(megaEar);

                }
                if (type == ServerMessageType.LightBlueText)
                {
                    p.WriteInt(0);
                }

                return p;
            }
        }
        #endregion

        #region 改变角色状态
        public static OutPacket EnableActions()
        {
            return UpdatePlayerStats(EmptyStatupdate, true);
        }

        public static OutPacket UpdatePlayerStats(List<Tuple<MapleStat, int>> stats)
        {
            return UpdatePlayerStats(stats, false);
        }

        public static OutPacket UpdatePlayerStats(List<Tuple<MapleStat, int>> stats, bool itemReaction)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.UpdateStats))
            {
                p.WriteBool(itemReaction);

                int updateMask = 0;
                foreach (var statupdate in stats)
                {
                    updateMask |= (int)statupdate.Item1;
                }

                var mystats = stats;
                if (mystats.Count>1)
                {
                    mystats.Sort((obj1, obj2) =>
                    {
                        int val1 = (int)obj1.Item1;
                        int val2 = (int)obj2.Item1;
                        return val1 < val2 ? -1 : (val1 == val2 ? 0 : 1);
                    });
                }

                p.WriteInt(updateMask);
                foreach (var statupdate in mystats)
                {
                    int valueleft = (int)statupdate.Item1;
                    short valueright = (short)statupdate.Item2;

                    if (valueleft >= 1)
                    {
                        if (valueleft == 0x1)
                        {
                            p.WriteShort(valueright);
                        }
                        else if (valueleft <= 0x4)
                        {
                            p.WriteInt(statupdate.Item2);
                        }
                        else if (valueleft < 0x80)
                        {
                            p.WriteByte((byte)valueright);
                        }
                        else if (valueleft < 0x40000)
                        {
                            p.WriteShort(valueright);
                        }
                        else {
                            p.WriteInt(statupdate.Item2);
                        }
                    }
                }

                return p;
            }
        }
        #endregion

        #region 地图相关
        public static OutPacket PartyPortal(int townId, int targetId, Point position)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.PartyOperation))
            {
                p.WriteShort(0x23);
                p.WriteInt(townId);
                p.WriteInt(targetId);
                p.WriteShort((short)position.X);
                p.WriteShort((short)position.Y);

                return p;
            }
        }

        public static OutPacket SpawnPortal(int townId, int targetId, Point pos)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.SpawnPortal))
            {
                p.WriteInt(townId);
                p.WriteInt(targetId);
                if (pos != null)
                {
                    p.WriteShort((short)pos.X);
                    p.WriteShort((short)pos.Y);
                }

                return p;
            }
        }

        public static OutPacket SpawnDoor(int oid, Point pos, bool town)
        {
            // [D3 00] [01] [93 AC 00 00] [6B 05] [37 03]
            using (OutPacket p = new OutPacket(SendOpcodes.SpawnDoor))
            {
                p.WriteBool(town);
                p.WriteInt(oid);
                p.WriteShort((short)pos.X);
                p.WriteShort((short)pos.Y);
                return p;
            }
        }

        public static OutPacket RemoveDoor(int oid, bool town)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.RemoveDoor))
            {
                if (town)
                {
                    p.WriteInt(999999999);
                    p.WriteInt(999999999);
                }
                else {
                    p.WriteByte(0);
                    p.WriteInt(oid);
                }

                return p;
            }
        }

        public static OutPacket SpawnPlayerMapobject(MapleCharacter chr)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.SpawnPlayer))
            {
                p.WriteInt(chr.Id);
                p.WriteByte(0x00);
                p.WriteMapleString(chr.Name);
                //if (chr.getGuildId() <= 0)
                //{
                p.WriteMapleString("");
                p.WriteBytes(new byte[6]);
                //}
                //else {
                //    MapleGuildSummary gs = chr.getClient().getChannelServer().getGuildSummary(chr.getGuildId());
                //    if (gs != null)
                //    {
                //        mplew.WriteMapleAsciiString(gs.getName());
                //        mplew.WriteShort(gs.getLogoBG());
                //        mplew.WriteByte(gs.getLogoBGColor());
                //        mplew.WriteShort(gs.getLogo());
                //        mplew.WriteByte(gs.getLogoColor());
                //    }
                //    else {
                //        mplew.WriteMapleAsciiString("");
                //        mplew.WriteBytes(new byte[6]);
                //    }
                //}
                p.WriteInt(0);
                p.WriteByte(0x00);
                p.WriteByte(0xE0);
                p.WriteByte(0x1F);
                p.WriteByte(0);
                p.WriteByte((byte)(chr.GetBuffedValue(MapleBuffStat.Morph) != null ? 0x02 : 0x00));
                p.WriteBytes(new byte[3]);

                long buffmask = 0;
                int? buffvalue = null;
                if (chr.GetBuffedValue(MapleBuffStat.Darksight) != null && !chr.IsHidden)
                {
                    buffmask |= (long)MapleBuffStat.Darksight;
                }
                if (chr.GetBuffedValue(MapleBuffStat.Combo) != null)
                {
                    buffmask |= (long)MapleBuffStat.Combo;
                    buffvalue = chr.GetBuffedValue(MapleBuffStat.Combo).Value;
                }
                if (chr.GetBuffedValue(MapleBuffStat.Shadowpartner) != null)
                {
                    buffmask |= (long)MapleBuffStat.Shadowpartner;
                }
                if (chr.GetBuffedValue(MapleBuffStat.Soularrow) != null)
                {
                    buffmask |= (long)MapleBuffStat.Soularrow;
                }
                if (chr.GetBuffedValue(MapleBuffStat.Morph) != null)
                {
                    buffvalue = chr.GetBuffedValue(MapleBuffStat.Morph).Value;
                }
                p.WriteInt((int)((buffmask >> 32) & 0xFFFFFFFFL));
                if (buffvalue != null)
                {
                    if (chr.GetBuffedValue(MapleBuffStat.Morph) != null)
                    {
                        p.WriteShort((short)buffvalue);
                    }
                    else
                    {
                        p.WriteByte((byte)buffvalue);
                    }
                }
                p.WriteInt((int)(buffmask & 0xFFFFFFFFL));
                p.WriteBytes(new byte[6]);
                int charMagicSpawn = Randomizer.Next();
                p.WriteInt(charMagicSpawn);//1
                p.WriteLong(0);
                p.WriteShort(0);
                p.WriteByte(0x00);
                p.WriteInt(charMagicSpawn);//2
                p.WriteLong(0);
                p.WriteShort(0);
                p.WriteByte(0x00);
                p.WriteInt(charMagicSpawn);//3
                p.WriteShort(0);
                p.WriteByte(0x00);

                IMapleItem mount;
                if (chr.GetBuffedValue(MapleBuffStat.MonsterRiding) != null &&
                    chr.Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(18, out mount))
                {
                    p.WriteInt(mount.ItemId);
                    p.WriteInt(1004);
                    p.WriteInt(0x01261F00);
                    p.WriteByte(0x00);
                }
                else
                {
                    p.WriteInt(charMagicSpawn);//4
                    p.WriteLong(0);
                    p.WriteByte(0x00);
                }
                p.WriteLong(0);
                p.WriteInt(charMagicSpawn);//5
                p.WriteByte(0x00);
                p.WriteByte(0x01);
                p.WriteByte(0x41);
                p.WriteByte(0x9A);
                p.WriteByte(0x70);
                p.WriteByte(7);
                p.WriteLong(0);
                p.WriteShort(0);
                p.WriteInt(charMagicSpawn);//6
                p.WriteLong(0);
                p.WriteInt(0);
                p.WriteByte(0x00);
                p.WriteInt(charMagicSpawn);//7
                p.WriteLong(0);
                p.WriteShort(0);
                p.WriteByte(0x00);
                p.WriteInt(charMagicSpawn);//8
                p.WriteByte(0x00);
                p.WriteShort(chr.Job.JobId);

                LoginPacket.AddCharLook(p, chr, false);
                p.WriteInt(chr.Inventorys[MapleInventoryType.Cash.Value].CountById(5110000));
                p.WriteInt(chr.ItemEffect);
                p.WriteInt(0);
                p.WriteInt(-1);
                p.WriteInt(chr.Chair);
                p.WriteShort((short)chr.Position.X);
                p.WriteShort((short)chr.Position.Y);
                p.WriteByte((byte)chr.Stance);
                p.WriteByte(0x00);
                p.WriteShort(0);
                p.WriteInt(1);
                p.WriteLong(0);
                p.WriteByte(0x00);
                p.WriteShort(0);
                MapleInventory iv = chr.Inventorys[MapleInventoryType.Equipped.Value];

                var equippedC = iv.Inventory.Values;
                List<Item> equipped = new List<Item>(equippedC.Count);
                foreach (var item in equippedC)
                {
                    equipped.Add((Item)item);
                }
                equipped.Sort();

                List<IEquip> rings = new List<IEquip>();
                foreach (var item in equipped)
                {
                    if (item.ItemId >= 1112800 && item.ItemId <= 1112802 || item.ItemId >= 1112001 && item.ItemId <= 1112003)
                    {
                        rings.Add(MapleRing.LoadFromDb(item.ItemId, item.Position, item.UniqueId));
                    }
                }
                rings.Sort();

                if (rings.Any())
                {
                    foreach (IEquip ring in rings)
                    {
                        p.WriteByte(0x01);
                        p.WriteInt(1);
                        p.WriteInt(ring.UniqueId);
                        p.WriteInt(0);
                        p.WriteInt(ring.PartnerUniqueId);
                        p.WriteInt(0);
                        p.WriteInt(ring.ItemId);
                    }
                    p.WriteShort(0);
                    p.WriteByte(0x00);
                }
                else
                {
                    p.WriteInt(0);
                }
                return p;
            }
        }

        public static OutPacket ShowForcedEquip()
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ForcedMapEquip))
            {
                p.WriteInt(0);
                return p;
            }
        }

        public static OutPacket AddTutorialStats()
        {
            using (OutPacket p = new OutPacket(SendOpcodes.EnableTemporaryStats))
            {
                p.WriteInt(3871);
                p.WriteShort(999);
                p.WriteShort(999);
                p.WriteShort(999);
                p.WriteShort(999);
                p.WriteShort(255);
                p.WriteShort(999);
                p.WriteShort(999);
                p.WriteByte(0x78);
                p.WriteByte(0x8C);
                return p;
            }
        }

        public static OutPacket RemoveTutorialStats()
        {
            using (OutPacket p = new OutPacket(SendOpcodes.DisableTemporaryStats))
            {
                return p;
            }
        }

        public static OutPacket SpawnTutorialSummon(int type)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.TutorialSummon))
            {
                p.WriteByte((byte)type);
                return p;
            }
        }

        public static OutPacket SpawnSpecialMapObject(MapleSummon summon, int skillLevel, bool animated)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.SpawnSpecialMapobject))
            {
                p.WriteInt(summon.Owner.Id);
                p.WriteInt(summon.ObjectId); // Supposed to be Object ID, but this works too! <3
                p.WriteInt(summon.SkillId);
                p.WriteByte(114); // test
                p.WriteByte((byte)skillLevel);
                p.WriteShort((short)summon.Position.X);
                p.WriteShort((short)summon.Position.Y);
                p.WriteByte(4); // test
                p.WriteByte(31); // test
                p.WriteByte(0); // test
                p.WriteByte((byte)summon.MovementType); // 0 = don't move, 1 = follow (4th mage summons?), 2/4 = only tele follow, 3 = bird follow
                p.WriteByte(1); // 0 and the summon can't attack - but puppets don't attack with 1 either ^.-
                p.WriteBool(!animated);
                return p;
            }
        }

        public static OutPacket GetClock(int time)
        {
            // time in seconds
            using (OutPacket p = new OutPacket(SendOpcodes.Clock))
            {
                p.WriteByte(2); // clock type. if you send 3 here you have to send another byte (which does not matter at all) before the timestamp
                p.WriteInt(time);
                return p;
            }
        }

        public static OutPacket GetClockTime(int hour, int min, int sec)
        {
            // Current Time
            using (OutPacket p = new OutPacket(SendOpcodes.Clock))
            {
                p.WriteByte(1); // Clock-Type
                p.WriteByte((byte)hour);
                p.WriteByte((byte)min);
                p.WriteByte((byte)sec);
                return p;
            }
        }

        public static OutPacket BoatPacket(bool type) => BoatPacket(type ? 1 : 2);

        public static OutPacket BoatPacket(int effect)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.BoatEffect))
            {
                p.WriteShort((short)effect); //1034: balrog boat comes, 1548: boat comes in ellinia station, 520: boat leaves ellinia station
                return p;
            }
        }

        public static OutPacket RemovePlayerFromMap(int cid)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.RemovePlayerFromMap))
            {
                p.WriteInt(cid);
                return p;
            }
        }

        public static OutPacket RemoveSpecialMapObject(MapleSummon summon, bool animated)
        {
            // [86 00] [6A 4D 27 00] 33 1F 00 00 02
            // 92 00 36 1F 00 00 0F 65 85 01 84 02 06 46 28 00 06 81 02 01 D9 00 BD FB D9 00 BD FB 38 04 2F 21 00 00 10 C1 2A 00 06 00 06 01 00 01 BD FB FC 00 BD FB 6A 04 88 1D 00 00 7D 01 AF FB
            using (OutPacket p = new OutPacket(SendOpcodes.RemoveSpecialMapobject))
            {
                p.WriteInt(summon.Owner.Id);
                p.WriteInt(summon.ObjectId);
                p.WriteByte((byte)(animated ? 4 : 1)); // ?
                return p;
            }
        }

        public static OutPacket MovePlayer(int cid, List<ILifeMovementFragment> moves)
        {

            using (OutPacket p = new OutPacket(SendOpcodes.MovePlayer))
            {
                p.WriteInt(cid);
                p.WriteInt(0);
                SerializeMovementList(p, moves);
                return p;
            }
        }

        private static void SerializeMovementList(OutPacket p, List<ILifeMovementFragment> moves)
        {
            p.WriteByte((byte)moves.Count);
            foreach (var move in moves)
            {
                move.Serialize(p);
            }
        }


        public static OutPacket GetWarpToMap(MapleMap to, byte spawnPoint, MapleCharacter chr)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.WarpToMap))
            {
                p.WriteInt(chr.Client.ChannelId);
                p.WriteByte(0x00);
                p.WriteByte(0x03);
                p.WriteShort(0);
                p.WriteByte(0x00);
                p.WriteInt(to.MapId);
                p.WriteByte(spawnPoint);
                p.WriteShort(chr.Hp);
                //mplew.Write(0); //取消此处可防止出现无可开始任务的错误。但是人物头上一直会有个灯泡！
                p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
                return p;
            }
        }

        public static OutPacket GetWarpToMap(int to, byte spawnPoint, MapleCharacter chr)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.WarpToMap))
            {
                p.WriteInt(chr.Client.ChannelId);
                p.WriteByte(0x01);
                p.WriteByte(0x02);
                p.WriteShort(0);
                p.WriteInt(to);
                p.WriteByte(spawnPoint);
                p.WriteShort(chr.Hp);
                p.WriteByte(0x00);
                p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
                return p;
            }
        }

        public static OutPacket DestroyReactor(MapleReactor reactor)
        {
            Point pos = reactor.Position;
            using (OutPacket p = new OutPacket(SendOpcodes.ReactorDestroy))
            {
                p.WriteInt(reactor.ObjectId);
                p.WriteByte(reactor.State);
                p.WriteShort((short)pos.X);
                p.WriteShort((short)pos.X);
                return p;
            }
        }

        public static OutPacket SpawnReactor(MapleReactor reactor)
        {
            Point pos = reactor.Position;
            using (OutPacket p = new OutPacket(SendOpcodes.ReactorSpawn))
            {
                p.WriteInt(reactor.ObjectId);
                p.WriteInt(reactor.ReactorId);
                p.WriteByte(reactor.State);
                p.WriteShort((short)pos.X);
                p.WriteShort((short)pos.X);
                p.WriteByte(0x00);
                return p;
            }
        }

        public static OutPacket TriggerReactor(MapleReactor reactor, int stance)
        {
            Point pos = reactor.Position;
            using (OutPacket p = new OutPacket(SendOpcodes.ReactorHit))
            {
                p.WriteInt(reactor.ObjectId);
                p.WriteByte(reactor.State);
                p.WriteShort((short)pos.X);
                p.WriteShort((short)pos.X);
                p.WriteShort((short)stance);
                p.WriteByte(0x00);
                p.WriteByte(0x05); // frame delay, set to 5 since there doesn't appear to be a fixed formula for it
                return p;
            }
        }

        public static OutPacket GetChannelChange(IPAddress inetAddr, short port)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ChangeChannel))
            {
                p.WriteByte(0x01);
                p.WriteBytes(inetAddr.GetAddressBytes());
                p.WriteShort(port);
                return p;
            }
        }

        #endregion

        public static OutPacket CancelForeignBuff(int cid, List<MapleBuffStat> statups)
        {
            using (OutPacket mplew = new OutPacket(SendOpcodes.CancelForeignBuff))
            {
                mplew.WriteInt(cid);
                long mask = GetLongMaskFromList(statups);
                long mask2 = 42949673024L;
                if (mask == (long)MapleBuffStat.MonsterRiding || mask == (long)MapleBuffStat.Dash || mask == mask2)
                {
                    mplew.WriteByte(0x00);
                }
                else {
                    mplew.WriteLong(0);
                }
                mplew.WriteLong(mask);
                if (mask == (long)MapleBuffStat.MonsterRiding || mask == (long)MapleBuffStat.Dash || mask == mask2)
                {
                    mplew.WriteInt(0);
                    mplew.WriteShort(0);
                    mplew.WriteByte(0x00);
                }
                return mplew;
            }
        }

        public static OutPacket CancelBuff(List<MapleBuffStat> statups)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.CancelBuff))
            {
                long mask = GetLongMaskFromList(statups);
                long mask2 = 42949673024L;
                if (mask == (long)MapleBuffStat.MonsterRiding || mask == (long)MapleBuffStat.Dash || mask == mask2)
                {
                    p.WriteByte(0x00);
                }
                else {
                    p.WriteLong(0);
                }
                p.WriteLong(mask);
                if (mask == (long)MapleBuffStat.MonsterRiding || mask == (long)MapleBuffStat.Dash || mask == mask2)
                {
                    p.WriteInt(0);
                    p.WriteShort(0);
                    p.WriteByte(0x00);
                }
                p.WriteByte((byte)(mask == (long)MapleBuffStat.Dash ? 4 : 3));
                return p;
            }
        }

        private static long GetLongMask<T>(List<Tuple<T, int>> statups) where T : struct
        {
            long mask = 0;
            foreach (var statup in statups)
            {
                mask |= Convert.ToInt64(statup.Item1);
            }
            return mask;
        }

        private static long GetLongMaskFromList<T>(List<T> statups) where T : struct
        {
            long mask = 0;
            foreach (T statup in statups)
            {
                mask |= Convert.ToInt64(statup);
            }
            return mask;
        }

        #region 好友
        public static OutPacket RequestBuddylistAdd(int cidFrom, string nameFrom)
        {

            using (OutPacket p = new OutPacket(SendOpcodes.Buddylist))
            {
                p.WriteByte(0x09);
                p.WriteInt(cidFrom);
                p.WriteMapleString(nameFrom);
                p.WriteInt(cidFrom);
                p.WriteString(nameFrom.PadRight(13, '\0'));
                p.WriteByte(0x01);
                p.WriteByte(0x05);
                p.WriteByte(0x00);
                p.WriteShort(0);
                p.WriteString("群未定".PadRight(17, '\0'));
                p.WriteInt(0);

                return p;
            }
        }
        #endregion

        public static OutPacket UpdateMount(int charid, MapleMount mount, bool levelup)
        {
            return UpdateMount(charid, mount.Level, mount.Exp, mount.Tiredness, levelup);
        }

        public static OutPacket UpdateMount(int charid, int newlevel, int newexp, int tiredness, bool levelup)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.UpdateMount))
            {
                p.WriteInt(charid);
                p.WriteInt(newlevel);
                p.WriteInt(newexp);
                p.WriteInt(tiredness);
                p.WriteBool(levelup);
                return p;
            }
        }

        #region NPC
        public static OutPacket RemoveNpc(int objid)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.RemoveNpc))
            {
                p.WriteInt(objid);
                return p;
            }
        }

        public static OutPacket SpawnNpc(MapleNpc npc)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.SpawnNpc))
            {
                p.WriteInt(npc.ObjectId);
                p.WriteInt(npc.Id);
                p.WriteShort((short)npc.Position.X);
                p.WriteShort((short)npc.Cy);
                p.WriteBool(npc.F != 1);
                p.WriteShort((short)npc.Fh);
                p.WriteShort((short)npc.Rx0);
                p.WriteShort((short)npc.Rx1);
                p.WriteBool(true);
                return p;
            }
        }

        public static OutPacket SpawnNpcRequestController(MapleNpc npc, bool show)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.SpawnNpcRequestController))
            {
                p.WriteByte(0x01);
                p.WriteInt(npc.ObjectId);
                p.WriteInt(npc.Id);
                p.WriteShort((short)npc.Position.X);
                p.WriteShort((short)npc.Cy);
                p.WriteBool(npc.F != 1);
                p.WriteShort((short)npc.Fh);
                p.WriteShort((short)npc.Rx0);
                p.WriteShort((short)npc.Rx1);
                p.WriteBool(show);
                return p;
            }
        }


        public enum NpcTalkType
        {
            Next,
            Prve,
            NextPrve,
            Ok,
            YesNo,
            AcceptDecline,
            Simple
        }

        public static OutPacket NpcTalk(NpcTalkType type, int npcId, string content, byte speaker = 0)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.NpcTalk))
            {
                p.WriteByte(0x04);
                p.WriteInt(npcId);

                switch (type)
                {
                    case NpcTalkType.YesNo:
                        p.WriteByte(0x01);
                        break;
                    case NpcTalkType.AcceptDecline:
                        p.WriteByte((byte)(speaker == 0 ? 0x0B : 0x0C));
                        break;
                    case NpcTalkType.Simple:
                        p.WriteByte(0x04);
                        break;
                    default:
                        p.WriteByte(0x00);
                        break;
                }

                p.WriteByte(speaker);
                p.WriteMapleString(content);

                switch (type)
                {
                    case NpcTalkType.Next:
                        p.WriteByte(0x00);
                        p.WriteByte(0x01);
                        break;
                    case NpcTalkType.Prve:
                        p.WriteByte(0x01);
                        p.WriteByte(0x00);
                        break;
                    case NpcTalkType.NextPrve:
                        p.WriteByte(0x01);
                        p.WriteByte(0x01);
                        break;
                    case NpcTalkType.Ok:
                        p.WriteByte(0x00);
                        p.WriteByte(0x00);
                        break;
                }

                return p;
            }
        }

        public static OutPacket NpcTalkStyle(int npc, string talk, int[] styles, int card)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.NpcTalk))
            {
                p.WriteByte(0x04); // ?
                p.WriteInt(npc);
                p.WriteByte(0x07);
                p.WriteByte(0x00);
                p.WriteMapleString(talk);
                p.WriteByte((byte)styles.Length);
                for (int i = 0; i < styles.Length; i++)
                {
                    p.WriteInt(styles[i]);
                }
                p.WriteInt(card);
                return p;
            }

        }

        public static OutPacket NpcTalkNum(int npc, string talk, int def, int min, int max)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.NpcTalk))
            {
                p.WriteByte(0x04); // ?
                p.WriteInt(npc);
                p.WriteByte(0x03);
                p.WriteByte(0x00);
                p.WriteMapleString(talk);
                p.WriteInt(def);
                p.WriteInt(min);
                p.WriteInt(max);
                return p;
            }
        }

        public static OutPacket NpcTalkText(int npc, string talk)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.NpcTalk))
            {
                p.WriteByte(0x04); // ?
                p.WriteInt(npc);
                p.WriteByte(0x02);
                p.WriteByte(0x00);
                p.WriteMapleString(talk);
                p.WriteInt(0);
                p.WriteInt(0);
                return p;
            }
        }


        #endregion

        #region 商店 物品
        public static OutPacket GetNpcShop(MapleClient c, int sid, List<MapleShopItem> items)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;

            using (OutPacket p = new OutPacket(SendOpcodes.OpenNpcShop))
            {
                p.WriteInt(sid);
                p.WriteShort((short)items.Count);
                foreach (MapleShopItem item in items)
                {
                    p.WriteInt(item.ItemId);
                    p.WriteInt(item.Price);
                    if (!ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId))
                    {
                        p.WriteShort(1);
                        p.WriteShort(item.Buyable);
                    }
                    else {
                        p.WriteShort(0);
                        p.WriteInt(0);
                        p.WriteShort((short)(BitConverter.DoubleToInt64Bits(ii.GetPrice(item.ItemId)) >> 48));
                        p.WriteShort(ii.GetSlotMax(c, item.ItemId));
                    }
                }
                return p;
            }
        }

        /**
         * code (8 = sell, 0 = buy, 0x20 = due to an error the trade did not happen
         * o.o)
         *
         * @param code
         * @return
         */
        public static OutPacket ConfirmShopTransaction(byte code)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ConfirmShopTransaction))
            {
                // mplew.WriteShort(0xE6); // 47 E4
                p.WriteByte(code); // recharge == 8?

                return p;
            }
        }

        /*
         * 19 reference 00 01 00 = new while adding 01 01 00 = add from drop 00 01 01 = update count 00 01 03 = clear slot
         * 01 01 02 = move to empty slot 01 02 03 = move and merge 01 02 01 = move and merge with rest
         */
        public static OutPacket AddInventorySlot(MapleInventoryType type, IMapleItem item)
        {
            return AddInventorySlot(type, item, false);
        }

        public static OutPacket AddInventorySlot(MapleInventoryType type, IMapleItem item, bool fromDrop)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ModifyInventoryItem))
            {
                p.WriteBool(fromDrop);
                p.WriteBytes(new byte[] { 0x01, 0x00 }); // add mode
                p.WriteByte(type.Value); // iv type
                p.WriteByte(item.Position); // slot id
                AddItemInfo(p, item, true, false, false);

                return p;
            }
        }

        public static OutPacket UpdateInventorySlot(MapleInventoryType type, IMapleItem item)
        {
            return UpdateInventorySlot(type, item, false);
        }

        public static OutPacket UpdateInventorySlot(MapleInventoryType type, IMapleItem item, bool fromDrop)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ModifyInventoryItem))
            {
                p.WriteBool(fromDrop);
                p.WriteBytes(new byte[] { 0x01, 0x01 }); // update   // mode
                p.WriteByte(type.Value); // iv type
                p.WriteByte(item.Position); // slot id
                p.WriteByte(0x00);
                p.WriteShort(item.Quantity);
                return p;
            }
        }

        public static OutPacket MoveInventoryItem(MapleInventoryType type, byte src, byte dst, byte equipIndicator = 0)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ModifyInventoryItem))
            {
                p.WriteBytes(new byte[] { 0x01, 0x01, 0x02 });
                p.WriteByte(type.Value);
                p.WriteShort(src);
                p.WriteShort(dst);
                if (equipIndicator != 0)
                {
                    p.WriteByte(equipIndicator);
                }
                return p;
            }
        }

        public static OutPacket MoveAndMergeInventoryItem(MapleInventoryType type, byte src, byte dst, short total)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ModifyInventoryItem))
            {
                p.WriteBytes(new byte[] { 0x01, 0x02, 0x03 });
                p.WriteByte(type.Value);
                p.WriteShort(src);
                p.WriteByte(0x01); // merge mode?
                p.WriteByte(type.Value);
                p.WriteShort(dst);
                p.WriteShort(total);

                return p;
            }
        }

        public static OutPacket MoveAndMergeWithRestInventoryItem(MapleInventoryType type, byte src, byte dst, short srcQ, short dstQ)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ModifyInventoryItem))
            {
                p.WriteBytes(new byte[] { 0x01, 0x02, 0x01 });
                p.WriteByte(type.Value);
                p.WriteShort(src);
                p.WriteShort(srcQ);
                p.WriteByte(0x01);
                p.WriteByte(type.Value);
                p.WriteShort(dst);
                p.WriteShort(dstQ);

                return p;
            }
        }

        public static OutPacket ClearInventoryItem(MapleInventoryType type, byte slot, bool fromDrop)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ModifyInventoryItem))
            {
                p.WriteBool(fromDrop);
                p.WriteBytes(new byte[] { 0x01, 0x03 });
                p.WriteByte(type.Value);
                p.WriteShort(slot);

                return p;
            }
        }

        public static OutPacket ScrolledItem(IMapleItem scroll, IMapleItem item, bool destroyed)
        {
            // 18 00 01 02 03 02 08 00 03 01 F7 FF 01

            using (OutPacket p = new OutPacket(SendOpcodes.ModifyInventoryItem))
            {
                p.WriteBool(true); // fromdrop always true
                p.WriteByte((byte)(destroyed ? 2 : 3));
                p.WriteByte((byte)(scroll.Quantity > 0 ? 1 : 3));
                p.WriteByte(MapleInventoryType.Use.Value);
                p.WriteShort(scroll.Position);

                if (scroll.Quantity > 0)
                {
                    p.WriteShort(scroll.Quantity);
                }
                p.WriteByte(0x03);

                if (!destroyed)
                {
                    p.WriteByte(MapleInventoryType.Equip.Value);
                    p.WriteShort(item.Position);
                    p.WriteByte(0x00);
                }
                p.WriteByte(MapleInventoryType.Equip.Value);
                p.WriteShort(item.Position);

                if (!destroyed)
                {
                    AddItemInfo(p, item, true, true, false);
                }
                p.WriteByte(0x01);

                return p;
            }
        }

        //public static OutPacket GetScrollEffect(int chr, ScrollResult scrollSuccess, bool legendarySpirit)
        //{


        //    using (OutPacket p = new OutPacket(SendOpcodes.SHOW_SCROLL_EFFECT))
        //    {
        //        p.WriteInt(chr);
        //        switch (scrollSuccess)
        //        {
        //            case SUCCESS:
        //                p.WriteShort(1);
        //                p.WriteShort(legendarySpirit ? 1 : 0);
        //                break;
        //            case FAIL:
        //                p.WriteShort(0);
        //                p.WriteShort(legendarySpirit ? 1 : 0);
        //                break;
        //            case CURSE:
        //                p.WriteByteInt(0);
        //                p.WriteByteInt(1);
        //                p.WriteShort(legendarySpirit ? 1 : 0);
        //                break;
        //            default:
        //                throw new IllegalArgumentException("effect in illegal range");
        //        }

        //        return p;
        //    }
        //}

        public static void AddItemInfo(OutPacket p, IMapleItem item, bool zeroPosition = false, bool leaveOut = false, bool cs = false)
        {
            if (item.UniqueId > 0)
            {
                if (item.ItemId >= 5000000 && item.ItemId <= 5000100)
                {
                    AddPetItemInfo(p, item, zeroPosition, leaveOut, cs);
                }
                else if ((item.ItemId >= 1112800 && item.ItemId <= 1112802) || (item.ItemId >= 1112001 && item.ItemId <= 1112003))
                {
                    AddRingItemInfo(p, item, zeroPosition, leaveOut, cs);
                }
                else
                {
                    AddCashItemInfo(p, item, zeroPosition, leaveOut, cs);
                }
            }
            else
            {
                AddNormalItemInfo(p, item, zeroPosition, leaveOut, false);
            }
        }

        private static void AddNormalItemInfo(OutPacket p, IMapleItem item, bool zeroPosition, bool leaveOut, bool cs)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            IEquip equip = null;
            bool masking = false;
            bool equipped = false;
            if (item.Type == MapleItemType.Equip)
            {
                equip = (IEquip)item;
            }

            byte pos = item.Position;
            if (zeroPosition)
            {
                if (!leaveOut)
                {
                    p.WriteByte(0x00);
                }
            }
            else if (pos <= 0xFF)
            {
                if (pos > 100)
                {
                    masking = true;
                    p.WriteByte((byte)(pos - 100));
                }
                else {
                    p.WriteByte(pos);
                }
                equipped = true;
            }
            else {
                p.WriteByte(item.Position);
            }

            p.WriteByte((byte)item.Type);
            p.WriteInt(item.ItemId);
            p.WriteBool((ii.IsCash(item.ItemId) && equipped) || cs);

            if ((ii.IsCash(item.ItemId) && equipped) || cs)
            {
                p.WriteLong(-1);
            }

            p.WriteLong(DateUtiliy.GetFileTimestamp(item.Expiration?.GetTimeMilliseconds() ?? FinalTime));

            if (item.Type == MapleItemType.Equip)
            {
                p.WriteByte(equip.UpgradeSlots);
                p.WriteByte(equip.Level);
                p.WriteShort(equip.Str); // str
                p.WriteShort(equip.Dex); // dex
                p.WriteShort(equip.Int); // int
                p.WriteShort(equip.Luk); // luk
                p.WriteShort(equip.Hp); // hp
                p.WriteShort(equip.Mp); // mp
                p.WriteShort(equip.Watk); // watk
                p.WriteShort(equip.Matk); // matk
                p.WriteShort(equip.Wdef); // wdef
                p.WriteShort(equip.Mdef); // mdef
                p.WriteShort(equip.Acc); // accuracy
                p.WriteShort(equip.Avoid); // avoid
                p.WriteShort(equip.Hands); // hands
                p.WriteShort(equip.Speed); // speed
                p.WriteShort(equip.Jump); // jump
                p.WriteMapleString(equip.Owner);
                p.WriteByte(equip.Locked);
                p.WriteShort(equip.Flag); //Item Flags
                if (!masking)
                {
                    p.WriteInt(0);
                    p.WriteByte(0x00);
                    p.WriteShort(equip.Vicious);
                    p.WriteShort(0);
                    p.WriteLong(0);
                    p.WriteLong(DateUtiliy.GetFileTimestamp(item.Expiration?.GetTimeMilliseconds() ?? FinalTime));
                }
                else {
                    p.WriteBytes(new byte[] { 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x70, 0x3E, 0xBC, 0x5C, 0x4C, 0x07, 0xCA, 0x01 });
                }
                p.WriteInt(-1);
            }
            else
            {
                p.WriteShort(item.Quantity);
                p.WriteMapleString(item.Owner);
                p.WriteShort(item.Flag);
                if (ii.IsThrowingStar(item.ItemId) || ii.IsBullet(item.ItemId))
                {
                    p.WriteBytes(new byte[] { 0x02, 0x00, 0x00, 0x00, 0x54, 0x00, 0x00, 0x34 });
                }
            }
        }

        private static void AddPetItemInfo(OutPacket p, IMapleItem item, bool zeroPosition, bool leaveOut, bool cs)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            byte pos = item.Position;
            if (zeroPosition)
            {
                if (!leaveOut)
                {
                    p.WriteByte(0x00);
                }
            }
            else if (pos <= 0xFF)
            {
                if (pos > 100)
                {
                    p.WriteByte((byte)(pos - 100));
                }
                else {
                    p.WriteByte(pos);
                }
            }
            else {
                p.WriteByte(item.Position);
            }

            p.WriteByte(0x03);
            p.WriteInt(item.ItemId);
            p.WriteByte(0x01);
            p.WriteInt(item.UniqueId);
            p.WriteInt(0);

            MaplePet pet = MaplePet.LoadFromDb(item.ItemId, item.Position, item.UniqueId);
            p.WriteLong(DateUtiliy.GetFileTimestamp(item.Expiration?.GetTimeMilliseconds() ?? FinalTime));
            string petname = pet.PetInfo.PetName;
            if (Encoding.Default.GetByteCount(petname) > 13)
            {
                petname = petname.Substring(0, 13);
            }
            p.WriteString(petname);
            for (int i = Encoding.Default.GetByteCount(petname); i < 13; i++)
            {
                p.WriteByte(0x00);
            }
            p.WriteByte(pet.PetInfo.Level);
            p.WriteShort(pet.PetInfo.Closeness);
            p.WriteByte(pet.PetInfo.Fullness);

            p.WriteLong(DateUtiliy.GetFileTimestamp(item.Expiration?.GetTimeMilliseconds() ?? FinalTime));
            p.WriteZero(10);

        }

        private static void AddRingItemInfo(OutPacket p, IMapleItem item, bool zeroPosition, bool leaveOut, bool cs)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            bool ring = false;
            IEquip equip = null;
            if (item.Type == MapleItemType.Equip)
            {
                equip = (IEquip)item;
                if (equip.IsRing)
                {
                    ring = true;
                }
            }
            byte pos = item.Position;

            if (zeroPosition)
            {
                if (!leaveOut)
                {
                    p.WriteByte(0x00);
                }
            }
            else if (pos <= 0xFF)
            {
                if (pos > 100 || pos == 128 || ring)
                {
                    p.WriteByte((byte)(pos - 100));
                }
                else {
                    p.WriteByte(pos);
                }
            }
            else {
                p.WriteByte(item.Position);
            }

            p.WriteByte((byte)item.Type);
            p.WriteInt(item.ItemId);
            p.WriteByte(0x01);
            p.WriteInt(equip.UniqueId);
            p.WriteInt(0);

            p.WriteLong(DateUtiliy.GetFileTimestamp(item.Expiration?.GetTimeMilliseconds() ?? FinalTime));

            p.WriteByte(equip.UpgradeSlots);
            p.WriteByte(equip.Level);
            p.WriteShort(equip.Str); // str
            p.WriteShort(equip.Dex); // dex
            p.WriteShort(equip.Int); // int
            p.WriteShort(equip.Luk); // luk
            p.WriteShort(equip.Hp); // hp
            p.WriteShort(equip.Mp); // mp
            p.WriteShort(equip.Watk); // watk
            p.WriteShort(equip.Matk); // matk
            p.WriteShort(equip.Wdef); // wdef
            p.WriteShort(equip.Mdef); // mdef
            p.WriteShort(equip.Acc); // accuracy
            p.WriteShort(equip.Avoid); // avoid
            p.WriteShort(equip.Hands); // hands
            p.WriteShort(equip.Speed); // speed
            p.WriteShort(equip.Jump); // jump
            p.WriteMapleString(equip.Owner);
            //道具交易次数？
            p.WriteShort(0);
            //道具经验？
            p.WriteShort(0);
            p.WriteByte(equip.Locked);
            p.WriteByte(0x00);
            p.WriteShort(0);
            p.WriteShort(0);
            p.WriteShort(0);
            p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
            p.WriteInt(-1);
        }

        private static void AddCashItemInfo(OutPacket p, IMapleItem item, bool zeroPosition, bool leaveOut, bool cs)
        {
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            IEquip equip = null;
            bool masking = false;
            bool equipped = false;
            if (item.Type == MapleItemType.Equip)
            {
                equip = (IEquip)item;
            }
            byte pos = item.Position;
            if (zeroPosition)
            {
                if (!leaveOut)
                {
                    p.WriteByte(0x00);
                }
            }
            else if (pos <= 0xFF)
            {
                if (pos > 100)
                {
                    p.WriteByte((byte)(pos - 100));
                    masking = true;
                }
                else {
                    p.WriteByte(pos);
                }
                equipped = true;
            }
            else {
                p.WriteByte(item.Position);
            }
            p.WriteByte((byte)item.Type);
            p.WriteInt(item.ItemId);
            p.WriteBool((ii.IsCash(item.ItemId) && equipped) || cs);
            if ((ii.IsCash(item.ItemId) && equipped) || cs)
            {
                p.WriteLong(item.UniqueId);
            }

            p.WriteLong(DateUtiliy.GetFileTimestamp(item.Expiration?.GetTimeMilliseconds() ?? FinalTime));

            if (item.Type == MapleItemType.Equip)
            {
                p.WriteByte(equip.UpgradeSlots);
                p.WriteByte(equip.Level);
                p.WriteShort(equip.Str); // str
                p.WriteShort(equip.Dex); // dex
                p.WriteShort(equip.Int); // int
                p.WriteShort(equip.Luk); // luk
                p.WriteShort(equip.Hp); // hp
                p.WriteShort(equip.Mp); // mp
                p.WriteShort(equip.Watk); // watk
                p.WriteShort(equip.Matk); // matk
                p.WriteShort(equip.Wdef); // wdef
                p.WriteShort(equip.Mdef); // mdef
                p.WriteShort(equip.Acc); // accuracy
                p.WriteShort(equip.Avoid); // avoid
                p.WriteShort(equip.Hands); // hands
                p.WriteShort(equip.Speed); // speed
                p.WriteShort(equip.Jump); // jump
                p.WriteMapleString(equip.Owner);
                //道具交易次数?
                p.WriteShort(0);
                //道具经验?
                p.WriteShort(0);
                // 0 normal; 1 locked
                p.WriteByte(equip.Locked);
                p.WriteByte(0x00);
                if (!masking && !cs)
                {
                    p.WriteLong(0);
                    p.WriteZero(6);
                    p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
                }
                else
                {
                    p.WriteZero(6);
                    p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
                }
                p.WriteInt(-1);
            }
            else
            {
                p.WriteShort(item.Quantity);
                p.WriteMapleString(item.Owner);
                p.WriteShort(0); // this seems to end the item entry but only if its not a THROWING STAR :))9 O.O!

                if (ii.IsThrowingStar(item.ItemId) || ii.IsBullet(item.ItemId))
                {
                    p.WriteBytes(new byte[] { 0x02, 0x00, 0x00, 0x00, 0x54, 0x00, 0x00, 0x34 });
                }
            }

        }

        public static OutPacket GetInventoryFull()
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ModifyInventoryItem))
            {
                p.WriteByte(0x01);
                p.WriteByte(0x00);
                return p;
            }
        }

        public static OutPacket GetShowInventoryFull() => GetShowInventoryStatus(0xFF);

        public static OutPacket ShowItemUnavailable() => GetShowInventoryStatus(0xFE);

        public static OutPacket GetShowInventoryStatus(byte mode)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.ShowStatusInfo))
            {
                p.WriteByte(0x00);
                p.WriteByte(mode);
                p.WriteInt(0);
                p.WriteInt(0);
                return p;
            }
        }

        #endregion


        public static OutPacket ShowPet(MapleCharacter chr, MaplePet pet, bool remove, bool hunger = false)
        {

            using (OutPacket p = new OutPacket(SendOpcodes.SpawnPet))
            {
                p.WriteInt(chr.Id);
                p.WriteByte((byte)chr.GetPetSlot(pet));
                if (remove)
                {
                    p.WriteByte(0x00);
                    p.WriteBool(hunger);
                }
                else {
                    p.WriteByte(0x01);
                    p.WriteByte(0x00);
                    p.WriteInt(pet.ItemId);
                    p.WriteMapleString(pet.PetInfo.PetName);
                    p.WriteInt(pet.UniqueId);
                    p.WriteInt(0);
                    p.WriteShort((short)pet.Pos.X);
                    p.WriteShort((short)pet.Pos.Y);
                    p.WriteByte((byte)pet.Stance);
                    p.WriteInt(pet.Fh);
                }

                return p;
            }
        }


        public static OutPacket UseChalkboard(MapleCharacter chr, bool close)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.Chalkboard))
            {
                p.WriteInt(chr.Id);
                if (close)
                {
                    p.WriteByte(0x00);
                }
                else {
                    p.WriteByte(0x01);
                    p.WriteMapleString(chr.ChalkBoardText);
                }
                return p;
            }
        }

        public static OutPacket GiveForeignEnergyCharge(int cid, short barammount)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.GiveForeignBuff))
            {
                p.WriteInt(cid);
                p.WriteLong(0);
                p.WriteLong((long)MapleBuffStat.EnergyCharge);
                p.WriteShort(0);
                p.WriteShort(barammount);
                p.WriteShort(0);
                p.WriteLong(0);
                p.WriteShort(0);
                p.WriteShort(0);
                return p;
            }
        }

        public static OutPacket UpdatePartyMemberHp(int cid, int curhp, int maxhp)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.UpdatePartymemberHp))
            {
                p.WriteInt(cid);
                p.WriteInt(curhp);
                p.WriteInt(maxhp);
                return p;
            }
        }

        public static OutPacket GiveDebuff(long mask, List<Tuple<MapleDisease, int>> statups, MobSkill skill)
        {
            // [1D 00] [00 00 00 00 00 00 00 00] [00 00 02 00 00 00 00 00] [00 00] [7B 00] [04 00] [B8 0B 00 00] [00 00] [84 03] [01]
            using (var p = new OutPacket(SendOpcodes.GiveBuff))
            {
                p.WriteLong(0);
                p.WriteLong(mask);
                foreach (var statup in statups)
                {
                    p.WriteShort((short)statup.Item1);
                    p.WriteShort(skill.skillId);
                    p.WriteShort(skill.skillLevel);
                    p.WriteInt(skill.duration);
                }
                p.WriteShort(0); // ??? wk charges have 600 here o.o
                p.WriteShort(900); //Delay
                p.WriteByte(0x02);

                return p;
            }
        }

        public static OutPacket GiveForeignDebuff(int cid, long mask, MobSkill skill)
        {
            using (var p = new OutPacket(SendOpcodes.GiveForeignBuff))
            {
                p.WriteInt(cid);
                p.WriteLong(0);
                p.WriteLong(mask);
                p.WriteShort(skill.skillId);
                p.WriteShort(skill.skillLevel);
                p.WriteShort(0);
                p.WriteShort(0x84);
                p.WriteByte(0x03);
                return p;
            }
        }

        public static OutPacket ShowOwnBuffEffect(int skillid, byte effectid)
        {
            using (var p = new OutPacket(SendOpcodes.ShowItemGainInchat))
            {
                p.WriteByte(effectid);
                p.WriteInt(skillid);
                p.WriteByte(0x01); //Ver0.78?
                p.WriteByte(0x01); // probably buff level but we don't know it and it doesn't really matter
                return p;
            }
        }

        public static OutPacket ShowBuffeffect(int cid, int skillid, byte effectid)
        {
            return ShowBuffeffect(cid, skillid, effectid, 3, false);
        }

        public static OutPacket ShowBuffeffect(int cid, int skillid, byte effectid, byte direction)
        {
            using (var p = new OutPacket(SendOpcodes.ShowForeignEffect))
            {
                p.WriteInt(cid); // ?
                p.WriteByte(effectid);
                p.WriteInt(skillid);
                p.WriteByte(0x02);
                p.WriteByte(0x01);
                if (direction !=  3)
                {
                    p.WriteByte(direction);
                }

                return p;
            }
        }

        public static OutPacket ShowBuffeffect(int cid, int skillid, byte effectid, byte direction, bool morph)
        {
            using (var p = new OutPacket(SendOpcodes.ShowForeignEffect))
            {
                p.WriteInt(cid);
                if (morph)
                {
                    p.WriteByte(0x01);
                    p.WriteInt(skillid);
                    p.WriteByte(direction);
                }
                p.WriteByte(effectid);
                p.WriteInt(skillid);
                p.WriteByte(0x01);
                if (direction !=  3)
                {
                    p.WriteByte(direction);
                }
                return p;
            }
        }

        public static OutPacket GiveGmHide(bool hidden)
        {
            using (var p = new OutPacket(SendOpcodes.Gm))
            {
                p.WriteByte(0x10);
                p.WriteBool(hidden);
                return p;
            }
        }

        #region 怪物
        public static OutPacket KillMonster(int oid, bool animation)
        {
            return KillMonster(oid, (byte)(animation ? 1 : 0));
        }

        /**
         * Gets a packet telling the client that a monster was killed.
         *
         * @param oid The objectID of the killed monster.
         * @param animation 0 = dissapear, 1 = fade out, 2+ = special
         * @return The kill monster packet.
         */
        public static OutPacket KillMonster(int oid, byte animation)
        {

            using (OutPacket p = new OutPacket(SendOpcodes.KillMonster))
            {
                p.WriteInt(oid);
                p.WriteByte(animation); // Not a boolean, really an int type
                return p;
            }
        }

        public static OutPacket SpawnFakeMonster(MapleMonster life, int effect)
        {
            using (OutPacket p = new OutPacket(SendOpcodes.SpawnMonsterControl))
            {
                p.WriteByte(0x01);
                p.WriteInt(life.ObjectId);
                p.WriteByte(0x01);
                p.WriteInt(life.Id);
                p.WriteByte(0x00);
                p.WriteShort(0);
                p.WriteLong(0);
                p.WriteInt(0);
                p.WriteByte(0x88);
                p.WriteInt(0);
                p.WriteShort(0);
                p.WriteShort((short)life.Position.X);
                p.WriteShort((short)life.Position.Y);
                p.WriteByte((byte)life.Stance);
                p.WriteShort((short)life.StartFh);
                p.WriteShort((short)life.Fh);
                if (effect > 0)
                {
                    p.WriteByte((byte)effect);
                    p.WriteByte(0);
                    p.WriteShort(0);
                }
                p.WriteShort(-2);
                p.WriteInt(0);

                return p;
            }
        }

        public static OutPacket SpawnMonster(MapleMonster life, bool newSpawn)
        {
            return SpawnMonsterInternal(life, false, newSpawn, false, 0, false);
        }

        public static OutPacket SpawnMonster(MapleMonster life, bool newSpawn, byte effect)
        {
            return SpawnMonsterInternal(life, false, newSpawn, false, effect, false);
        }

        public static OutPacket ControlMonster(MapleMonster life, bool newSpawn, bool aggro)
        {
            return SpawnMonsterInternal(life, true, newSpawn, aggro, 0, false);
        }

        public static OutPacket StopControllingMonster(int oid)
        {
            using (var p = new OutPacket(SendOpcodes.SpawnMonsterControl))
            {
                p.WriteByte(0x00);
                p.WriteInt(oid);
                return p;
            }
        }


        private static OutPacket SpawnMonsterInternal(MapleMonster life, bool requestController, bool newSpawn, bool aggro, byte effect, bool makeInvis)
        {
            if (makeInvis)
            {
                using (OutPacket p = new OutPacket(SendOpcodes.SpawnMonsterControl))
                {
                    p.WriteByte(0x00);
                    p.WriteInt(life.ObjectId);
                    return p;
                }
            }

            using (OutPacket p = new OutPacket(requestController ? SendOpcodes.SpawnMonsterControl : SendOpcodes.SpawnMonster))
            {
                if (requestController)
                {
                    if (aggro)
                    {
                        p.WriteByte(0x02);
                    }
                    else
                    {
                        p.WriteByte(0x01);
                    }
                }



                p.WriteInt(life.ObjectId);
                p.WriteByte(0x01); // ????!? either 5 or 1?
                p.WriteInt(life.Id);
                p.WriteByte(0x00);
                p.WriteShort(0);
                p.WriteLong(0);
                p.WriteInt(0);
                p.WriteByte(0x88);
                p.WriteInt(0);
                p.WriteShort(0);
                p.WriteShort((short)life.Position.X);
                p.WriteShort((short)life.Position.Y);
                p.WriteByte((byte)life.Stance);
                p.WriteShort(0); // ?
                p.WriteShort((short)life.Fh);
                if (effect > 0)
                {
                    p.WriteByte(effect);
                    p.WriteByte(0x00);
                    p.WriteShort(0);
                    if (effect == 15)
                    {
                        //(Dojo spawn effect)
                        p.WriteByte(0x00);
                    }
                }
                if (newSpawn)
                {
                    p.WriteShort(-2);
                }
                else
                {
                    p.WriteShort(-1);
                }
                p.WriteInt(0);

                return p;
            }
        }

        public static OutPacket ApplyMonsterStatus(int oid, Dictionary<MonsterStatus, int> stats, int skill, bool monsterSkill, int delay)
        {
            return ApplyMonsterStatus(oid, stats, skill, monsterSkill, delay, null);
        }

        public static OutPacket ApplyMonsterStatus(int oid, Dictionary<MonsterStatus, int> stats, int skill, bool monsterSkill, int delay, MobSkill mobskill)
        {
            // 9B 00 67 40 6F 00 80 00 00 00 01 00 FD FE 30 00 08 00 64 00 01
            // 1D 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 10 00 01 00 79 00 01 00 B4 78 00 00 00 00 84 03
            // B4 00 A8 90 03 00 00 00 04 00 01 00 8C 00 03 00 14 00 4C 04 02
            // D8 00 EF AE F4 00 00 00 01 00 01 00 5D 43 23 00 0E 00 E8 03 02

            using (var p = new OutPacket(SendOpcodes.ApplyMonsterStatus))
            {
                p.WriteInt(oid);
                int mask = 0;
                foreach (MonsterStatus stat in stats.Keys)
                {
                    mask |= (int)stat;
                }
                p.WriteLong(0);
                p.WriteInt(0);
                p.WriteInt(mask);
                foreach (int val in stats.Values)
                {
                    p.WriteShort((short)val);
                    if (monsterSkill)
                    {
                        p.WriteShort(mobskill.skillId);
                        p.WriteShort(mobskill.skillLevel);
                    }
                    else
                    {
                        p.WriteInt(skill);
                    }
                    p.WriteShort(0); // as this looks similar to giveBuff this
                    // might actually be the buffTime but it's not displayed anywhere

                }
                p.WriteShort((short)delay); // delay in ms
                p.WriteByte(0x02); // ?

                return p;
            }
        }


        public static OutPacket MoveMonsterResponse(int objectid, short moveid, int currentMp, bool useSkills, byte skillId = 0, byte skillLevel = 0)
        {
            // A1 00 18 DC 41 00 01 00 00 1E 00 00 00
            // A1 00 22 22 22 22 01 00 00 00 00 00 00
            // EE 00 D5 C9 38 00 07 00 00 0F 00 00 00
            // F2 00 2E 96 00 00 01 00 00 0F 00 00 00

            using (var p = new OutPacket(SendOpcodes.MoveMonsterResponse))
            {
                p.WriteInt(objectid);
                p.WriteShort(moveid);
                p.WriteBool(useSkills);
                p.WriteShort((short)currentMp);
                p.WriteByte(skillId);
                p.WriteByte(skillLevel);
                return p;
            }
        }

        public static OutPacket MoveMonster(int useskill, int skill, int skill1, int skill2, int skill3, int oid, Point startPos, List<ILifeMovementFragment> moves)
        {
            using (var mplew = new OutPacket(SendOpcodes.MoveMonster))
            {
                mplew.WriteInt(oid);
                mplew.WriteByte(0x00);
                mplew.WriteByte((byte)useskill); // 0
                mplew.WriteByte((byte)skill); // -1
                mplew.WriteByte((byte)skill1); // 0
                mplew.WriteByte((byte)skill2); // 0
                mplew.WriteByte((byte)skill3); // 0
                mplew.WriteByte(0); // 0
                mplew.WriteShort((short)startPos.X);
                mplew.WriteShort((short)startPos.Y);
                serializeMovementList(mplew, moves);

                return mplew;
            }
        }

        private static void serializeMovementList(OutPacket p, List<ILifeMovementFragment> moves)
        {
            p.WriteByte((byte)moves.Count);
            foreach (ILifeMovementFragment move in moves)
            {
                move.Serialize(p);
            }
        }

        public static OutPacket ShowMonsterHp(int oid, byte remhppercentage)
        {
            using (var p = new OutPacket(SendOpcodes.ShowMonsterHp))
            {
                p.WriteInt(oid);
                p.WriteByte(remhppercentage);
                return p;
            }
        }

        public static OutPacket ShowBossHp(int oid, int currHp, int maxHp, byte tagColor, byte tagBgColor)
        {
            //53 00 05 21 B3 81 00 46 F2 5E 01 C0 F3 5E 01 04 01
            //00 81 B3 21 = 8500001 = Pap monster ID
            //01 5E F3 C0 = 23,000,000 = Pap max HP
            //04, 01 - boss bar color/background color as provided in WZ 

            using (var p = new OutPacket(SendOpcodes.BossEnv))
            {
                p.WriteByte(0x05);
                p.WriteInt(oid);
                p.WriteInt(currHp);
                p.WriteInt(maxHp);
                p.WriteByte(tagColor);
                p.WriteByte(tagBgColor);
                return p;
            }
        }

        public static OutPacket DamageMonster(int oid, int damage)
        {
            using (var p = new OutPacket(SendOpcodes.DamageMonster))
            {
                p.WriteInt(oid);
                p.WriteByte(0x00);
                p.WriteInt(damage);
                return p;
            }
        }

        public static OutPacket HealMonster(int oid, int heal)
        {
            using (var p = new OutPacket(SendOpcodes.DamageMonster))
            {
                p.WriteInt(oid);
                p.WriteByte(0x00);
                p.WriteInt(-heal);
                return p;
            }
        }

        public static OutPacket CancelMonsterStatus(int oid, Dictionary<MonsterStatus, int> stats)
        {
            // D9 00 EF AE F4 00 00 00 01 00 03 //074
            using (var p = new OutPacket(SendOpcodes.CancelMonsterStatus))
            {
                p.WriteInt(oid);
                int mask = stats.Keys.Aggregate(0, (current, stat) => current | (int) stat);
                p.WriteLong(0);
                p.WriteInt(0);
                p.WriteInt(mask);
                p.WriteByte(0x03);

                return p;
            }
        }

        #endregion
    }
}
