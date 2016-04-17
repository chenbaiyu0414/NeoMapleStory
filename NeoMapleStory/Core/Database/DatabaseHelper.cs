using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NeoMapleStory.Core.Encryption;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Server;

namespace NeoMapleStory.Core.Database
{
    public static class DatabaseHelper
    {
        public static void ChangeGender(string username, byte gender)
        {
            MySqlCommand cmd = new MySqlCommand("UPDATE Account SET Gender=@Gender Where Username=@Username");
            cmd.Parameters.Add(new MySqlParameter("@Username", username));
            cmd.Parameters.Add(new MySqlParameter("@Gender", gender != 0));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static bool CheckNameUsed(string name)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT Id FROM Character Where Name=@Name");
            cmd.Parameters.Add(new MySqlParameter("@Name", name));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        /**
* 3: ID deleted or blocked<br>
* 4: Incorrect password<br>
* 5: Not a registered id<br>
* 6: System error<br>
* 7: Already logged in<br>
* 8: System error<br>
* 9: System error<br>
* 10: Cannot process so many connections<br>
* 11: Only users older than 20 can use this channel<br>
* 13: Unable to log on as master at this ip<br>
* 14: Wrong gateway or personal info and weird korean button<br>
* 15: Processing request with that korean button!<br>
* 16: Please verify your account through email...<br>
* 17: Wrong gateway or personal info<br>
* 21: Please verify your account through email...<br>
* 23: License agreement<br>
* 25: Maple Europe notice =[<br>
* 27: Some weird full client notice, probably for trial versions<br>
*
*/
        public enum LoginResultCode
        {
            Success = 0,                                                        //登陆成功
            Refresh,                                                            //刷新  同意协议 选择性别时
            Banned = 6,                                                         //封号
            ShieldLogin = 3,                                                    //屏蔽了账号登录功能或者已经被删除、终止的账号
            IncorrectPassword = 4,                                              //屏蔽了静态密码或密码输入错误
            Unfind = 5,                                                         //未登录的账号
            IsLogged = 7,                                                       //当前连接不稳定。请更换其它频道或世界。为您带来不便，请谅解。6 or 8 or 9
            ServerBusy = 10,                                                    //目前因链接邀请过多 服务器未能处理。
            GenderNeeded
        }

        public static LoginResultCode Login(string username, string password)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM Account WHERE Username=@Username");
            cmd.Parameters.Add(new MySqlParameter("@Username", username));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if ((bool)reader["IsLogged"])
                        return LoginResultCode.IsLogged;
                    if ((string)reader["Password"] != Sha256.Get(password,(string)reader["PasswordSalt"]))
                        return LoginResultCode.IncorrectPassword;
                    if ((bool)reader["PermanentBan"] || reader["TempBanDate"] !=DBNull.Value)
                        return LoginResultCode.Banned;
                    if (reader["Gender"] == DBNull.Value)
                        return LoginResultCode.GenderNeeded;

                    reader.Close();

                    cmd.CommandText = "UPDATE Account SET IsLogged='true' WHERE Username=@Username";
                    //cmd.Parameters.Add(new SqlParameter("@Username", username));

                    if (cmd.ExecuteNonQuery() > 0)
                        return LoginResultCode.Success;

                    return LoginResultCode.ServerBusy;
                }
                return LoginResultCode.Unfind;
            }
        }


        public static List<MapleCharacter> LoadCharacters(MapleClient client)
        {
            var chars = new List<MapleCharacter>();
            MySqlCommand cmd = new MySqlCommand("SELECT Id FROM Character WHERE Account_Id=@Account_Id");
            cmd.Parameters.Add(new MySqlParameter("@Account_Id", client.Account.Id));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    chars.Add(MapleCharacter.LoadCharFromDb((int)reader["Id"], client, false));
                }

                return chars;
            }
        }

        public static int GetMaxIndex(string tableName, string columnName)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT MAX(@ColumnName) FROM @TableName");
            cmd.Parameters.Add(new MySqlParameter("@ColumnName", columnName));
            cmd.Parameters.Add(new MySqlParameter("@TableName", tableName));
            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                return (int) cmd.ExecuteScalar();
            }
        }

        public static Account LoadAccount(string username)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT Id,Username,Gender,IsGm,NexonPoint,MaplePoint FROM Account WHERE Username=@Username");
            cmd.Parameters.Add(new MySqlParameter("@Username", username));
            
            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return new Account()
                    {
                        Id = (int) reader["Id"],
                        Username = (string) reader["Username"],
                        Gender = reader["Gender"]==DBNull.Value?null: (bool?)reader["Gender"],
                        IsGm = (bool) reader["IsGm"],
                        NexonPoint = (int) reader["NexonPoint"],
                        MaplePoint = (int) reader["MaplePoint"]
                    };
                throw new Exception("加载账号错误");
            }
        }

        public static void PlayerExit(Account account)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT IsLogged FROM Account WHERE Username=@Username");
            cmd.Parameters.Add(new MySqlParameter("@Username", account.Username));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                var reader = cmd.ExecuteReader();
                if (!reader.Read()) return;
                if (!(bool) reader["IsLogged"]) return;

                reader.Close();
                cmd.CommandText = "UPDATE Account SET IsLogged='false' WHERE Username=@Username";
                Console.WriteLine(cmd.ExecuteNonQuery() > 0 ? "角色下线成功！" : "角色下线失败！");
            }
        }
    }
}
