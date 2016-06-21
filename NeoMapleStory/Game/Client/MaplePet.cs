using System.Drawing;
using System.Linq;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Core.Database.Models;
using NeoMapleStory.Game.Inventory;
using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Movement;

namespace NeoMapleStory.Game.Client
{
    public class MaplePet : Item
    {
        private MaplePet(int id, byte position, int uniqueid) : base(id, position, 1)
        {
            UniqueId = uniqueid;
        }

        public PetModel PetInfo { get; set; }

        public int Fh { get; set; }

        public int Stance { get; set; }

        public Point Pos { get; set; }

        public static MaplePet Load(int itemid, byte position, int uniqueid)
        {
            var ret = new MaplePet(itemid, position, uniqueid);

            using (var db = new NeoMapleStoryDatabase())
            {
                var result = db.Pets.Where(x => x.UniqueId == uniqueid).Select(x => x).FirstOrDefault();
                if (result == null)
                    return null;

                ret.PetInfo = result;
                return ret;
            }
        }


        public void Save()
        {
            try
            {
                using (var db = new NeoMapleStoryDatabase())
                {
                    var pet = db.Pets.Where(x => x.UniqueId == UniqueId).Select(x => x).FirstOrDefault();
                    if (pet == null)
                        return;
                    pet = PetInfo;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static int Create(int itemid, MapleCharacter chr)
        {
            try
            {
                MapleItemInformationProvider mii = MapleItemInformationProvider.Instance;
                using (var db = new NeoMapleStoryDatabase())
                {
                    var petmodel = new PetModel()
                    {
                        Name = mii.GetName(itemid),
                        Level = 1,
                        Closeness = 0,
                        Fullness = 100,
                        UniqueId = MapleCharacter.GetNextUniqueId()
                    };
                    db.Pets.Add(petmodel);
                    db.SaveChanges();
                    chr.Save();
                    return petmodel.Id;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }

        public static int Create(int itemid)
        {
            try
            {
                MapleItemInformationProvider mii = MapleItemInformationProvider.Instance;
                using (var db = new NeoMapleStoryDatabase())
                {
                    var petmodel = new PetModel()
                    {
                        Name = mii.GetName(itemid),
                        Level = 1,
                        Closeness = 0,
                        Fullness = 100
                    };
                    db.Pets.Add(petmodel);
                    db.SaveChanges();
                    return petmodel.Id;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }

        }

        public bool CanConsume(int itemId) => MapleItemInformationProvider.Instance.PetsCanConsume(itemId).Any(petId => petId == ItemId);

        public void UpdatePosition(List<ILifeMovementFragment> movement)
        {
            foreach (var move in movement)
            {
                var lifemovement = move as ILifeMovement;
                if (lifemovement == null) continue;
                var absoluteLifemovent = lifemovement as AbsoluteLifeMovement;
                if (absoluteLifemovent != null)
                {
                    Pos = lifemovement.Position;
                }
                Stance = lifemovement.Newstate;
            }
        }
    }
}