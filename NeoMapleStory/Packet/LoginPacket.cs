using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NeoMapleStory.Core;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Server;
using NeoMapleStory.Settings;

namespace NeoMapleStory.Packet
{
    public static class LoginPacket
    {
        public static OutPacket GenderNeeded(string username)
        {
            using (var packet = new OutPacket(SendOpcodes.ChooseGender))
            {
                packet.WriteMapleString(username);
                return packet;
            }
        }

        public static OutPacket GenderChanged(string username, string accountId)
        {
            using (var packet = new OutPacket(SendOpcodes.GenderSet))
            {
                packet.WriteByte(0x00);
                packet.WriteMapleString(username);
                packet.WriteMapleString(accountId);
                return packet;
            }
        }

        public static OutPacket LicenseRequest()
        {
            using (var packet = new OutPacket(SendOpcodes.LoginStatus))
            {
                packet.WriteByte(0x16);
                return packet;
            }
        }

        public static OutPacket LicenseResult()
        {
            using (var packet = new OutPacket(SendOpcodes.LicenseResult))
            {
                packet.WriteByte(0x01);
                return packet;
            }
        }

        public static OutPacket AuthSuccess(string username, int accountId, bool gender)
        {
            using (var packet = new OutPacket(SendOpcodes.LoginStatus))
            {
                packet.WriteByte(0x00);
                packet.WriteInt(accountId);
                packet.WriteBool(gender);
                packet.WriteShort(0);
                packet.WriteMapleString(username);
                packet.WriteBytes(new byte[]
                {0x00, 0x00, 0x00, 0x03, 0x01, 0x00, 0x00, 0x00, 0xE2, 0xED, 0xA3, 0x7A, 0xFA, 0xC9, 0x01});
                packet.WriteInt(0);
                packet.WriteLong(0);
                packet.WriteMapleString(accountId.ToString());
                packet.WriteMapleString(username);
                packet.WriteByte(0x01);
                return packet;
            }
        }

        public static OutPacket AuthAccountFailed(int reasonCode)
        {
            using (var packet = new OutPacket(SendOpcodes.LoginStatus))
            {
                packet.WriteInt(reasonCode);
                packet.WriteShort(0);
                return packet;
            }
        }

        public static OutPacket ReLogResponse()
        {
            using (var packet = new OutPacket(SendOpcodes.RelogResponse))
            {
                packet.WriteInt(0x01);
                return packet;
            }
        }

        public static OutPacket ServerList(int[] channelUserLogged)
        {
            using (var packet = new OutPacket(SendOpcodes.Serverlist))
            {
                packet.WriteByte(0); //serverid
                packet.WriteMapleString(ServerSettings.ServerName);
                packet.WriteByte(0x03); //0: 正常 1: 火爆 2: 热 3: 新开
                packet.WriteMapleString(ServerSettings.ServerName); //eventmsg
                packet.WriteByte(0x64);
                packet.WriteByte(0x00);
                packet.WriteByte(0x64);
                packet.WriteByte(0x00);

                packet.WriteByte(ServerSettings.ChannelCount);
                packet.WriteInt(500);

                for (var i = 0; i < ServerSettings.ChannelCount; i++)
                {
                    var load = channelUserLogged[i];
                    packet.WriteMapleString(ServerSettings.ServerName + "-" + (i + 1));
                    packet.WriteInt(load);
                    packet.WriteByte(0x00); //serverid
                    packet.WriteShort((short) i);
                }
                packet.WriteShort(0);

                return packet;
            }
        }

        public static OutPacket ServerListEnd()
        {
            using (var packet = new OutPacket(SendOpcodes.Serverlist))
            {
                packet.WriteByte(0xFF);
                return packet;
            }
        }

        public static OutPacket ServerStatus(byte status)
        {
            using (var packet = new OutPacket(SendOpcodes.Serverstatus))
            {
                packet.WriteByte(status);
                return packet;
            }
        }

        public static OutPacket GetCharList(MapleClient mc)
        {
            using (var packet = new OutPacket(SendOpcodes.Charlist))
            {
                packet.WriteByte(0x00);
                packet.WriteInt(0);
                var chars = DatabaseHelper.LoadCharacters(mc);
                packet.WriteByte((byte) chars.Count);
                foreach (var chr in chars)
                {
                    AddCharEntry(packet, chr);
                }
                packet.WriteShort(3);
                packet.WriteInt(ServerSettings.MaxCharacterCouldCreate);
                return packet;
            }
        }

        public static OutPacket CharNameResponse(string name, bool nameUsed)
        {
            using (var packet = new OutPacket(SendOpcodes.CharNameResponse))
            {
                packet.WriteMapleString(name);
                packet.WriteBool(nameUsed);
                return packet;
            }
        }

        public static OutPacket AddNewCharEntry(MapleCharacter chr, bool worked)
        {
            using (var packet = new OutPacket(SendOpcodes.AddNewCharEntry))
            {
                packet.WriteBool(!worked);
                AddCharEntry(packet, chr);
                return packet;
            }
        }

        private static void AddCharEntry(OutPacket p, MapleCharacter chr)
        {
            AddCharStats(p, chr);
            AddCharLook(p, chr, false);
            p.WriteByte(0x00);
            if (chr.Job == MapleJob.Gm)
            {
                p.WriteByte(0x02);
            }
        }

        public static void AddCharStats(OutPacket p, MapleCharacter chr)
        {
            p.WriteInt(chr.Id); // character id
            p.WriteString(chr.Name);
            // 填充名字字符
            // p.WriteZero(13 - Encoding.Default.GetByteCount(chr.CharacterName));
            for (var x = Encoding.Default.GetByteCount(chr.Name); x < 13; x++)
            {
                // fill to maximum name length
                p.WriteByte(0);
            }

            p.WriteBool(chr.Gender); // gender (0 = male, 1 = female)
            p.WriteByte(chr.Skin.ColorId); // skin color
            p.WriteInt(chr.Face); // face
            p.WriteInt(chr.Hair); // hair
            p.WriteLong(0);
            p.WriteLong(0);
            p.WriteLong(0);
            p.WriteByte(chr.Level); // level
            p.WriteShort(chr.Job.JobId); // job
            p.WriteShort(chr.Str); // str
            p.WriteShort(chr.Dex); // dex
            p.WriteShort(chr.Int); // int
            p.WriteShort(chr.Luk); // luk
            p.WriteShort(chr.Hp); // hp (?)
            p.WriteShort(chr.MaxHp); // maxhp
            p.WriteShort(chr.Mp); // mp (?)
            p.WriteShort(chr.MaxMp); // maxmp
            p.WriteShort(chr.RemainingAp); // remaining ap
            p.WriteShort(chr.RemainingSp); // remaining sp
            p.WriteInt(chr.Exp.Value); // current exp
            p.WriteShort(chr.Fame); // fame
            p.WriteInt(0);
            p.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
            p.WriteInt(chr.Map?.MapId ?? 10000); // current map id
            p.WriteByte(chr.InitialSpawnPoint); // spawnpoint
        }


        public static void AddCharLook(OutPacket p, MapleCharacter chr, bool mega)
        {
            p.WriteBool(chr.Gender);
            p.WriteByte(chr.Skin.ColorId); // skin color
            p.WriteInt(chr.Face); // face
            p.WriteBool(!mega);
            p.WriteInt(chr.Hair); // hair

            var equip = chr.Inventorys[MapleInventoryType.Equipped.Value];
            var myEquip = new Dictionary<byte, int>();
            var maskedEquip = new Dictionary<byte, int>();

            lock (equip)
            {
                foreach (var item in equip.Inventory.Values)
                {
                    var pos = item.Position;
                    if (pos < 100 && !myEquip.ContainsKey(pos))
                    {
                        myEquip.Add(pos, item.ItemId);
                    }
                    else if ((pos > 100 || pos == 128) && pos != 111)
                    {
                        pos -= 100;
                        if (myEquip.ContainsKey(pos))
                        {
                            maskedEquip.Add(pos, myEquip[pos]);
                        }
                        myEquip.Add(pos, item.ItemId);
                    }
                    else if (myEquip.ContainsKey(pos))
                    {
                        maskedEquip.Add(pos, item.ItemId);
                    }
                }

                foreach (var entry in myEquip)
                {
                    p.WriteByte(entry.Key);
                    p.WriteInt(entry.Value);
                }
                p.WriteByte(0xFF);

                foreach (var entry in maskedEquip)
                {
                    p.WriteByte(entry.Key);
                    p.WriteInt(entry.Value);
                }
                p.WriteByte(0xFF);

                IMapleItem cWeapon;
                p.WriteInt(equip.Inventory.TryGetValue(111, out cWeapon) ? cWeapon.ItemId : 0);
            }
            p.WriteInt(0);
            p.WriteLong(0);
        }

        public static OutPacket GetServerIp(IPAddress address, short port, int clientId)
        {
            using (var packet = new OutPacket(SendOpcodes.ServerIp))
            {
                packet.WriteShort(0);
                packet.WriteBytes(address.GetAddressBytes());
                packet.WriteShort(port);
                packet.WriteInt(clientId);
                packet.WriteBytes(new byte[] {0x01, 0x00, 0x00, 0x00, 0x00});
                return packet;
            }
        }
    }
}