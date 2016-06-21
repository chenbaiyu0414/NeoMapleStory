using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using NeoMapleStory.Core;
using NeoMapleStory.Core.Database.Models;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Client.AntiCheat;
using NeoMapleStory.Game.Client.Message;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Movement;
using NeoMapleStory.Game.Quest;
using NeoMapleStory.Game.Script.NPC;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Packet;
using static NeoMapleStory.Game.Life.AbstractDealDamageHandler;

namespace NeoMapleStory.Server
{
    public static class ChannelPacketHandlers
    {
        public static void PLAYER_LOGGEDIN(MapleClient c, InPacket p)
        {
            var charId = p.ReadInt();

            var player =new MapleCharacter(charId, c, true);
            c.Player = player;
            c.Account = player.Account;

            var state = c.State;
            var allowLogin = true;
            var channelServer = MasterServer.Instance.ChannelServers[c.ChannelId];

            lock (c)
            {
                //try
                //{
                //    if (state == MapleClient.LoginState.ServerTransition)
                //    {

                //        for (String charName : c.loadCharacterNames(c.getWorld()))
                //        {
                //            if (worldInterface.isConnected(charName))
                //            {
                //                allowLogin = false;
                //                break;
                //            }
                //        }
                //    }
                //}
                //catch (RemoteException e)
                //{
                //    channelServer.reconnectWorld();
                //    allowLogin = false;
                //}
                if (state !=  LoginStateType.ServerTransition || allowLogin == false)
                {
                    c.Close();
                    return;
                }
                c.State =  LoginStateType.LoggedIn;
            }

            channelServer.Characters.Add(player);
            player.Client.Send(PacketCreator.ServerMessage("欢迎来到 NeoMapleStory，服务端测试中可能频繁掉线或卡死！"));
            //try
            //{
            //    List<PlayerBuffValueHolder> buffs = ChannelServer.getInstance(c.getChannel()).getWorldInterface().getBuffsFromStorage(cid);
            //    if (buffs != null)
            //    {
            //        c.Character.silentGiveBuffs(buffs);
            //    }
            //}
            //catch (RemoteException e)
            //{
            //    c.getChannelServer().reconnectWorld();
            //}

            //Connection con = DatabaseConnection.getConnection();
            //try
            //{
            //    PreparedStatement ps = con.prepareStatement("SELECT skillid, starttime,length FROM cooldowns WHERE characterid = ?");
            //    ps.setInt(1, c.getPlayer.getId());
            //    ResultSet rs = ps.executeQuery();
            //    while (rs.next())
            //    {
            //        if (rs.getLong("length") + rs.getLong("starttime") - System.currentTimeMillis() <= 0)
            //        {
            //            continue;
            //        }
            //        c.getPlayer.giveCoolDowns(rs.getInt("skillid"), rs.getLong("starttime"), rs.getLong("length"));
            //    }
            //    rs.close();
            //    ps.close();
            //    ps = con.prepareStatement("DELETE FROM cooldowns WHERE characterid = ?");
            //    ps.setInt(1, c.getPlayer.getId());
            //    ps.executeUpdate();
            //    ps.close();
            //}
            //catch (SQLException se)
            //{
            //    se.printStackTrace();
            //}

            c.Send(ChannelPacket.GetCharInfo(player));

            if (player.IsGm)
            {
                int[] skills = { 9001004, 9001001 };
                foreach (int i in skills)
                {
                    SkillFactory.GetSkill(i).GetEffect(SkillFactory.GetSkill(i).MaxLevel).ApplyTo(player);
                }
            }


            if (player.GmLevel == 0)
            {
                Console.WriteLine($"玩家 [ {player.Name} ],Lv {player.Level} 进入了游戏服务器");
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, $"您进入了{c.ChannelId + 1} 线"));
            }
            else
            {
                Console.WriteLine($"管理员 [ {player.Name} ] 进入游戏服务器");
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, $"您进入了{c.ChannelId + 1} 线"));
                player.DropMessage($"[欢迎] 尊敬的管理员 {player.Name} ,当前在线人数为: {c.AppServer.SessionCount}");
            }

            var prefix = "";
            IMapleItem equip = null;
            player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(0xEF, out equip);

            if (equip != null && equip.ItemId == 1122017)
            {
                //1122018
                prefix = $"由于装备了{MapleItemInformationProvider.Instance.GetName(equip.ItemId)},打猎时额外获得10%的经验值奖励";
                player.DropMessage(PacketCreator.ServerMessageType.PinkText, prefix);
            }
            if (equip != null && equip.ItemId == 1122018)
            {
                //1122018
                prefix = $"由于装备了{MapleItemInformationProvider.Instance.GetName(equip.ItemId)},可以获得 2 倍的经验加成!";
                player.DropMessage(PacketCreator.ServerMessageType.PinkText, prefix);
            }

            if (player.GmLevel == 0)
            {
                if (player.Job.JobId >= 111 && player.Job.JobId <= 112)
                {
                    //清空斗气技能
                }
                else
                {
                    player.CancelAllBuffs();
                    player.ChangeSkillLevel(SkillFactory.GetSkill(1111002), 0, 0);
                }

                if (player.Job.JobId >= 511 && player.Job.JobId <= 512)
                {
                    //变身
                }
                else
                {
                    player.CancelAllBuffs();
                    player.ChangeSkillLevel(SkillFactory.GetSkill(5111005), 0, 0);
                }
                if (player.Job.JobId >= 510 && player.Job.JobId <= 512)
                {
                    //伪装木桶
                }
                else
                {
                    player.ChangeSkillLevel(SkillFactory.GetSkill(5101007), 0, 0);
                }
                if (player.Job.JobId != 512)
                {
                    //超級變身
                    player.CancelAllBuffs();
                    player.ChangeSkillLevel(SkillFactory.GetSkill(5121003), 0, 0);
                }

                if (player.Job.JobId >= 1311 && player.Job.JobId <= 1312)
                {
                    //信天翁
                }
                else
                {
                    player.CancelAllBuffs();
                    player.ChangeSkillLevel(SkillFactory.GetSkill(13111005), 0, 0);
                }

                if (player.Job.JobId >= 310 && player.Job.JobId <= 311)
                {
                    //快速箭
                }
                else
                {
                    player.CancelAllBuffs();
                    player.ChangeSkillLevel(SkillFactory.GetSkill(3101002), 0, 0);
                }

                if (player.Job.JobId >= 1411 && player.Job.JobId <= 1412)
                {
                    //骑士团的影分身
                }
                else
                {
                    player.CancelAllBuffs();
                    player.ChangeSkillLevel(SkillFactory.GetSkill(14111000), 0, 0);
                }

                if (player.Job.JobId >= 411 && player.Job.JobId <= 412)
                {
                    //影分身
                }
                else
                {
                    player.CancelAllBuffs();
                    player.ChangeSkillLevel(SkillFactory.GetSkill(4111002), 0, 0);
                }

                if (player.Job.JobId != 1511)
                {
                    //騎士團的變身
                    player.CancelAllBuffs();
                    player.ChangeSkillLevel(SkillFactory.GetSkill(15111002), 0, 0);
                }
            }

            if (player.Job == MapleJob.GhostKnight && player.Level <= 9)
            {
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, "[注意] 请在10级转职之前，加好基本技能点，转职请找NPC！"));
            }

            //player.sendKeymap();
            c.Send(PacketCreator.SendAutoHpPot(player.AutoHpPot));
            c.Send(PacketCreator.SendAutoMpPot(player.AutoMpPot));
            player.Map.AddPlayer(player);

            try
            {
                var buddies = player.Buddies.GetBuddies();
                var buddyIds = player.Buddies.GetBuddyIdList();
                //channelServer.getWorldInterface().loggedOn(player.getName(), player.getId(), c.getChannel(), buddyIds);
                //if (player.getParty() != null)
                //{
                //    channelServer.getWorldInterface().updateParty(player.getParty().getId(), PartyOperation.LOG_ONOFF, new MaplePartyCharacter(player));
                //}
                //CharacterIdChannelPair[] onlineBuddies = channelServer.getWorldInterface().multiBuddyFind(player.getId(), buddyIds);
                //for (CharacterIdChannelPair onlineBuddy : onlineBuddies)
                //{
                //    BuddylistEntry ble = player.getBuddylist().get(onlineBuddy.getCharacterId());
                //    ble.setChannel(onlineBuddy.getChannel());
                //    player.getBuddylist().put(ble);
                //}

                c.Send(PacketCreator.UpdateBuddylist(buddies));
                c.Send(PacketCreator.LoadFamily());

                //if (player.getFamilyId() > 0)
                //{
                //    c.getSession().write(MaplePacketCreator.getFamilyInfo(player));
                //}

                player.SendMacros();

                //if (player.getGuildId() > 0)
                //{
                //    c.getChannelServer().getWorldInterface().setGuildMemberOnline(player.getMGC(), true, c.getChannel());
                //    c.getSession().write(MaplePacketCreator.showGuildInfo(player));
                //    int allianceId = player.getGuild().getAllianceId();
                //    if (allianceId > 0)
                //    {
                //        MapleAlliance newAlliance = channelServer.getWorldInterface().getAlliance(allianceId);
                //        if (newAlliance == null)
                //        {
                //            newAlliance = MapleAlliance.loadAlliance(allianceId);
                //            channelServer.getWorldInterface().addAlliance(allianceId, newAlliance);
                //        }
                //        c.getSession().write(MaplePacketCreator.getAllianceInfo(newAlliance));
                //        c.getSession().write(MaplePacketCreator.getGuildAlliances(newAlliance, c));
                //        c.getChannelServer().getWorldInterface().allianceMessage(allianceId, MaplePacketCreator.allianceMemberOnline(player, true), player.getId(), -1);
                //    }
                //}
            }
            catch
            {
            }
            player.UpdatePartyMemberHp();
            //for (MapleQuestStatus status : player.getStartedQuests())
            //{
            //    if (status.hasMobKills())
            //    {
            //        c.getSession().write(MaplePacketCreator.updateQuestMobKills(status));
            //    }
            //}
            //CharacterNameAndId pendingBuddyRequest = player.getBuddylist().pollPendingRequest();
            //if (pendingBuddyRequest != null)
            //{
            //    player.getBuddylist().put(new BuddylistEntry(pendingBuddyRequest.getName(), pendingBuddyRequest.getId(), -1, false));
            //    c.getSession().write(MaplePacketCreator.requestBuddylistAdd(pendingBuddyRequest.getId(), pendingBuddyRequest.getName()));
            //}

            player.ShowNote();

            //if (!c.getPlayer.hasMerchant() && c.getPlayer.tempHasItems())
            //{
            //    c.getPlayer.dropMessage(1, "你有物品,可以通过雇用Npc领取物品!\r\n暂时还在调试中该功能!");
            //}

            //player.checkMessenger();
            //player.showMapleTips();
            //player.checkBerserk();
            //player.checkDuey();NPC杜宜 快递物品

            //player.expirationTask();过期物品
            c.Send(PacketCreator.ShowCharCash(player));
            c.Send(PacketCreator.WeirdStatUpdate());
        }

        public static void PLAYER_UPDATE(MapleClient c, InPacket p)
        {
            c.Player.Save();
            if ((c.Player.Map.MapId == 677000013) || (c.Player.Map.MapId == 677000013))
            {
                c.Player.StartMapEffect("尝试一下惨败的味道吧……哈哈哈哈哈", 5120000);
                c.Player.Save();
            }
        }

        public static void CHANGE_MAP_SPECIAL(MapleClient c, InPacket p)
        {
            p.ReadByte();
            var startwp = p.ReadMapleString();
            p.ReadShort();
            var portal = c.Player.Map.GetPortal(startwp);
            if (portal != null)
                portal.EnterPortal(c);
            else
                c.Send(PacketCreator.EnableActions());
        }

        public static void NPC_ACTION(MapleClient c, InPacket p)
        {
            using (var packet = new OutPacket(SendOpcodes.NpcAction))
            {
                var length = (int)p.AvailableCount;
                if (length == 6)
                {
                    packet.WriteInt(p.ReadInt());
                    packet.WriteShort(p.ReadShort());
                }
                else if (length > 6)
                {
                    packet.WriteBytes(p.ReadBytes(length - 9));
                }
                c.Send(packet);
            }
        }

        public static void MOVE_PLAYER(MapleClient c, InPacket p)
        {
            p.ReadByte();
            p.ReadLong();
            p.ReadLong(); //v079
            p.ReadLong(); //v079
            p.ReadLong(); //v079
            var res = AbstractMovementPacketHandler.ParseMovement(p);
            c.Player.Lastres = res;
            if (res != null)
            {
                if (p.AvailableCount != 18)
                {
                    Console.WriteLine("p.available != 18 (movement parsing error)");
                    return;
                }
                var player = c.Player;
                //try
                //{
                if (!player.IsHidden)
                {
                    c.Player.Map.BroadcastMessage(player, PacketCreator.MovePlayer(player.Id, res), false);
                }

                //if (CheatingOffense.FAST_MOVE.isEnabled() || CheatingOffense.HIGH_JUMP.isEnabled())
                //{
                //    checkMovementSpeed(player, res);
                //}

                AbstractMovementPacketHandler.UpdatePosition(res, player, 0);
                player.Map.MovePlayer(player, player.Position);
                //}
                //catch (ConcurrentModificationException cme)
                //{
                //}
                //catch (Exception e)
                //{
                //    log.warn("Failed to move player (" + player.getName() + ")");
                //}
            }
        }

        public static void CHANGE_MAP(MapleClient c, InPacket p)
        {
            if (p.AvailableCount == 0)
            {
                var ip = IPAddress.Parse(c.ChannelServer.Config.Ip);
                var port = (short)c.ChannelServer.Config.Port;

                c.Player.Save();
                c.Player.InCashShop = false;
                c.Player.CancelSavedBuffs();

                c.ChannelServer.Characters.Remove(c.Player);
                c.State =  LoginStateType.ServerTransition;

                c.Send(PacketCreator.GetChannelChange(ip, port));
            }
            else
            {
                p.ReadByte(); // 1 = from dying 2 = regular portals
                var targetid = p.ReadInt(); // FF FF FF FF

                var startwp = p.ReadMapleString();
                var portal = c.Player.Map.GetPortal(startwp);

                var player = c.Player;
                if (targetid != -1 && !c.Player.IsAlive)
                {
                    var executeStandardPath = true;
                    //if (player.getEventInstance() != null)
                    //{
                    //    executeStandardPath = player.getEventInstance().revivePlayer(player);
                    //}
                    if (executeStandardPath)
                    {
                        if (c.Player.HaveItem(5510000, 1, false, true))
                        {
                            c.Player.Hp = 50;
                            MapleInventoryManipulator.RemoveById(c, MapleInventoryType.Cash, 5510000, 1, true, false);
                            c.Player.ChangeMap(c.Player.Map, c.Player.Map.GetPortal(0));
                            c.Player.UpdateSingleStat(MapleStat.Hp, 50);
                            c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText,
                                "使用了原地复活术。死亡后您在当前地图复活。"));
                        }
                        else
                        {
                            player.Hp = 50;
                            if (c.Player.Map.ForcedReturnMapId != 999999999)
                            {
                                var to = c.Player.Map.ForcedReturnMap;
                                var pto = to.GetPortal(0);
                                player.Stance = 0;
                                player.ChangeMap(to, pto);
                            }
                            else
                            {
                                var to = c.Player.Map.ReturnMap;
                                var pto = to.GetPortal(0);
                                player.Stance = 0;
                                player.ChangeMap(to, pto);
                            }
                        }
                    }
                }
                else if (targetid != -1 && c.Player.GmLevel > 0)
                {
                    var to = c.ChannelServer.MapFactory.GetMap(targetid);
                    var pto = to.GetPortal(0);
                    player.ChangeMap(to, pto);
                }
                else if (targetid != -1 && c.Player.GmLevel == 0)
                {
                    var to = c.ChannelServer.MapFactory.GetMap(targetid);
                    if (c.Player.GmLevel > 0 || (player.Map.MapId == 0 && to.MapId == 10000) ||
                        (player.Map.MapId == 914090010 && to.MapId == 914090011) ||
                        (player.Map.MapId == 914090011 && to.MapId == 914090012) ||
                        (player.Map.MapId == 914090012 && to.MapId == 914090013) ||
                        (player.Map.MapId == 914090013 && to.MapId == 140090000))
                    {
                        var pto = to.GetPortal(0);
                        player.ChangeMap(to, pto);
                    }
                    else
                    {
                        c.Send(PacketCreator.EnableActions());
                        Console.WriteLine("玩家 {0} 试图以非正常方式切换地图！", c.Player.Name);
                    }
                }
                else if (portal != null)
                {
                    portal.EnterPortal(c);
                }
                else
                {
                    c.Send(PacketCreator.EnableActions());
                    Console.WriteLine("Portal {0} not found on map {1}", startwp, c.Player.Map.MapId);
                }
            }
        }

        public static void GENERAL_CHAT(MapleClient c, InPacket p)
        {
            var text = p.ReadMapleString();
            var show = p.ReadBool();
            Console.WriteLine(show);
            if (c.Player.AntiCheatTracker.TextSpam(text) && c.Player.GmLevel == 0)
            {
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, "Too much chatting"));
                return;
            }

            if (text.Length > 70 && c.Player.GmLevel == 0)
            {
                return;
            }

            c.Player.Map.BroadcastMessage(PacketCreator.GetChatText(c.Player.Id, text,
                c.Player.GmLevel >= 3 && /*c.getChannelServer().allowGmWhiteText()*/ true, show));

            //if (!CommandProcessor.getInstance().processCommand(c, text))
            //{
            //    if (c.getPlayer.isMuted() || (c.getPlayer.getMap().getMuted() && !c.getPlayer.isGM()))
            //    {
            //        c.getPlayer.dropMessage(5, c.getPlayer.isMuted() ? "You are " : "The map is " + "muted, therefore you are unable to talk.");
            //        return;
            //    }
            //    c.getPlayer.getMap().broadcastMessage(MaplePacketCreator.getChatText(c.getPlayer.getId(), text, c.getPlayer.hasGMLevel(3) && c.getChannelServer().allowGmWhiteText(), show));
            //}
        }

        public static void NPC_TALK(MapleClient c, InPacket p)
        {
            var player = c.Player;
            //player.setCurrenttime(System.currentTimeMillis());
            //if (player.getCurrenttime() - player.getLasttime() < player.getDeadtime())
            //{
            //    player.dropMessage("系统错误.请稍后再试");
            //    c.getSession().write(MaplePacketCreator.enableActions());
            //    return;
            //}
            //player.setLasttime(System.currentTimeMillis());

            var oid = p.ReadInt();
            p.ReadInt();

            if (player.Map.Mapobjects.ContainsKey(oid) == false ||
                player.Map.Mapobjects[oid].GetType() != MapleMapObjectType.Npc)
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }

            var npc = (MapleNpc)player.Map.Mapobjects[oid];

            //if (npc.Id == 9010009)
            //{
            //    c.Send(PacketCreator.sendDuey((byte)9, DueyActionHandler.loadItems(player)));
            //}

            if (npc.HasShop())
            {
                //if (player.getShop() != null)
                //{
                //    player.setShop(null);
                //    c.Send(PacketCreator.confirmShopTransaction((byte)20));
                //}
                //npc.sendShop(c);
            }
            else
            {
                //if (c.getCM() != null || c.getQM() != null)
                //{
                //    c.Send(PacketCreator.EnableActions());
                //    return;
                //}
                //if (c.getCM() == null)
                //{
                NpcScriptManager.Instance.Start(c, npc.Id);
                //}
                // 0 = next button
                // 1 = yes no
                // 2 = accept decline
                // 5 = select a link
            }
        }

        public static void NPC_TALK_MORE(MapleClient c, InPacket p)
        {
            var talkType = p.ReadByte(); // 00 (last msg type I think)
            var isCountinue = p.ReadByte(); // 00 = end chat, 01 == follow action


            if (talkType == 2)
            {
                //if (action != 0)
                //{
                //    String returnText = p.ReadMapleString();
                //    if (c.qm != null)
                //    {
                //        c.getQM().setGetText(returnText);
                //        if (c.getQM().isStart())
                //        {
                //            QuestScriptManager.getInstance().start(c, action, lastMsg, -1);
                //        }
                //        else
                //        {
                //            QuestScriptManager.getInstance().end(c, action, lastMsg, -1);
                //        }
                //    }
                //    else
                //    {
                //        c.getCM().setGetText(returnText);
                //        NPCScriptManager.getInstance().action(c, action, lastMsg, -1);
                //    }
                //}
                //else
                //{
                //    if (c.getQM() != null)
                //    {
                //        c.getQM().dispose();
                //    }
                //    else
                //    {
                //        c.getCM().dispose();
                //    }
                //}
            }
            else
            {
                var selection = -1;
                if (p.AvailableCount >= 4)
                {
                    selection = p.ReadInt();
                    if (selection < 0)
                    {
                        //if (c.getQM() != null)
                        //{
                        //    c.getQM().dispose();
                        //}
                        //else
                        //{
                        //    c.getCM().dispose();
                        //}
                        return;
                    }
                }
                else if (p.AvailableCount > 0)
                {
                    selection = p.ReadByte();
                    if (p.AvailableCount == 1)
                    {
                        //c.getCM().setCash(p.ReadByte() == 1);
                    }
                }
                //if (c.getQM() != null)
                //{
                //    if (c.getQM().isStart())
                //    {
                //        QuestScriptManager.getInstance().start(c, action, lastMsg, selection);
                //    }
                //    else
                //    {
                //        QuestScriptManager.getInstance().end(c, action, lastMsg, selection);
                //    }
                //}
                //else 
                if (NpcScriptManager.Instance.GetCm(c) != null)
                {
                    NpcScriptManager.Instance.Choice(c, isCountinue, talkType, selection);
                    Console.WriteLine($"{isCountinue} {talkType} {selection}");
                }
            }
        }

        public static void MOVE_LIFE(MapleClient c, InPacket p)
        {
            var objectid = p.ReadInt();
            var moveid = p.ReadShort();

            IMapleMapObject mmo;
            if (!c.Player.Map.Mapobjects.TryGetValue(objectid, out mmo) || mmo.GetType() != MapleMapObjectType.Monster)
            {
                return;
            }

            var monster = (MapleMonster)mmo;
            var noPacket = monster.IsMoveLock;

            List<ILifeMovementFragment> res = null;
            var skillByte = p.ReadByte();
            var skill = p.ReadByte();
            var skill1 = p.ReadByte();
            var skill2 = p.ReadByte();
            var skill3 = p.ReadByte();
            p.ReadByte();

            MobSkill toUse = null;

            if (skillByte == 1 && monster.Stats.GetSkillsCount() > 0)
            {
                var random = (int)(Randomizer.NextDouble() * monster.Stats.GetSkillsCount());
                var skillToUse = monster.Stats.GetSkills()[random];
                toUse = MobSkillFactory.GetMobSkill(skillToUse.Item1, skillToUse.Item2);
                if (!monster.CanUseSkill(toUse))
                {
                    toUse = null;
                }
            }

            if (skill1 >= 100 && skill1 <= 200 && monster.Stats.HasSkill(skill1, skill2))
            {
                var skillData = MobSkillFactory.GetMobSkill(skill1, skill2);
                if (skillData != null && monster.CanUseSkill(skillData))
                {
                    skillData.ApplyEffect(c.Player, monster, true);
                }
            }

            p.ReadByte();
            p.ReadInt(); // whatever
            p.ReadLong();

            int startX = p.ReadShort(); // hmm.. startpos?
            int startY = p.ReadShort(); // hmm...
            var startPos = new Point(startX, startY);

            res = AbstractMovementPacketHandler.ParseMovement(p);

            if (monster.GetController() != c.Player)
            {
                if (monster.IsAttackedBy(c.Player))
                {
                    // aggro and controller change
                    monster.SwitchController(c.Player, true);
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (skill == 0 && monster.ControllerKnowsAboutAggro && !monster.Stats.IsMobile)
                {
                    monster.ControllerHasAggro = false;
                    monster.ControllerKnowsAboutAggro = false;
                }
                if (!monster.Stats.IsFirstAttack)
                {
                    monster.ControllerHasAggro = true;
                    monster.ControllerKnowsAboutAggro = true;
                }
            }

            var aggro = monster.ControllerHasAggro;
            if (toUse != null)
            {
                if (!noPacket)
                {
                    c.Send(PacketCreator.MoveMonsterResponse(objectid, moveid, monster.Mp, aggro, toUse.SkillId,
                        toUse.SkillLevel));
                }
            }
            else
            {
                if (!noPacket)
                {
                    c.Send(PacketCreator.MoveMonsterResponse(objectid, moveid, monster.Mp, aggro));
                }
            }

            if (aggro)
            {
                monster.ControllerKnowsAboutAggro = true;
            }

            if (res != null)
            {
                //if (p.available() != 9) {
                //log.warn("p.available != 9 (movement parsing error)");
                //return;
                //}
                var packet = PacketCreator.MoveMonster(skillByte, skill, skill1, skill2, skill3, objectid, startPos, res);
                c.Player.Map.BroadcastMessage(c.Player, packet, monster.Position);
                AbstractMovementPacketHandler.UpdatePosition(res, monster, -1);
                c.Player.Map.MoveMonster(monster, monster.Position);
                c.Player.AntiCheatTracker.CheckMoveMonster(monster.Position);
            }
        }

        private static bool IsFinisher(int skillId)
        {
            return (skillId >= 1111003 && skillId <= 1111006) || (skillId >= 11111002 && skillId <= 11111003) ||
                   skillId == 21110004 || skillId == 21100004 || skillId == 21120006;
        }

        public static void CLOSE_RANGE_ATTACK(MapleClient c, InPacket p)
        {
            var attack = ParseDamage(c.Player, p, false);
            var player = c.Player;
            var packet = PacketCreator.CloseRangeAttack(player.Id, attack.Skill, attack.Stance,
                attack.NumAttackedAndDamage, attack.AllDamage, attack.Speed, attack.Pos);
            player.Map.BroadcastMessage(player, packet, false, true);
            // handle combo orb consume
            var numFinisherOrbs = 0;
            var comboBuff = player.GetBuffedValue(MapleBuffStat.Combo);
            var aranCombo = SkillFactory.GetSkill(21000000);
            var aranComboSkillLevel = player.GetSkillLevel(aranCombo);
            if (IsFinisher(attack.Skill))
            {
                if (comboBuff != null)
                {
                    numFinisherOrbs = comboBuff.Value - 1;
                }
                //player.handleOrbconsume();
            }
            else if (attack.NumAttacked > 0)
            {
                // handle combo orbgain
                if (attack.Skill != 1111008 && comboBuff != null)
                {
                    // 虎咆哮不给予combo?14101006
                    //player.handleOrbgain();
                }
                if (aranComboSkillLevel > 0)
                {
                    for (var i = 0; i < attack.NumAttacked; i++)
                    {
                        //player.handleComboGain();
                    }
                }
            }
            if (attack.Skill != 14101006 && comboBuff != null)
            {
                // 吸血�?能不给连击点�?
                //player.handleOrbgain();
            }
            if (aranComboSkillLevel > 0)
            {
                for (var i = 0; i < attack.NumAttacked; i++)
                {
                    //player.handleComboGain();
                }
            }

            //if (attack.numAttacked > 0 && attack.skill == 1311005)
            //{ 
            //    //龙之献祭
            //    int totDamageToOneMonster = attack.allDamage.get(0).getRight().get(0); // sacrifice attacks only 1 mob with 1 attack
            //    player.setHp(player.getHp() - totDamageToOneMonster * attack.getAttackEffect(player).getX() / 100);
            //    player.updateSingleStat(MapleStat.HP, player.getHp());
            //}

            // handle charged blow
            //if (attack.numAttacked > 0 && attack.skill == 1211002)
            //{ 
            //    //属�?�攻�? - [�?高等�?:30]\n赋予武器全属�?, 攻击6个以下多个�?�物。一定的几率使�?�物昏迷�?
            //    boolean advcharge_prob = false;
            //    int advcharge_level = player.getSkillLevel(SkillFactory.getSkill(1220010));
            //    if (advcharge_level > 0)
            //    {
            //        MapleStatEffect advcharge_effect = SkillFactory.getSkill(1220010).getEffect(advcharge_level);
            //        advcharge_prob = advcharge_effect.makeChanceResult();
            //    }
            //    else
            //    {
            //        advcharge_prob = false;
            //    }
            //    if (!advcharge_prob)
            //    {
            //        player.cancelEffectFromBuffStat(MapleBuffStat.WK_CHARGE);
            //    }
            //}

            var maxdamage = player.LocalMaxBasedDamage;
            var attackCount = 1;
            if (attack.Skill != 0)
            {
                //MapleStatEffect effect = attack.getAttackEffect(player);
                //attackCount = effect.getattackcount();
                //maxdamage *= effect.getDamage() / 100.0;
                //maxdamage *= attackCount;
            }
            maxdamage = Math.Min(maxdamage, 199999);
            //if (attack.skill == 4211006)
            //{ //金钱炸弹
            //    maxdamage = 700000;
            //}
            //else if (numFinisherOrbs > 0)
            //{
            //    maxdamage *= numFinisherOrbs;
            //}
            //else if (comboBuff != null)
            //{
            //    ISkill combo = SkillFactory.getSkill(1111002);
            //    int comboLevel = player.getSkillLevel(combo);
            //    if (comboLevel == 0)
            //    {
            //        combo = SkillFactory.getSkill(11111001);
            //        comboLevel = player.getSkillLevel(combo);
            //    }
            //    MapleStatEffect comboEffect = combo.getEffect(comboLevel);
            //    double comboMod = 1.0 + (comboEffect.getDamage() / 100.0 - 1.0) * (comboBuff - 1);
            //    maxdamage *= comboMod;
            //}
            if (numFinisherOrbs == 0 && IsFinisher(attack.Skill))
            {
                return; // can only happen when lagging o.o
            }
            if (IsFinisher(attack.Skill))
            {
                maxdamage = 199999; // FIXME reenable damage calculation for finishers
            }
            if (attack.Skill > 0)
            {
                var skill = SkillFactory.GetSkill(attack.Skill);
                var skillLevel = player.GetSkillLevel(skill);
                var effect = skill.GetEffect(skillLevel);
                //if (effect_.GetCoolDown() > 0)
                //{
                //    player.Client.Send(PacketCreator.SkillCooldown(attack.skill, effect_.getCooldown()));
                //    var jobname = TimerManager.Instance.ScheduleJob(new CancelCooldownAction(c.getPlayer, attack.skill), effect_.getCooldown());
                // player.AddCooldown(attack.skill, DateTime.Now.GetTimeMilliseconds(), effect_.getCooldown() , jobname);
                //}
            }
            //if (attack.skill == 21120002 || attack.skill == 21110002 || attack.skill == 21110006 || attack.skill == 21120009 || attack.skill == 21120010 || attack.skill == 21110007 || attack.skill == 21110004 || attack.skill == 21100004)
            //{
            //    ISkill skill = SkillFactory.getSkill(attack.skill);
            //    int skillLevel = c.getPlayer.getSkillLevel(skill);
            //    MapleStatEffect effect_ = skill.getEffect(skillLevel);
            //    if (effect_.getCooldown() > 0)
            //    {
            //        c.getSession().write(MaplePacketCreator.skillCooldown(attack.skill, effect_.getCooldown()));
            //        ScheduledFuture <?> timer = TimerManager.getInstance().schedule(new CancelCooldownAction(c.getPlayer, attack.skill), effect_.getCooldown() * 1000);
            //        c.getPlayer.addCooldown(attack.skill, System.currentTimeMillis(), effect_.getCooldown() * 1000, timer);
            //    }
            //}
            ApplyAttack(attack, player, maxdamage, attackCount);
        }

        public static void TAKE_DAMAGE(MapleClient c, InPacket p)
        {
            var player = c.Player;
            p.Skip(4);
            int damagefrom = p.ReadByte();
            p.Skip(1);
            var damage = p.ReadInt();
            var oid = 0;
            var monsteridfrom = 0;
            byte pgmr = 0;
            byte direction = 0;
            byte posX = 0;
            byte posY = 0;
            var fake = 0;
            var isPgmr = false;
            var isPg = true;
            var mpattack = 0;
            MapleMonster attacker = null;
            if (damagefrom != 0xFE)
            {
                monsteridfrom = p.ReadInt();
                oid = p.ReadInt();
                if (!player.Map.Mapobjects.ContainsKey(oid) ||
                    !player.Map.Mapobjects[oid].GetType().Equals(MapleMapObjectType.Monster))
                {
                    c.Send(PacketCreator.EnableActions());
                    return;
                }
                attacker = (MapleMonster)player.Map.Mapobjects[oid];
                direction = p.ReadByte();
            }
            try
            {
                if (damagefrom != 0xFF && damagefrom != 0xFE && attacker != null)
                {
                    var attackInfo = MobAttackInfoFactory.GetMobAttackInfo(attacker, damagefrom);
                    if (damage != 0xFF)
                    {
                        if (attackInfo.IsDeadlyAttack)
                        {
                            mpattack = player.Mp - 1;
                        }
                        else
                        {
                            mpattack += attackInfo.MpBurn;
                        }
                    }

                    var skill = MobSkillFactory.GetMobSkill(attackInfo.DiseaseSkill, attackInfo.DiseaseLevel);
                    if (skill != null && damage > 0)
                    {
                        skill.ApplyEffect(player, attacker, false);
                    }
                    attacker.Mp -= attackInfo.MpCon;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (damage == 0xFF)
            {
                var job = player.Job.JobId / 10 - 40;
                fake = 4020002 + job * 100000;
                if (damagefrom == -1 && damagefrom != -2 &&
                    player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.ContainsKey(10))
                {
                    int[] guardianSkillId = { 1120005, 1220006 };
                    foreach (var guardian in guardianSkillId)
                    {
                        var guardianSkill = SkillFactory.GetSkill(guardian);
                        if (player.GetSkillLevel(guardianSkill) > 0 && attacker != null)
                        {
                            var monsterStatusEffect =
                                new MonsterStatusEffect(new Dictionary<MonsterStatus, int> { { MonsterStatus.Stun, 1 } },
                                    guardianSkill, false);
                            attacker.ApplyStatus(player, monsterStatusEffect, false, 2 * 1000);
                        }
                    }
                }
            }

            if ((damage < 0xFF || damage > 60000) && player.GmLevel == 0)
            {
                Console.WriteLine($"{player.Name} 受到异常的怪物攻击 {monsteridfrom} : {damage}");
                c.Close();
                return;
            }

            player.AntiCheatTracker.CheckTakeDamage();

            if (damage > 0)
            {
                player.AntiCheatTracker.SetAttacksWithoutHit(0);
                player.AntiCheatTracker.ResetHpRegen();
                player.AntiCheatTracker.ResetMpRegen();
                //player.resetAfkTimer();
            }
            if (damage == 0xFF)
            {
                player.AntiCheatTracker.RegisterOffense(CheatingOffense.AlwaysOneHit);
            }

            if (!player.IsHidden && player.IsAlive /* && !player.hasGodmode() && !player.getInvincible()*/)
            {
                if (player.GetBuffedValue(MapleBuffStat.Morph) != null && damage > 0)
                {
                    //player.cancelMorphs();
                }
                //if (player.hasBattleShip())
                //{
                //    player.handleBattleShipHpLoss(damage);
                //    player.getMap().broadcastMessage(player, MaplePacketCreator.damagePlayer(damagefrom, monsteridfrom, player.getId(), damage, fake, direction, is_pgmr, pgmr, is_pg, oid, pos_x, pos_y), false);
                //    player.checkBerserk();
                //}
                if (damagefrom == 0xFF)
                {
                    var pguard = player.GetBuffedValue(MapleBuffStat.Powerguard);
                    if (pguard != null)
                    {
                        IMapleMapObject temp;
                        player.Map.Mapobjects.TryGetValue(oid, out temp);
                        attacker = temp as MapleMonster;
                        if (attacker != null)
                        {
                            var bouncedamage = (int)(damage * (pguard / 100));
                            bouncedamage = Math.Min(bouncedamage, attacker.MaxHp / 10);
                            player.Map.DamageMonster(player, attacker, bouncedamage);
                            damage -= bouncedamage;
                            player.Map.BroadcastMessage(player, PacketCreator.DamageMonster(oid, bouncedamage), false,
                                true);
                            player.CheckMonsterAggro(attacker);
                        }
                    }
                }
                if (damagefrom == 0 && attacker != null)
                {
                    var manaReflection = player.GetBuffedValue(MapleBuffStat.ManaReflection);
                    if (manaReflection != null)
                    {
                        var skillId = player.GetBuffSource(MapleBuffStat.ManaReflection);
                        var manaReflectSkill = SkillFactory.GetSkill(skillId);
                        if (manaReflectSkill.GetEffect(player.GetSkillLevel(manaReflectSkill)).MakeChanceResult())
                        {
                            var bouncedamage = (int)(damage * (manaReflection / 100.0));
                            if (bouncedamage > attacker.MaxHp * 0.2)
                            {
                                bouncedamage = (int)(attacker.MaxHp * 0.2);
                            }
                            player.Map.DamageMonster(player, attacker, bouncedamage);
                            player.Map.BroadcastMessage(player, PacketCreator.DamageMonster(oid, bouncedamage), false,
                                true);
                            player.Client.Send(PacketCreator.ShowOwnBuffEffect(skillId, 5));
                            player.Map.BroadcastMessage(player, PacketCreator.ShowBuffeffect(player.Id, skillId, 5, 3),
                                false);
                        }
                    }
                }
                if (damagefrom == 0xFF)
                {
                    try
                    {
                        int[] achillesSkillId = { 1120004, 1220005, 1320005 };
                        foreach (var achilles in achillesSkillId)
                        {
                            var achillesSkill = SkillFactory.GetSkill(achilles);
                            if (player.GetSkillLevel(achillesSkill) > 0)
                            {
                                var multiplier = achillesSkill.GetEffect(player.GetSkillLevel(achillesSkill)).X / 1000.0;
                                var newdamage = (int)(multiplier * damage);
                                damage = newdamage;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                if (player.GetBuffedValue(MapleBuffStat.MagicGuard) != null && mpattack == 0)
                {
                    var mploss = (short)(damage * (player.GetBuffedValue(MapleBuffStat.MagicGuard) / 100.0));
                    var hploss = (short)(damage - mploss);
                    if (mploss > player.Mp)
                    {
                        hploss += (short)(mploss - player.Mp);
                        mploss = (short)player.Mp;
                    }


                    player.Hp -= hploss;
                    player.Mp -= mploss;
                    var stats = new List<Tuple<MapleStat, int>>
                    {
                        new Tuple<MapleStat, int>(MapleStat.Hp, player.Hp),
                        new Tuple<MapleStat, int>(MapleStat.Mp, player.Mp)
                    };

                    c.Send(PacketCreator.UpdatePlayerStats(stats));
                }
                else if (player.GetBuffedValue(MapleBuffStat.Mesoguard) != null)
                {
                    damage = damage % 2 == 0 ? damage / 2 : damage / 2 + 1;
                    var mesoloss = (int)(damage * (player.GetBuffedValue(MapleBuffStat.Mesoguard) / 100.0));
                    if (player.Meso.Value < mesoloss)
                    {
                        player.GainMeso(-player.Meso.Value, false);
                        player.CancelBuffStats(MapleBuffStat.Mesoguard);
                    }
                    else
                    {
                        player.GainMeso(-mesoloss, false);
                    }

                    player.Hp -= (short)damage;
                    player.Mp -= (short)mpattack;
                    var stats = new List<Tuple<MapleStat, int>>
                    {
                        new Tuple<MapleStat, int>(MapleStat.Hp, player.Hp),
                        new Tuple<MapleStat, int>(MapleStat.Mp, player.Mp)
                    };

                    c.Send(PacketCreator.UpdatePlayerStats(stats));
                }
                else
                {
                    player.Hp -= (short)damage;
                    player.Mp -= (short)mpattack;
                    var stats = new List<Tuple<MapleStat, int>>
                    {
                        new Tuple<MapleStat, int>(MapleStat.Hp, player.Hp),
                        new Tuple<MapleStat, int>(MapleStat.Mp, player.Mp)
                    };

                    c.Send(PacketCreator.UpdatePlayerStats(stats));
                }
                if (damagefrom == 0xFE)
                {
                    player.Map.BroadcastMessage(player,
                        PacketCreator.DamagePlayer(0xFF, 9400711, player.Id, damage, 0, 0, false, 0, false, 0, 0, 0),
                        false);
                }
                else
                {
                    player.Map.BroadcastMessage(player,
                        PacketCreator.DamagePlayer((byte)damagefrom, monsteridfrom, player.Id, damage, fake, direction,
                            isPgmr, pgmr, isPg, oid, posX, posY), false);
                }
                //player.checkBerserk();
            }
            if (player.Map.MapId >= 925020000 && player.Map.MapId < 925030000)
            {
                //player.setDojoEnergy(player.isGM() ? 300 : player.getDojoEnergy() < 300 ? player.getDojoEnergy() + 1 : 0);
                //player.Client.Send(PacketCreator.getEnergy(player.getDojoEnergy()));
            }
        }

        public static void ITEM_PICKUP(MapleClient c, InPacket p)
        {
            p.ReadByte();
            p.ReadLong();
            var oid = p.ReadInt();
            var ob = c.Player.Map.Mapobjects[oid];
            if (!c.Player.Map.IsLootable && c.Player.GmLevel == 0)
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }
            if (ob == null)
            {
                c.Send(PacketCreator.GetInventoryFull());
                c.Send(PacketCreator.GetShowInventoryFull());
                return;
            }
            var item = ob as MapleMapItem;
            if (item != null)
            {
                var mapitem = item;
                lock (mapitem)
                {
                    if (mapitem.IsPickedUp)
                    {
                        c.Send(PacketCreator.GetInventoryFull());
                        c.Send(PacketCreator.GetShowInventoryFull());
                    }
                    var distance = c.Player.Position.DistanceSquare(mapitem.Position);
                    c.Player.AntiCheatTracker.CheckPickupAgain();
                    if (distance > 90000.0)
                    {
                        AutobanManager.Instance.AddPoints(c, 100, 300000, "Itemvac");
                        c.Player.AntiCheatTracker.RegisterOffense(CheatingOffense.Itemvac);
                    }
                    else if (distance > 30000.0)
                    {
                        c.Player.AntiCheatTracker.RegisterOffense(CheatingOffense.ShortItemvac);
                    }
                    if (mapitem.Money > 0)
                    {
                        if (c.Player.Party != null && mapitem.Dropper != c.Player)
                        {
                            var cserv = c.ChannelServer;
                            var mesosamm = mapitem.Money;
                            var partynum = 0;
                            foreach (var partymem in c.Player.Party.GetMembers())
                            {
                                if (partymem.IsOnline && partymem.MapId == c.Player.Map.MapId &&
                                    partymem.ChannelId == c.ChannelId)
                                {
                                    partynum++;
                                }
                            }
                            var mesosgain = mesosamm / partynum;
                            foreach (var partymem in c.Player.Party.GetMembers())
                            {
                                if (partymem.IsOnline && partymem.MapId == c.Player.Map.MapId)
                                {
                                    var somecharacter =
                                        cserv.Characters.FirstOrDefault(x => x.Id == partymem.CharacterId);
                                    somecharacter?.GainMeso(mesosgain, true, true);
                                }
                            }
                        }
                        else
                        {
                            c.Player.GainMeso(mapitem.Money, true, true);
                        }
                        c.Player.Map.BroadcastMessage(
                            PacketCreator.RemoveItemFromMap(mapitem.ObjectId, 2, c.Player.Id),
                            mapitem.Position);
                        c.Player.AntiCheatTracker.PickupComplete();
                        c.Player.Map.RemoveMapObject(ob);
                    }
                    //else if (useItem(c, mapitem.getItem().ItemId))
                    //{
                    //    if (mapitem.getItem().ItemId / 10000 == 238)
                    //    {
                    //        c.Character.getMonsterBook().addCard(c, mapitem.getItem().ItemId);
                    //    }
                    //    mapitem.setPickedUp(true);
                    //    c.Character.getMap().broadcastMessage(PacketCreator.removeItemFromMap(mapitem.getObjectId(), 2, c.Character.getId()), mapitem.getPosition());
                    //    c.Character.getMap().removeMapObject(ob);
                    //}
                    else
                    {
                        if (mapitem.Item.ItemId >= 5000000 && mapitem.Item.ItemId <= 5000100)
                        {
                            //int petId = MaplePet.createPet(mapitem.getItem().ItemId);
                            //if (petId == -1)
                            //{
                            //    return;
                            //}
                            //MapleInventoryManipulator.addById(c, mapitem.getItem().ItemId, mapitem.getItem().getQuantity(), "Cash Item was purchased.", null, petId);
                            //c.Character.getMap().broadcastMessage(
                            //        PacketCreator.removeItemFromMap(mapitem.getObjectId(), 2, c.Character.getId()),
                            //        mapitem.getPosition());
                            //c.Character.getCheatTracker().pickupComplete();
                            //c.Character.getMap().removeMapObject(ob);
                        }
                        else
                        {
                            if (MapleInventoryManipulator.AddFromDrop(c, mapitem.Item, true,
                                "Picked up by " + c.Player.Name))
                            {
                                c.Player.Map.BroadcastMessage(
                                    PacketCreator.RemoveItemFromMap(mapitem.ObjectId, 2, c.Player.Id),
                                    mapitem.Position);
                                c.Player.AntiCheatTracker.PickupComplete();
                                c.Player.Map.RemoveMapObject(ob);
                            }
                            else
                            {
                                c.Player.AntiCheatTracker.PickupComplete();
                                return;
                            }
                        }
                    }
                    mapitem.IsPickedUp = true;
                }
            }
            c.Send(PacketCreator.EnableActions());
        }

        public static void DAMAGE_REACTOR(MapleClient c, InPacket p)
        {
            var oid = p.ReadInt();
            var charPos = p.ReadInt();
            var stance = p.ReadShort();

            var reactor = c.Player.Map.GetReactorByOid(oid);
            if (reactor != null && reactor.IsAlive)
            {
                reactor.HitReactor(charPos, stance, c);
            }
        }

        public static void DISTRIBUTE_AP(MapleClient c, InPacket p)
        {
            var statupdate = new List<Tuple<MapleStat, int>>(2);
            c.Send(PacketCreator.UpdatePlayerStats(statupdate, true));
            var actionId = p.ReadInt();
            if (actionId <= c.LastActionId)
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }
            c.LastActionId = actionId;

            var update = p.ReadInt();
            if (c.Player.RemainingAp > 0)
            {
                switch (update)
                {
                    case 256: // Str
                        if (c.Player.Str >= 999)
                        {
                            return;
                        }
                        c.Player.Str += 1;
                        statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Str, c.Player.Str));
                        break;
                    case 512: // Dex
                        if (c.Player.Dex >= 999)
                        {
                            return;
                        }
                        c.Player.Dex += 1;
                        statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Dex, c.Player.Dex));
                        break;
                    case 1024: // Int
                        if (c.Player.Int >= 999)
                        {
                            return;
                        }
                        c.Player.Int += 1;
                        statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Int, c.Player.Int));
                        break;
                    case 2048: // Luk
                        if (c.Player.Luk >= 999)
                        {
                            return;
                        }
                        c.Player.Luk += 1;
                        statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Luk, c.Player.Luk));
                        break;
                    case 8192: // HP
                        int maxHp = c.Player.MaxHp;
                        if ( /*c.Player.getHpApUsed() == 10000 ||*/ maxHp == 30000)
                        {
                            return;
                        }
                        ISkill improvingMaxHp = null;
                        var improvingMaxHpLevel = 0;
                        if (c.Player.Job == MapleJob.Beginner)
                        {
                            maxHp += Rand(8, 12);
                        }
                        else if (c.Player.Job == MapleJob.Warrior)
                        {
                            improvingMaxHp = SkillFactory.GetSkill(1000001);
                            improvingMaxHpLevel = c.Player.GetSkillLevel(improvingMaxHp);
                            if (improvingMaxHpLevel >= 1)
                            {
                                maxHp += Rand(20, 24) + improvingMaxHp.GetEffect(improvingMaxHpLevel).Y;
                            }
                            else
                            {
                                maxHp += Rand(20, 24);
                            }
                        }
                        else if (c.Player.Job == MapleJob.Magician)
                        {
                            maxHp += Rand(6, 10);
                        }
                        else if (c.Player.Job == MapleJob.Bowman)
                        {
                            maxHp += Rand(16, 20);
                        }
                        else if (c.Player.Job == MapleJob.Thief)
                        {
                            maxHp += Rand(20, 24);
                        }
                        else if (c.Player.Job == MapleJob.Pirate)
                        {
                            improvingMaxHp = SkillFactory.GetSkill(5100000);
                            improvingMaxHpLevel = c.Player.GetSkillLevel(improvingMaxHp);
                            if (improvingMaxHpLevel >= 1)
                            {
                                maxHp += Rand(16, 20) + improvingMaxHp.GetEffect(improvingMaxHpLevel).Y;
                            }
                            else
                            {
                                maxHp += Rand(16, 20);
                            }
                        }
                        maxHp = Math.Min(30000, maxHp);
                        var hpSkillAdd = 0;
                        c.Player.MaxHp += (short)(1 + hpSkillAdd);
                        statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Maxhp, c.Player.MaxHp));
                        break;
                    case 32768: // MP
                        int maxMp = c.Player.MaxHp;
                        if ( /*c.Player.getMpApUsed() == 10000 || */maxMp == 30000)
                        {
                            return;
                        }
                        if (c.Player.Job == MapleJob.Beginner)
                        {
                            maxMp += Rand(6, 8);
                        }
                        else if (c.Player.Job == MapleJob.Warrior)
                        {
                            maxMp += Rand(2, 4);
                        }
                        else if (c.Player.Job == MapleJob.Magician)
                        {
                            var improvingMaxMp = SkillFactory.GetSkill(2000001);
                            var improvingMaxMpLevel = c.Player.GetSkillLevel(improvingMaxMp);
                            if (improvingMaxMpLevel >= 1)
                            {
                                maxMp += Rand(18, 20) + improvingMaxMp.GetEffect(improvingMaxMpLevel).Y;
                            }
                            else
                            {
                                maxMp += Rand(18, 20);
                            }
                        }
                        else if (c.Player.Job == MapleJob.Bowman)
                        {
                            maxMp += Rand(10, 12);
                        }
                        else if (c.Player.Job == MapleJob.Thief)
                        {
                            maxMp += Rand(10, 12);
                        }
                        else if (c.Player.Job == MapleJob.Pirate)
                        {
                            maxMp += Rand(10, 12);
                        }
                        maxMp = Math.Min(30000, maxMp);
                        var mpSkillAdd = 0;
                        c.Player.MaxMp += (short)(1 + mpSkillAdd);
                        statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Maxmp, c.Player.MaxMp));
                        break;
                    default:
                        c.Send(PacketCreator.UpdatePlayerStats(PacketCreator.EmptyStatupdate, true));
                        return;
                }
                c.Player.RemainingAp -= 1;
                statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Availableap, c.Player.RemainingAp));
                c.Send(PacketCreator.UpdatePlayerStats(statupdate, true));
            }
        }

        private static int Rand(int lbound, int ubound)
        {
            return (int)(Randomizer.NextDouble() * (ubound - lbound + 1) + lbound);
        }

        public static void DISTRIBUTE_AUTO_AP(MapleClient c, InPacket p)
        {
            var statupdate = new List<Tuple<MapleStat, int>>();
            p.Skip(8);
            if (c.Player.RemainingAp > 0)
            {
                while (p.AvailableCount > 0)
                {
                    var update = p.ReadInt();
                    var add = (short)p.ReadInt();
                    if (c.Player.RemainingAp < add)
                    {
                        return;
                    }
                    switch (update)
                    {
                        case 256: // Str
                            if (c.Player.Str >= short.MaxValue)
                            {
                                return;
                            }
                            c.Player.Str += add;
                            statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Str, c.Player.Str));
                            break;
                        case 512: // Dex
                            if (c.Player.Dex >= short.MaxValue)
                            {
                                return;
                            }
                            c.Player.Dex += add;
                            statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Dex, c.Player.Dex));
                            break;
                        case 1024: // Int
                            if (c.Player.Int >= short.MaxValue)
                            {
                                return;
                            }
                            c.Player.Int += add;
                            statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Int, c.Player.Int));
                            break;
                        case 2048: // Luk
                            if (c.Player.Luk >= short.MaxValue)
                            {
                                return;
                            }
                            c.Player.Luk += add;
                            statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Luk, c.Player.Luk));
                            break;
                        default:
                            c.Send(PacketCreator.UpdatePlayerStats(PacketCreator.EmptyStatupdate, true));
                            return;
                    }
                    c.Player.RemainingAp -= add;
                }
                statupdate.Add(new Tuple<MapleStat, int>(MapleStat.Availableap, c.Player.RemainingAp));
                c.Send(PacketCreator.UpdatePlayerStats(statupdate, true));
            }
            c.Send(PacketCreator.EnableActions());
        }

        public static void DISTRIBUTE_SP(MapleClient c, InPacket p)
        {
            var actionId = p.ReadInt();
            if (actionId <= c.LastActionId)
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }
            c.LastActionId = actionId;
            var skillid = p.ReadInt();
            var isBegginnerSkill = false;

            var player = c.Player;
            int remainingSp = player.RemainingSp;
            if (skillid == 1000 || skillid == 1001 || skillid == 1002)
            {
                // Beginner Skills
                var snailsLevel = player.GetSkillLevel(SkillFactory.GetSkill(1000));
                var recoveryLevel = player.GetSkillLevel(SkillFactory.GetSkill(1001));
                var nimbleFeetLevel = player.GetSkillLevel(SkillFactory.GetSkill(1002));
                remainingSp = Math.Min(player.Level - 1, 6) - snailsLevel - recoveryLevel - nimbleFeetLevel;
                isBegginnerSkill = true;
            }
            if (skillid == 1005 || skillid == 1006 || skillid == 1003 || skillid == 1004 || skillid == 10001004 ||
                skillid == 20001004)
            {
                return;
            }
            if ((skillid == 1121011 || skillid == 1221012 || skillid == 1321010 || skillid == 2121008 ||
                 skillid == 2221008 || skillid == 2321009 || skillid == 3121009 || skillid == 3221008 ||
                 skillid == 4121009 || skillid == 4221008) &&
                player.GetQuest(MapleQuest.GetInstance(6304)).Status != MapleQuestStatusType.Completed)
            {
                return;
            }
            var skill = SkillFactory.GetSkill(skillid);
            var maxlevel = skill.IsFourthJob ? player.GetMasterLevel(skill) : skill.MaxLevel;
            var curLevel = player.GetSkillLevel(skill);
            if (remainingSp > 0 && curLevel + 1 <= maxlevel && skill.CanBeLearned(player.Job))
            {
                if (!isBegginnerSkill)
                {
                    player.RemainingSp -= 1;
                }
                player.UpdateSingleStat(MapleStat.Availablesp, player.RemainingSp);
                player.ChangeSkillLevel(skill,(byte) (curLevel + 1),player.GetMasterLevel(skill));
            }
            else if (!skill.CanBeLearned(player.Job))
            {
            }
            else if (!(remainingSp > 0 && curLevel + 1 <= maxlevel) && !player.IsGm)
            {
            }
        }

        public static void MAGIC_ATTACK(MapleClient c, InPacket p)
        {
            var attack = ParseDamage(c.Player, p, false);
            var player = c.Player;
            int beforeMp = player.Mp;
            var packet = PacketCreator.MagicAttack(player.Id, attack.Skill, attack.Stance, attack.NumAttackedAndDamage,
                attack.AllDamage, -1, attack.Speed, attack.Pos);
            if (attack.Skill == 2121001 || attack.Skill == 2221001 || attack.Skill == 2321001)
            {
                packet = PacketCreator.MagicAttack(player.Id, attack.Skill, attack.Stance, attack.NumAttackedAndDamage,
                    attack.AllDamage, attack.Charge, attack.Speed, attack.Pos);
            }
            player.Map.BroadcastMessage(player, packet, false, true);
            var effect = attack.GetAttackEffect(player);
            int maxdamage;
            // TODO fix magic damage calculation
            maxdamage = 99999;
            var skill = SkillFactory.GetSkill(attack.Skill);
            var skillLevel = player.GetSkillLevel(skill);
            var effect_ = skill.GetEffect(skillLevel);
            //if (effect_._cooldown > 0)
            //{
            //    if (player.skillisCooling(attack.skill))
            //    {
            //        player.AntiCheatTracker.registerOffense(CheatingOffense.CooldownHack);
            //        return;
            //    }
            //    else
            //    {
            //        c.Send(PacketCreator.SkillCooldown(attack.skill, effect_._cooldown));
            //        ScheduledFuture <?> timer = TimerManager.Instance.RunOnceTask(new CancelCooldownAction(player, attack.skill), effect_.getCooldown() * 1000);
            //        player.addCooldown(attack.skill, System.currentTimeMillis(), effect_.getCooldown() * 1000, timer);
            //    }
            //}
            ApplyAttack(attack, player, maxdamage, effect.AttackCount);
            if (player.Mp - beforeMp < effect.MpCon && c.Player.GetBuffedValue(MapleBuffStat.Infinity) == null)
            {
                var remainingMp = (short)(beforeMp - effect.MpCon);
                c.Player.Mp = remainingMp;
                c.Player.UpdateSingleStat(MapleStat.Mp, remainingMp);
            }
            // MP Eater
            for (var i = 1; i <= 3; i++)
            {
                var eaterSkill = SkillFactory.GetSkill(2000000 + i * 100000);
                var eaterLevel = player.GetSkillLevel(eaterSkill);
                if (eaterLevel > 0)
                {
                    foreach (var singleDamage in attack.AllDamage)
                    {
                        eaterSkill.GetEffect(eaterLevel)
                            .ApplyPassive(player, player.Map.Mapobjects[singleDamage.Item1], 0);
                    }
                    break;
                }
            }
        }

        public static void HEAL_OVERTIME(MapleClient c, InPacket p)
        {
            p.Skip(4);
            var healHp = p.ReadShort();
            var healMp = p.ReadShort();

            if (!c.Player.IsAlive)
            {
                return;
            }
            if (healHp != 0)
            {
                if (healHp > 140)
                {
                    c.Player.AntiCheatTracker.RegisterOffense(CheatingOffense.RegenHighHp, healHp.ToString());
                    return;
                }
                c.Player.AntiCheatTracker.CheckHpRegen();
                if (c.Player.MaxHp == c.Player.Hp)
                {
                    c.Player.AntiCheatTracker.ResetHpRegen();
                }
                c.Player.Hp += healHp;
                c.Player.UpdateSingleStat(MapleStat.Hp, c.Player.Hp);
                //c.Player.checkBerserk();
            }
            if (healMp != 0)
            {
                if (healMp > 1000) // Definitely impossible
                {
                    return;
                }
                var theoreticalRecovery =
                    (float)
                        Math.Floor((float)c.Player.GetSkillLevel(SkillFactory.GetSkill(2000000)) / 10 * c.Player.Level + 3);
                if (healMp > theoreticalRecovery)
                {
                    if (healMp > 300)
                    {
                        // seems almost impossible
                        c.Player.AntiCheatTracker.RegisterOffense(CheatingOffense.RegenHighMp, healMp.ToString());
                    }
                }
                c.Player.AntiCheatTracker.CheckMpRegen();
                c.Player.Mp += healMp;
                c.Player.UpdateSingleStat(MapleStat.Mp, c.Player.Mp);
                if (c.Player.MaxMp == c.Player.Mp)
                {
                    c.Player.AntiCheatTracker.ResetMpRegen();
                }
            }
        }
        public static void USE_ITEM(MapleClient c, InPacket p)
        {
            if (!c.Player.IsAlive)
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }
            MapleItemInformationProvider ii = MapleItemInformationProvider.Instance;
            p.ReadInt();
            byte slot = (byte)p.ReadShort();
            int itemId = p.ReadInt();
            IMapleItem toUse;
            if (c.Player.Inventorys[MapleInventoryType.Use.Value].Inventory.TryGetValue(slot, out toUse) && toUse.Quantity > 0
                && toUse.ItemId == itemId)
            {
                if (itemId == 2022178 || itemId == 2022433 || itemId == 2050004)
                {
                    c.Player.DispelDebuffs();
                    RemoveItem(c, slot);
                    return;
                }
                if (itemId == 2050003)
                {
                    c.Player.DispelDebuffsi();
                    RemoveItem(c, slot);
                    return;
                }
                if (itemId >= 2030000 && itemId < 2030021)
                {
                    if (ii.GetItemEffect(toUse.ItemId).ApplyTo(c.Player))
                    {
                        RemoveItem(c, slot);
                    }
                    c.Send(PacketCreator.EnableActions());
                    return;
                }
                RemoveItem(c, slot);
                ii.GetItemEffect(toUse.ItemId).ApplyTo(c.Player);
                c.Player.CheckBerserk();
            }
        }
        private static void RemoveItem(MapleClient c, byte slot)
        {
            MapleInventoryManipulator.RemoveFromSlot(c, MapleInventoryType.Use, slot, 1, false);
            c.Send(PacketCreator.EnableActions());
        }

        public static void SPECIAL_MOVE(MapleClient c, InPacket p)
        {
            p.Skip(4);
            int skillid = p.ReadInt();
            /*↓圣灵之剑↓*/
            if ((skillid == 4001003 || skillid == 14001003 || skillid == 4221006 || skillid == 1221003 || skillid == 5101007) &&
                !c.Player.IsGm && c.Player.Map.CannotInvincible)
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }
            Point pos = Point.Empty;
            int _skillLevel = p.ReadByte();
            ISkill skill = SkillFactory.GetSkill(skillid);
            int skillLevel = c.Player.GetSkillLevel(skill);
            MapleStatEffect effect = skill.GetEffect(skillLevel);
            c.Player.ResetAfkTimer();
            int beforeMp = c.Player.Mp;
            if (skillid % 10000000 == 1010 || skillid % 10000000 == 1011)
            {
                skillLevel = 1;
                c.Player.DojoEnergy = 0;
                c.Send(PacketCreator.GetEnergy(0));
            }
            if (effect.Cooldown > 0)
            {
                if (c.Player.SkillisCooling(skillid))
                    return;

                if (skillid != 5221006)
                {
                    c.Send(PacketCreator.SkillCooldown(skillid, effect.Cooldown));
                    var timer = TimerManager.Instance.RunOnceTask(() => new CancelCooldownAction(c.Player, skillid).Run(), effect.Cooldown * 1000);
                    c.Player.AddCooldown(skillid, DateTime.Now.GetTimeMilliseconds(), effect.Cooldown * 1000, timer);
                }
            }
            //monster magnet
            try
            {
                if (skillid == 1121001 || skillid == 1221001 || skillid == 1321001)
                {
                    int num = p.ReadInt();
                    for (int i = 0; i < num; i++)
                    {
                        var mobId = p.ReadInt();
                        var success = p.ReadByte();
                        c.Player.Map.BroadcastMessage(c.Player, PacketCreator.ShowMagnet(mobId, success), false);
                        MapleMonster monster = c.Player.Map.GetMonsterByOid(mobId);
                        monster?.SwitchController(c.Player, monster.ControllerHasAggro);
                    }
                    byte direction = p.ReadByte();
                    c.Player.Map.BroadcastMessage(c.Player,
                        PacketCreator.ShowBuffeffect(c.Player.Id, skillid, 1, direction), false);
                    c.Send(PacketCreator.EnableActions());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (skillid % 20000000 == 1004)
            {
                p.ReadShort();
            }
            if (p.AvailableCount == 5)
            {
                pos = new Point(p.ReadShort(), p.ReadShort());
                Console.WriteLine("新位置,X-Y");
            }
            if (skillLevel == 0 || skillLevel != _skillLevel)
            {
                c.Disconnect();
            }
            else
            {
                if (c.Player.IsAlive)
                {
                    if (skillid == 9001004 && c.Player.IsGm)
                    {
                        c.Player.IsHidden = !c.Player.IsHidden;
                    }
                    if ((skill.SkillId != 2311002) || c.Player.CanDoor)
                    {
                        skill.GetEffect(skillLevel).ApplyTo(c.Player, pos);
                        if ((skill.SkillId != 2301002) && (effect.MpCon != 0) && (c.Player.Mp - beforeMp < skill.GetEffect(skillLevel).MpCon))
                        {
                            int remainingMp = beforeMp - skill.GetEffect(skillLevel).MpCon;
                            c.Player.Mp = (short)remainingMp;
                            c.Player.UpdateSingleStat(MapleStat.Mp, remainingMp);
                        }
                    }
                    else
                    {
                        new ServernoticeMapleClientMessageCallback(5, c).DropMessage("请等候5秒再使用时空门!");
                        c.Send(PacketCreator.EnableActions());
                    }
                }
                else
                {
                    c.Send(PacketCreator.EnableActions());
                }
                c.Send(PacketCreator.EnableActions());
            }
        }
        public static void ENTER_CASHSHOP(MapleClient c, InPacket p)
        {
            if (c.ChannelServer.AllowEnterCashShop)
            {
                if (c.Player.GetBuffedValue(MapleBuffStat.Summon) != null)
                {
                    c.Player.CancelEffectFromBuffStat(MapleBuffStat.Summon);
                }
                //try
                //{
                //    WorldChannelInterface wci = ChannelServer.getInstance(c.getChannel()).getWorldInterface();
                //    wci.addBuffsToStorage(c.Player.getId(), c.Player.getAllBuffs());
                //}
                //catch (Exception e)
                //{
                //    c.getChannelServer().reconnectWorld();
                //}
                c.Player.Map.RemovePlayer(c.Player);
                c.Send(PacketCreator.WarpToCashShop(c));
                c.Player.InCashShop = true;
                c.Send(PacketCreator.SendWishList(c.Player.Id));
                c.Send(PacketCreator.ShowNXMapleTokens(c.Player));
                c.Send(PacketCreator.GetCashShopInventory(c.Player));
                c.Send(PacketCreator.GetCashShopGifts(c.Player));
                c.Player.Save();
                Console.WriteLine($"{c.Player.Name}进入商城服务器");
            }
            else
            {
                c.Send(PacketCreator.SendBlockedMessage(3));
                c.Send(PacketCreator.EnableActions());
            }
        }
        public static void ENTER_MTS(MapleClient c, InPacket p)
        {
            NpcScriptManager.Instance.Start(c, 9000018);
            c.Send(PacketCreator.EnableActions());
        }
        public static void CASHSHOP(MapleClient c, InPacket p)
        {
            int action = p.ReadByte();
            int accountId = c.Account.Id;
            if (action == 3)
            {
                //购买物品
                byte currencyType = p.ReadByte();
                int snCS = p.ReadInt();
                CashItem item = CashItemFactory.GetItem(snCS);
                if (c.Player.GetCashShopPoints(currencyType) >= item.Price)
                {
                    c.Player.ChangeCashShopPoints(currencyType, -item.Price);
                }
                else
                {
                    c.Send(PacketCreator.EnableActions());
                    AutobanManager.Instance.Autoban(c, "试图购买现金物品，但是没有足够的点券。");
                    return;
                }
                if (item.Price == 0)
                {
                    //检测物品的价格为0就返回不处理，你可以自定义添加其它的了。
                    return;
                }

                //这里屏掉了经验卡和小黑板  大家可以自由发挥~
                if (item.ItemId == 5370000 || item.ItemId == 5370001 || item.ItemId >= 4100000 && item.ItemId <= 4100006 || item.ItemId == 1112800 || item.ItemId >= 1112001 && item.ItemId <= 1112012)
                {
                    c.Player.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Popup, "系统错误，请稍后再试"));

                    return;
                }
                if (item.ItemId >= 5000000 && item.ItemId <= 5000100)
                {
                    int petId = MaplePet.Create(item.ItemId, c.Player);
                    if (petId == -1)
                    {
                        return;
                    }
                    var citem = new MapleCashShopInventoryItem(petId, item.ItemId, snCS, item.Count, false);
                    long period = 90;
                    citem.Expire = DateTime.Now.AddSeconds(period * 24 * 60 * 60);
                    c.Player.CashShopInventory.AddItem(citem);
                    c.Send(PacketCreator.EnableCashShopOrMTS());
                    c.Send(PacketCreator.ShowBoughtCashShopItem(c, citem));
                }
                else
                {
                    var citem = new MapleCashShopInventoryItem(MapleCharacter.GetNextUniqueId(), item.ItemId, snCS, item.Count, false);
                    long period = item.Period;
                    DateTime? expirationDate = DateTime.Now.AddSeconds(period * 24 * 60 * 60);
                    if (period == 0)
                    {
                        expirationDate = null;
                    }
                    if (item.ItemId == 5211047 || item.ItemId == 5360014)
                    {
                        expirationDate = DateTime.Now.AddSeconds(3 * 60 * 60);
                    }
                    citem.Expire = expirationDate;
                    c.Player.CashShopInventory.AddItem(citem);
                    c.Send(PacketCreator.ShowBoughtCashShopItem(c, citem));
                }
                c.Player.Save();
                c.Send(PacketCreator.ShowNXMapleTokens(c.Player));
                c.Send(PacketCreator.EnableActions());
            }
            else if (action == 4)
            {
                ////赠送礼物
                //int snCS = p.ReadInt();
                //byte type = p.ReadByte();
                //string recipient = p.ReadMapleString();
                //string message = p.ReadMapleString();
                //CashItem item = CashItemFactory.GetItem(snCS);
                //if (c.Player.GetCashShopPoints(type) >= item.Price)
                //{
                //    if (MapleCharacter.getAccountIdByName(recipient) != -1)
                //    {
                //        if (MapleCharacter.getAccountIdByName(recipient) == c.Player.AccountId)
                //        {
                //            c.Send(PacketCreator.ShowCannotToMe());
                //        }
                //        else
                //        {
                //            c.Player.ChangeCashShopPoints(type, -item.Price);
                //            var gift = new MapleCashShopInventoryItem(0, item.ItemId, snCS, item.Count, true);
                //            gift.Sender = c.Player.Name;
                //            gift.Message = message;
                //            gift.Expire = DateTime.Now.AddSeconds(item.Period * 24 * 60 * 60);
                //            try
                //            {
                //                Connection con = DatabaseConnection.getConnection();
                //                PreparedStatement ps = con.prepareStatement("INSERT INTO csgifts (accountid, itemid, sn, quantity, sender, message, expiredate) VALUES (?, ?, ?, ?, ?, ?, ?)");
                //                ps.setInt(1, MapleCharacter.getAccountIdByName(recipient));
                //                ps.setInt(2, item.ItemId);
                //                ps.setInt(3, snCS);
                //                ps.setInt(4, item.getCount());
                //                ps.setString(5, c.Player.getName());
                //                ps.setString(6, message);
                //                Timestamp ExpirationDate = new Timestamp(System.currentTimeMillis());
                //                if ((item.ItemId >= 5000000 && item.ItemId <= 5000100) || item.ItemId == 1112906 || item.ItemId == 1112905)
                //                {
                //                    ExpirationDate = new Timestamp(((System.currentTimeMillis() / 1000) + (90 * 24 * 60 * 60)) * 1000);
                //                }
                //                else if (item.ItemId == 5211047 || item.ItemId == 5360014)
                //                {
                //                    ExpirationDate = new Timestamp(((System.currentTimeMillis() / 1000) + (3 * 60 * 60)) * 1000);
                //                }
                //                else if (item.getPeriod() != 0)
                //                {
                //                    ExpirationDate = new Timestamp(((System.currentTimeMillis() / 1000) + (item.getPeriod() * 24 * 60 * 60)) * 1000);
                //                }
                //                else
                //                {
                //                    ExpirationDate = null;
                //                }
                //                ps.setTimestamp(7, ExpirationDate);
                //                ps.executeUpdate();
                //                ps.close();
                //            }
                //            catch (Exception e)
                //            {
                //                Console.WriteLine(e.Message);
                //            }
                //            c.Send(PacketCreator.getGiftFinish(c.Player.getName(), item.ItemId, (short)item.getCount()));
                //        }
                //    }
                //    else
                //    {
                //        c.getSession().write(MaplePacketCreator.showCheckName());
                //    }
                //}
                //else
                //{
                //    c.getSession().write(MaplePacketCreator.enableActions());
                //    AutobanManager.getInstance().autoban(c, "试图购买现金物品，但是没有足够的点券。");
                //    return;
                //}
                //c.Player.saveToDB(true);
                //c.getSession().write(MaplePacketCreator.enableCSorMTS());
                //c.getSession().write(MaplePacketCreator.showNXMapleTokens(c.Player));
                //c.getSession().write(MaplePacketCreator.enableActions());
            }
            else if (action == 5)
            {
                //try
                //{
                //    Connection con = DatabaseConnection.getConnection();
                //    PreparedStatement ps = con.prepareStatement("DELETE FROM wishlist WHERE charid = ?");
                //    ps.setInt(1, c.Player.getId());
                //    ps.executeUpdate();
                //    ps.close();

                //    int i = 10;
                //    while (i > 0)
                //    {
                //        int sn = p.ReadInt();
                //        if (sn != 0)
                //        {
                //            ps = con.prepareStatement("INSERT INTO wishlist(charid, sn) VALUES(?, ?) ");
                //            ps.setInt(1, c.Player.getId());
                //            ps.setInt(2, sn);
                //            ps.executeUpdate();
                //            ps.close();
                //        }
                //        i--;
                //    }
                //}
                //catch (SQLException se)
                //{
                //    log.error("Wishlist SQL Error", se);
                //}
                //c.getSession().write(MaplePacketCreator.updateWishList(c.Player.getId()));
            }
            else if (action == 6)
            {
                ////扩充仓库
                //int useNX = p.ReadByte();
                //byte add = p.ReadByte();
                //if (add == 0)
                //{
                //    byte type = p.ReadByte();
                //    MapleInventoryType invtype = MapleInventoryType.getByType(type);
                //    byte slots = c.Player.getInventory(invtype).getSlots();
                //    if (c.Player.getCSPoints(useNX) < 600)
                //    {
                //        c.getSession().write(MaplePacketCreator.enableCSorMTS());
                //        c.getSession().write(MaplePacketCreator.showNXMapleTokens(c.Player));
                //        c.getSession().write(MaplePacketCreator.enableActions());
                //        return;
                //    }
                //    if (slots <= 92)
                //    {
                //        c.Player.modifyCSPoints(useNX, -600);
                //        c.Player.getInventory(invtype).setSlotLimit((byte)(slots + 4));
                //        c.getSession().write(MaplePacketCreator.serverNotice(1, "扩充成功."));
                //    }
                //    else
                //    {
                //        c.getSession().write(MaplePacketCreator.serverNotice(1, "您无法继续进行扩充."));
                //    }
                //}
                //else if (add == 1)
                //{
                //    int sn = p.ReadInt();
                //    byte type = 1;
                //    switch (sn)
                //    {
                //        case 50200018:
                //            type = 1;
                //            break;
                //        case 50200019:
                //            type = 2;
                //            break;
                //        case 50200020:
                //            type = 3;
                //            break;
                //        case 50200021:
                //            type = 4;
                //            break;
                //        case 50200043:
                //            type = 5;
                //            break;
                //    }
                //    MapleInventoryType invtype = MapleInventoryType.getByType(type);
                //    byte slots = c.Player.getInventory(invtype).getSlots();
                //    if (c.Player.getCSPoints(useNX) < 1100)
                //    {
                //        c.getSession().write(MaplePacketCreator.enableCSorMTS());
                //        c.getSession().write(MaplePacketCreator.showNXMapleTokens(c.Player));
                //        c.getSession().write(MaplePacketCreator.enableActions());
                //        return;
                //    }
                //    if (slots <= 86)
                //    {
                //        c.Player.modifyCSPoints(useNX, -1100);
                //        c.Player.getInventory(invtype).setSlotLimit((byte)(slots + 8));
                //        c.getSession().write(MaplePacketCreator.serverNotice(1, "扩充成功."));
                //    }
                //    else
                //    {
                //        c.getSession().write(MaplePacketCreator.serverNotice(1, "您无法继续进行扩充."));
                //    }
                //}
                //c.Player.saveToDB(true);
                //c.getSession().write(MaplePacketCreator.enableCSorMTS());
                //c.getSession().write(MaplePacketCreator.showNXMapleTokens(c.Player));
                //c.getSession().write(MaplePacketCreator.enableActions());
            }
            else if (action == 0x0D)
            {
                //商城=>包裹
                int uniqueid = p.ReadInt(); //csid.. not like we need it anyways
                p.ReadInt();//0
                p.ReadByte();//0
                byte type = p.ReadByte();
                byte unknown = p.ReadByte();
                IMapleItem item = c.Player.CashShopInventory.GetItem(uniqueid).ToItem();
                if (item != null)
                {
                    byte slot = c.Player.Inventorys[MapleItemInformationProvider.Instance.GetInventoryType(item.ItemId).Value].GetNextFreeSlot();
                    if (slot == 0xFF)
                    {
                        c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Popup, "您的包裹已满"));
                    }
                    else
                    {
                        c.Player.Inventorys[MapleItemInformationProvider.Instance.GetInventoryType(item.ItemId).Value].AddItem(item);
                        c.Player.CashShopInventory.RemoveItem(uniqueid);
                        c.Send(PacketCreator.transferFromCSToInv(item, slot));
                    }
                }
                c.Player.Save();
                c.Send(PacketCreator.EnableCashShopOrMTS());
                c.Send(PacketCreator.ShowNXMapleTokens(c.Player));
                c.Send(PacketCreator.EnableActions());
            }
            else if (action == 0x0E)
            {
                //包裹=>商城
                int uniqueid = p.ReadInt();
                p.ReadInt();//0
                p.ReadByte(); //1?

                IMapleItem item = null;
                foreach (MapleInventory inventory in c.Player.Inventorys)
                {
                    item = inventory.FindByUniqueId(uniqueid);
                    if (item != null)
                    {

                        MapleCashShopInventoryItem citem = new MapleCashShopInventoryItem(item.UniqueId, item.ItemId, CashItemFactory.GetSnFromId(item.ItemId), item.Quantity, false);
                        citem.Expire=item.Expiration;
                        c.Player.CashShopInventory.AddItem(citem);

                        inventory.RemoveItem(item.Position, item.Quantity, false);
                        c.Send(PacketCreator.TransferFromInvToCs(c.Player, citem));
                        break;
                    }
                }
                c.Player.Save();
                c.Send(PacketCreator.EnableCashShopOrMTS());
                c.Send(PacketCreator.ShowNXMapleTokens(c.Player));
                c.Send(PacketCreator.EnableActions());
            }
            else if (action == 0x21)
            {
                ////购买任务物品
                //int snCS = p.ReadInt();
                //CashItem item = CashItemFactory.GetItem(snCS);
                //if (c.Player.getMeso() >= item.getPrice())
                //{
                //    c.Player.gainMeso(-item.getPrice(), false);
                //    MapleInventoryManipulator.addById(c, item.ItemId, (short)item.getCount(), "购买了任务物品");
                //    MapleInventory etcInventory = c.Player.getInventory(MapleInventoryType.ETC);
                //    byte slot = etcInventory.findById(item.ItemId).getPosition();
                //    c.getSession().write(MaplePacketCreator.showBoughtCSQuestItem(slot, item.ItemId));
                //}
                //else
                //{
                //    c.getSession().write(MaplePacketCreator.enableActions());
                //    return;
                //}
                //c.getSession().write(MaplePacketCreator.showNXMapleTokens(c.Player));
                //c.getSession().write(MaplePacketCreator.enableCSorMTS());
                //c.getSession().write(MaplePacketCreator.enableActions());
                //return;
            }
            else if (action == 0x1F)
            {
                ////购买礼包
                //int type = p.ReadByte();
                //int snCS = p.ReadInt();
                //CashItem cashPackage = CashItemFactory.GetItem(snCS);
                //if (c.Player.getCSPoints(type) >= cashPackage.getPrice())
                //{
                //    c.Player.modifyCSPoints(type, -cashPackage.getPrice());
                //}
                //else
                //{
                //    c.getSession().write(MaplePacketCreator.enableActions());
                //    return;
                //}
                //for (CashItem item : CashItemFactory.getPackageItems(cashPackage.ItemId))
                //{
                //    if (item.ItemId >= 5000000 && item.ItemId <= 5000100)
                //    {
                //        int petId = MaplePet.createPet(item.ItemId, c.Player);
                //        if (petId == -1)
                //        {
                //            return;
                //        }
                //        MapleCSInventoryItem citem = new MapleCSInventoryItem(petId, item.ItemId, snCS, (short)item.getCount(), false);
                //        long period = 90;
                //        Timestamp ExpirationDate = new Timestamp(System.currentTimeMillis() + (period * 24 * 60 * 60 * 1000));
                //        citem.setExpire(ExpirationDate);
                //        c.Player.getCSInventory().addItem(citem);
                //        c.getSession().write(MaplePacketCreator.showBoughtCSItem(c, citem));
                //    }
                //    else
                //    {
                //        MapleCSInventoryItem citem = new MapleCSInventoryItem(MapleCharacter.getNextUniqueId(), item.ItemId, snCS, (short)item.getCount(), false);
                //        long period = item.getPeriod();
                //        Timestamp ExpirationDate = new Timestamp(System.currentTimeMillis());
                //        ExpirationDate = new Timestamp(((System.currentTimeMillis() / 1000) + (period * 24 * 60 * 60)) * 1000);
                //        if (period == 0)
                //        {
                //            ExpirationDate = null;
                //        }
                //        if (item.ItemId == 5211047 || item.ItemId == 5360014)
                //        {
                //            ExpirationDate = new Timestamp(((System.currentTimeMillis() / 1000) + (3 * 60 * 60)) * 1000);
                //        }
                //        if (item.ItemId == 1112906 && item.ItemId == 1112905)
                //        {
                //            ExpirationDate = new Timestamp(((System.currentTimeMillis() / 1000) + (period * 24 * 60 * 60)) * 1000);
                //        }
                //        citem.setExpire(ExpirationDate);
                //        c.Player.getCSInventory().addItem(citem);
                //        c.getSession().write(MaplePacketCreator.showBoughtCSItem(c, citem));
                //    }
                //    c.Player.getCSInventory().saveToDB();
                //}
                //c.Player.saveToDB(true);
                //c.getSession().write(MaplePacketCreator.enableCSorMTS());
                //c.getSession().write(MaplePacketCreator.showNXMapleTokens(c.Player));
                //c.getSession().write(MaplePacketCreator.enableActions());
            }
            else if (action == 0x1D)
            {
                ////购买结婚戒指相关
                //int snCS = p.ReadInt();
                //String recipient = p.ReadMapleAsciiString();
                //String message = p.ReadMapleAsciiString();
                //CashItem item = CashItemFactory.GetItem(snCS);
                //if (c.Player.getCSPoints(0) >= item.getPrice())
                //{
                //    if (MapleCharacter.getAccountIdByName(recipient) != -1)
                //    {
                //        c.Player.modifyCSPoints(0, -item.getPrice());
                //        MapleCSInventoryItem gift = new MapleCSInventoryItem(0, item.ItemId, snCS, (short)item.getCount(), true);
                //        MapleCSInventoryItem citem = new MapleCSInventoryItem(MapleCharacter.getNextUniqueId(), item.ItemId, snCS, (short)item.getCount(), false);
                //        gift.setSender(c.Player.getName());
                //        gift.setMessage(message);
                //        gift.setExpire(new Timestamp(((System.currentTimeMillis() / 1000) + (item.getPeriod() * 24 * 60 * 60)) * 1000));
                //        try
                //        {
                //            Connection con = DatabaseConnection.getConnection();
                //            PreparedStatement ps = con.prepareStatement("INSERT INTO csgifts (accountid, itemid, sn, quantity, sender, message, expiredate) VALUES (?, ?, ?, ?, ?, ?, ?)");
                //            ps.setInt(1, MapleCharacter.getAccountIdByName(recipient));
                //            ps.setInt(2, item.ItemId);
                //            ps.setInt(3, snCS);
                //            ps.setInt(4, item.getCount());
                //            ps.setString(5, c.Player.getName());
                //            ps.setString(6, message);
                //            ps.setTimestamp(7, null);
                //            ps.executeUpdate();
                //            ps.close();
                //        }
                //        catch (SQLException se)
                //        {
                //            log.error("Error saving gift to database", se);
                //        }
                //        c.Player.getCSInventory().addItem(citem);
                //        c.getSession().write(MaplePacketCreator.showBoughtCSItem(c, citem));
                //        MapleRing.createRing(MapleCharacter.getNextUniqueId(), MapleCharacter.getNextUniqueId() + 1, c.Player.getId(), MapleCharacter.getIdByName(recipient), c.Player.getName(), recipient);
                //    }
                //    else
                //    {
                //        c.getSession().write(MaplePacketCreator.serverNotice(1, "未登陆的角色"));
                //    }
                //}
                //else
                //{
                //    c.getSession().write(MaplePacketCreator.enableActions());
                //    return;
                //}
                //c.Player.getCSInventory().saveToDB();
                //c.getSession().write(MaplePacketCreator.enableCSorMTS());
                //c.getSession().write(MaplePacketCreator.showNXMapleTokens(c.Player));
                //c.getSession().write(MaplePacketCreator.enableActions());
            }
            else if (action == 0x24)
            {
                ////购买挚友戒指相关
                //int snCS = p.ReadInt();
                //String recipient = p.ReadMapleAsciiString();
                //String message = p.ReadMapleAsciiString();
                //CashItem item = CashItemFactory.GetItem(snCS);
                //if (c.Player.getCSPoints(0) >= item.getPrice())
                //{
                //    if (MapleCharacter.getAccountIdByName(recipient) != -1)
                //    {
                //        c.Player.modifyCSPoints(0, -item.getPrice());
                //        MapleCSInventoryItem gift = new MapleCSInventoryItem(0, item.ItemId, snCS, (short)item.getCount(), true);
                //        MapleCSInventoryItem citem = new MapleCSInventoryItem(MapleCharacter.getNextUniqueId(), item.ItemId, snCS, (short)item.getCount(), false);
                //        gift.setSender(c.Player.getName());
                //        gift.setMessage(message);
                //        gift.setExpire(new Timestamp(((System.currentTimeMillis() / 1000) + (item.getPeriod() * 24 * 60 * 60)) * 1000));
                //        try
                //        {
                //            Connection con = DatabaseConnection.getConnection();
                //            PreparedStatement ps = con.prepareStatement("INSERT INTO csgifts (accountid, itemid, sn, quantity, sender, message, expiredate) VALUES (?, ?, ?, ?, ?, ?, ?)");
                //            ps.setInt(1, MapleCharacter.getAccountIdByName(recipient));
                //            ps.setInt(2, item.ItemId);
                //            ps.setInt(3, snCS);
                //            ps.setInt(4, item.getCount());
                //            ps.setString(5, c.Player.getName());
                //            ps.setString(6, message);
                //            ps.setTimestamp(7, null);
                //            ps.executeUpdate();
                //            ps.close();
                //        }
                //        catch (SQLException se)
                //        {
                //            log.error("Error saving gift to database", se);
                //        }
                //        c.Player.getCSInventory().addItem(citem);
                //        c.getSession().write(MaplePacketCreator.showBoughtCSItem(c, citem));
                //        MapleRing.createRing(MapleCharacter.getNextUniqueId(), MapleCharacter.getNextUniqueId() + 1, c.Player.getId(), MapleCharacter.getIdByName(recipient), c.Player.getName(), recipient);
                //    }
                //    else
                //    {
                //        c.getSession().write(MaplePacketCreator.serverNotice(1, "未登陆的角色"));
                //    }
                //}
                //else
                //{
                //    c.getSession().write(MaplePacketCreator.enableActions());
                //    return;
                //}
                //c.Player.getCSInventory().saveToDB();
                //c.getSession().write(MaplePacketCreator.enableCSorMTS());
                //c.getSession().write(MaplePacketCreator.showNXMapleTokens(c.Player));
                //c.getSession().write(MaplePacketCreator.enableActions());
            }
            c.Player.CashShopInventory.Save();
            c.Send(PacketCreator.EnableCashShopOrMTS());
            c.Send(PacketCreator.ShowNXMapleTokens(c.Player));
            c.Send(PacketCreator.EnableActions());
        }
        public static void ITEM_MOVE(MapleClient c, InPacket p)
        {
            int actionId = p.ReadInt();
            if (actionId <= c.LastActionId)
            {
                c.Send(PacketCreator.EnableActions());
                return;
            }
            c.LastActionId = actionId;
            MapleInventoryType type = MapleInventoryType.GetByType(p.ReadByte());
            MapleInventory inventory = c.Player.Inventorys[type.Value];
            short src = p.ReadShort();
            short dst = p.ReadShort();

            byte srcSlot = (byte)src;
            byte dstSlot = (byte)dst;

            short quantity = p.ReadShort();
            if (srcSlot > 127 && dstSlot < 128)
            {
                MapleInventoryManipulator.UnEquip(c,src, dst);
            }
            else if (dstSlot > 127)
            {
                //log.error("你当前使用的道具是:"+src+"模式为:"+dst+"还不知道11111"+c);
                if (inventory.Inventory[srcSlot].ItemId == 1122017)
                    c.Player.DropMessage(PacketCreator.ServerMessageType.PinkText, "由于装备了[精灵吊坠],打猎时额外获得10%的经验值奖励!");

                if (inventory.Inventory[srcSlot].ItemId == 1122018)
                    c.Player.DropMessage(PacketCreator.ServerMessageType.PinkText, "由于装备了[温暖的围脖],打猎时额外获得100%的经验值外加奖励!");

                MapleInventoryManipulator.Equip(c, src, dst);
            }
            else if (inventory.Inventory.ContainsKey(srcSlot) && inventory.Inventory[srcSlot].ItemId == 5110000)
            {
                c.Send(PacketCreator.EnableActions());
            }
            else if (dstSlot == 0)
            {
                if (quantity < 0)
                {
                    c.Player.DropMessage(PacketCreator.ServerMessageType.Popup, "Either you're Tryst, or you're a hacker. So GT*O here.\r\n(P.S. If you're Tryst, I ask you to **** OFF. Either way, you're a hacker.)");
                    //try
                    //{
                    //    c.getChannelServer().getWorldInterface().broadcastGMMessage(c.getPlayer().getName(), MaplePacketCreator.serverNotice(0, "Duper alert: " + c.getPlayer().getName() + " is dropping negative amount of items.").getBytes());
                    //}
                    //catch (Throwable u)
                    //{
                    //}
                    return;
                }
                if (!c.Player.Inventorys[type.Value].Inventory.ContainsKey(srcSlot))
                {
                    c.Send(PacketCreator.EnableActions());
                    return;
                }
                MapleInventoryManipulator.Drop(c, type, srcSlot, quantity);
            }
            else
            {
                MapleInventoryManipulator.Move(c, type, src, dstSlot);
            }
        }
        public static void TOUCHING_CASHSHOP(MapleClient c, InPacket p)
        {
            c.Send(PacketCreator.ShowNXMapleTokens(c.Player));
        }
    }
}