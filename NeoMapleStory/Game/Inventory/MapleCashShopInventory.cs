using System;
using System.Collections.Generic;
using System.Linq;
using NeoMapleStory.Core;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Core.Database.Models;
using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.Inventory
{
    public class MapleCashShopInventory
    {
        private readonly int m_accountId;
        private readonly MapleCharacter m_player;
        public Dictionary<int, MapleCashShopInventoryItem> CashShopItems { get; } = new Dictionary<int, MapleCashShopInventoryItem>();
        public Dictionary<int, MapleCashShopInventoryItem> CashShopGifts { get; } = new Dictionary<int, MapleCashShopInventoryItem>();

        public MapleCashShopInventory(MapleCharacter chr)
        {
            m_accountId = chr.Account.Id;
            m_player = chr;
            Load(m_accountId);
        }

        public void Load(int id)
        {
            try
            {
                using (var db = new NeoMapleStoryDatabase())
                {
                    var cashItemQuery = db.CashShopInventories.Where(x => x.AId == m_accountId).Select(x => x);

                    foreach (var item in cashItemQuery)
                    {
                        var citem = new MapleCashShopInventoryItem(item.UniqueId, item.ItemId, item.Sn, item.Quantity, item.IsGift)
                        {
                            Expire = item.ExpireDate,
                            Sender = item.Sender
                        };
                        if (CashShopItems.ContainsKey(citem.UniqueId))
                            CashShopItems[citem.UniqueId] = citem;
                        else
                            CashShopItems.Add(citem.UniqueId, citem);
                    }


                    var cashGiftQuery = db.CashShopGifts.Where(x => x.AId == m_accountId).Select(x => x);

                    foreach (var giftinfo in cashGiftQuery)
                    {
                        MapleCashShopInventoryItem gift;
                        if (giftinfo.ItemId >= 5000000 && giftinfo.ItemId <= 5000100)
                        {
                            int petId = MaplePet.Create(giftinfo.ItemId, m_player);
                            gift = new MapleCashShopInventoryItem(petId, giftinfo.ItemId, giftinfo.Sn, 1, true);
                        }
                        else
                        {
                            gift = new MapleCashShopInventoryItem(giftinfo.RingUniqueId, giftinfo.ItemId, giftinfo.Sn, giftinfo.Quantity, true)
                            {
                                IsRing = giftinfo.RingUniqueId > 0
                            };
                        }
                        gift.Expire = giftinfo.ExpireDate;
                        gift.Sender = giftinfo.Sender;
                        gift.Message = giftinfo.Message;

                        if (CashShopGifts.ContainsKey(gift.UniqueId))
                            CashShopGifts[gift.UniqueId] = gift;
                        else
                            CashShopGifts.Add(gift.UniqueId, gift);

                        if (CashShopItems.ContainsKey(gift.UniqueId))
                            CashShopItems[gift.UniqueId] = gift;
                        else
                            CashShopItems.Add(gift.UniqueId, gift);

                        Save();
                    }

                    var deleteGiftQuery = db.CashShopGifts.Where(x => x.AId == m_accountId).Select(x => x);
                    db.CashShopGifts.RemoveRange(deleteGiftQuery);

                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void Save()
        {
            try
            {
                using (var db = new NeoMapleStoryDatabase())
                {
                    var deleteQuery = db.CashShopInventories.Where(x => x.AId == m_accountId).Select(x => x);
                    db.CashShopInventories.RemoveRange(deleteQuery);

                    List<CashShopInventoryModel> list = new List<CashShopInventoryModel>();
                    foreach (var citem in CashShopItems.Values)
                    {
                        list.Add(new CashShopInventoryModel()
                        {
                        UniqueId= citem.UniqueId,
                        ItemId= citem.ItemId,
                        Sn= citem.Sn,
                        Quantity= citem.Quantity,
                        Sender=citem.Sender,
                        Message= citem.Message,
                        ExpireDate= citem.Expire,
                        IsGift= citem.IsGift,
                        IsRing= citem.IsRing,
                        });
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void AddItem(MapleCashShopInventoryItem citem)
        {
            if (CashShopItems.ContainsKey(citem.UniqueId))
                CashShopItems[citem.UniqueId] = citem;
            else
                CashShopItems.Add(citem.UniqueId, citem);
        }

        public void RemoveItem(int uniqueid)=> CashShopItems.Remove(uniqueid);

        public MapleCashShopInventoryItem GetItem(int uniqueid)=> CashShopItems.FirstOrDefault(x => x.Key == uniqueid).Value;

    }
}
