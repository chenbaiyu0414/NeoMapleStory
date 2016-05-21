using System.Drawing;
using NeoMapleStory.Core.Database.DataModel;
using NeoMapleStory.Game.Inventory;

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

        public static MaplePet LoadFromDb(int itemid, byte position, int uniqueid)
        {
            var ret = new MaplePet(itemid, position, uniqueid);

            //using (var db = new NeoDatabase())
            //{
            //    var result = db.Pets.Where(x => x.UniqueId == uniqueid).Select(x => x).FirstOrDefault();
            //    if (result == null)
            //        return null;

            //    ret.PetInfo.PetName = result.PetName;
            //    ret.PetInfo.Closeness = result.Closeness;
            //    ret.PetInfo.Level = result.Level;
            //    ret.PetInfo.Fullness = result.Fullness;
            //    return ret;
            //}
            return null;
        }


        public void SaveToDb()
        {
            //Connection con = DatabaseConnection.getConnection(); // Get a connection to the database
            //PreparedStatement ps = con.prepareStatement("UPDATE pets SET name = ?, level = ?, closeness = ?, fullness = ? WHERE uniqueid = ?");
            //ps.setString(1, getName()); // Set name
            //ps.setInt(2, getLevel()); // Set Level
            //ps.setInt(3, getCloseness()); // Set Closeness
            //ps.setInt(4, getFullness()); // Set Fullness
            //ps.setInt(5, getUniqueId()); // Set ID
            //ps.executeUpdate(); // Execute statement
            //ps.close();
        }

        //        PreparedStatement ps = con.prepareStatement("INSERT INTO pets (name, level, closeness, fullness, uniqueid) VALUES (?, ?, ?, ?, ?)");

        //        Connection con = DatabaseConnection.getConnection();
        //        MapleItemInformationProvider mii = MapleItemInformationProvider.Instance;
        //    {
        //    try
        //{

        //public static int createPet(int itemid, MapleCharacter chr)
        //        ps.setString(1, mii.getName(itemid));
        //        ps.setInt(2, 1);
        //        ps.setInt(3, 0);
        //        ps.setInt(4, 100);
        //        int ret = MapleCharacter.getNextUniqueId();
        //        ps.setInt(5, ret);
        //        ps.executeUpdate();
        //        ps.close();
        //        chr.saveToDB(true);
        //        return ret;
        //    }
        //    catch 
        //    {

        //        return -1;
        //    }

        //}

        //public static int createPet(int itemid)
        //{
        //    try
        //    {
        //        MapleItemInformationProvider mii = MapleItemInformationProvider.getInstance();

        //        Connection con = DatabaseConnection.getConnection();
        //        PreparedStatement ps = con.prepareStatement("INSERT INTO pets (name, level, closeness, fullness) VALUES (?, ?, ?, ?)");
        //        ps.setString(1, mii.getName(itemid));
        //        ps.setInt(2, 1);
        //        ps.setInt(3, 0);
        //        ps.setInt(4, 100);
        //        ps.executeUpdate();
        //        ResultSet rs = ps.getGeneratedKeys();
        //        rs.next();
        //        int ret = rs.getInt(1);
        //        rs.close();
        //        ps.close();

        //        return ret;
        //    }
        //    catch (SQLException ex)
        //    {

        //        return -1;
        //    }

        //}

        //public bool canConsume(int itemId)
        //{
        //    MapleItemInformationProvider mii = MapleItemInformationProvider.getInstance();
        //    for (int petId : mii.petsCanConsume(itemId))
        //    {
        //        if (petId == this.getItemId())
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public void updatePosition(List<LifeMovementFragment> movement)
        //{
        //    for (LifeMovementFragment move : movement)
        //    {
        //        if (move instanceof LifeMovement) {
        //        if (move instanceof AbsoluteLifeMovement) {
        //            Point position = ((LifeMovement)move).getPosition();
        //            this.setPos(position);
        //        }
        //        this.setStance(((LifeMovement)move).getNewstate());
        //    }
        //}
    }
}