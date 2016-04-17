using NeoMapleStory.Core;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Server;
using NeoMapleStory.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.Inventory
{
    public class MapleItemInformationProvider
    {
        public static MapleItemInformationProvider Instance { get; } = new MapleItemInformationProvider();
        protected IMapleDataProvider ItemData;
        protected IMapleDataProvider EquipData;
        protected IMapleDataProvider StringData;
        protected IMapleData CashStringData;
        protected IMapleData ConsumeStringData;
        protected IMapleData EqpStringData;
        protected IMapleData EtcStringData;
        protected IMapleData InsStringData;
        protected IMapleData PetStringData;
        protected Dictionary<int, MapleInventoryType> InventoryTypeCache = new Dictionary<int, MapleInventoryType>();
        protected Dictionary<int, short> SlotMaxCache = new Dictionary<int, short>();
        protected Dictionary<int, MapleStatEffect> ItemEffects = new Dictionary<int, MapleStatEffect>();
        protected Dictionary<int, Dictionary<string, int>> EquipStatsCache = new Dictionary<int, Dictionary<string, int>>();
        protected Dictionary<int, Equip> EquipCache = new Dictionary<int, Equip>();
        protected Dictionary<int, double> PriceCache = new Dictionary<int, double>();
        protected Dictionary<int, int> WholePriceCache = new Dictionary<int, int>();
        protected Dictionary<int, int> ProjectileWatkCache = new Dictionary<int, int>();
        protected Dictionary<int, string> NameCache = new Dictionary<int, string>();
        protected Dictionary<int, string> DescCache = new Dictionary<int, string>();
        protected Dictionary<int, string> MsgCache = new Dictionary<int, string>();
        protected Dictionary<int, bool> DropRestrictionCache = new Dictionary<int, bool>();
        protected Dictionary<int, bool> PickupRestrictionCache = new Dictionary<int, bool>();
        protected Dictionary<int, bool> IsQuestItemCache = new Dictionary<int, bool>();
        protected Dictionary<int, List<SummonEntry>> SummonEntryCache = new Dictionary<int, List<SummonEntry>>();
        protected List<Tuple<int, string>> ItemNameCache = new List<Tuple<int, string>>();
        protected Dictionary<int, int> GetMesoCache = new Dictionary<int, int>();
        protected Dictionary<int, int> getExpCache = new Dictionary<int, int>();
        protected Dictionary<int, string> ItemTypeCache = new Dictionary<int, string>();
        protected Dictionary<int, Dictionary<string, string>> GetExpCardTimes = new Dictionary<int, Dictionary<string, string>>();
        protected Dictionary<int, int> ScriptedItemCache = new Dictionary<int, int>();
        protected Dictionary<int, int> MonsterBookId = new Dictionary<int, int>();
        protected Dictionary<int, bool> ConsumeOnPickupCache = new Dictionary<int, bool>();
        protected Dictionary<int, List<int>> ScrollRestrictionCache = new Dictionary<int, List<int>>();
        protected Dictionary<int, bool> KarmaCache = new Dictionary<int, bool>();
        //protected Dictionary<int, List<MapleFish>> fishingCache = new Dictionary<int, List<MapleFish>>();

        protected MapleItemInformationProvider()
        {
            //loadCardIdData();
            ItemData = MapleDataProviderFactory.GetDataProvider("Item.wz");
            EquipData = MapleDataProviderFactory.GetDataProvider("Character.wz");
            StringData = MapleDataProviderFactory.GetDataProvider("String.wz");
            CashStringData = MapleDataProviderFactory.GetDataProvider("String.wz").GetData("Cash.img");
            ConsumeStringData = MapleDataProviderFactory.GetDataProvider("String.wz").GetData("Consume.img");
            EqpStringData = MapleDataProviderFactory.GetDataProvider("String.wz").GetData("Eqp.img");
            EtcStringData = MapleDataProviderFactory.GetDataProvider("String.wz").GetData("Etc.img");
            InsStringData = MapleDataProviderFactory.GetDataProvider("String.wz").GetData("Ins.img");
            PetStringData = MapleDataProviderFactory.GetDataProvider("String.wz").GetData("Pet.img");
        }

        public MapleInventoryType GetInventoryType(int itemId)
        {
            if (InventoryTypeCache.ContainsKey(itemId))
            {
                return InventoryTypeCache[itemId];
            }
            MapleInventoryType ret;
            string idStr = "0" + itemId.ToString();
            IMapleDataDirectoryEntry root = ItemData.GetRoot();
            foreach (IMapleDataDirectoryEntry topDir in root.GetSubDirectories())
            {
                foreach (IMapleDataFileEntry iFile in topDir.GetFiles())
                {
                    if (iFile.Name == idStr.Substring(0, 4) + ".img")
                    {
                        ret = MapleInventoryType.GetByWzName(topDir.Name);
                        InventoryTypeCache.Add(itemId, ret);
                        return ret;
                    }
                    else if (iFile.Name.Equals(idStr.Substring(1) + ".img"))
                    {
                        ret = MapleInventoryType.GetByWzName(topDir.Name);
                        InventoryTypeCache.Add(itemId, ret);
                        return ret;
                    }
                }
            }
            root = EquipData.GetRoot();
            foreach (IMapleDataDirectoryEntry topDir in root.GetSubDirectories())
            {
                foreach (IMapleDataFileEntry iFile in topDir.GetFiles())
                {
                    if (iFile.Name.Equals(idStr + ".img"))
                    {
                        ret = MapleInventoryType.Equip;
                        InventoryTypeCache.Add(itemId, ret);
                        return ret;
                    }
                }
            }
            ret = MapleInventoryType.Undefined;
            InventoryTypeCache.Add(itemId, ret);
            return ret;
        }

        public int GetNextUniqueId() => DatabaseHelper.GetMaxIndex("InventoryItem", "UniqueId") + 1;


        public List<Tuple<int, string>> GetAllItems()
        {
            if (ItemNameCache.Any())
            {
                return ItemNameCache;
            }
            List<Tuple<int, string>> itemPairs = new List<Tuple<int, string>>();
            IMapleData itemsData;

            itemsData = StringData.GetData("Cash.img");
            foreach (IMapleData itemFolder in itemsData.Children)
            {
                int itemId = int.Parse(itemFolder.Name);
                string itemName = MapleDataTool.GetString("name", itemFolder, "NO-NAME");
                itemPairs.Add(Tuple.Create(itemId, itemName));
            }

            itemsData = StringData.GetData("Consume.img");
            foreach (IMapleData itemFolder in itemsData.Children)
            {
                int itemId = int.Parse(itemFolder.Name);
                string itemName = MapleDataTool.GetString("name", itemFolder, "NO-NAME");
                itemPairs.Add(Tuple.Create(itemId, itemName));
            }

            itemsData = StringData.GetData("Eqp.img").GetChildByPath("Eqp");
            foreach (IMapleData eqpType in itemsData.Children)
            {
                foreach (IMapleData itemFolder in eqpType.Children)
                {
                    int itemId = int.Parse(itemFolder.Name);
                    string itemName = MapleDataTool.GetString("name", itemFolder, "NO-NAME");
                    itemPairs.Add(Tuple.Create(itemId, itemName));
                }
            }

            itemsData = StringData.GetData("Etc.img").GetChildByPath("Etc");
            foreach (IMapleData itemFolder in itemsData.Children)
            {
                int itemId = int.Parse(itemFolder.Name);
                string itemName = MapleDataTool.GetString("name", itemFolder, "NO-NAME");
                itemPairs.Add(Tuple.Create(itemId, itemName));
            }

            itemsData = StringData.GetData("Ins.img");
            foreach (IMapleData itemFolder in itemsData.Children)
            {
                int itemId = int.Parse(itemFolder.Name);
                string itemName = MapleDataTool.GetString("name", itemFolder, "NO-NAME");
                itemPairs.Add(Tuple.Create(itemId, itemName));
            }

            itemsData = StringData.GetData("Pet.img");
            foreach (IMapleData itemFolder in itemsData.Children)
            {
                int itemId = int.Parse(itemFolder.Name);
                string itemName = MapleDataTool.GetString("name", itemFolder, "NO-NAME");
                itemPairs.Add(Tuple.Create(itemId, itemName));
            }
            ItemNameCache.AddRange(itemPairs);
            return itemPairs;
        }

        public int GetScriptedItemNpc(int itemId)
        {
            if (ScriptedItemCache.ContainsKey(itemId))
            {
                return ScriptedItemCache[itemId];
            }
            IMapleData data = GetItemData(itemId);
            int npcId = MapleDataTool.GetInt("spec/npc", data, 0);
            ScriptedItemCache.Add(itemId, npcId);

            return ScriptedItemCache[itemId];
        }

        public bool IsExpOrDropCardTime(int itemId)
        {
            string day = MapleDayInt.GetDayInt(DateTime.Now.DayOfWeek);

            Dictionary<string, string> times;
            if (GetExpCardTimes.ContainsKey(itemId))
            {
                times = GetExpCardTimes[itemId];
            }
            else {
                List<IMapleData> data = GetItemData(itemId).GetChildByPath("info").GetChildByPath("time").Children;
                Dictionary<string, string> hours = new Dictionary<string, string>();
                data.ForEach(childdata =>
                {
                    string[] time = MapleDataTool.GetString(childdata).Split(':');
                    //MON:03-07
                    hours.Add(time[0], time[1]);
                });
                times = hours;
                GetExpCardTimes.Add(itemId, hours);
            }
            if (times.ContainsKey(day))
            {
                string[] hourspan = times[day].Split('-');
                int starthour = int.Parse(hourspan[0]);
                int endhour = int.Parse(hourspan[1]);
                if (DateTime.Now.Hour >= starthour && DateTime.Now.Hour <= endhour)
                {
                    return true;
                }
            }
            return false;
        }

        public static class MapleDayInt
        {

            public static string GetDayInt(DayOfWeek day)
            {
                string[] weekday = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
                return weekday[(int)day];
            }
        }

        protected IMapleData GetStringData(int itemId)
        {
            string cat = "null";
            IMapleData theData;
            if (itemId >= 5010000)
            {
                theData = CashStringData;
            }
            else if (itemId >= 2000000 && itemId < 3000000)
            {
                theData = ConsumeStringData;
            }
            else if (itemId >= 1010000 && itemId < 1040000 || itemId > 1122000 && itemId < 1143000 && itemId != 1122007)
            {
                theData = EqpStringData;
                cat = "Accessory";
            }
            else if (itemId >= 1000000 && itemId < 1010000)
            {
                theData = EqpStringData;
                cat = "Cap";
            }
            else if (itemId >= 1102000 && itemId < 1103000)
            {
                theData = EqpStringData;
                cat = "Cape";
            }
            else if (itemId >= 1040000 && itemId < 1050000)
            {
                theData = EqpStringData;
                cat = "Coat";
            }
            else if (itemId >= 20000 && itemId < 22000)
            {
                theData = EqpStringData;
                cat = "Face";
            }
            else if (itemId >= 1080000 && itemId < 1090000)
            {
                theData = EqpStringData;
                cat = "Glove";
            }
            else if (itemId >= 30000 && itemId < 32000)
            {
                theData = EqpStringData;
                cat = "Hair";
            }
            else if (itemId >= 1050000 && itemId < 1060000)
            {
                theData = EqpStringData;
                cat = "Longcoat";
            }
            else if (itemId >= 1060000 && itemId < 1070000)
            {
                theData = EqpStringData;
                cat = "Pants";
            }
            else if (itemId >= 1802000 && itemId < 1803000 || itemId >= 1812000 && itemId < 1813000 || itemId == 1822000 || itemId == 1832000)
            {
                theData = EqpStringData;
                cat = "PetEquip";
            }
            else if (itemId >= 1112000 && itemId < 1120000)
            {
                theData = EqpStringData;
                cat = "Ring";
            }
            else if (itemId >= 1092000 && itemId < 1100000)
            {
                theData = EqpStringData;
                cat = "Shield";
            }
            else if (itemId >= 1070000 && itemId < 1080000)
            {
                theData = EqpStringData;
                cat = "Shoes";
            }
            else if (itemId >= 1900000 && itemId < 2000000)
            {
                theData = EqpStringData;
                cat = "Taming";
            }
            else if (itemId >= 1300000 && itemId < 1800000)
            {
                theData = EqpStringData;
                cat = "Weapon";
            }
            else if (itemId >= 4000000 && itemId < 5000000)
            {
                theData = EtcStringData;
            }
            else if (itemId >= 3000000 && itemId < 4000000)
            {
                theData = InsStringData;
            }
            else if (itemId >= 5000000 && itemId < 5010000)
            {
                theData = PetStringData;
            }
            else {
                return null;
            }
            if (cat == "null")
            {
                if (theData != EtcStringData || itemId == 4280000 || itemId == 4280001)
                {
                    return theData.GetChildByPath(itemId.ToString());
                }
                else {
                    return theData.GetChildByPath("Etc/" + itemId.ToString());
                }
            }
            else
            {
                if (theData == EqpStringData)
                {
                    return theData.GetChildByPath("Eqp/" + cat + "/" + itemId);
                }
                else {
                    return theData.GetChildByPath(cat + "/" + itemId);
                }
            }
        }

        protected IMapleData GetItemData(int itemId)
        {
            IMapleData ret = null;
            string idStr = "0" + itemId.ToString();
            IMapleDataDirectoryEntry root = ItemData.GetRoot();
            foreach (IMapleDataDirectoryEntry topDir in root.GetSubDirectories())
            {
                // we should have .img files here beginning with the first 4 IID
                foreach (IMapleDataFileEntry iFile in topDir.GetFiles())
                {
                    if (iFile.Name.Equals(idStr.Substring(0, 4) + ".img"))
                    {
                        ret = ItemData.GetData(topDir.Name + "/" + iFile.Name);
                        if (ret == null)
                        {
                            return null;
                        }
                        ret = ret.GetChildByPath(idStr);
                        return ret;
                    }
                    else if (iFile.Name.Equals(idStr.Substring(1) + ".img"))
                    {
                        return ItemData.GetData(topDir.Name + "/" + iFile.Name);
                    }
                }
            }
            root = EquipData.GetRoot();
            foreach (IMapleDataDirectoryEntry topDir in root.GetSubDirectories())
            {
                foreach (IMapleDataFileEntry iFile in topDir.GetFiles())
                {
                    if (iFile.Name.Equals(idStr + ".img"))
                    {
                        return EquipData.GetData(topDir.Name + "/" + iFile.Name);
                    }
                }
            }
            return ret;
        }

        public short GetSlotMax(MapleClient c, int itemId)
        {
            if (SlotMaxCache.ContainsKey(itemId))
            {
                return SlotMaxCache[itemId];
            }
            short ret = 0;
            IMapleData item = GetItemData(itemId);
            if (item != null)
            {
                IMapleData smEntry = item.GetChildByPath("info/slotMax");
                if (smEntry == null)
                {
                    if (GetInventoryType(itemId) == MapleInventoryType.Equip)
                    {
                        ret = 1;
                    }
                    else {
                        ret = 100;
                    }
                }
                else {
                    if (IsThrowingStar(itemId) || IsBullet(itemId) || (MapleDataTool.GetInt(smEntry) == 0))
                    {
                        ret = 1;
                    }
                    ret = (short)MapleDataTool.GetInt(smEntry);
                    if (IsThrowingStar(itemId))
                    {
                        //ret += c.GetPlayer().GetSkillLevel(SkillFactory.GetSkill(4100000)) * 10;
                    }
                    else {
                        // ret += c.GetPlayer().GetSkillLevel(SkillFactory.GetSkill(5200000)) * 10;
                    }

                }
            }
            if (!IsThrowingStar(itemId) && !IsBullet(itemId))
            {
                SlotMaxCache.Add(itemId, ret);
            }

            return ret;
        }

        public bool IsThrowingStar(int itemId)
        {
            return itemId >= 2070000 && itemId < 2080000;
        }

        public int GetMeso(int itemId)
        {
            if (GetMesoCache.ContainsKey(itemId))
            {
                return GetMesoCache[itemId];
            }
            IMapleData item = GetItemData(itemId);
            if (item == null)
            {
                return -1;
            }
            int pEntry = 0;
            IMapleData pData = item.GetChildByPath("info/meso");
            if (pData == null)
            {
                return -1;
            }
            pEntry = MapleDataTool.GetInt(pData);

            GetMesoCache.Add(itemId, pEntry);
            return pEntry;
        }

        public int GetWholePrice(int itemId)
        {
            if (WholePriceCache.ContainsKey(itemId))
            {
                return WholePriceCache[itemId];
            }
            IMapleData item = GetItemData(itemId);
            if (item == null)
            {
                return -1;
            }
            int pEntry = 0;
            IMapleData pData = item.GetChildByPath("info/price");
            if (pData == null)
            {
                return -1;
            }
            pEntry = MapleDataTool.GetInt(pData);

            WholePriceCache.Add(itemId, pEntry);
            return pEntry;
        }

        public string GetType(int itemId)
        {
            if (ItemTypeCache.ContainsKey(itemId))
            {
                return ItemTypeCache[itemId];
            }
            IMapleData item = GetItemData(itemId);
            if (item == null)
            {
                return "";
            }
            string pEntry;
            IMapleData pData = item.GetChildByPath("info/islot");
            if (pData == null)
            {
                return "";
            }
            pEntry = MapleDataTool.GetString(pData);

            ItemTypeCache.Add(itemId, pEntry);
            return pEntry;
        }

        public double GetPrice(int itemId)
        {
            if (PriceCache.ContainsKey(itemId))
            {
                return PriceCache[itemId];
            }
            IMapleData item = GetItemData(itemId);
            if (item == null)
            {
                return -1;
            }
            double pEntry = 0.0;
            IMapleData pData = item.GetChildByPath("info/unitPrice");
            if (pData != null)
            {
                try
                {
                    pEntry = MapleDataTool.GetDouble(pData);
                }
                catch
                {
                    pEntry = MapleDataTool.GetInt(pData);
                }
            }
            else {
                pData = item.GetChildByPath("info/price");
                if (pData == null)
                {
                    return -1;
                }
                pEntry = MapleDataTool.GetInt(pData);
            }

            PriceCache.Add(itemId, pEntry);
            return pEntry;
        }

        protected Dictionary<string, int> GetEquipStats(int itemId)
        {
            if (EquipStatsCache.ContainsKey(itemId))
            {
                return EquipStatsCache[itemId];
            }
            Dictionary<string, int> ret = new Dictionary<string, int>();
            IMapleData item = GetItemData(itemId);
            if (item == null)
            {
                return null;
            }
            IMapleData info = item.GetChildByPath("info");
            if (info == null)
            {
                return null;
            }
            foreach (IMapleData data in info.Children)
            {
                if (data.Name.StartsWith("inc"))
                {
                    ret.Add(data.Name.Substring(3), MapleDataTool.ConvertToInt(data));
                }
            }
            ret.Add("tuc", MapleDataTool.GetInt("tuc", info, 0));
            ret.Add("reqLevel", MapleDataTool.GetInt("reqLevel", info, 0));
            ret.Add("reqJob", MapleDataTool.GetInt("reqJob", info, 0));
            ret.Add("reqSTR", MapleDataTool.GetInt("reqSTR", info, 0));
            ret.Add("reqDEX", MapleDataTool.GetInt("reqDEX", info, 0));
            ret.Add("reqINT", MapleDataTool.GetInt("reqINT", info, 0));
            ret.Add("reqLUK", MapleDataTool.GetInt("reqLUK", info, 0));
            ret.Add("cash", MapleDataTool.GetInt("cash", info, 0));
            ret.Add("cursed", MapleDataTool.GetInt("cursed", info, 0));
            ret.Add("success", MapleDataTool.GetInt("success", info, 0));
            EquipStatsCache.Add(itemId, ret);
            return ret;
        }

        public int GetReqLevel(int itemId)
        {
            int req;
            return !GetEquipStats(itemId).TryGetValue("reqLevel", out req) ? 0 : req;
        }

        public int GetReqJob(int itemId)
        {
            int req;
            return !GetEquipStats(itemId).TryGetValue("reqJob", out req) ? 0 : req;
        }

        public int GetReqStr(int itemId)
        {
            int req;
            return !GetEquipStats(itemId).TryGetValue("reqSTR", out req) ? 0 : req;
        }

        public int GetReqDex(int itemId)
        {
            int req;
            return !GetEquipStats(itemId).TryGetValue("reqDEX", out req) ? 0 : req;
        }

        public int GetReqInt(int itemId)
        {
            int req;
            return !GetEquipStats(itemId).TryGetValue("reqINT", out req) ? 0 : req;
        }

        public int GetReqLuk(int itemId)
        {
            int req;
            return !GetEquipStats(itemId).TryGetValue("reqLUK", out req) ? 0 : req;
        }

        public bool IsCash(int itemId)
        {
            int req;

            if (GetEquipStats(itemId) == null || !GetEquipStats(itemId).TryGetValue("cash", out req) || req == 0)
            {
                return false;
            }
            return true;
        }

        public List<int> GetScrollReqs(int itemId)
        {
            if (ScrollRestrictionCache.ContainsKey(itemId))
            {
                return ScrollRestrictionCache[itemId];
            }
            List<int> ret = new List<int>();
            IMapleData data = GetItemData(itemId);
            data = data.GetChildByPath("req");
            if (data == null)
            {
                return ret;
            }
            foreach (IMapleData req in data.Children)
            {
                ret.Add(MapleDataTool.GetInt(req));
            }
            return ret;
        }

        public List<SummonEntry> GetSummonMobs(int itemId)
        {
            if (SummonEntryCache.ContainsKey(itemId))
            {
                return SummonEntryCache[itemId];
            }
            IMapleData data = GetItemData(itemId);
            int mobSize = data.GetChildByPath("mob").Children.Count;
            List<SummonEntry> ret = new List<SummonEntry>();
            for (int x = 0; x < mobSize; x++)
            {
                ret.Add(new SummonEntry(MapleDataTool.ConvertToInt("mob/" + x + "/id", data), MapleDataTool.ConvertToInt("mob/" + x + "/prob", data)));
            }
            if (!ret.Any())
            {
                Console.WriteLine($"Empty summon bag, itemID: {itemId}");
            }
            SummonEntryCache.Add(itemId, ret);
            return ret;
        }

        public bool IsWeapon(int itemId)
        {
            return itemId >= 1302000 && itemId < 1492024;
        }

        public MapleWeaponType GetWeaponType(int itemId)
        {
            int cat = itemId / 10000;
            cat = cat % 100;
            switch (cat)
            {
                case 30:
                    return MapleWeaponType.Sword1H;
                case 31:
                    return MapleWeaponType.Axe1H;
                case 32:
                    return MapleWeaponType.Blunt1H;
                case 33:
                    return MapleWeaponType.Dagger;
                case 37:
                    return MapleWeaponType.Wand;
                case 38:
                    return MapleWeaponType.Staff;
                case 40:
                    return MapleWeaponType.Sword2H;
                case 41:
                    return MapleWeaponType.Axe2H;
                case 42:
                    return MapleWeaponType.Blunt2H;
                case 43:
                    return MapleWeaponType.Spear;
                case 44:
                    return MapleWeaponType.PoleArm;
                case 45:
                    return MapleWeaponType.Bow;
                case 46:
                    return MapleWeaponType.Crossbow;
                case 47:
                    return MapleWeaponType.Claw;
                case 39: // Barefists
                case 48:
                    return MapleWeaponType.Knuckle;
                case 49:
                    return MapleWeaponType.Gun;

            }
            return MapleWeaponType.NotAWeapon;
        }

        public bool IsShield(int itemId)
        {
            int cat = itemId / 10000;
            cat = cat % 100;
            return cat == 9;
        }

        public bool IsEquip(int itemId)
        {
            return itemId / 1000000 == 1;
        }

        public bool IsCleanSlate(int scrollId)
        {
            switch (scrollId)
            {
                case 2049000:
                case 2049001:
                case 2049002:
                case 2049003:
                    return true;
            }
            return false;

        }

        public IMapleItem ScrollEquipWithId(IMapleItem equip, int scrollId, bool usingWhiteScroll, bool checkIfGm)
        {
            if (equip is Equip)
            {
                Equip nEquip = (Equip)equip;
                Dictionary<string, int> stats = GetEquipStats(scrollId);
                Dictionary<string, int> eqstats = GetEquipStats(equip.ItemId);
                if ((nEquip.UpgradeSlots > 0 || IsCleanSlate(scrollId)) && Math.Ceiling(Randomizer.NextDouble() * 100.0) <= stats["success"] || (checkIfGm == true))
                {
                    switch (scrollId)
                    {
                        case 2040727:
                            nEquip.Flag |= (byte)InventorySettings.Items.Flags.Spikes;
                            return equip;
                        case 2041058:
                            nEquip.Flag |= (byte)InventorySettings.Items.Flags.Spikes;
                            return equip;
                        case 2049000:
                        case 2049001:
                        case 2049002:
                        case 2049003:
                            if (nEquip.Level + nEquip.UpgradeSlots < eqstats["tuc"])
                            {
                                nEquip.UpgradeSlots++;
                            }
                            break;
                        case 2049100:
                        case 2049101:
                        case 2049102:
                            int increase = 1;
                            if (Math.Ceiling(Randomizer.NextDouble() * 100.0) <= 50)
                            {
                                increase = increase * -1;
                            }
                            if (nEquip.Str > 0)
                            {
                                nEquip.Str += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Dex > 0)
                            {
                                nEquip.Dex += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Int > 0)
                            {
                                nEquip.Int += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Luk > 0)
                            {
                                nEquip.Luk += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Watk > 0)
                            {
                                nEquip.Watk += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Wdef > 0)
                            {
                                nEquip.Wdef += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Matk > 0)
                            {
                                nEquip.Matk += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Mdef > 0)
                            {
                                nEquip.Mdef += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Acc > 0)
                            {
                                nEquip.Acc += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Avoid > 0)
                            {
                                nEquip.Avoid += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Speed > 0)
                            {
                                nEquip.Speed += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Jump > 0)
                            {
                                nEquip.Jump += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Hp > 0)
                            {
                                nEquip.Hp += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            if (nEquip.Mp > 0)
                            {
                                nEquip.Mp += (short)(Math.Ceiling(Randomizer.NextDouble() * 5.0) * increase);
                            }
                            break;
                        default:
                            foreach (var stat in stats)
                            {
                                if (stat.Key.Equals("STR"))
                                {
                                    nEquip.Str += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("DEX"))
                                {
                                    nEquip.Dex += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("INT"))
                                {
                                    nEquip.Int += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("LUK"))
                                {
                                    nEquip.Luk += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("PAD"))
                                {
                                    nEquip.Watk += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("PDD"))
                                {
                                    nEquip.Wdef += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("MAD"))
                                {
                                    nEquip.Matk += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("MDD"))
                                {
                                    nEquip.Mdef += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("ACC"))
                                {
                                    nEquip.Acc += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("EVA"))
                                {
                                    nEquip.Avoid += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("Speed"))
                                {
                                    nEquip.Speed += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("Jump"))
                                {
                                    nEquip.Jump += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("MHP"))
                                {
                                    nEquip.Hp += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("MMP"))
                                {
                                    nEquip.Mp += (short)stat.Value;
                                }
                                else if (stat.Key.Equals("afterImage"))
                                {
                                }
                            }
                            break;
                    }
                    if (!IsCleanSlate(scrollId))
                    {
                        nEquip.UpgradeSlots--;
                        nEquip.Level++;
                    }
                }
                else {
                    if (!usingWhiteScroll && !IsCleanSlate(scrollId))
                    {
                        nEquip.UpgradeSlots--;
                    }
                    if (Math.Ceiling(1.0 + Randomizer.NextDouble() * 100.0) < stats["cursed"])
                    {
                        return null;
                    }
                }
            }
            return equip;
        }

        public IMapleItem GetEquipById(int equipId)
        {
            Equip nEquip;
            nEquip = new Equip(equipId, 0);
            nEquip.Quantity = 1;
            Dictionary<string, int> stats = GetEquipStats(equipId);
            if (stats != null)
            {
                foreach (var stat in stats)
                {
                    if (stat.Key.Equals("STR"))
                    {
                        nEquip.Str = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("DEX"))
                    {
                        nEquip.Dex = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("INT"))
                    {
                        nEquip.Int = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("LUK"))
                    {
                        nEquip.Luk = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("PAD"))
                    {
                        nEquip.Watk = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("PDD"))
                    {
                        nEquip.Wdef = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("MAD"))
                    {
                        nEquip.Matk = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("MDD"))
                    {
                        nEquip.Mdef = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("ACC"))
                    {
                        nEquip.Acc = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("EVA"))
                    {
                        nEquip.Avoid = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("Speed"))
                    {
                        nEquip.Speed = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("Jump"))
                    {
                        nEquip.Jump = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("MHP"))
                    {
                        nEquip.Hp = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("MMP"))
                    {
                        nEquip.Mp = (short)stat.Value;
                    }
                    else if (stat.Key.Equals("tuc"))
                    {
                        nEquip.UpgradeSlots = (byte)stat.Value;
                    }
                    else if (IsDropRestricted(equipId))
                    {
                        nEquip.Flag |= (byte)InventorySettings.Items.Flags.Untradeable;
                    }
                    else if (stat.Key.Equals("afterImage"))
                    {
                    }
                }
            }
            EquipCache.Add(equipId, nEquip);
            return nEquip.Copy();
        }

        private short GetRandStat(short defaultValue, int maxRange)
        {
            if (defaultValue == 0)
            {
                return 0;
            }

            // vary no more than ceil of 10% of stat
            int lMaxRange = (int)Math.Min(Math.Ceiling(defaultValue * 0.1), maxRange);
            return (short)(defaultValue - lMaxRange + Math.Floor(Randomizer.NextDouble() * (lMaxRange * 2 + 1)));
        }

        public Equip RandomizeStats(Equip equip)
        {
            equip.Str = GetRandStat(equip.Str, 5);
            equip.Dex = GetRandStat(equip.Dex, 5);
            equip.Int = GetRandStat(equip.Int, 5);
            equip.Luk = GetRandStat(equip.Luk, 5);
            equip.Matk = GetRandStat(equip.Matk, 5);
            equip.Watk = GetRandStat(equip.Watk, 5);
            equip.Acc = GetRandStat(equip.Acc, 5);
            equip.Avoid = GetRandStat(equip.Avoid, 5);
            equip.Jump = GetRandStat(equip.Jump, 5);
            equip.Speed = GetRandStat(equip.Speed, 5);
            equip.Wdef = GetRandStat(equip.Wdef, 10);
            equip.Mdef = GetRandStat(equip.Mdef, 10);
            equip.Hp = GetRandStat(equip.Hp, 10);
            equip.Mp = GetRandStat(equip.Mp, 10);
            return equip;
        }

        public MapleStatEffect GetItemEffect(int itemId)
        {
            MapleStatEffect ret = ItemEffects[itemId];
            if (ret == null)
            {
                IMapleData item = GetItemData(itemId);
                if (item == null)
                {
                    return null;
                }
                IMapleData spec = item.GetChildByPath("spec");
                ret = MapleStatEffect.LoadItemEffectFromData(spec, itemId);
                ItemEffects.Add(itemId, ret);
            }
            return ret;
        }

        public bool IsBullet(int itemId)
        {
            int id = itemId / 10000;
            if (id == 233)
            {
                return true;
            }
            else {
                return false;
            }
        }

        public bool IsRechargable(int itemId)
        {
            int id = itemId / 10000;
            if (id == 233 || id == 207)
            {
                return true;
            }
            else {
                return false;
            }
        }

        public bool IsOverall(int itemId)
        {
            return itemId >= 1050000 && itemId < 1060000;
        }

        public bool IsPet(int itemId)
        {
            return itemId >= 5000000 && itemId <= 5000100;
        }

        public bool IsArrowForCrossBow(int itemId)
        {
            return itemId >= 2061000 && itemId < 2062000;
        }

        public bool IsArrowForBow(int itemId)
        {
            return itemId >= 2060000 && itemId < 2061000;
        }

        public bool IsTwoHanded(int itemId)
        {
            var type = GetWeaponType(itemId);
            if (type == MapleWeaponType.Axe2H ||
                type == MapleWeaponType.Blunt2H ||
              type == MapleWeaponType.Bow ||
               type == MapleWeaponType.Claw ||
              type == MapleWeaponType.Crossbow ||
               type == MapleWeaponType.PoleArm ||
type == MapleWeaponType.Spear ||
              type == MapleWeaponType.Sword2H ||
              type == MapleWeaponType.Gun ||
               type == MapleWeaponType.Knuckle)
            {
                return true;
            }
            else
                return false;

        }

        public bool IsTownScroll(int itemId)
        {
            return itemId >= 2030000 && itemId < 2030020;
        }

        public bool IsGun(int itemId)
        {
            return itemId >= 1492000 && itemId <= 1492024;
        }

        public bool IsWritOfSolomon(int itemId)
        {
            return itemId >= 2370000 && itemId <= 2370012;
        }

        public int GetExpCache(int itemId)
        {
            if (getExpCache.ContainsKey(itemId))
            {
                return getExpCache[itemId];
            }
            IMapleData item = GetItemData(itemId);
            if (item == null)
            {
                return 0;
            }
            int pEntry = 0;
            IMapleData pData = item.GetChildByPath("spec/exp");
            if (pData == null)
            {
                return 0;
            }
            pEntry = MapleDataTool.GetInt(pData);

            getExpCache.Add(itemId, pEntry);
            return pEntry;
        }

        public int GetWatkForProjectile(int itemId)
        {
            int atk;
            if (ProjectileWatkCache.TryGetValue(itemId, out atk))
            {
                return atk;
            }
            IMapleData data = GetItemData(itemId);
            atk = MapleDataTool.GetInt("info/incPAD", data, 0);
            ProjectileWatkCache.Add(itemId, atk);
            return atk;
        }

        public bool CanScroll(int scrollid, int itemid)
        {
            int scrollCategoryQualifier = scrollid / 100 % 100;
            int itemCategoryQualifier = itemid / 10000 % 100;
            return scrollCategoryQualifier == itemCategoryQualifier;
        }

        public string GetName(int itemId)
        {
            if (NameCache.ContainsKey(itemId))
            {
                return NameCache[itemId];
            }
            IMapleData strings = GetStringData(itemId);
            if (strings == null)
            {
                return null;
            }
            string ret = MapleDataTool.GetString("name", strings, null);
            NameCache.Add(itemId, ret);
            return ret;
        }

        public string GetDesc(int itemId)
        {
            if (DescCache.ContainsKey(itemId))
            {
                return DescCache[itemId];
            }
            IMapleData strings = GetStringData(itemId);
            if (strings == null)
            {
                return null;
            }
            string ret = MapleDataTool.GetString("desc", strings, null);
            DescCache.Add(itemId, ret);
            return ret;
        }

        public string GetMsg(int itemId)
        {
            if (MsgCache.ContainsKey(itemId))
            {
                return MsgCache[itemId];
            }
            IMapleData strings = GetStringData(itemId);
            if (strings == null)
            {
                return null;
            }
            string ret = MapleDataTool.GetString("msg", strings, null);
            MsgCache.Add(itemId, ret);
            return ret;
        }

        public bool IsDropRestricted(int itemId)
        {
            if (DropRestrictionCache.ContainsKey(itemId))
            {
                return DropRestrictionCache[itemId];
            }
            IMapleData data = GetItemData(itemId);

            bool bRestricted = MapleDataTool.ConvertToInt("info/tradeBlock", data, 0) == 1;
            if (!bRestricted)
            {
                bRestricted = MapleDataTool.ConvertToInt("info/quest", data, 0) == 1;
            }
            DropRestrictionCache.Add(itemId, bRestricted);

            return bRestricted;
        }

        public bool IsPickupRestricted(int itemId)
        {
            if (PickupRestrictionCache.ContainsKey(itemId))
            {
                return PickupRestrictionCache[itemId];
            }
            IMapleData data = GetItemData(itemId);
            bool bRestricted = MapleDataTool.ConvertToInt("info/only", data, 0) == 1;

            PickupRestrictionCache.Add(itemId, bRestricted);
            return bRestricted;
        }

        public Dictionary<string, int> GetSkillStats(int itemId, double playerJob)
        {
            Dictionary<string, int> ret = new Dictionary<string, int>();
            IMapleData item = GetItemData(itemId);
            if (item == null)
            {
                return null;
            }
            IMapleData info = item.GetChildByPath("info");
            if (info == null)
            {
                return null;
            }
            foreach (IMapleData data in info.Children)
            {
                if (data.Name.StartsWith("inc"))
                {
                    ret.Add(data.Name.Substring(3), MapleDataTool.ConvertToInt(data));
                }
            }
            ret.Add("masterLevel", MapleDataTool.GetInt("masterLevel", info, 0));
            ret.Add("reqSkillLevel", MapleDataTool.GetInt("reqSkillLevel", info, 0));
            ret.Add("success", MapleDataTool.GetInt("success", info, 0));

            IMapleData skill = info.GetChildByPath("skill");
            int curskill = 1;
            int size = skill.Children.Count;
            for (int i = 0; i < size; i++)
            {
                curskill = MapleDataTool.GetInt(i.ToString(), skill, 0);
                if (curskill == 0) // end - no more;
                {
                    break;
                }
                double skillJob = Math.Floor(curskill / 10000D);
                if (skillJob == playerJob)
                {
                    ret.Add("skillid", curskill);
                    break;
                }
            }

            if (!ret.ContainsKey("skillid"))
            {
                ret.Add("skillid", 0);
            }
            return ret;
        }

        public List<int> PetsCanConsume(int itemId)
        {
            List<int> ret = new List<int>();
            IMapleData data = GetItemData(itemId);
            int curPetId = 0;
            int size = data.Children.Count;
            for (int i = 0; i < size; i++)
            {
                curPetId = MapleDataTool.GetInt("spec/" + i.ToString(), data, 0);
                if (curPetId == 0)
                {
                    break;
                }
                ret.Add(curPetId);
            }
            return ret;
        }

        public bool IsQuestItem(int itemId)
        {
            if (IsQuestItemCache.ContainsKey(itemId))
            {
                return IsQuestItemCache[itemId];
            }
            IMapleData data = GetItemData(itemId);
            bool questItem = MapleDataTool.ConvertToInt("info/quest", data, 0) == 1;
            IsQuestItemCache.Add(itemId, questItem);
            return questItem;
        }

        public bool IsMiniDungeonMap(int mapId)
        {
            switch (mapId)
            {
                case 100020000:
                case 105040304:
                case 105050100:
                case 221023400:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsDragonItem(int itemId)
        {
            switch (itemId)
            {
                case 1372032:
                case 1312031:
                case 1412026:
                case 1302059:
                case 1442045:
                case 1402036:
                case 1432038:
                case 1422028:
                case 1472051:
                case 1472052:
                case 1332049:
                case 1332050:
                case 1322052:
                case 1452044:
                case 1462039:
                case 1382036:
                    return true;
                default:
                    return false;
            }
        }

        public class SummonEntry
        {

            private readonly int _chance;
            private readonly int _mobId;

            public SummonEntry(int a, int b)
            {
                _mobId = a;
                _chance = b;
            }

            public int GetChance()
            {
                return _chance;
            }

            public int GetMobId()
            {
                return _mobId;
            }
        }

        public bool IsKarmaAble(int itemId)
        {
            if (KarmaCache.ContainsKey(itemId))
            {
                return KarmaCache[itemId];
            }
            IMapleData data = GetItemData(itemId);
            bool bRestricted = MapleDataTool.ConvertToInt("info/tradeAvailable", data, 0) > 0;
            KarmaCache.Add(itemId, bRestricted);
            return bRestricted;
        }

        public bool IsConsumeOnPickup(int itemId)
        {
            if (ConsumeOnPickupCache.ContainsKey(itemId))
            {
                return ConsumeOnPickupCache[itemId];
            }

            IMapleData data = GetItemData(itemId);

            bool consume = MapleDataTool.ConvertToInt("spec/consumeOnPickup", data, 0) == 1 || MapleDataTool.ConvertToInt("specEx/consumeOnPickup", data, 0) == 1;

            ConsumeOnPickupCache.Add(itemId, consume);
            return consume;
        }

        //private void loadCardIdData()
        //{
        //    PreparedStatement ps = null;
        //    ResultSet rs = null;
        //    try
        //    {
        //        ps = DatabaseConnection.GetConnection().prepareStatement("SELECT cardid, mobid FROM monstercarddata");
        //        rs = ps.executeQuery();
        //        while (rs.next())
        //            monsterBookID.Add(rs.GetInt(1), rs.GetInt(2));
        //    }
        //    catch (SQLException e)
        //    {
        //    }
        //    ly
        //    {
        //        try
        //        {
        //            if (rs != null)
        //                rs.close();
        //            if (ps != null)
        //                ps.close();
        //        }
        //        catch (SQLException e)
        //        {
        //        }
        //    }
        //}

        //public int GetCardMobId(int id)
        //{
        //    return monsterBookID.Get(id);
        //}

        //public List<MapleFish> GetFishReward(int itemId)
        //{
        //    if (fishingCache.ContainsKey(itemId))
        //    {
        //        return fishingCache.Get(itemId);
        //    }
        //    else {
        //        List<MapleFish> rewards = new ArrayList<>();
        //        IMapleData data = GetItemData(itemId);
        //        IMapleData rewardData = data.GetChildByPath("reward");
        //        for (IMapleData child : rewardData.Children)
        //        {
        //            int rewardItem = MapleDataTool.GetInt("item", child, 0);
        //            int prob = MapleDataTool.GetInt("prob", child, 0);
        //            int count = MapleDataTool.GetInt("count", child, 0);
        //            string effect = MapleDataTool.GetString("Effect", child, "");
        //            rewards.Add(new MapleFish(rewardItem, prob, count, effect));
        //        }
        //        fishingCache.Add(itemId, rewards);
        //        return rewards;
        //    }
        //}
    }
}
