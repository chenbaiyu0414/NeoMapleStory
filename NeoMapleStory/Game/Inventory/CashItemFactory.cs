using System;
using NeoMapleStory.Game.Data;
using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.Inventory
{
    public class CashItemFactory
    {
        private static readonly Dictionary<int, int> SnLookup = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> IdLookup = new Dictionary<int, int>();
        private static readonly Dictionary<int, CashItem> ItemStats = new Dictionary<int, CashItem>();
        private static readonly IMapleDataProvider Data = MapleDataProviderFactory.GetDataProvider("Etc.wz");
        private static readonly IMapleData Commodities = Data.GetData("Commodity.img".PadLeft(11, '0'));
        private static readonly Dictionary<int, List<CashItem>> CashPackages = new Dictionary<int, List<CashItem>>();

        public static CashItem GetItem(int sn)
        {
            CashItem stats;
            if (!ItemStats.TryGetValue(sn,out stats))
            {
                int cid;
                if (!SnLookup.ContainsKey(sn))
                {
                    cid = int.Parse(Commodities.Children.FirstOrDefault(x => (int)x.GetChildByPath("SN").Data == sn)?.Name ?? "-1");
                    if (cid != -1)
                    {
                        if (SnLookup.ContainsKey(sn))
                            SnLookup[sn] = cid;
                        else
                            SnLookup.Add(sn, cid);
                    }
                }
                else
                {
                    cid = SnLookup[sn];
                }
                
                if (cid != -1)
                {
                    int itemId = MapleDataTool.GetInt(cid + "/ItemId", Commodities);
                    int count = MapleDataTool.GetInt(cid + "/Count", Commodities, 1);
                    int price = MapleDataTool.GetInt(cid + "/Price", Commodities, 0);
                    int period = MapleDataTool.GetInt(cid + "/Period", Commodities, 0);
                    int gender = MapleDataTool.GetInt(cid + "/Gender", Commodities, 2);
                    bool onSale = MapleDataTool.GetInt(cid + "/OnSale", Commodities, 0) == 1;

                    stats = new CashItem(sn, itemId, (short)count, price, period, gender, onSale);

                    if (ItemStats.ContainsKey(sn))
                        ItemStats[sn] = stats;
                    else
                        ItemStats.Add(sn, stats);
                }else
                {
                    throw new Exception("不存在");
                }
            }

            return stats;
        }

        public static List<CashItem> GetPackageItems(int itemId)
        {
            if (CashPackages.ContainsKey(itemId))
            {
                return CashPackages[itemId];
            }
            List<CashItem> packageItems = new List<CashItem>();
            IMapleDataProvider dataProvider = MapleDataProviderFactory.GetDataProvider("Etc.wz");
            IMapleData a = dataProvider.GetData("CashPackage.img");
            foreach (var b in a.Children)
            {
                if (itemId == int.Parse(b.Name))
                {
                    foreach (var c in b.Children)
                    {
                        foreach (var d in c.Children)
                        {
                            int sn = MapleDataTool.GetInt("" + int.Parse(d.Name), c);
                            packageItems.Add(GetItem(sn));
                        }
                    }
                    break;
                }
            }
            
            if (CashPackages.ContainsKey(itemId))
                CashPackages[itemId] = packageItems;
            else
                CashPackages.Add(itemId, packageItems);

            return packageItems;
        }

        public static int GetSnFromId(int id)
        {
            int cid;

            if (!IdLookup.ContainsKey(id))
            {
                cid = int.Parse(Commodities.Children.First(x => (int)x.GetChildByPath("ItemId").Data == id).Name);

                if (IdLookup.ContainsKey(id))
                    IdLookup[id] = cid;
                else
                    IdLookup.Add(id, cid);           
            }
            else
            {
                cid = IdLookup[id];
            }
            return MapleDataTool.GetInt(cid + "/SN", Commodities);
        }
    }
}
