using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Core.Database.Models;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Packet;
using NeoMapleStory.Settings;

namespace NeoMapleStory.Server
{
    public static class LoginPacketHandlers
    {
        public static void PONG(MapleClient c, InPacket p)
        {
            c.LastPongTime = DateTime.Now;
        }
        public static void OnGENDER_RESULT(MapleClient c, InPacket p)
        {
            var gender = p.ReadByte(); //00 男 01女
            var username = p.ReadMapleString(); //username 
            DatabaseHelper.ChangeGender(c,username, gender);
            c.Send(LoginPacket.GenderChanged(username, c.Account.Id.ToString()));
            c.Send(LoginPacket.LicenseRequest());
        }

        public static void OnLISENCE_RESULT(MapleClient c, InPacket p)
        {
            var isAgree = p.ReadBool();
            if (isAgree)
            {
                c.State = LoginStateType.NotLogin;
                c.Send(LoginPacket.LicenseResult());
            }
            else
                c.Close();
        }

        public static void LOGIN_AUTH(MapleClient c, InPacket p)
        {
            var username = p.ReadMapleString();
            var password = p.ReadMapleString();

            var result = DatabaseHelper.Login(c, username, password);

            switch (result)
            {
                case DatabaseHelper.LoginResultCode.GenderNeeded:
                    c.State =  LoginStateType.WaitingForDetail;
                    c.Send(LoginPacket.GenderNeeded(username));
                    break;
                case DatabaseHelper.LoginResultCode.Success:
                    c.State = LoginStateType.LoggedIn;
                    var userLogged = new List<int>(ServerSettings.ChannelCount);
                    MasterServer.Instance.ChannelServers.ForEach(x => userLogged.Add(x.UserLogged));
                    c.Send(LoginPacket.AuthSuccess(username, c.Account.Id, c.Account.Gender != null && c.Account.Gender.Value));
                    c.State =  LoginStateType.LoggedIn;
                    c.Send(LoginPacket.ServerList(userLogged.ToArray()));
                    c.Send(LoginPacket.ServerListEnd());
                    break;
                default:
                    c.Send(LoginPacket.AuthAccountFailed((int) result));
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
            var load = MasterServer.Instance.ChannelServers.Sum(cservs => cservs.ClientCount);

            if (MasterServer.Instance.LoginServer.Config.MaxConnectionNumber <= load)
            {
                c.Send(LoginPacket.ServerStatus(2));
            }
            else if (MasterServer.Instance.LoginServer.Config.MaxConnectionNumber*0.9 <= load)
            {
                c.Send(LoginPacket.ServerStatus(1));
            }
            else
            {
                c.Send(LoginPacket.ServerStatus(0));
            }
        }

        public static void CHARLIST_REQUEST(MapleClient c, InPacket p)
        {
            if (c.State ==  LoginStateType.LoggedIn)
            {
                c.WorldId = p.ReadByte();
                c.ChannelId = p.ReadByte();
                c.Send(LoginPacket.GetCharList(c));
            }
        }

        public static void CHECK_CHAR_NAME(MapleClient c, InPacket p)
        {
            var name = p.ReadMapleString();
            var nameused = DatabaseHelper.CheckNameUsed(c,name);
            c.Send(LoginPacket.CharNameResponse(name, nameused));
        }

        public static void CREATE_CHAR(MapleClient c, InPacket p)
        {
            var name = p.ReadMapleString();

            var job = p.ReadInt();
            var face = p.ReadInt();
            var hair = p.ReadInt();
            var hairColor = 0;
            byte skinColor;

            if (job == 0)
            {
                skinColor = 10;
            }
            else if (job == 2)
            {
                skinColor = 11;
            }
            else
            {
                skinColor = 0;
            }

            var top = p.ReadInt();
            var bottom = p.ReadInt();
            var shoes = p.ReadInt();
            var weapon = p.ReadInt();

            var newchar = new MapleCharacter();
            newchar.Create(c, job, top, bottom, shoes, weapon);

            if (c.Account.IsGm)
            {
                newchar.GmLevel = 1;
            }

            //newchar.WorldId = c.WorldId;
            newchar.Face = face;
            newchar.Hair = hair + hairColor;
            newchar.Gender = c.Account.Gender ?? false;

            newchar.Name = name;
            newchar.Skin = MapleSkinColor.GetByColorId(skinColor);

            

            newchar.Save();
            c.Send(LoginPacket.AddNewCharEntry(newchar, true));
        }

        public static void CHAR_SELECT(MapleClient c, InPacket p)
        {
            if (c.State !=  LoginStateType.LoggedIn)
                return;

            var charId = p.ReadInt();

            c.State =  LoginStateType.ServerTransition;
            c.Send(LoginPacket.GetServerIp(IPAddress.Parse("127.0.0.1"), 7575, charId));
        }

        public static void ERROR_LOG(MapleClient c, InPacket p)
        {
            Console.WriteLine($"错误信息：{p.ReadMapleString()}");
        }

        public static void PLAYER_UPDATE(MapleClient c, InPacket p)
        {
            c?.Player?.Save();
        }
    }
}