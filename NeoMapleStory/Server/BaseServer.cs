#define Debug
using System;
using System.Collections.Generic;
using System.Linq;
using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace NeoMapleStory.Server
{
    public abstract class BaseServer : AppServer<MapleClient, PacketRequestInfo>
    {
        protected PacketProcessor Processor;

        protected BaseServer()
            : base(new DefaultReceiveFilterFactory<PacketReceiveFilter, PacketRequestInfo>())
        {
            NewSessionConnected += OnNewClientConnected;
            NewRequestReceived += OnNewRequestReceived;
            OnPacketHandlers();
        }

        public List<MapleClient> Clients => GetAllSessions().ToList();

        public int ClientCount => SessionCount;

        protected abstract void OnNewClientConnected(MapleClient client);
        protected abstract void OnPacketHandlers();

        public void OnNewRequestReceived(MapleClient client, PacketRequestInfo packetInfo)
        {
            if (!client.RecvCipher.CheckPacket(packetInfo.Header))
                throw new Exception("封包不正确");

            var data = new byte[packetInfo.Data.Length];
            Buffer.BlockCopy(packetInfo.Data, 0, data, 0, data.Length);
            client.RecvCipher.Transform(data);

            var p = new InPacket(data);
            var handler = Processor[p.ReadShort()];

            if (handler != null)
                handler(client, p);
            else
            {
                Console.WriteLine($"未知封包 长度：{data.Length} 封包：{BitTool.GetHexStr(data)}");
            }
        }

        protected override void OnSessionClosed(MapleClient session, CloseReason reason)
        {
            lock (session)
            {
                session.Disconnect();
            }

            base.OnSessionClosed(session, reason);
        }
    }
}