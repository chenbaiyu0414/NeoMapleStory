using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Movement;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Packet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Script.NPC;

namespace NeoMapleStory.Server
{

    public static class ChannelPacketHandlers
    {
        public static void PLAYER_LOGGEDIN(MapleClient c, InPacket p)
        {
            int charId = p.ReadInt();


            c.Character = MapleCharacter.LoadCharFromDb(charId, c, true);
                
          
            MapleCharacter player = c.Character;
            //player.Account = c.Account= DatabaseHelper.LoadAccount()
            //int state = c.getLoginState();
            //bool allowLogin = true;
            ChannelServer channelServer = MasterServer.Instance.ChannelServers[c.ChannelId];

            //lock(this) {
            //    try
            //    {
            //        WorldChannelInterface worldInterface = channelServer.getWorldInterface();
            //        if (state == MapleClient.LOGIN_SERVER_TRANSITION)
            //        {
            //            for (String charName : c.loadCharacterNames(c.getWorld()))
            //            {
            //                if (worldInterface.isConnected(charName))
            //                {
            //                    allowLogin = false;
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    catch (RemoteException e)
            //    {
            //        channelServer.reconnectWorld();
            //        allowLogin = false;
            //    }
            //    if (state != MapleClient.LOGIN_SERVER_TRANSITION || !allowLogin)
            //    {
            //        c.setPlayer(null);
            //        c.getSession().close();
            //        return;
            //    }
            //    c.updateLoginState(MapleClient.LOGIN_LOGGEDIN);
            //}

            channelServer.Characters.Add(player);

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
            //    ps.setInt(1, c.getPlayer().getId());
            //    ResultSet rs = ps.executeQuery();
            //    while (rs.next())
            //    {
            //        if (rs.getLong("length") + rs.getLong("starttime") - System.currentTimeMillis() <= 0)
            //        {
            //            continue;
            //        }
            //        c.getPlayer().giveCoolDowns(rs.getInt("skillid"), rs.getLong("starttime"), rs.getLong("length"));
            //    }
            //    rs.close();
            //    ps.close();
            //    ps = con.prepareStatement("DELETE FROM cooldowns WHERE characterid = ?");
            //    ps.setInt(1, c.getPlayer().getId());
            //    ps.executeUpdate();
            //    ps.close();
            //}
            //catch (SQLException se)
            //{
            //    se.printStackTrace();
            //}

            c.Send(ChannelPacket.GetCharInfo(player));

            //if (player.GMLevel > 0)
            //{
            //    int[] skills = { 9001004, 9001001 };
            //    foreach (int i in skills)
            //    {
            //        SkillFactory.GetSkill(i).GetEffect(SkillFactory.GetSkill(i).MaxLevel).applyTo(player);
            //    }
            //}


            if (player.GmLevel == 0)
            {
                Console.WriteLine($"玩家 [ {player.Name} ],Lv {player.Level} 进入了游戏服务器.");
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, $"您进入了{c.ChannelId + 1} 线"));
            }
            else
            {
                Console.WriteLine($"管理员 [ {player.Name} ] 进入游戏服务器.");
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, $"您进入了{c.ChannelId + 1} 线"));
                player.DropMessage($"[欢迎] 尊敬的管理员 {player.Name} ,当前在线人数为: {c.AppServer.SessionCount}");
            }

            if (player.Hp < 50)
            {
                //这里判断暂时这样写吧！
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, "[游戏公告] 生命低于 50 !请留意HP!"));
            }

            string prefix = "";
            IMapleItem equip = null;
            player.Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(17, out equip);

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
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText,
                    "[注意] 请在10级转职之前，加好基本技能点，转职请找NPC！"));
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

            //if (!c.getPlayer().hasMerchant() && c.getPlayer().tempHasItems())
            //{
            //    c.getPlayer().dropMessage(1, "你有物品,可以通过雇用Npc领取物品!\r\n暂时还在调试中该功能!");
            //}

            //player.checkMessenger();
            //player.showMapleTips();
            //player.checkBerserk();
            //player.checkDuey();

            //player.expirationTask();
            //c.Send(PacketCreator.ShowCharCash(player));
            c.Send(PacketCreator.WeirdStatUpdate());
        }

        public static void PLAYER_UPDATE(MapleClient c, InPacket p)
        {
            c.Character.SaveToDb(true);
            if ((c.Character.Map.MapId == 677000013) || (c.Character.Map.MapId == 677000013))
            {
                c.Character.StartMapEffect("尝试一下惨败的味道吧……哈哈哈哈哈", 5120000);
                c.Character.SaveToDb(true);
            }
        }

        public static void CHANGE_MAP_SPECIAL(MapleClient c, InPacket p)
        {
            p.ReadByte();
            string startwp = p.ReadMapleString();
            p.ReadShort();
            IMaplePortal portal = c.Character.Map.getPortal(startwp);
            if (portal != null)
                portal.EnterPortal(c);
            else
                c.Send(PacketCreator.EnableActions());
        }

        public static void NPC_ACTION(MapleClient c, InPacket p)
        {
            using (OutPacket packet = new OutPacket(SendOpcodes.NpcAction))
            {
                int length = (int) p.AvailableCount;
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
            List<ILifeMovementFragment> res = AbstractMovementPacketHandler.ParseMovement(p);
            c.Character.Lastres = res;
            if (res != null)
            {
                if (p.AvailableCount != 18)
                {
                    Console.WriteLine("slea.available != 18 (movement parsing error)");
                    return;
                }
                MapleCharacter player = c.Character;
                //try
                //{
                if (!player.IsHidden)
                {
                    c.Character.Map.BroadcastMessage(player, PacketCreator.MovePlayer(player.Id, res), false);
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
                //if (c.Character.Party != null)
                //{
                //    c.Character.setParty(c.getPlayer().getParty());
                //}


                var ip = IPAddress.Parse(c.ChannelServer.Config.Ip);
                short port = (short)c.ChannelServer.Config.Port;

                //c.getPlayer().saveToDB(true);
                //c.getPlayer().setInCS(false);
                //c.getPlayer().setInMTS(false);
                //c.getPlayer().cancelSavedBuffs();

                c.ChannelServer.Characters.Remove(c.Character);
                //c.updateLoginState(MapleClient.LOGIN_SERVER_TRANSITION);

                c.Send(PacketCreator.GetChannelChange(ip, port));
                c.Close();
            }
            else
            {
                p.ReadByte(); // 1 = from dying 2 = regular portals
                int targetid = p.ReadInt(); // FF FF FF FF

                string startwp = p.ReadMapleString();
                IMaplePortal portal = c.Character.Map.getPortal(startwp);

                MapleCharacter player = c.Character;
                if (targetid != -1 && !c.Character.IsAlive)
                {
                    bool executeStandardPath = true;
                    //if (player.getEventInstance() != null)
                    //{
                    //    executeStandardPath = player.getEventInstance().revivePlayer(player);
                    //}
                    if (executeStandardPath)
                    {
                        if (c.Character.haveItem(5510000, 1, false, true))
                        {
                            c.Character.Hp = 50;
                            MapleInventoryManipulator.removeById(c, MapleInventoryType.Cash, 5510000, 1, true, false);
                            c.Character.changeMap(c.Character.Map, c.Character.Map.getPortal(0));
                            c.Character.UpdateSingleStat(MapleStat.Hp, 50);
                            c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText,
                                "使用了原地复活术。死亡后您在当前地图复活。"));
                        }
                        else
                        {
                            player.SetHp(50);
                            if (c.Character.Map.ForcedReturnMapId != 999999999)
                            {
                                MapleMap to = c.Character.Map.ForcedReturnMap;
                                IMaplePortal pto = to.getPortal(0);
                                player.Stance = 0;
                                player.changeMap(to, pto);
                            }
                            else
                            {
                                MapleMap to = c.Character.Map.ReturnMap;
                                IMaplePortal pto = to.getPortal(0);
                                player.Stance = 0;
                                player.changeMap(to, pto);
                            }
                        }
                    }
                }
                else if (targetid != -1 && c.Character.GmLevel > 0)
                {
                    MapleMap to = c.ChannelServer.MapFactory.GetMap(targetid);
                    IMaplePortal pto = to.getPortal(0);
                    player.changeMap(to, pto);
                }
                else if (targetid != -1 && c.Character.GmLevel == 0)
                {
                    MapleMap to = c.ChannelServer.MapFactory.GetMap(targetid);
                    if (c.Character.GmLevel > 0 || (player.Map.MapId == 0 && to.MapId == 10000) || (player.Map.MapId == 914090010 && to.MapId == 914090011) || (player.Map.MapId == 914090011 && to.MapId == 914090012) || (player.Map.MapId == 914090012 && to.MapId == 914090013) || (player.Map.MapId == 914090013 && to.MapId == 140090000))
                    {
                        IMaplePortal pto = to.getPortal(0);
                        player.changeMap(to, pto);
                    }
                    else
                    {
                        c.Send(PacketCreator.EnableActions());
                        Console.WriteLine("玩家 {0} 试图以非正常方式切换地图！", c.Character.Name);
                    }
                }
                else if (portal != null)
                {
                    portal.EnterPortal(c);
                }
                else
                {
                    c.Send(PacketCreator.EnableActions());
                    Console.WriteLine("Portal {0} not found on map {1}", startwp, c.Character.Map.MapId);
                }
            }
        }

        public static void GENERAL_CHAT(MapleClient c, InPacket p)
        {
            string text = p.ReadMapleString();
            bool show = p.ReadBool();
            Console.WriteLine(show);
            if (c.Character.AntiCheatTracker.TextSpam(text) && c.Character.GmLevel ==0)
            {
                c.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, "Too much chatting"));
                return;
            }

            if (text.Length > 70 && c.Character.GmLevel == 0)
            {
                return;
            }

            c.Character.Map.BroadcastMessage(PacketCreator.GetChatText(c.Character.Id, text,
                c.Character.GmLevel >= 3 && /*c.getChannelServer().allowGmWhiteText()*/ true, show));

            //if (!CommandProcessor.getInstance().processCommand(c, text))
            //{
            //    if (c.getPlayer().isMuted() || (c.getPlayer().getMap().getMuted() && !c.getPlayer().isGM()))
            //    {
            //        c.getPlayer().dropMessage(5, c.getPlayer().isMuted() ? "You are " : "The map is " + "muted, therefore you are unable to talk.");
            //        return;
            //    }
            //    c.getPlayer().getMap().broadcastMessage(MaplePacketCreator.getChatText(c.getPlayer().getId(), text, c.getPlayer().hasGMLevel(3) && c.getChannelServer().allowGmWhiteText(), show));
            //}
        }

        public static void NPC_TALK(MapleClient c, InPacket p)
        {

            MapleCharacter player = c.Character;
            //player.setCurrenttime(System.currentTimeMillis());
            //if (player.getCurrenttime() - player.getLasttime() < player.getDeadtime())
            //{
            //    player.dropMessage("系统错误.请稍后再试");
            //    c.getSession().write(MaplePacketCreator.enableActions());
            //    return;
            //}
            //player.setLasttime(System.currentTimeMillis());

            int oid = p.ReadInt();
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
            else {
                //if (c.getCM() != null || c.getQM() != null)
                //{
                //    c.Send(PacketCreator.EnableActions());
                //    return;
                //}
                //if (c.getCM() == null)
                //{
                    NPCScriptManager.Instance.Start(c, npc.Id);
                //}
                // 0 = next button
                // 1 = yes no
                // 2 = accept decline
                // 5 = select a link
            }
        }

        public static void MOVE_LIFE(MapleClient c, InPacket p)
        {
            int objectid = p.ReadInt();
            short moveid = p.ReadShort();

            IMapleMapObject mmo ;
            if (!c.Character.Map.Mapobjects.TryGetValue(objectid,out mmo) || mmo.GetType() != MapleMapObjectType.Monster)
            {
                return;
            }

            MapleMonster monster = (MapleMonster)mmo;
            bool noPacket = monster.IsMoveLock;

            List<ILifeMovementFragment> res = null;
            byte skillByte = p.ReadByte();
            byte skill = p.ReadByte();
            byte skill1 = p.ReadByte();
            byte skill2 = p.ReadByte();
            byte skill3 = p.ReadByte();
            p.ReadByte();

            MobSkill toUse = null;

            if (skillByte == 1 && monster.Stats.GetSkillsCount() > 0)
            {
                int random = (int)(Randomizer.NextDouble() * monster.Stats.GetSkillsCount());
                var skillToUse = monster.Stats.GetSkills()[random];
                toUse = MobSkillFactory.getMobSkill(skillToUse.Item1, skillToUse.Item2);
                if (!monster.canUseSkill(toUse))
                {
                    toUse = null;
                }
            }

            if (skill1 >= 100 && skill1 <= 200 && monster.Stats.HasSkill(skill1, skill2))
            {
                MobSkill skillData = MobSkillFactory.getMobSkill(skill1, skill2);
                if (skillData != null && monster.canUseSkill(skillData))
                {
                    skillData.applyEffect(c.Character, monster, true);
                }
            }

            p.ReadByte();
            p.ReadInt(); // whatever
            p.ReadLong();

            int startX = p.ReadShort(); // hmm.. startpos?
            int startY = p.ReadShort(); // hmm...
            Point startPos = new Point(startX, startY);

            res = AbstractMovementPacketHandler.ParseMovement(p);

            if (monster.GetController() != c.Character)
            {
                if (monster.isAttackedBy(c.Character))
                { // aggro and controller change
                    monster.switchController(c.Character, true);
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
                    monster.ControllerHasAggro=(false);
                    monster.ControllerKnowsAboutAggro=(false);
                }
                if (!monster.Stats.IsFirstAttack)
                {
                    monster.ControllerHasAggro=(true);
                    monster.ControllerKnowsAboutAggro=(true);
                }
            }

            bool aggro = monster.ControllerHasAggro;
            if (toUse != null)
            {
                if (!noPacket)
                {
                    c.Send(PacketCreator.moveMonsterResponse(objectid, moveid, monster.Stats.Mp, aggro, toUse.skillId,toUse.skillLevel));
                }
            }
            else
            {
                if (!noPacket)
                {
                    c.Send(PacketCreator.moveMonsterResponse(objectid, moveid, monster.Stats.Mp, aggro));
                }

            }

            if (aggro)
            {
                monster.ControllerKnowsAboutAggro=(true);
            }

            if (res != null)
            {
                //if (slea.available() != 9) {
                //log.warn("slea.available != 9 (movement parsing error)");
                //return;
                //}
                OutPacket packet = PacketCreator.moveMonster(skillByte, skill, skill1, skill2, skill3, objectid, startPos, res);
                c.Character.Map.BroadcastMessage(c.Character, packet, monster.Position);
                AbstractMovementPacketHandler.UpdatePosition(res, monster, -1);
                c.Character.Map.moveMonster(monster, monster.Position);
                c.Character.AntiCheatTracker.CheckMoveMonster(monster.Position);
            }
        }
    }
}
