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
            Success = 0, //登陆成功
            Refresh, //刷新  同意协议 选择性别时
            Banned = 6, //封号
            ShieldLogin = 3, //屏蔽了账号登录功能或者已经被删除、终止的账号
            IncorrectPassword = 4, //屏蔽了静态密码或密码输入错误
            Unfind = 5, //未登录的账号
            IsLogged = 7, //当前连接不稳定。请更换其它频道或世界。为您带来不便，请谅解。6 or 8 or 9
            ServerBusy = 10, //目前因链接邀请过多 服务器未能处理。
            GenderNeeded
        }

        public static void ChangeGender(string username, byte gender)
        {
            var cmd = new MySqlCommand("UPDATE Accounts SET Gender=@Gender Where Username=@Username");
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
            var cmd = new MySqlCommand("SELECT Id FROM Characters Where Name=@Name");
            cmd.Parameters.Add(new MySqlParameter("@Name", name));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        public static LoginResultCode Login(MapleClient client, string username, string password)
        {
            var cmd =
                new MySqlCommand(
                    "SELECT Id,Username,Password,PasswordSalt,PermanentBan,TempBanDate,Gender,IsGm,NexonPoint,MaplePoint,LoginState FROM Accounts WHERE Username=@Username");
            cmd.Parameters.Add(new MySqlParameter("@Username", username));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if (
                        (MapleClient.LoginState)
                            Enum.Parse(typeof(MapleClient.LoginState), (string) reader["LoginState"]) !=
                        MapleClient.LoginState.NotLogin)
                        return LoginResultCode.IsLogged;
                    if ((string) reader["Password"] != Sha256.Get(password, Guid.Parse((string) reader["PasswordSalt"])))
                        return LoginResultCode.IncorrectPassword;
                    if ((bool) reader["PermanentBan"] || reader["TempBanDate"] != DBNull.Value)
                        return LoginResultCode.Banned;
                    if (reader["Gender"] == DBNull.Value)
                        return LoginResultCode.GenderNeeded;

                    client.AccountId = (int) reader["Id"];
                    client.Gender = (bool) reader["Gender"];
                    client.IsGm = (bool) reader["IsGm"];

                    client.State =
                        (MapleClient.LoginState)
                            Enum.Parse(typeof(MapleClient.LoginState), (string) reader["LoginState"]);

                    reader.Close();
                    return LoginResultCode.Success;
                }
                return LoginResultCode.Unfind;
            }
        }


        public static List<MapleCharacter> LoadCharacters(MapleClient client)
        {
            var chars = new List<MapleCharacter>();
            var cmd = new MySqlCommand("SELECT Id FROM Characters WHERE AId=@AId");
            cmd.Parameters.Add(new MySqlParameter("@AId", client.AccountId));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    chars.Add(MapleCharacter.LoadCharFromDb((int) reader["Id"], client, false));
                }

                return chars;
            }
        }

        public static int GetMaxIndex(string tableName, string columnName)
        {
            var cmd = new MySqlCommand("SELECT MAX(@ColumnName) FROM @TableName");
            cmd.Parameters.Add(new MySqlParameter("@ColumnName", columnName));
            cmd.Parameters.Add(new MySqlParameter("@TableName", tableName));
            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                return (int) cmd.ExecuteScalar();
            }
        }

        //public static Account LoadAccount(string username)
        //{
        //    MySqlCommand cmd =
        //        new MySqlCommand(
        //            "SELECT Id,Username,Gender,IsGm,NexonPoint,MaplePoint FROM Accounts WHERE Username=@Username");
        //    cmd.Parameters.Add(new MySqlParameter("@Username", username));

        //    using (var con = DbConnectionManager.Instance.GetConnection())
        //    {
        //        cmd.Connection = con;
        //        con.Open();
        //        var reader = cmd.ExecuteReader();
        //        if (reader.Read())
        //            return new Account
        //            {
        //                Id = (int) reader["Id"],
        //                Username = (string) reader["Username"],
        //                Gender = (bool) reader["Gender"],
        //                IsGm = (bool) reader["IsGm"],
        //                NexonPoint = (int) reader["NexonPoint"],
        //                MaplePoint = (int) reader["MaplePoint"]
        //            };
        //        throw new Exception("加载账号错误");
        //    }
        //}

        public static void PlayerExit(Account account)
        {
            var cmd = new MySqlCommand("SELECT IsLogged FROM Accounts WHERE Username=@Username");
            cmd.Parameters.Add(new MySqlParameter("@Username", account.Username));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                var reader = cmd.ExecuteReader();
                if (!reader.Read()) return;
                if (!(bool) reader["IsLogged"]) return;

                reader.Close();
                cmd.CommandText = "UPDATE Accounts SET IsLogged=false WHERE Username=@Username";
                Console.WriteLine(cmd.ExecuteNonQuery() > 0 ? "角色下线成功！" : "角色下线失败！");
            }
        }

        public static void UpdateState(int id, MapleClient.LoginState state)
        {
            var cmd = new MySqlCommand("UPDATE Accounts SET LoginState = @LoginState WHERE Id = @Id");
            cmd.Parameters.Add(new MySqlParameter("@LoginState", state.ToString()));
            cmd.Parameters.Add(new MySqlParameter("@Id", id));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static MapleClient.LoginState GetState(int id)
        {
            var cmd = new MySqlCommand("SELECT LoginState FROM Accounts WHERE Id=@Id");
            cmd.Parameters.Add(new MySqlParameter("@Id", id));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;
                con.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return (MapleClient.LoginState)
                        Enum.Parse(typeof(MapleClient.LoginState), (string) reader["LoginState"]);
                }
            }
            return MapleClient.LoginState.NotLogin;
        }
    }
}