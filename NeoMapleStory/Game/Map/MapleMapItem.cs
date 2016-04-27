using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public class MapleMapItem : AbstractMapleMapObject
    {
        public IMapleItem Item { get; }
        public IMapleMapObject Dropper { get; }
        public MapleCharacter Owner { get; }
        public int Money { get; }
        public bool IsPickedUp { get; set; }

        public MapleMapItem(IMapleItem item, Point position, IMapleMapObject dropper, MapleCharacter owner)
        {
            Position=position;
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

        public override MapleMapObjectType GetType() => MapleMapObjectType.Item;


        public override void SendDestroyData(MapleClient client)
        {
            client.Send(PacketCreator.removeItemFromMap(ObjectId, 1, 0));
        }

        public override void SendSpawnData(MapleClient client)
        {
            if (Money > 0)
            {
                client.Send(PacketCreator.dropMesoFromMapObject(Money, ObjectId, Dropper.ObjectId, Owner.Id, Point.Empty, Position, 2));
            }
            else
            {
                client.Send(PacketCreator.dropItemFromMapObject(Item.ItemId, ObjectId, 0, Owner.Id, Point.Empty, Position, 2));
            }
        }

        public bool isQuestItem(int itemid)
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

        public int getItemQuestId(int itemid)
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
