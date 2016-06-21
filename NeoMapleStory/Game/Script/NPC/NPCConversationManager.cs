using System.Collections.Generic;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Shop;
using NeoMapleStory.Game.World;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using NeoMapleStory.Game.Skill;

namespace NeoMapleStory.Game.Script.NPC
{
    public class NpcConversationManager : AbstractPlayerInteraction
    {
        public string ReturnText { get; set; }
        public bool IsCash { get; set; } = false;
        private List<MaplePartyCharacter> m_otherParty;

        public NpcConversationManager(MapleClient c, int npc)
            : base(c)
        {
            NpcId = npc;
        }

        public NpcConversationManager(MapleClient c, int npc, MapleCharacter chr)
            : base(c)
        {
            NpcId = npc;
        }


        public NpcConversationManager(MapleClient c, int npc, List<MaplePartyCharacter> otherParty, int b)
            : base(c)
        {
            //CPQ
            NpcId = npc;
            this.m_otherParty = otherParty;
        }

        public int NpcId { get; }

        public void GainNexonPoint(int nxcash) => Player.GainNexonPoint(nxcash);

        //public int getboss()
        //{
        //    int money = 0;
        //    try
        //    {
        //        int cid = getPlayer().getAccountID();
        //        Connection con = DatabaseConnection.getConnection();
        //        PreparedStatement limitCheck = con.prepareStatement("SELECT * FROM accounts WHERE id=" + cid + "");
        //        ResultSet rs = limitCheck.executeQuery();
        //        if (rs.next())
        //        {
        //            money = rs.getInt("money");
        //        }
        //        limitCheck.close();
        //        rs.close();
        //    }
        //    catch (SQLException ex)
        //    {
        //    }
        //    return money;
        //}

        //public void setboss(int slot)
        //{
        //    try
        //    {
        //        int cid = getPlayer().getAccountID();
        //        Connection con = DatabaseConnection.getConnection();
        //        PreparedStatement ps = con.prepareStatement("UPDATE accounts SET money =money+ " + slot + " WHERE id = " + cid + "");
        //        ps.executeUpdate();
        //        ps.close();
        //    }
        //    catch (SQLException ex)
        //    {
        //    }
        //}


        //public MapleCharacter getSquadMember(MapleSquadType type, int index)
        //{
        //    MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //    MapleCharacter ret = null;
        //    if (squad != null)
        //    {
        //        ret = squad.getMembers().get(index);
        //    }
        //    return ret;
        //}

        //public int getSquadState(MapleSquadType type)
        //{
        //    MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //    if (squad != null)
        //    {
        //        return squad.getStatus();
        //    }
        //    else {
        //        return 0;
        //    }
        //}

        //public void setSquadState(MapleSquadType type, int state)
        //{
        //    MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //    if (squad != null)
        //    {
        //        squad.setStatus(state);
        //    }
        //}

        //public void warpRandom(int mapid)
        //{
        //    MapleMap target = c.getChannelServer().getMapFactory().getMap(mapid);
        //    MaplePortal portal = target.getPortal((int)(Math.random() * (target.getPortals().size()))); //generate random portal
        //    getPlayer().changeMap(target, portal);
        //}

        //public void reloadChar()
        //{
        //    getPlayer().saveToDB(true); //更新数据
        //    getPlayer().getClient().getSession().write(MaplePacketCreator.getCharInfo(getPlayer()));
        //    getPlayer().getMap().removePlayer(getPlayer());
        //    getPlayer().getMap().addPlayer(getPlayer());
        //}

        public void Close()=>NpcScriptManager.Instance.Close(this);

        public void SendNext(string text, int speaker = 0)
            => Client.Send(PacketCreator.NpcTalk(PacketCreator.NpcTalkType.Next, NpcId, text, (byte) speaker));


        public void SendPrev(string text, int speaker = 0)
            => Client.Send(PacketCreator.NpcTalk(PacketCreator.NpcTalkType.Prve, NpcId, text, (byte) speaker));


        public void SendNextPrev(string text, int speaker = 0)
            => Client.Send(PacketCreator.NpcTalk(PacketCreator.NpcTalkType.NextPrve, NpcId, text, (byte) speaker));


        public void SendOk(string text, int speaker = 0)
            => Client.Send(PacketCreator.NpcTalk(PacketCreator.NpcTalkType.Ok, NpcId, text, (byte) speaker));


        public void SendYesNo(string text, int speaker = 0)
            => Client.Send(PacketCreator.NpcTalk(PacketCreator.NpcTalkType.YesNo, NpcId, text, (byte) speaker));


        public void SendAcceptDecline(string text, int speaker = 0)
            => Client.Send(PacketCreator.NpcTalk(PacketCreator.NpcTalkType.AcceptDecline, NpcId, text, (byte) speaker));


        public void SendChoice(string text, int speaker = 0)
            => Client.Send(PacketCreator.NpcTalk(PacketCreator.NpcTalkType.Simple, NpcId, text, (byte) speaker));


        public void SendStyle(string text, int[] styles, int card)
            => Client.Send(PacketCreator.NpcTalkStyle(NpcId, text, styles, card));


        public void SendGetNumber(string text, int def, int min, int max)
            => Client.Send(PacketCreator.NpcTalkNum(NpcId, text, def, min, max));


        public void SendGetText(string text) => Client.Send(PacketCreator.NpcTalkText(NpcId, text));

        public void OpenShop(int id)=> MapleShopFactory.Instance.GetShop(id).SendShop(Client);

        public void OpenNpc(int id)
        {
            Close();
            NpcScriptManager.Instance.Start(Client, id);
        }

        public void ChangeJob(MapleJob job)=> Player.ChangeJob(job);

        public void ChangeJob(short jobId)=> ChangeJob(new MapleJob(jobId));

        public MapleJob GetJob() => Player.Job;

        //        public void startQuest(int id)
        //        {
        //            startQuest(id, false);
        //        }

        //        public void startQuest(int id, boolean force)
        //        {
        //            MapleQuest.getInstance(id).start(getPlayer(), npc, force);
        //        }

        //        public void completeQuest(int id)
        //        {
        //            completeQuest(id, false);
        //        }

        //        public void completeQuest(int id, boolean force)
        //        {
        //            MapleQuest.getInstance(id).complete(getPlayer(), npc, force);
        //        }

        //        public void forfeitQuest(int id)
        //        {
        //            MapleQuest.getInstance(id).forfeit(getPlayer());
        //        }

        public int GetMeso() => Player.Meso.Value;

        public void GiveMeso(int value) => Player.GainMeso(value, true, false, true);

        public void GiveExp(int value) => Player.GainExp(value, true, true);

        //        public int getNpc()
        //        {
        //            return npc;
        //        }

        //        @Deprecated  //增加BOSS函数
        //    public MapleCharacter getBOSS()
        //        {
        //            return getPlayer();
        //        }

        public byte GetLevel() => Player.Level;

        //        public void unequipEverything()
        //        {
        //            MapleInventory equipped = getPlayer().getInventory(MapleInventoryType.EQUIPPED);
        //            MapleInventory equip = getPlayer().getInventory(MapleInventoryType.EQUIP);
        //            List<Byte> ids = new LinkedList<>();
        //            for (IItem item : equipped.list())
        //            {
        //                ids.add(item.getPosition());
        //            }
        //            for (byte id : ids)
        //            {
        //                MapleInventoryManipulator.unequip(getC(), id, equip.getNextFreeSlot());
        //            }
        //        }

        public void TeachSkill(int id, byte level, byte masterlevel) => Player.ChangeSkillLevel(SkillFactory.GetSkill(id), level, masterlevel);

        public void ClearSkills()
        {
            foreach (var skill in Player.Skills)
            {
                Player.ChangeSkillLevel(skill.Key, 0, 0);
            }
        }

        //        public EventManager getEventManager(String event) {
        //            return getClient().getChannelServer().getEventSM().getEventManager(event);
        //        }

        //        public void showEffect(String effect)
        //        {
        //            getPlayer().getMap().broadcastMessage(MaplePacketCreator.showEffect(effect));
        //        }

        //        public void playSound(String sound)
        //        {
        //            getClient().getPlayer().getMap().broadcastMessage(MaplePacketCreator.playSound(sound));
        //        }

        //        @Override
        //    public String toString()
        //        {
        //            return "Conversation with NPC: " + npc;
        //        }

        //        public void updateBuddyCapacity(int capacity)
        //        {
        //            getPlayer().setBuddyCapacity(capacity);
        //        }

        //        public int getBuddyCapacity()
        //        {
        //            return getPlayer().getBuddyCapacity();
        //        }

        //        public void setHair(int hair)
        //        {
        //            getPlayer().setHair(hair);
        //            getPlayer().updateSingleStat(MapleStat.HAIR, hair);
        //            getPlayer().equipChanged();
        //        }

        //        public void setFace(int face)
        //        {
        //            getPlayer().setFace(face);
        //            getPlayer().updateSingleStat(MapleStat.FACE, face);
        //            getPlayer().equipChanged();
        //        }

        //        public void setSkin(int color)
        //        {
        //            getPlayer().setSkinColor(c.getPlayer().getSkinColor().getById(color));
        //            getPlayer().updateSingleStat(MapleStat.SKIN, color);
        //            getPlayer().equipChanged();
        //        }

        //        public void warpParty(int mapId)
        //        {
        //            MapleMap target = getMap(mapId);
        //            for (MaplePartyCharacter chrs : getPlayer().getParty().getMembers())
        //            {
        //                MapleCharacter curChar = c.getChannelServer().getPlayerStorage().getCharacterByName(chrs.getName());
        //                if ((curChar.getEventInstance() == null && c.getPlayer().getEventInstance() == null) || curChar.getEventInstance() == getPlayer().getEventInstance())
        //                {
        //                    curChar.changeMap(target, target.getPortal(0));
        //                }
        //            }
        //        }

        //        public void warpPartyWithExp(int mapId, int exp)
        //        {
        //            MapleMap target = getMap(mapId);
        //            for (MaplePartyCharacter chrs : getPlayer().getParty().getMembers())
        //            {
        //                MapleCharacter curChar = c.getChannelServer().getPlayerStorage().getCharacterByName(chrs.getName());
        //                if ((curChar.getEventInstance() == null && c.getPlayer().getEventInstance() == null) || curChar.getEventInstance() == getPlayer().getEventInstance())
        //                {
        //                    curChar.changeMap(target, target.getPortal(0));
        //                    curChar.gainExp(exp, true, false, true);
        //                }
        //            }
        //        }

        //        public void givePartyExp(int exp)
        //        {
        //            for (MaplePartyCharacter chrs : getPlayer().getParty().getMembers())
        //            {
        //                MapleCharacter curChar = c.getChannelServer().getPlayerStorage().getCharacterByName(chrs.getName());
        //                curChar.gainExp(exp, true, false, true);
        //            }
        //        }

        //        public void warpPartyWithExpMeso(int mapId, int exp, int meso)
        //        {
        //            MapleMap target = getMap(mapId);
        //            for (MaplePartyCharacter chrs : getPlayer().getParty().getMembers())
        //            {
        //                MapleCharacter curChar = c.getChannelServer().getPlayerStorage().getCharacterByName(chrs.getName());
        //                if ((curChar.getEventInstance() == null && c.getPlayer().getEventInstance() == null) || curChar.getEventInstance() == getPlayer().getEventInstance())
        //                {
        //                    curChar.changeMap(target, target.getPortal(0));
        //                    curChar.gainExp(exp, true, false, true);
        //                    curChar.gainMeso(meso, true);
        //                }
        //            }
        //        }

        //        public List<MapleCharacter> getPartyMembers()
        //        {
        //            return c.getPlayer().getParty().getPartyMembers();
        //        }

        //        public int itemQuantity(int itemid)
        //        {
        //            return getPlayer().getInventory(MapleItemInformationProvider.getInstance().getInventoryType(itemid)).countById(itemid);
        //        }

        //        public MapleSquad createMapleSquad(MapleSquadType type)
        //        {
        //            MapleSquad squad = new MapleSquad(c.getChannel(), getPlayer());
        //            if (getSquadState(type) == 0)
        //            {
        //                c.getChannelServer().addMapleSquad(squad, type);
        //            }
        //            else {
        //                return null;
        //            }
        //            return squad;
        //        }

        //        public boolean checkSquadLeader(MapleSquadType type)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            if (squad != null)
        //            {
        //                if (squad.getLeader().getId() == getPlayer().getId())
        //                {
        //                    return true;
        //                }
        //                else {
        //                    return false;
        //                }
        //            }
        //            else {
        //                return false;
        //            }
        //        }

        //        public void removeMapleSquad(MapleSquadType type)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            if (squad != null)
        //            {
        //                if (squad.getLeader().getId() == getPlayer().getId())
        //                {
        //                    squad.clear();
        //                    c.getChannelServer().removeMapleSquad(squad, type);
        //                }
        //            }
        //        }

        //        public int numSquadMembers(MapleSquadType type)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            int ret = 0;
        //            if (squad != null)
        //            {
        //                ret = squad.getSquadSize();
        //            }
        //            return ret;
        //        }

        //        public boolean isSquadMember(MapleSquadType type)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            boolean ret = false;
        //            if (squad.containsMember(getPlayer()))
        //            {
        //                ret = true;
        //            }
        //            return ret;
        //        }

        //        public void addSquadMember(MapleSquadType type)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            if (squad != null)
        //            {
        //                squad.addMember(getPlayer());
        //            }
        //        }

        //        public void addRandomItem(int id)
        //        {
        //            MapleItemInformationProvider i = MapleItemInformationProvider.getInstance();
        //            MapleInventoryManipulator.addFromDrop(getClient(), i.randomizeStats((Equip)i.getEquipById(id)), true);
        //        }

        //        public void removeSquadMember(MapleSquadType type, MapleCharacter chr, boolean ban)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            if (squad != null)
        //            {
        //                squad.banMember(chr, ban);
        //            }
        //        }

        //        public void removeSquadMember(MapleSquadType type, int index, boolean ban)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            if (squad != null)
        //            {
        //                MapleCharacter chrs = squad.getMembers().get(index);
        //                squad.banMember(chrs, ban);
        //            }
        //        }

        //        public boolean canAddSquadMember(MapleSquadType type)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            if (squad != null)
        //            {
        //                if (squad.isBanned(getPlayer()))
        //                {
        //                    return false;
        //                }
        //                else {
        //                    return true;
        //                }
        //            }
        //            return false;
        //        }

        //        public void warpSquadMembers(MapleSquadType type, int mapId)
        //        {
        //            MapleSquad squad = c.getChannelServer().getMapleSquad(type);
        //            MapleMap map = c.getChannelServer().getMapFactory().getMap(mapId);
        //            if (squad != null)
        //            {
        //                if (checkSquadLeader(type))
        //                {
        //                    for (MapleCharacter chrs : squad.getMembers())
        //                    {
        //                        chrs.changeMap(map, map.getPortal(0));
        //                    }
        //                }
        //            }
        //        }

        //        public MapleSquad getMapleSquad(MapleSquadType type)
        //        {
        //            return c.getChannelServer().getMapleSquad(type);
        //        }

        //        public void setSquadBossLog(MapleSquadType type, String boss)
        //        {
        //            if (getMapleSquad(type) != null)
        //            {
        //                MapleSquad squad = getMapleSquad(type);
        //                for (MapleCharacter chrs : squad.getMembers())
        //                {
        //                    chrs.setBossLog(boss);
        //                }
        //            }
        //        }

        //        public MapleCharacter getCharByName(String name)
        //        {
        //            try
        //            {
        //                return c.getChannelServer().getPlayerStorage().getCharacterByName(name);
        //            }
        //            catch (Exception e)
        //            {
        //                return null;
        //            }
        //        }

        //        public void resetReactors()
        //        {
        //            getPlayer().getMap().resetReactors();
        //        }

        //        public void displayGuildRanks()
        //        {
        //            MapleGuild.displayGuildRanks(getClient(), npc);
        //        }

        //        public MapleCharacter getCharacter()
        //        {
        //            return chr;
        //        }

        //        public void warpAllInMap(int mapid, int portal)
        //        {
        //            MapleMap outMap;
        //            MapleMapFactory mapFactory;
        //            mapFactory = ChannelServer.getInstance(c.getChannel()).getMapFactory();
        //            outMap = mapFactory.getMap(mapid);
        //            for (MapleCharacter aaa : outMap.getCharacters())
        //            {
        //                //Warp everyone out
        //                mapFactory = ChannelServer.getInstance(aaa.getClient().getChannel()).getMapFactory();
        //                aaa.getClient().getPlayer().changeMap(outMap, outMap.getPortal(portal));
        //                outMap = mapFactory.getMap(mapid);
        //                aaa.getClient().getPlayer().getEventInstance().unregisterPlayer(aaa.getClient().getPlayer()); //Unregister them all
        //            }
        //        }

        //        public int countMonster()
        //        {
        //            MapleMap map = c.getPlayer().getMap();
        //            double range = Double.POSITIVE_INFINITY;
        //            List<MapleMapObject> monsters = map.getMapObjectsInRange(c.getPlayer().getPosition(), range, Arrays.asList(MapleMapObjectType.MONSTER));
        //            return monsters.size();
        //        }

        //        public int countReactor()
        //        {
        //            MapleMap map = c.getPlayer().getMap();
        //            double range = Double.POSITIVE_INFINITY;
        //            List<MapleMapObject> reactors = map.getMapObjectsInRange(c.getPlayer().getPosition(), range, Arrays.asList(MapleMapObjectType.REACTOR));
        //            return reactors.size();
        //        }

        //        public int getDayOfWeek()
        //        {
        //            Calendar cal = Calendar.getInstance();
        //            int dayy = cal.get(Calendar.DAY_OF_WEEK);
        //            return dayy;
        //        }

        //        public void giveNPCBuff(MapleCharacter chr, int itemID)
        //        {
        //            MapleItemInformationProvider mii = MapleItemInformationProvider.getInstance();
        //            MapleStatEffect statEffect = mii.getItemEffect(itemID);
        //            statEffect.applyTo(chr);
        //        }

        //        public void giveWonkyBuff(MapleCharacter chr)
        //        {
        //            long what = Math.round(Math.random() * 4);
        //            int what1 = (int)what;
        //            int Buffs[] = { 2022090, 2022091, 2022092, 2022093 };
        //            int buffToGive = Buffs[what1];
        //            MapleItemInformationProvider mii = MapleItemInformationProvider.getInstance();
        //            MapleStatEffect statEffect = mii.getItemEffect(buffToGive);
        //            MapleCharacter character = chr;
        //            statEffect.applyTo(character);
        //        }

        //        public boolean hasSkill(int skillid)
        //        {
        //            ISkill theSkill = SkillFactory.getSkill(skillid);
        //            if (theSkill != null)
        //            {
        //                return c.getPlayer().getSkillLevel(theSkill) > 0;
        //            }
        //            else {
        //                return false;
        //            }
        //        }

        //        public void spawnMonster(int mobid, int HP, int MP, int level, int EXP, int boss, int undead, int amount, int x, int y)
        //        {
        //            MapleMonsterStats newStats = new MapleMonsterStats();
        //            Point spawnPos = new Point(x, y);
        //            if (HP >= 0)
        //            {
        //                newStats.setHp(HP);
        //            }
        //            if (MP >= 0)
        //            {
        //                newStats.setMp(MP);
        //            }
        //            if (level >= 0)
        //            {
        //                newStats.setLevel(level);
        //            }
        //            if (EXP >= 0)
        //            {
        //                newStats.setExp(EXP);
        //            }
        //            newStats.setBoss(boss == 1);
        //            newStats.setUndead(undead == 1);
        //            for (int i = 0; i < amount; i++)
        //            {
        //                MapleMonster npcmob = MapleLifeFactory.getMonster(mobid);
        //                npcmob.setOverrideStats(newStats);
        //                npcmob.setHp(npcmob.getMaxHp());
        //                npcmob.setMp(npcmob.getMaxMp());
        //                getPlayer().getMap().spawnMonsterOnGroundBelow(npcmob, spawnPos);
        //            }
        //        }

        //        public int getExpRate()
        //        {
        //            return getClient().getChannelServer().getExpRate();
        //        }

        //        public int getDropRate()
        //        {
        //            return getClient().getChannelServer().getDropRate();
        //        }

        //        public int getBossDropRate()
        //        {
        //            return getClient().getChannelServer().getBossDropRate();
        //        }

        //        public int getMesoRate()
        //        {
        //            return getClient().getChannelServer().getMesoRate();
        //        }

        //        public boolean removePlayerFromInstance()
        //        {
        //            if (getClient().getPlayer().getEventInstance() != null)
        //            {
        //                getClient().getPlayer().getEventInstance().removePlayer(getClient().getPlayer());
        //                return true;
        //            }
        //            return false;
        //        }

        //        public boolean isPlayerInstance()
        //        {
        //            if (getClient().getPlayer().getEventInstance() != null)
        //            {
        //                return true;
        //            }
        //            return false;
        //        }

        //        public void openDuey()
        //        {
        //            c.getSession().write(MaplePacketCreator.sendDuey((byte)9, DueyActionHandler.loadItems(c.getPlayer())));
        //        }

        //        public void finishAchievement(int id)
        //        {
        //            getPlayer().finishAchievement(id);
        //        }

        //        public void changeStat(byte slot, int type, short amount)
        //        {
        //            Equip sel = (Equip)c.getPlayer().getInventory(MapleInventoryType.EQUIPPED).getItem(slot);
        //            switch (type)
        //            {
        //                case 0:
        //                    sel.setStr(amount);
        //                    break;
        //                case 1:
        //                    sel.setDex(amount);
        //                    break;
        //                case 2:
        //                    sel.setInt(amount);
        //                    break;
        //                case 3:
        //                    sel.setLuk(amount);
        //                    break;
        //                case 4:
        //                    sel.setHp(amount);
        //                    break;
        //                case 5:
        //                    sel.setMp(amount);
        //                    break;
        //                case 6:
        //                    sel.setWatk(amount);
        //                    break;
        //                case 7:
        //                    sel.setMatk(amount);
        //                    break;
        //                case 8:
        //                    sel.setWdef(amount);
        //                    break;
        //                case 9:
        //                    sel.setMdef(amount);
        //                    break;
        //                case 10:
        //                    sel.setAcc(amount);
        //                    break;
        //                case 11:
        //                    sel.setAvoid(amount);
        //                    break;
        //                case 12:
        //                    sel.setHands(amount);
        //                    break;
        //                case 13:
        //                    sel.setSpeed(amount);
        //                    break;
        //                case 14:
        //                    sel.setJump(amount);
        //                    break;
        //                default:
        //                    break;
        //            }
        //            c.getPlayer().equipChanged();
        //        }

        //        public void removeHiredMerchantItem(int id)
        //        {
        //            Connection con = DatabaseConnection.getConnection();
        //            try
        //            {
        //                PreparedStatement ps = con.prepareStatement("DELETE FROM hiredmerchant WHERE id = ?");
        //                ps.setInt(1, id);
        //                ps.executeUpdate();
        //                ps.close();
        //            }
        //            catch (SQLException se)
        //            {
        //            }
        //        }

        //        public boolean hasTemp()
        //        {
        //            if (!getPlayer().hasMerchant() && getPlayer().tempHasItems())
        //            {
        //                return true;
        //            }
        //            else {
        //                return false;
        //            }
        //        }

        //        public void removeHiredMerchantItem(boolean tempItem, int itemId)
        //        {
        //            String Table = "hiredmerchant";
        //            if (tempItem)
        //            {
        //                Table = "hiredmerchanttemp";
        //            }
        //            Connection con = DatabaseConnection.getConnection();
        //            try
        //            {
        //                PreparedStatement ps = con.prepareStatement("DELETE FROM " + Table + " WHERE itemid = ? AND ownerid = ? LIMIT 1");
        //                ps.setInt(1, itemId);
        //                ps.setInt(2, getPlayer().getId());
        //                ps.executeUpdate();
        //                ps.close();
        //            }
        //            catch (SQLException se)
        //            {
        //            }
        //        }

        //        public long getHiredMerchantMesos()
        //        {
        //            Connection con = DatabaseConnection.getConnection();
        //            long mesos;
        //            try
        //            {
        //                PreparedStatement ps = con.prepareStatement("SELECT MerchantMesos FROM characters WHERE id = ?");
        //                ps.setInt(1, getPlayer().getId());
        //                ResultSet rs = ps.executeQuery();
        //                rs.next();
        //                mesos = rs.getLong("MerchantMesos");
        //                rs.close();
        //                ps.close();
        //            }
        //            catch (SQLException se)
        //            {
        //                return 0;
        //            }
        //            return mesos;
        //        }

        //        public void setHiredMerchantMesos(long set)
        //        {
        //            Connection con = DatabaseConnection.getConnection();
        //            try
        //            {
        //                PreparedStatement ps = con.prepareStatement("UPDATE characters SET MerchantMesos = ? WHERE id = ?");
        //                ps.setLong(1, set);
        //                ps.setInt(2, getPlayer().getId());
        //                ps.executeUpdate();
        //                ps.close();
        //            }
        //            catch (Exception e)
        //            {
        //                e.printStackTrace();
        //            }
        //        }

        //        public List<Pair<Integer, IItem>> getStoredMerchantItems()
        //        {
        //            Connection con = DatabaseConnection.getConnection();
        //            List<Pair<Integer, IItem>> items = new ArrayList<>();
        //            try
        //            {
        //                PreparedStatement ps = con.prepareStatement("SELECT * FROM hiredmerchant WHERE ownerid = ? AND onSale = false");
        //                ps.setInt(1, getPlayer().getId());
        //                ResultSet rs = ps.executeQuery();
        //                while (rs.next())
        //                {
        //                    if (rs.getInt("type") == 1)
        //                    {
        //                        Equip eq = new Equip(rs.getInt("itemid"), (byte)0);
        //                        eq.setUpgradeSlots((byte)rs.getInt("upgradeslots"));
        //                        eq.setLevel((byte)rs.getInt("level"));
        //                        eq.setStr((short)rs.getInt("str"));
        //                        eq.setDex((short)rs.getInt("dex"));
        //                        eq.setInt((short)rs.getInt("int"));
        //                        eq.setLuk((short)rs.getInt("luk"));
        //                        eq.setHp((short)rs.getInt("hp"));
        //                        eq.setMp((short)rs.getInt("mp"));
        //                        eq.setWatk((short)rs.getInt("watk"));
        //                        eq.setMatk((short)rs.getInt("matk"));
        //                        eq.setWdef((short)rs.getInt("wdef"));
        //                        eq.setMdef((short)rs.getInt("mdef"));
        //                        eq.setAcc((short)rs.getInt("acc"));
        //                        eq.setAvoid((short)rs.getInt("avoid"));
        //                        eq.setHands((short)rs.getInt("hands"));
        //                        eq.setSpeed((short)rs.getInt("speed"));
        //                        eq.setJump((short)rs.getInt("jump"));
        //                        eq.setOwner(rs.getString("owner"));
        //                        items.add(new Pair<>(rs.getInt("id"), eq));
        //                    }
        //                    else if (rs.getInt("type") == 2)
        //                    {
        //                        Item newItem = new Item(rs.getInt("itemid"), (byte)0, (short)rs.getInt("quantity"));
        //                        newItem.setOwner(rs.getString("owner"));
        //                        items.add(new Pair<>(rs.getInt("id"), newItem));
        //                    }
        //                }
        //                ps.close();
        //                rs.close();
        //            }
        //            catch (SQLException se)
        //            {
        //                se.printStackTrace();
        //                return null;
        //            }
        //            return items;
        //        }

        //        public int getAverageLevel(int mapid)
        //        {
        //            int count = 0, total = 0;
        //            for (MapleMapObject mmo : c.getChannelServer().getMapFactory().getMap(mapid).getAllPlayers())
        //            {
        //                total += ((MapleCharacter)mmo).getLevel();
        //                count++;
        //            }
        //            return (total / count);
        //        }

        //        public void sendCPQMapLists()
        //        {
        //            String msg = "Pick a field:\\r\\n";
        //            for (int i = 0; i < 6; i++)
        //            {
        //                if (fieldTaken(i))
        //                {
        //                    if (fieldLobbied(i))
        //                    {
        //                        msg += "#b#L" + i + "#Monster Carnival Field " + (i + 1) + " Avg Lvl: " + getAverageLevel(980000100 + i * 100) + "#l\\r\\n";
        //                    }
        //                    else {
        //                        continue;
        //                    }
        //                }
        //                else {
        //                    msg += "#b#L" + i + "#Monster Carnival Field " + (i + 1) + "#l\\r\\n";
        //                }
        //            }
        //            sendSimple(msg);
        //        }

        //        public boolean fieldLobbied(int field)
        //        {
        //            if (c.getChannelServer().getMapFactory().getMap(980000100 + field * 100).getAllPlayers().size() >= 2)
        //            {
        //                return true;
        //            }
        //            else {
        //                return false;
        //            }
        //        }

        //        public boolean fieldTaken(int field)
        //        {
        //            MapleMapFactory mf = c.getChannelServer().getMapFactory();
        //            if ((mf.getMap(980000100 + field * 100).getAllPlayers().size() != 0)
        //                    || (mf.getMap(980000101 + field * 100).getAllPlayers().size() != 0)
        //                    || (mf.getMap(980000102 + field * 100).getAllPlayers().size() != 0))
        //            {
        //                return true;
        //            }
        //            else {
        //                return false;
        //            }
        //        }

        //        public void CPQLobby(int field)
        //        {
        //            try
        //            {
        //                MapleMap map;
        //                ChannelServer cs = c.getChannelServer();
        //                map = cs.getMapFactory().getMap(980000100 + 100 * field);
        //                for (MaplePartyCharacter mpc : c.getPlayer().getParty().getMembers())
        //                {
        //                    MapleCharacter mc;
        //                    mc = cs.getPlayerStorage().getCharacterByName(mpc.getName());
        //                    if (mc != null)
        //                    {
        //                        mc.changeMap(map, map.getPortal(0));
        //                        String msg = "You will now receive challenges from other parties. If you do not accept a challenge in 3 minutes, you will be kicked out.";
        //                        mc.getClient().getSession().write(MaplePacketCreator.serverNotice(5, msg));
        //                        mc.getClient().getSession().write(MaplePacketCreator.getClock(3 * 60));
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                e.printStackTrace();
        //            }
        //        }

        //        public void deleteItem(int inventorytype)
        //        {
        //            Connection con;
        //            try
        //            {
        //                con = DatabaseConnection.getConnection();
        //                PreparedStatement ps = con.prepareStatement("Select * from inventoryitems where characterid=? and inventorytype=?");
        //                ps.setInt(1, getPlayer().getId());
        //                ps.setInt(2, inventorytype);
        //                ResultSet re = ps.executeQuery();
        //                MapleInventoryType type = null;
        //                switch (inventorytype)
        //                {
        //                    case 1:
        //                        type = MapleInventoryType.EQUIP;
        //                        break;
        //                    case 2:
        //                        type = MapleInventoryType.USE;
        //                        break;
        //                    case 3:
        //                        type = MapleInventoryType.SETUP;
        //                        break;
        //                    case 4:
        //                        type = MapleInventoryType.ETC;
        //                        break;
        //                    case 5:
        //                        type = MapleInventoryType.CASH;
        //                }

        //                while (re.next())
        //                {
        //                    MapleInventoryManipulator.removeById(getC(), type, re.getInt("itemid"), 1, true, true);
        //                }

        //                re.close();
        //                ps.close();
        //            }
        //            catch (SQLException ex)
        //            {
        //            }
        //        }

        //        public void closePortal(int mapid, String pName)
        //        {
        //            getClient().getChannelServer().getMapFactory().getMap(mapid).getPortal(pName).setPortalState(false);
        //        }

        //        public void openPortal(int mapid, String pName)
        //        {
        //            getClient().getChannelServer().getMapFactory().getMap(mapid).getPortal(pName).setPortalState(true);
        //        }

        //        public void challengeParty(int field)
        //        {
        //            MapleCharacter leader = null;
        //            MapleMap map = c.getChannelServer().getMapFactory().getMap(980000100 + 100 * field);
        //            for (MapleMapObject mmo : map.getAllPlayers())
        //            {
        //                MapleCharacter mc = (MapleCharacter)mmo;
        //                if (mc.getParty().getLeader().getId() == mc.getId())
        //                {
        //                    leader = mc;
        //                    break;
        //                }
        //            }
        //            if (leader != null)
        //            {
        //                if (!leader.isCPQChallenged())
        //                {
        //                    List<MaplePartyCharacter> challengers = new LinkedList<>();
        //                    for (MaplePartyCharacter member : c.getPlayer().getParty().getMembers())
        //                    {
        //                        challengers.add(member);
        //                    }
        //                    NPCScriptManager.getInstance().start("cpqchallenge", leader.getClient(), npc, challengers);
        //                }
        //                else {
        //                    sendOk("The other party is currently taking on a different challenge.");
        //                }
        //            }
        //            else {
        //                sendOk("Could not find leader!");
        //            }
        //        }

        //        public void startCPQ(final MapleCharacter challenger, int field)
        //        {
        //            try
        //            {
        //                if (challenger != null)
        //                {
        //                    if (challenger.getParty() == null)
        //                    {
        //                        throw new RuntimeException("ERROR: CPQ Challenger's party was null!");
        //                    }
        //                    for (MaplePartyCharacter mpc : challenger.getParty().getMembers())
        //                    {
        //                        MapleCharacter mc;
        //                        mc = c.getChannelServer().getPlayerStorage().getCharacterByName(mpc.getName());
        //                        if (mc != null)
        //                        {
        //                            mc.changeMap(c.getPlayer().getMap(), c.getPlayer().getMap().getPortal(0));
        //                            mc.getClient().getSession().write(MaplePacketCreator.getClock(10));
        //                        }
        //                    }
        //                }
        //                final int mapid = c.getPlayer().getMap().getId() + 1;
        //                TimerManager.getInstance().schedule(new Runnable() {

        //                @Override
        //                public void run()
        //        {
        //            MapleMap map;
        //            ChannelServer cs = c.getChannelServer();
        //            map = cs.getMapFactory().getMap(mapid);
        //            new MapleMonsterCarnival(getPlayer().getParty(), challenger.getParty(), mapid);
        //            map.broadcastMessage(MaplePacketCreator.serverNotice(5, "The Monster Carnival has begun!"));
        //        }
        //    }, 10000);
        //            mapMessage(5, "The Monster Carnival will begin in 10 seconds!");
        //} catch (Exception e) {
        //            e.printStackTrace();
        //        }
        //    }

        //    public int partyMembersInMap()
        //{
        //    int inMap = 0;
        //    for (MapleCharacter char2 : getPlayer().getMap().getCharacters())
        //    {
        //        if (char2.getParty() == getPlayer().getParty())
        //        {
        //            inMap++;
        //        }
        //    }
        //    return inMap;
        //}

        //public boolean gotoEvent()
        //{
        //    ChannelServer cserv = c.getChannelServer();
        //    MapleMap map = cserv.getMapFactory().getMap(cserv.eventmap);
        //    int level = getPlayer().getLevel();
        //    if (level >= cserv.level[0] && level <= cserv.level[1])
        //    {
        //        c.getPlayer().changeMap(map, map.getPortal(0));
        //        return true;
        //    }
        //    return false;
        //}

        //public boolean partyMemberHasItem(int iid)
        //{
        //    List<MapleCharacter> lmc = this.getPartyMembers();
        //    if (lmc == null)
        //    {
        //        return this.haveItem(iid);
        //    }
        //    for (MapleCharacter mc : lmc)
        //    {
        //        if (mc.haveItem(iid, 1, false, false))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public void spawnMob(int mobid, int HP, int MP, int level, int EXP, int boss, int undead, int amount, int x, int y)
        //{ //npc支持怪物召唤
        //    MapleMonsterStats newStats = new MapleMonsterStats();
        //    Point spawnPos = new Point(x, y);
        //    if (HP != 0)
        //    {
        //        newStats.setHp(HP);
        //    }
        //    if (MP != 0)
        //    {
        //        newStats.setMp(MP);
        //    }
        //    if (level != 0)
        //    {
        //        newStats.setLevel(level);
        //    }
        //    if (EXP != 0)
        //    {
        //        newStats.setExp(EXP);
        //    }
        //    if (boss == 1)
        //    {
        //        newStats.setBoss(true);
        //    }
        //    if (undead == 1)
        //    {
        //        newStats.setUndead(true);
        //    }
        //    for (int i = 0; i < amount; i++)
        //    {
        //        MapleMonster npcmob = MapleLifeFactory.getMonster(mobid);
        //        npcmob.setOverrideStats(newStats);
        //        npcmob.setHp(npcmob.getMaxHp());
        //        npcmob.setMp(npcmob.getMaxMp());
        //        getPlayer().getMap().spawnMonsterOnGroundBelow(npcmob, spawnPos);
        //    }
        //}

        //public void spawnMonster(int mobid, int x, int y)
        //{
        //    Point spawnPos = new Point(x, y);
        //    MapleMonster npcmob = MapleLifeFactory.getMonster(mobid);
        //    getPlayer().getMap().spawnMonsterOnGroundBelow(npcmob, spawnPos);
        //}

        //public void partyNotice(String message)
        //{
        //    List<MapleCharacter> lmc = this.getPartyMembers();
        //    if (lmc == null)
        //    {
        //        this.playerMessage(5, message);
        //        return;
        //    }
        //    else {
        //        for (MapleCharacter mc : lmc)
        //        {
        //            mc.dropMessage(5, message);
        //        }
        //    }
        //}

        //public String showSpeedRankings(int type)
        //{
        //    StringBuilder ranks = new StringBuilder("#b#eRankings for ");
        //    ranks.append(type == 0 ? "Zakum" : "Papulatus");
        //    ranks.append("#k#n\r\n\r\n");
        //    for (int i = 0; i < 10; i++)
        //    {
        //        long time = SpeedRankings.getTime(i, type);
        //        long mins = time / 1000 / 60;
        //        time -= mins * 1000 * 60;
        //        long seconds = time / 1000;
        //        ranks.append(i + 1);
        //        ranks.append(")#r ");
        //        ranks.append(SpeedRankings.getTeamMembers(i, type));
        //        ranks.append("#k ~ #g");
        //        ranks.append(mins);
        //        ranks.append("#km#d ");
        //        ranks.append(seconds);
        //        ranks.append("#ks");
        //        ranks.append("\r\n");
        //    }
        //    return ranks.toString();
        //}

        //public void serverNotice(String Text)
        //{
        //    getClient().getChannelServer().broadcastPacket(MaplePacketCreator.serverNotice(6, Text));
        //}

        //public boolean getHiredMerchantItems(boolean tempTable)
        //{
        //    boolean temp = false, compleated = false;
        //    String Table = "hiredmerchant";
        //    if (tempTable)
        //    {
        //        Table = "hiredmerchanttemp";
        //    }
        //    if (tempTable)
        //    {
        //        temp = true;
        //    }
        //    Connection con = DatabaseConnection.getConnection();
        //    try
        //    {
        //        PreparedStatement ps = con.prepareStatement("SELECT * FROM " + Table + " WHERE ownerid = ?");
        //        ps.setInt(1, getPlayer().getId());
        //        ResultSet rs = ps.executeQuery();
        //        while (rs.next())
        //        {
        //            if (rs.getInt("type") == 1)
        //            {
        //                Equip spItem = new Equip(rs.getInt("itemid"), (byte)0, false);
        //                spItem.setUpgradeSlots((byte)rs.getInt("upgradeslots"));
        //                spItem.setLevel((byte)rs.getInt("level"));
        //                spItem.setStr((short)rs.getInt("str"));
        //                spItem.setDex((short)rs.getInt("dex"));
        //                spItem.setInt((short)rs.getInt("int"));
        //                spItem.setLuk((short)rs.getInt("luk"));
        //                spItem.setHp((short)rs.getInt("hp"));
        //                spItem.setMp((short)rs.getInt("mp"));
        //                spItem.setWatk((short)rs.getInt("watk"));
        //                spItem.setMatk((short)rs.getInt("matk"));
        //                spItem.setWdef((short)rs.getInt("wdef"));
        //                spItem.setMdef((short)rs.getInt("mdef"));
        //                spItem.setAcc((short)rs.getInt("acc"));
        //                spItem.setAvoid((short)rs.getInt("avoid"));
        //                spItem.setHands((short)rs.getInt("hands"));
        //                spItem.setSpeed((short)rs.getInt("speed"));
        //                spItem.setJump((short)rs.getInt("jump"));
        //                spItem.setOwner(rs.getString("owner"));
        //                if (!getPlayer().getInventory(MapleInventoryType.EQUIP).isFull())
        //                {
        //                    MapleInventoryManipulator.addFromDrop(c, spItem, true);
        //                    removeHiredMerchantItem(temp, spItem.getItemId());
        //                }
        //                else {
        //                    rs.close();
        //                    ps.close();
        //                    return false;
        //                }
        //            }
        //            else {
        //                Item spItem = new Item(rs.getInt("itemid"), (byte)0, (short)rs.getInt("quantity"));
        //                MapleItemInformationProvider ii = MapleItemInformationProvider.getInstance();
        //                MapleInventoryType type = ii.getInventoryType(spItem.getItemId());
        //                if (!getPlayer().getInventory(type).isFull())
        //                {
        //                    MapleInventoryManipulator.addFromDrop(c, spItem, true);
        //                    removeHiredMerchantItem(temp, spItem.getItemId());
        //                }
        //                else {
        //                    rs.close();
        //                    ps.close();
        //                    return false;
        //                }
        //            }
        //        }
        //        rs.close();
        //        ps.close();
        //        compleated = true;
        //    }
        //    catch (SQLException se)
        //    {
        //        se.printStackTrace();
        //        return compleated;
        //    }
        //    return compleated;
        //}

        //@Override
        //    public void gainItem(int id, short quantity)
        //{
        //    if (quantity >= 0)
        //    {
        //        StringBuilder logInfo = new StringBuilder(c.getPlayer().getName());
        //        logInfo.append(" 收到数据 ");
        //        logInfo.append(quantity);
        //        logInfo.append(" 从脚本 PlayerInteraction (");
        //        logInfo.append(this.toString());
        //        logInfo.append(")");
        //        MapleInventoryManipulator.addById(c, id, quantity, logInfo.toString());
        //    }
        //    else {
        //        MapleInventoryManipulator.removeById(c, MapleItemInformationProvider.getInstance().getInventoryType(id), id, -quantity, true, false);
        //    }
        //    c.getSession().write(MaplePacketCreator.getShowItemGain(id, quantity, true));
        //}

        //@Override
        //    public void resetMap(int mapid)
        //{
        //    getClient().getChannelServer().getMapFactory().getMap(mapid).resetReactors();
        //}

        //public void maxAllSkills()
        //{
        //    getPlayer().maxAllSkills();
        //    getPlayer().getClient().getSession().write(MaplePacketCreator.serverNotice(6, "得到增益效果!！"));
        //}

        //public void summonBean(int mobid, int amount)
        //{
        //    MapleMonsterStats newStats = new MapleMonsterStats();
        //    if (amount <= 1)
        //    {
        //        MapleMonster npcmob = MapleLifeFactory.getMonster(mobid);
        //        npcmob.setOverrideStats(newStats);
        //        npcmob.setHp(npcmob.getMaxHp());
        //        Point pos = new Point(8, -42);
        //        getPlayer().getMap().spawnMonsterOnGroundBelow(npcmob, pos);
        //    }
        //    else {
        //        for (int i = 0; i < amount; i++)
        //        {
        //            Point pos = new Point(8, -42);
        //            MapleMonster npcmob = MapleLifeFactory.getMonster(mobid);
        //            npcmob.setOverrideStats(newStats);
        //            npcmob.setHp(npcmob.getMaxHp());
        //            getPlayer().getMap().spawnMonsterOnGroundBelow(npcmob, pos);
        //        }
        //    }
        //}
        ////得到当前玩家坐标位置 cm.getPosition()

        //public Point getPosition()
        //{
        //    Point pos = getPlayer().getPosition();
        //    return pos;
        //}
        ////得到当前NPC坐标位置 cm.getNPCPosition()

        //public Point getNPCPosition()
        //{
        //    MapleNPC thenpc = MapleLifeFactory.getNPC(this.npc);
        //    Point pos = thenpc.getPosition();
        //    return pos;
        //}
        ////刷出指定数量的怪物（简单1） cm.summonMob(怪物id,地图ID,怪物移动的横坐标范围, 怪物移动的纵坐标范围)

        //public void spawnMob(int mapid, int mid, int xpos, int ypos)
        //{
        //    getClient().getChannelServer().getMapFactory().getMap(mapid).spawnMonsterOnGroudBelow(MapleLifeFactory.getMonster(mid), new Point(xpos, ypos));
        //}

        ////刷出指定数量的怪物（简单2） cm.summonMob(怪物id)
        //public void summonMob(int mobid)
        //{
        //    getPlayer().getMap().spawnMonsterOnGroudBelow(MapleLifeFactory.getMonster(mobid), getNPCPosition());
        //}

        ////刷出指定数量的怪物（高级控制1）cm.summonMobAtPosition(怪物id, 怪物HP, 怪物经验, 刷怪数量, 怪物移动的横坐标范围, 怪物移动的纵坐标范围)
        ////刷出指定数量的怪物（高级控制2）cm.summonMobAtPosition(怪物id, 刷怪数量, 怪物移动的横坐标范围, 怪物移动的纵坐标范围)
        ////得到指定地图的怪总数
        //public int countRemoteMapMonster(int mapid)
        //{
        //    int MonsterNumber = 0;
        //    MapleMap map = c.getChannelServer().getMapFactory().getMap(mapid);
        //    double range = Double.POSITIVE_INFINITY;
        //    List<MapleMapObject> monsters = map.getMapObjectsInRange(c.getPlayer().getPosition(), range, Arrays.asList(MapleMapObjectType.MONSTER));
        //    for (MapleMapObject monstermo : monsters)
        //    {
        //        MonsterNumber = MonsterNumber + 1;
        //    }
        //    return MonsterNumber;
        //}
        ////闯关任务 - 接任务

        //public void TaskMake(int missionid)
        //{
        //    getPlayer().TaskMake(missionid);
        //}

        ////闯关任务 - 检查是否接过任务
        //public boolean TaskStatus(int missionid)
        //{
        //    return getPlayer().TaskStatus(missionid);
        //}

        ////闯关任务 - 得到当前关卡积分
        //public int TaskExp(int missionid)
        //{
        //    return getPlayer().TaskExp(missionid);
        //}

        ////闯关任务 - 得到闯关积分
        //public void TaskAddExp(int missionid, int addexp)
        //{
        //    getPlayer().TaskAddExp(missionid, addexp);
        //}

        ////高级任务系统 - 检查基础条件是否符合所有任务前置条件
        //public boolean MissionCanMake(int missionid)
        //{
        //    return getPlayer().MissionCanMake(missionid);
        //}

        ////高级任务系统 - 检查基础条件是否符合指定任务前置条件
        //public boolean MissionCanMake(int missionid, int checktype)
        //{
        //    return getPlayer().MissionCanMake(missionid, checktype);
        //}

        ////高级任务函数 - 得到任务的等级数据
        //public int MissionGetIntData(int missionid, int checktype)
        //{
        //    return getPlayer().MissionGetIntData(missionid, checktype);
        //}

        ////高级任务函数 - 得到任务的的字符串型数据
        //public String MissionGetStrData(int missionid, int checktype)
        //{
        //    return getPlayer().MissionGetStrData(missionid, checktype);
        //}

        ////高级任务函数 - 直接输出需要的职业列表串
        //public String MissionGetJoblist(String joblist)
        //{
        //    return getPlayer().MissionGetJoblist(joblist);
        //}

        ////高级任务系统 - 任务创建
        //public void MissionMake(int charid, int missionid, int repeat, int repeattime, int lockmap, int mobid)
        //{
        //    getPlayer().MissionMake(charid, missionid, repeat, repeattime, lockmap, mobid);
        //}

        ////高级任务系统 - 重新做同一个任务
        //public void MissionReMake(int charid, int missionid, int repeat, int repeattime, int lockmap)
        //{
        //    getPlayer().MissionReMake(charid, missionid, repeat, repeattime, lockmap);
        //}

        ////高级任务系统 - 任务完成
        //public void MissionFinish(int charid, int missionid)
        //{
        //    getPlayer().MissionFinish(charid, missionid);
        //}

        ////高级任务系统 - 放弃任务
        //public void MissionDelete(int charid, int missionid)
        //{
        //    getPlayer().MissionDelete(charid, missionid);
        //}

        ////得到指定地图的角色总数
        //public int countRemoteMapPlayers(int mapid)
        //{
        //    int PlayerNumber = 0;
        //    MapleMap map = c.getChannelServer().getMapFactory().getMap(mapid);
        //    double range = Double.POSITIVE_INFINITY;
        //    List<MapleMapObject> players = map.getMapObjectsInRange(c.getPlayer().getPosition(), range, Arrays.asList(MapleMapObjectType.PLAYER));
        //    for (MapleMapObject playermo : players)
        //    {
        //        PlayerNumber = PlayerNumber + 1;
        //    }
        //    return PlayerNumber;
        //}

        ////高级任务系统 - 指定任务的需要最大打怪数量
        //public void MissionMaxNum(int missionid, int maxnum)
        //{
        //    getPlayer().MissionMaxNum(missionid, maxnum);
        //}
        ////高级任务系统 - 放弃所有未完成任务

        //public void MissionDeleteNotFinish(int charid)
        //{
        //    getPlayer().MissionDeleteNotFinish(charid);
        //}

        ////高级任务系统 - 获得任务是否可以做
        //public boolean MissionStatus(int charid, int missionid, int maxtimes, int checktype)
        //{
        //    return getPlayer().MissionStatus(charid, missionid, maxtimes, checktype);
        //}

        ////弹出单人消息框
        //public void startPopMessage(String msg)
        //{
        //    new ServernoticeMapleClientMessageCallback(1, c).dropMessage(msg);
        //}

        ////远程弹出单人消息框
        //public void startPopMessage(int charid, String msg)
        //{
        //    for (ChannelServer cs : ChannelServer.getAllInstances())
        //    {
        //        for (MapleCharacter chr : cs.getPlayerStorage().getAllCharacters())
        //        {
        //            if (chr.getId() == charid)
        //            {
        //                new ServernoticeMapleClientMessageCallback(1, chr.getClient()).dropMessage(msg);
        //            }
        //        }
        //    }
        //}

        //public void callGM(String Text)
        //{
        //    for (ChannelServer cservs : ChannelServer.getAllInstances())
        //    {
        //        for (MapleCharacter players : cservs.getPlayerStorage().getAllCharacters())
        //        {
        //            if (players.isGM())
        //            {
        //                players.getClient().getSession().write(MaplePacketCreator.serverNotice(6, c.getPlayer().getName() + " 给你发送了一封邮件: " + Text));
        //            }
        //        }
        //    }
        //}
    }
}