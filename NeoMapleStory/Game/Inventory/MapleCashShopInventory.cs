using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Game.Inventory
{
    public class MapleCashShopInventory
    {
    private int accountid;
        private int characterid;
        private MapleCharacter chr;
        public Dictionary<int, MapleCashShopInventoryItem> CashShopItems { get; private set; } = new Dictionary<int, MapleCashShopInventoryItem>();
        public Dictionary<int, MapleCashShopInventoryItem> CashShopGifts { get; private set; } = new Dictionary<int, MapleCashShopInventoryItem>();

        public MapleCashShopInventory(MapleCharacter chr)
        {
            this.accountid = chr.AccountId;
            this.characterid = chr.Id;
            this.chr = chr;
            LoadFromDb(accountid);
        }

        public void LoadFromDb(int id)
        {
            try
            {
                var cmd = new MySqlCommand("SELECT * FROM CashShopInventory WHERE AccountId = @AccountId");
                cmd.Parameters.Add(new MySqlParameter("@AccountId", id));
                using (var con = DbConnectionManager.Instance.GetConnection())
                {
                    cmd.Connection = con;
                    con.Open();

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        MapleCashShopInventoryItem citem = new MapleCashShopInventoryItem(reader.GetInt32("UniqueId"),reader.GetInt32("ItemId"), reader.GetInt32("Sn"), (short)reader.GetInt32("Quantity"), reader.GetBoolean("IsGift"));
                        citem.Expire= (DateTime?)reader["ExpireDate"];
                        citem.Sender= (reader.GetString("sender"));
                        if (CashShopItems.ContainsKey(citem.UniqueId))
                            CashShopItems[citem.UniqueId] = citem;
                        else
                            CashShopItems.Add(citem.UniqueId, citem);
                    }
                    reader.Close();

                    cmd.CommandText="SELECT * FROM CashShopGifts WHERE AId = @AccountId";

                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        MapleCashShopInventoryItem gift;
                        if (reader.GetInt32("ItemId") >= 5000000 && reader.GetInt32("ItemId") <= 5000100)
                        {
                            //int petId = MaplePet.CreatePet(rs.getInt("itemid"), chr);
                            //gift = new MapleCashShopInventoryItem(petId, rs.getInt("itemid"), rs.getInt("sn"), (short)1, true);
                        }
                        else
                        {
                            //if (reader.GetInt32("Ring") > 0)
                            //{
                            //    gift = new MapleCashShopInventoryItem(reader.GetInt32("Ring"), reader.GetInt32("ItemId"), reader.GetInt32("Sn"), (short)reader.GetInt32("Quantity"), true);
                            //    gift.IsRing = true;
                            //}
                            //else
                            //{
                            //    gift = new MapleCashShopInventoryItem(MapleCharacter, reader.GetInt32("ItemId"), reader.GetInt32("Sn"), (short)reader.GetInt32("Quantity"), true);
                            //}
                        }
                        //gift.setExpire(reader.getTimestamp("expiredate"));
                        //gift.setSender(reader.getString("sender"));
                        //gift.setMessage(reader.getString("message"));
                        //CashShopGifts.put(gift.getUniqueId(), gift);
                        //CashShopItems.put(gift.getUniqueId(), gift);
                        saveToDB();
                    }
                    //reader.close();
                    //ps.close();
                    //ps = con.prepareStatement("DELETE FROM csgifts WHERE accountid = ?");
                    //ps.setInt(1, accountid);
                    //ps.executeUpdate();
                    //ps.close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void saveToDB()
        {
            try
            {
                //Connection con = DatabaseConnection.getConnection();
                //PreparedStatement ps = con.prepareStatement("DELETE FROM csinventory WHERE accountid = ?");
                //ps.setInt(1, accountid);
                //ps.executeUpdate();
                //ps.close();

                //ps = con.prepareStatement("INSERT INTO csinventory (accountid, uniqueid, itemid, sn, quantity, sender, message, expiredate, gift, isRing) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
                //for (MapleCSInventoryItem citem : CashShopItems.values())
                //{
                //    ps.setInt(1, accountid);
                //    ps.setInt(2, citem.getUniqueId());
                //    ps.setInt(3, citem.getItemId());
                //    ps.setInt(4, citem.getSn());
                //    ps.setInt(5, citem.getQuantity());
                //    ps.setString(6, citem.getSender());
                //    ps.setString(7, citem.getMessage());
                //    ps.setTimestamp(8, citem.getExpire());
                //    ps.setBoolean(9, citem.isGift());
                //    ps.setBoolean(10, citem.isRing());
                //    ps.executeUpdate();
                //}
                //ps.close();

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
