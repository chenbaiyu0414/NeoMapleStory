using System.Collections.Generic;
using NeoMapleStory.Core.TimeManager;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Quest;
using NeoMapleStory.Game.World;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Script.NPC
{

    public class AbstractPlayerInteraction
    {
        public MapleClient Client { get; private set; }
        public MapleCharacter Player { get; private set; }

        public AbstractPlayerInteraction(MapleClient c)
        {
            Client = c;
            Player = c.Character;
        }

        public string ServerName
        {
            get { return Settings.ServerSettings.ServerName; }
        }

        public string AdvertisementText
        {
            get { return Settings.ServerSettings.ServerAdvertisement; }
        }

        public void ClearAranPolearm() => Player.Inventorys[MapleInventoryType.Equipped.Value].RemoveItem(11);

        public void WarpMap(int map) => Player.changeMap(GetWarpMap(map), GetWarpMap(map).getPortal(0));

        public void WarpMap(int map, int portal) => Player.changeMap(GetWarpMap(map), GetWarpMap(map).getPortal(portal));

        public void WarpMap(int map, string portal) => Player.changeMap(GetWarpMap(map), GetWarpMap(map).getPortal(portal));

        protected MapleMap GetWarpMap(int map)
            =>
                Player.EventInstanceManager == null
                    ? Client.ChannelServer.MapFactory.GetMap(map)
                    : Player.EventInstanceManager.getMapInstance(map);

        public MapleMap GetMap(int map) => GetWarpMap(map);

        public bool ContainItem(int itemid, int quantity = 1, bool checkEquipped = false, bool exact = false)
            => Player.haveItem(itemid, quantity, checkEquipped, exact);

        public bool CanHold(int itemid)
            =>
                Player.Inventorys[MapleItemInformationProvider.Instance.GetInventoryType(itemid).Value].GetNextFreeSlot() >
                128;

        public MapleQuestStatusType GetQuestStatus(int id) => Player.GetQuest(MapleQuest.GetInstance(id)).Status;

        public bool IsQuestActive(int id)
            => Player.GetQuest(MapleQuest.GetInstance(id)).Status == MapleQuestStatusType.Started;

        public bool IsQuestFinished(int id)
            => Player.GetQuest(MapleQuest.GetInstance(id)).Status == MapleQuestStatusType.Completed;

        public void SetTimeOut(int time, int mapId) => TimerManager.Instance.ScheduleJob(() =>
        {
            MapleMap map = Player.Map;
            MapleMap outMap = Client.ChannelServer.MapFactory.GetMap(mapId);
            foreach (var player in map.Characters)
            {
                player.changeMap(outMap, outMap.getPortal(0));
            }

        }, time);

        /**
         * Gives item with the specified id or takes it if the quantity is negative.
         * Note that this does NOT take items from the equipped inventory.
         * randomStats for generating random stats on the generated equip.
         *
         * @param id
         * @param quantity
         * @param randomStats
         */

        public void GainItem(int id, short quantity, bool randomStats = false)
        {
            if (quantity >= 0)
            {
                var ii = MapleItemInformationProvider.Instance;
                var item = ii.GetEquipById(id);
                MapleInventoryType type = ii.GetInventoryType(id);
                string logInfo =
                    $"{Player.Name} received {quantity} frome a scripted PlayerInteraction ({ToString()})";

                if (!MapleInventoryManipulator.checkSpace(Client, id, quantity, ""))
                {
                    Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Popup, "你的背包已满"));
                    return;
                }
                if (type == MapleInventoryType.Equip && !ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId))
                {
                    if (randomStats)
                    {
                        MapleInventoryManipulator.addFromDrop(Client, ii.RandomizeStats((Equip)item), logInfo, false);
                    }
                    else
                    {
                        MapleInventoryManipulator.addFromDrop(Client, (Equip)item, logInfo, false);
                    }
                }
                else
                {
                    MapleInventoryManipulator.addById(Client, id, quantity, logInfo);
                }
            }
            else
            {
                MapleInventoryManipulator.removeById(Client, MapleItemInformationProvider.Instance.GetInventoryType(id),
                    id, -quantity, true, false);
            }
            Client.Send(PacketCreator.GetShowItemGain(id, quantity, true));
        }

        public bool AddItem(int id, short quantity, bool randomStats)
        {
            if (quantity >= 0)
            {
                var ii = MapleItemInformationProvider.Instance;
                var item = ii.GetEquipById(id);
                MapleInventoryType type = ii.GetInventoryType(id);
                string logInfo =
                    $"{Player.Name} received {quantity} frome a scripted PlayerInteraction ({ToString()})";

                if (!MapleInventoryManipulator.checkSpace(Client, id, quantity, ""))
                {
                    MapleInventoryType invtype = ii.GetInventoryType(id);
                    Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Popup, "你的背包已满"));
                    return false;
                }
                if (type == MapleInventoryType.Equip && !ii.IsThrowingStar(item.ItemId) && !ii.IsBullet(item.ItemId))
                {
                    if (randomStats)
                        MapleInventoryManipulator.addFromDrop(Client, ii.RandomizeStats((Equip)item), logInfo, false);
                    else
                        MapleInventoryManipulator.addFromDrop(Client, (Equip)item, logInfo, false);
                }
                else
                {
                    MapleInventoryManipulator.addById(Client, id, quantity, logInfo);
                }
            }
            else
            {
                MapleInventoryManipulator.removeById(Client, MapleItemInformationProvider.Instance.GetInventoryType(id),
                    id, -quantity, true, false);
            }
            Client.Send(PacketCreator.GetShowItemGain(id, quantity, true));
            return true;
        }

        public void ChangeMusic(string songName) => Player.Map.BroadcastMessage(PacketCreator.musicChange(songName));


        public void PlayerMessage(string message,
            PacketCreator.ServerMessageType type = PacketCreator.ServerMessageType.PinkText)
            => Client.Send(PacketCreator.ServerNotice(type, message));

        public void MapMessage(string message,
            PacketCreator.ServerMessageType type = PacketCreator.ServerMessageType.PinkText)
            => Player.Map.BroadcastMessage(PacketCreator.ServerNotice(type, message));


        //    public void guildMessage(string message)
        //    {
        //        guildMessage(5, message);
        //    }

        //    public void guildMessage(int type, string message)
        //    {
        //        MapleGuild guild = getGuild();
        //        if (guild != null)
        //        {
        //            guild.guildMessage(MaplePacketCreator.serverNotice(type, message));
        //        }
        //    }

        //    public MapleGuild getGuild()
        //    {
        //        try
        //        {
        //            return c.getChannelServer().getWorldInterface().getGuild(getPlayer().getGuildId(), new MapleGuildCharacter(getPlayer()));
        //        }
        //        catch (RemoteException ex)
        //        {
        //            Logger.getLogger(AbstractPlayerInteraction.class.getName()).log(Level.SEVERE, null, ex);
        //}
        //    return null;
        //}

        //public void gainGP(int amount)
        //{
        //    try
        //    {
        //        c.getChannelServer().getWorldInterface().gainGP(getPlayer().getGuildId(), amount);
        //    }
        //    catch (RemoteException e)
        //    {
        //        c.getChannelServer().reconnectWorld();
        //    }
        //}

        public MapleParty GetParty() => Player.Party;

        public bool IsLeader() => GetParty().Leader.Equals(new MaplePartyCharacter(Player));

        /**
         * PQ methods: give items/exp to all party members
         */

        public void GivePartyItems(int id, short quantity, List<MapleCharacter> party)
        {
            foreach (var chr in party)
            {
                MapleClient cl = chr.Client;
                if (quantity >= 0)
                {
                    string logInfo =
                        $"{cl.Character.Name} received {quantity} from event {chr.EventInstanceManager.Name}";

                    MapleInventoryManipulator.addById(cl, id, quantity, logInfo);
                }
                else
                {
                    MapleInventoryManipulator.removeById(cl, MapleItemInformationProvider.Instance.GetInventoryType(id),
                        id, -quantity, true, false);
                }
                cl.Send(PacketCreator.GetShowItemGain(id, quantity, true));
            }
        }

        public void GivePartyExp(int amount, List<MapleCharacter> party) => party.ForEach(chr =>
        {
            chr.gainExp(amount * Client.ChannelServer.ExpRate, true, true);
        });

        /**
         * remove all items of type from party; combination of haveItem and gainItem
         */

        public void RemoveFromParty(int id, List<MapleCharacter> party)
        {
            foreach (MapleCharacter chr in party)
            {
                MapleClient cl = chr.Client;
                MapleInventoryType type = MapleItemInformationProvider.Instance.GetInventoryType(id);
                MapleInventory iv = cl.Character.Inventorys[type.Value];
                int possesed = iv.CountById(id);

                if (possesed > 0)
                {
                    MapleInventoryManipulator.removeById(Client,
                        MapleItemInformationProvider.Instance.GetInventoryType(id), id, possesed, true, false);
                    cl.Send(PacketCreator.GetShowItemGain(id, (short)-possesed, true));
                }
            }
        }

        public void removeAll(int id) => removeAll(id, Client);

        public void removeAll(int id, MapleClient cl)
        {
            int possessed =
                cl.Character.Inventorys[MapleItemInformationProvider.Instance.GetInventoryType(id).Value].CountById(id);
            if (possessed > 0)
            {
                MapleInventoryManipulator.removeById(cl, MapleItemInformationProvider.Instance.GetInventoryType(id), id,
                    possessed, true, false);
                cl.Send(PacketCreator.GetShowItemGain(id, (short)-possessed, true));
            }
        }

        //public void gainCloseness(int closeness, int index)
        //{
        //    MaplePet pet = getPlayer().getPet(index);
        //    if (pet != null)
        //    {
        //        pet.setCloseness(pet.getCloseness() + closeness);
        //        getClient().getSession().write(MaplePacketCreator.updatePet(pet, true));
        //    }
        //}

        //public void gainClosenessAll(int closeness)
        //{
        //    for (MaplePet pet : getPlayer().getPets())
        //    {
        //        if (pet != null)
        //        {
        //            pet.setCloseness(pet.getCloseness() + closeness);
        //            getClient().getSession().write(MaplePacketCreator.updatePet(pet, true));
        //        }
        //    }
        //}

        //public int getMapId()
        //{
        //    return c.getPlayer().getMap().getId();
        //}

        //public int getPlayerCount(int mapid)
        //{
        //    return c.getChannelServer().getMapFactory().getMap(mapid).getCharacters().size();
        //}

        //public int getCurrentPartyId(int mapid)
        //{
        //    return getMap(mapid).getCurrentPartyId();
        //}

        //    public void showInstruction(string msg, int width, int height)
        //    {
        //        c.getSession().write(MaplePacketCreator.sendHint(msg, width, height));
        //        c.getSession().write(MaplePacketCreator.enableActions());
        //    }

        //    public void worldMessage(int type, string message)
        //    {
        //        net.sf.odinms.net.MaplePacket packet = MaplePacketCreator.serverNotice(type, message);
        //        MapleCharacter chr = c.getPlayer();
        //        try
        //        {
        //            ChannelServer.getInstance(chr.getClient().getChannel()).getWorldInterface().broadcastMessage(chr.getName(), packet.getBytes());
        //        }
        //        catch (RemoteException e)
        //        {
        //            chr.getClient().getChannelServer().reconnectWorld();
        //        }
        //    }

        //    public int getBossLog(string bossid)
        //    {
        //        return getPlayer().getBossLog(bossid);
        //    }

        //    public void setBossLog(string bossid)
        //    {
        //        getPlayer().setBossLog(bossid);
        //    }

        //    public void sendMessage(string message)
        //    {
        //        new ServernoticeMapleClientMessageCallback(0, c).dropMessage(message);
        //    }

        //    public void resetMap(int mapid)
        //    {
        //        getMap(mapid).resetReactors();
        //        getMap(mapid).killAllMonsters();
        //        for (MapleMapObject i : getMap(mapid).getMapObjectsInRange(c.getPlayer().getPosition(), double.POSITIVE_INFINITY, Arrays.asList(MapleMapObjectType.ITEM)))
        //        {
        //            getMap(mapid).removeMapObject(i);
        //            getMap(mapid).broadcastMessage(MaplePacketCreator.removeItemFromMap(i.getObjectId(), 0, c.getPlayer().getId()));
        //        }
        //    }

        //    public void sendClock(MapleClient d, int time)
        //    {
        //        d.getSession().write(MaplePacketCreator.getClock((int)(time - System.currentTimeMillis()) / 1000));
        //    }

        //    public void useItem(int id)
        //    {
        //        MapleItemInformationProvider.getInstance().getItemEffect(id).applyTo(c.getPlayer());
        //        c.getSession().write(MaplePacketCreator.getStatusMsg(id));
        //    }

        //    public void aranTemporarySkills()
        //    {
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000017), 0, -1);
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000018), 0, -1);
        //        c.getPlayer().setRemainingSp(0);
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000017), 1, 0);
        //        c.getPlayer().setRemainingSp(0);
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000018), 1, 0);
        //        c.getPlayer().setRemainingSp(0);
        //    }

        //    public void aranTemporarySkills2()
        //    {
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000014), 0, -1);
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000015), 0, -1);
        //        c.getPlayer().setRemainingSp(0);
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000014), 1, 0);
        //        c.getPlayer().setRemainingSp(0);
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000015), 1, 0);
        //    }

        //    public void aranTemporarySkills3()
        //    {
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000016), 0, -1);
        //        c.getPlayer().setRemainingSp(0);
        //        c.getPlayer().changeSkillLevel(SkillFactory.getSkill(20000016), 1, 0);
        //    }

        //    public void showWZEffect(string path, int info)
        //    {
        //        c.getSession().write(MaplePacketCreator.showWZEffect(path, info));
        //    }

        //    public void updateAranIntroState(string mode)
        //    {
        //        c.getPlayer().addAreaData(21002, mode);
        //        c.getSession().write(MaplePacketCreator.updateIntroState(mode, 21002));
        //    }

        //    public void updateAranIntroState2(string mode)
        //    {
        //        c.getPlayer().addAreaData(21019, mode);
        //        c.getSession().write(MaplePacketCreator.updateIntroState(mode, 21019));
        //    }

        //    public bool getAranIntroState(string mode)
        //    {
        //        if (c.getPlayer().ares_data.contains(mode))
        //        {
        //            return true;
        //        }
        //        return false;
        //    }

        //    public void updateCygnusIntroState(string mode)
        //    {
        //        c.getPlayer().addAreaData(20021, mode);
        //        c.getSession().write(MaplePacketCreator.updateIntroState(mode, 20021));
        //    }

        //    public bool getCygnusIntroState(string mode)
        //    {
        //        if (c.getPlayer().ares_data.contains(mode))
        //        {
        //            return true;
        //        }
        //        return false;
        //    }

        //    public void playWZSound(string path)
        //    {
        //        c.getSession().write(MaplePacketCreator.playWZSound(path));
        //    }

        //    public void updateQuest(int questid, string status)
        //    {
        //        c.getSession().write(MaplePacketCreator.updateQuest(questid, status));
        //    }

        //    public void displayGuide(int guide)
        //    {
        //        c.getSession().write(MaplePacketCreator.displayGuide(guide));
        //    }

        //    public void removeTutorialSummon()
        //    {
        //        c.getSession().write(MaplePacketCreator.spawnTutorialSummon(0));
        //    }

        //    public void spawnTutorialSummon()
        //    {
        //        c.getSession().write(MaplePacketCreator.spawnTutorialSummon(1));
        //    }

        //    public void tutorialSpeechBubble(string message)
        //    {
        //        c.getSession().write(MaplePacketCreator.tutorialSpeechBubble(message));
        //    }

        //    public void showInfo(string message)
        //    {
        //        c.getSession().write(MaplePacketCreator.showInfo(message));
        //    }

        //    public void showMapEffect(string path)
        //    {
        //        c.getSession().write(MaplePacketCreator.showMapEffect(path));
        //    }

        //public void lockUI()
        //    {
        //        c.getPlayer().tutorial = true;
        //        c.getSession().write(MaplePacketCreator.lockUI(true));
        //        c.getSession().write(MaplePacketCreator.disableUI(true));
        //    }

        //public void unlockUI()
        //    {
        //        c.getPlayer().tutorial = false;
        //        c.getSession().write(MaplePacketCreator.lockUI(false));
        //        c.getSession().write(MaplePacketCreator.disableUI(false));
        //    }

        //public bool inIntro()
        //    {
        //        return c.getPlayer().tutorial;
        //    }
    }
}
