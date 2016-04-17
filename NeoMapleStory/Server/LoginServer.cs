using NeoMapleStory.Packet;
using System;

namespace NeoMapleStory.Server
{
    
    public sealed class LoginServer : BaseServer
    {

        protected override void OnNewClientConnected(MapleClient client)
        {
            Console.WriteLine($"玩家 {client.SocketSession.RemoteEndPoint.Address.ToString()}:{client.SocketSession.RemoteEndPoint.Port} 已连接");
            client.SendRaw(PacketCreator.Handshake(client.SendIv, client.RecvIv));
        }

        protected override void OnPacketHandlers()
        {
            MProcessor = new PacketProcessor("登录服务器");

            MProcessor.AppendHandler(RecvOpcodes.LoginPassword, LoginPacketHandlers.LOGIN_AUTH);
            MProcessor.AppendHandler(RecvOpcodes.SetGender, LoginPacketHandlers.OnGENDER_RESULT);
            MProcessor.AppendHandler(RecvOpcodes.LicenseRequest, LoginPacketHandlers.OnLISENCE_RESULT);
            MProcessor.AppendHandler(RecvOpcodes.ServerstatusRequest, LoginPacketHandlers.SERVERSTATUS_REQUEST);
            MProcessor.AppendHandler(RecvOpcodes.ServerlistRequest, LoginPacketHandlers.SERVER_LIST_REQUEST);
            MProcessor.AppendHandler(RecvOpcodes.ServerlistRerequest, LoginPacketHandlers.SERVER_LIST_REQUEST);
            MProcessor.AppendHandler(RecvOpcodes.CharlistRequest, LoginPacketHandlers.CHARLIST_REQUEST);
            MProcessor.AppendHandler(RecvOpcodes.CheckCharName, LoginPacketHandlers.CHECK_CHAR_NAME);
            MProcessor.AppendHandler(RecvOpcodes.CreateChar, LoginPacketHandlers.CREATE_CHAR);
            MProcessor.AppendHandler(RecvOpcodes.CharSelect, LoginPacketHandlers.CHAR_SELECT);
            //m_processor.AppendHandler(RecvOpcodes.RELOG, LoginPacketHandlers.RELOG);
            MProcessor.AppendHandler(RecvOpcodes.ErrorLog, LoginPacketHandlers.ERROR_LOG);

        }

        public override bool Start()
        {
            Console.WriteLine($"正在启动 {MProcessor.Label} 监听端口: {Config.Port}");
            bool result = base.Start();
            if (result)
                Console.WriteLine($"{MProcessor.Label} 启动成功");
            else
                Console.WriteLine($"{MProcessor.Label} 启动失败");
            return result;
        }
        public override void Stop()
        {
            Console.WriteLine($"正在停止 {MProcessor.Label}");
            base.Stop();
            Console.WriteLine($"{MProcessor.Label}已停止");
        }
    }
}
