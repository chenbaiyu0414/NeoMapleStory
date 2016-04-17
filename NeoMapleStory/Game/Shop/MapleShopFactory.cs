using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Game.Shop
{
    public class MapleShopFactory
    {
        private readonly Dictionary<int, MapleShop> _mShops = new Dictionary<int, MapleShop>();
        private readonly Dictionary<int, MapleShop> _mNpcShops = new Dictionary<int, MapleShop>();
        public static MapleShopFactory Instance { get; } = new MapleShopFactory();


        private MapleShop LoadShop(int id, bool isShopId)
        {
            MapleShop ret = MapleShop.CreateFromDb(id, isShopId);
            if (ret != null)
            {
                if (_mShops.ContainsKey(ret.ShopId))
                    _mShops[ret.ShopId] = ret;
                else
                    _mShops.Add(id, ret);

                if (_mNpcShops.ContainsKey(ret.ShopId))
                    _mNpcShops[ret.ShopId] = ret;
                else
                    _mNpcShops.Add(id, ret);

            }
            else if (isShopId)
            {
                if (_mShops.ContainsKey(id))
                    _mShops[id] = null;
                else
                    _mShops.Add(id, null);
            }
            else {
                if (_mNpcShops.ContainsKey(id))
                    _mNpcShops[id] = null;
                else
                    _mNpcShops.Add(id, null);
            }

            return ret;
        }

        public MapleShop GetShop(int shopId) => _mShops.FirstOrDefault(x => x.Key == shopId).Value ?? LoadShop(shopId, true);

        public MapleShop GetShopForNpc(int npcId) => _mNpcShops.FirstOrDefault(x => x.Key == npcId).Value ?? LoadShop(npcId, false);

    }
}
