using System;
using NeoMapleStory.Game.Inventory;

namespace NeoMapleStory.Game.Client
{
    public class MapleRing : IComparable<MapleRing>
    {
        private MapleRing(int id, int id2, int partnerId, int itemid, string partnername)
        {
            RingId = id;
            PartnerRingId = id2;
            PartnerCharacterId = partnerId;
            ItemId = itemid;
            PartnerName = partnername;
        }

        public int RingId { get; }
        public int PartnerRingId { get; private set; }
        public int PartnerCharacterId { get; set; }
        public int ItemId { get; private set; }
        public string PartnerName { get; private set; }
        public bool IsEquipped { get; set; }

        public int CompareTo(MapleRing other)
        {
            if (RingId < other.RingId)
            {
                return -1;
            }
            return RingId == other.RingId ? 0 : 1;
        }

        public static Equip LoadFromDb(int itemid, byte position, int uniqueid)
        {
            try
            {
                //using (var db = new NeoDatabase())
                //{
                //    var ring = db.Rings.Where(x => x.RingId == uniqueid).Select(x => x).FirstOrDefault();
                //    if (ring == null)
                //    {
                //        Console.WriteLine($"根据UniqueID:{uniqueid} 查找戒指失败");
                //        return null;
                //    }

                //    return new Equip(itemid, position, true, ring.PartnerRingId, ring.PartnerCharacterId, ring.PartnerName);
                //}
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading ring from DB" + ex);
                return null;
            }
        }

        public static void CreateRing(int id1, int id2, int chrId1, int chrId2, string partnerName1, string partnerName2)
        {
            try
            {
                //using (var db = new NeoDatabase())
                //{
                //    RingModel ring1 = new RingModel
                //    {
                //        RingId = id1,
                //        PartnerRingId = id2,
                //        PartnerCharacterId = chrId2,
                //        PartnerName = partnerName2
                //    };
                //    RingModel ring2 = new RingModel
                //    {
                //        RingId = id2,
                //        PartnerRingId = id1,
                //        PartnerCharacterId = chrId1,
                //        PartnerName = partnerName1
                //    };
                //    db.Rings.Add(ring1);
                //    db.Rings.Add(ring2);
                //    db.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving ring to DB" + ex);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is MapleRing)
            {
                if (((MapleRing) obj).RingId == RingId)
                {
                    return true;
                }
                return false;
            }
            return false;
        }


        public override int GetHashCode() => 52*5 + RingId;


        public static bool CheckRingDb(MapleCharacter player)
        {
            try
            {
                //using (var db = new NeoDatabase())
                //{
                //    return db.Rings.Any(x => x.PartnerCharacterId == player.Id);
                //}
                return false;
            }
            catch
            {
                return true;
            }
        }

        public static void RemoveRingFromDb(MapleCharacter player)
        {
            //using (var db = new NeoDatabase())
            //{
            //    var result = db.Rings.Where(x => x.PartnerCharacterId == player.Id).Select(x => x).FirstOrDefault();

            //    if (result == null)
            //        throw new Exception("尝试删除一个不存在的Ring");

            //    var otherId = result.PartnerRingId;
            //    db.Rings.Remove(db.Rings.First(x => x.PartnerCharacterId == player.Id));
            //    db.Rings.Remove(db.Rings.First(x => x.PartnerCharacterId == otherId));
            //    db.SaveChanges();
            //}
        }
    }
}