using NeoMapleStory.Core.Database;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Packet;
using NeoMapleStory.Settings;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NeoMapleStory.Server
{
    public static class LoginPacketHandlers
    {
        public static void OnGENDER_RESULT(MapleClient c, InPacket p)
        {
            byte gender = p.ReadByte();//00 男 01女
            string username = p.ReadMapleString();//username 
            DatabaseHelper.ChangeGender(username, gender);
            c.Send(LoginPacket.GenderChanged(username, c.Account.Id.ToString()));
            c.Send(LoginPacket.LicenseRequest());
        }

        public static void OnLISENCE_RESULT(MapleClient c, InPacket p)
        {
            bool isAgree = p.ReadBool();
            if (isAgree)
                c.Send(LoginPacket.LicenseResult());
            else
                c.Close();
        }

        public static void LOGIN_AUTH(MapleClient c, InPacket p)
        {
            string username = p.ReadMapleString();
            string password = p.ReadMapleString();

            var result = DatabaseHelper.Login(username, password);
            c.Account = DatabaseHelper.LoadAccount(username);

            switch (result)
            {
                case DatabaseHelper.LoginResultCode.GenderNeeded:
                    c.Send(LoginPacket.GenderNeeded(username));
                    break;
                case DatabaseHelper.LoginResultCode.Success:
                    var userLogged = new List<int>(ServerSettings.ChannelCount);
                    MasterServer.Instance.ChannelServers.ForEach(x => userLogged.Add(x.UserLogged));
                    c.Send(LoginPacket.AuthSuccess(username, c.Account.Id, (byte)(c.Account.Gender.Value ? 1 : 0), (byte)(c.Account.IsGm ? 1 : 0)));
                    c.Send(LoginPacket.ServerList(userLogged.ToArray()));
                    c.Send(LoginPacket.ServerListEnd());
                    break;
                default:
                    c.Send(LoginPacket.AuthAccountFailed((int)result));
                    break;
            }
        }

        public static void SERVER_LIST_REQUEST(MapleClient c, InPacket p)
        {
            var userLogged = new List<int>(ServerSettings.ChannelCount);
            MasterServer.Instance.ChannelServers.ForEach(x => userLogged.Add(x.UserLogged));
            c.Send(LoginPacket.ServerList(userLogged.ToArray()));
            c.Send(LoginPacket.ServerListEnd());
        }

        public static void Relog(MapleClient c, InPacket p)
        {
            //if (c.Account.IsLogged)
            //    c.Send(LoginPacket.ReLogResponse());
        }

        public static void SERVERSTATUS_REQUEST(MapleClient c, InPacket p)
        {
            int load = MasterServer.Instance.ChannelServers.Sum(cservs => cservs.ClientCount);

            if (MasterServer.Instance.LoginServer.Config.MaxConnectionNumber <= load)
            {
                c.Send(LoginPacket.ServerStatus(2));
            }
            else if (MasterServer.Instance.LoginServer.Config.MaxConnectionNumber * 0.9 <= load)
            {
                c.Send(LoginPacket.ServerStatus(1));
            }
            else {
                c.Send(LoginPacket.ServerStatus(0));
            }
        }

        public static void CHARLIST_REQUEST(MapleClient c, InPacket p)
        {
            c.WorldId = p.ReadByte();
            c.ChannelId = p.ReadByte();
            c.Send(LoginPacket.GetCharList(c));
        }

        public static void CHECK_CHAR_NAME(MapleClient c, InPacket p)
        {
            string name = p.ReadMapleString();
            bool nameused = DatabaseHelper.CheckNameUsed(name);
            c.Send(LoginPacket.CharNameResponse(name, nameused));
        }

        public static void CREATE_CHAR(MapleClient c, InPacket p)
        {
            string name = p.ReadMapleString();

            int job = p.ReadInt();
            int face = p.ReadInt();
            int hair = p.ReadInt();
            int hairColor = 0;
            int skinColor = 0;

            if (job == 0)
            {
                skinColor = 10;
            }
            else if (job == 2)
            {
                skinColor = 11;
            }
            else {
                skinColor = 0;
            }

            int top = p.ReadInt();
            int bottom = p.ReadInt();
            int shoes = p.ReadInt();
            int weapon = p.ReadInt();

            MapleCharacter newchar = MapleCharacter.GetDefault(c.Account);
            if (c.Account.IsGm)
            {
                newchar.GmLevel = 1;
            }

            newchar.WorldId = c.WorldId;
            newchar.Face = face;
            newchar.Hair = hair + hairColor;
            newchar.Gender = c.Account.Gender.Value;

            if (job == 2)
            {
                newchar.Str = 11;
                newchar.Dex = 6;
                newchar.Int = 4;
                newchar.Luk = 4;
                newchar.RemainingAp = 0;
            }
            else {
                newchar.Str = 4;
                newchar.Dex = 4;
                newchar.Int = 4;
                newchar.Luk = 4;
                newchar.RemainingAp = 9;
            }

            newchar.Name = name;
            newchar.Skin = MapleSkinColor.GetByColorId(skinColor);

            if (job == 1)
            {
                newchar.Job = MapleJob.Beginner;
                newchar.Inventorys[MapleInventoryType.Etc.Value].AddItem(new Item(4161001, 0, 1));
            }
            else if (job == 0)
            {
                newchar.Job = MapleJob.Knight;
                newchar.Inventorys[MapleInventoryType.Etc.Value].AddItem(new Item(4161047, 0, 1));
            }
            else if (job == 2)
            {
                newchar.Job = MapleJob.Ares;
                newchar.Inventorys[MapleInventoryType.Etc.Value].AddItem(new Item(4161048, 0, 1));
            }

            MapleInventory equip = newchar.Inventorys[MapleInventoryType.Equipped.Value];

            Equip equipTop = new Equip(top, 5)
            {
                Wdef = 3,
                UpgradeSlots = 7
            };
            equip.AddFromDb(equipTop.Copy());

            Equip equipBottom = new Equip(bottom, 6)
            {
                Wdef = 2,
                UpgradeSlots = 7
            };
            equip.AddFromDb(equipBottom.Copy());

            Equip equipShoes = new Equip(shoes, 7)
            {
                Wdef = 2,
                UpgradeSlots = 7
            };
            equip.AddFromDb(equipShoes.Copy());

            Equip equipWeapon = new Equip(weapon, 11)
            {
                Watk = 15,
                UpgradeSlots = 7
            };
            equip.AddFromDb(equipWeapon.Copy());


            newchar.SaveToDb(false);
            c.Send(LoginPacket.AddNewCharEntry(newchar, true));
        }

        public static void CHAR_SELECT(MapleClient c,InPacket p)
        {
            int charId = p.ReadInt();
            c.Send(LoginPacket.GetServerIp(System.Net.IPAddress.Parse("127.0.0.1"), 7575, charId));
        }

        public static void ERROR_LOG(MapleClient c, InPacket p)
        {
            Console.WriteLine($"错误信息：{p.ReadMapleString()}");
        }
        public static void PLAYER_UPDATE(MapleClient c, InPacket p)
        {
            c.Character.SaveToDb(true);
        }
    }
}