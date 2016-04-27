using NeoMapleStory.Core.Database;
using NeoMapleStory.Core.Encryption;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Settings;
using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NeoMapleStory.Server
{

    public sealed class MapleClient : AppSession<MapleClient, PacketRequestInfo>
    {
        public MapleCipher SendCipher { get; private set; }
        public MapleCipher RecvCipher { get; private set; }

        public byte[] SendIv { get; private set; } = new byte[4];
        public byte[] RecvIv { get; private set; } = new byte[4];

        public Account Account { get; set; }
        public byte WorldId { get; set; }
        public byte ChannelId { get; set; }
        public ChannelServer ChannelServer => MasterServer.Instance.ChannelServers[ChannelId];


        public List<MapleCharacter> Characters { get; set; } = new List<MapleCharacter>();
        public MapleCharacter Character { get; set; }

 

        public MapleClient()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(SendIv);
                rng.GetNonZeroBytes(RecvIv);
            }

            unchecked
            {
                SendCipher = new MapleCipher((short) (0xFFFF - ServerSettings.MapleVersion), SendIv,
                    MapleCipher.CipherType.Encrypt);
                RecvCipher = new MapleCipher(ServerSettings.MapleVersion, RecvIv, MapleCipher.CipherType.Decrypt);
            }
        }

        #region 发送封包
        public void Send(OutPacket packet)
        {
            Send(packet.ToArray());
        }

        private void Send(byte[] packetData)
        {
            var headerData = SendCipher.GetPacketHeader(packetData.Length);
            //Console.WriteLine($"发送封包：{BitTool.GetHexStr(packetData)}");
            SendCipher.Transform(packetData);

            var finalData = new byte[packetData.Length + headerData.Length];
            Buffer.BlockCopy(headerData, 0, finalData, 0, headerData.Length);
            Buffer.BlockCopy(packetData, 0, finalData, 4, packetData.Length);

            SendRaw(finalData);
        }

        public void SendRaw(byte[] data)
        {
            Send(data, 0, data.Length);
        }
        #endregion



    }
}
