using System.Drawing;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public class MapleMapItem : AbstractMapleMapObject
    {
        public MapleMapItem(IMapleItem item, Point position, IMapleMapObject dropper, MapleCharacter owner)
        {
            Position = position;
            Item = item;
            Dropper = dropper;
            Owner = owner;
            Money = 0;
        }

        public MapleMapItem(int meso, Point position, IMapleMapObject dropper, MapleCharacter owner)
        {
            Position = position;
            Item = null;
            Money = meso;
            Dropper = dropper;
            Owner = owner;
        }

        public IMapleItem Item { get; }
        public IMapleMapObject Dropper { get; }
        public MapleCharacter Owner { get; }
        public int Money { get; }
        public bool IsPickedUp { get; set; }

        public override MapleMapObjectType GetType() => MapleMapObjectType.Item;


        public override void SendDestroyData(MapleClient client)
        {
            client.Send(PacketCreator.RemoveItemFromMap(ObjectId, 1, 0));
        }

        public override void SendSpawnData(MapleClient client)
        {
            if (Money > 0)
            {
                client.Send(PacketCreator.DropMesoFromMapObject(Money, ObjectId, Dropper.ObjectId, Owner.Id, Point.Empty,
                    Position, 2));
            }
            else
            {
                client.Send(PacketCreator.DropItemFromMapObject(Item.ItemId, ObjectId, 0, Owner.Id, Point.Empty,
                    Position, 2));
            }
        }

        public bool IsQuestItem(int itemid)
        {
            //int numrow = 0;
            //Connection con = DatabaseConnection.getConnection();
            //try
            //{ //reading from the DB instead of XML parsing; since SQL might not be complete
            //    PreparedStatement ps = con.prepareStatement("SELECT * FROM monsterquestdrops WHERE itemid = ?");
            //    ps.setInt(1, itemid);
            //    ResultSet rs = ps.executeQuery();
            //    rs.last();
            //    numrow = rs.getRow();
            //    rs.close();
            //    ps.close();
            //}
            //catch (Exception e)
            //{
            //    log.error("Exception: " + e);
            //}

            //if (numrow > 0)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return false;
        }

        public int GetItemQuestId(int itemid)
        {
            //Connection con = DatabaseConnection.getConnection();
            //int questid = -1;
            //try
            //{
            //    PreparedStatement ps = con.prepareStatement("SELECT * FROM monsterquestdrops WHERE itemid = ?");
            //    ps.setInt(1, itemid);
            //    ResultSet rs = ps.executeQuery();
            //    questid = rs.getInt("questid");
            //    rs.close();
            //    ps.close();
            //}
            //catch (SQLException SQLe)
            //{
            //    SQLe.printStackTrace();
            //}
            //catch (Exception e)
            //{
            //    log.error("Exception: " + e);
            //}
            //return questid;
            return -1;
        }
    }
}