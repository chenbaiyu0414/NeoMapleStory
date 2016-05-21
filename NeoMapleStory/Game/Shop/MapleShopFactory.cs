using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.Shop
{
    public class MapleShopFactory
    {
        private readonly Dictionary<int, MapleShop> m_mNpcShops = new Dictionary<int, MapleShop>();
        private readonly Dictionary<int, MapleShop> m_mShops = new Dictionary<int, MapleShop>();
        public static MapleShopFactory Instance { get; } = new MapleShopFactory();


        private MapleShop LoadShop(int id, bool isShopId)
        {
            var ret = MapleShop.CreateFromDb(id, isShopId);
            if (ret != null)
            {
                if (m_mShops.ContainsKey(ret.ShopId))
                    m_mShops[ret.ShopId] = ret;
                else
                    m_mShops.Add(id, ret);

                if (m_mNpcShops.ContainsKey(ret.ShopId))
                    m_mNpcShops[ret.ShopId] = ret;
                else
                    m_mNpcShops.Add(id, ret);
            }
            else if (isShopId)
            {
                if (m_mShops.ContainsKey(id))
                    m_mShops[id] = null;
                else
                    m_mShops.Add(id, null);
            }
            else
            {
                if (m_mNpcShops.ContainsKey(id))
                    m_mNpcShops[id] = null;
                else
                    m_mNpcShops.Add(id, null);
            }

            return ret;
        }

        public MapleShop GetShop(int shopId)
            => m_mShops.FirstOrDefault(x => x.Key == shopId).Value ?? LoadShop(shopId, true);

        public MapleShop GetShopForNpc(int npcId)
            => m_mNpcShops.FirstOrDefault(x => x.Key == npcId).Value ?? LoadShop(npcId, false);
    }
}