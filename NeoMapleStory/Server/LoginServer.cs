using System;
using NeoMapleStory.Packet;

namespace NeoMapleStory.Server
{
    public sealed class LoginServer : BaseServer
    {
        protected override void OnNewClientConnected(MapleClient client)
        {
            Console.WriteLine(
                $"玩家 {client.SocketSession.RemoteEndPoint.Address}:{client.SocketSession.RemoteEndPoint.Port} 已连接");
            client.SendRaw(PacketCreator.Handshake(client.SendIv, client.RecvIv));
        }

        protected override void OnPacketHandlers()
        {
            Processor = new PacketProcessor("登录服务器");

            Processor.AppendHandler(RecvOpcodes.LoginPassword, LoginPacketHandlers.LOGIN_AUTH);
            Processor.AppendHandler(RecvOpcodes.SetGender, LoginPacketHandlers.OnGENDER_RESULT);
            Processor.AppendHandler(RecvOpcodes.LicenseRequest, LoginPacketHandlers.OnLISENCE_RESULT);
            Processor.AppendHandler(RecvOpcodes.ServerstatusRequest, LoginPacketHandlers.SERVERSTATUS_REQUEST);
            Processor.AppendHandler(RecvOpcodes.ServerlistRequest, LoginPacketHandlers.SERVER_LIST_REQUEST);
            Processor.AppendHandler(RecvOpcodes.ServerlistRerequest, LoginPacketHandlers.SERVER_LIST_REQUEST);
            Processor.AppendHandler(RecvOpcodes.CharlistRequest, LoginPacketHandlers.CHARLIST_REQUEST);
            Processor.AppendHandler(RecvOpcodes.CheckCharName, LoginPacketHandlers.CHECK_CHAR_NAME);
            Processor.AppendHandler(RecvOpcodes.CreateChar, LoginPacketHandlers.CREATE_CHAR);
            Processor.AppendHandler(RecvOpcodes.CharSelect, LoginPacketHandlers.CHAR_SELECT);
            //m_processor.AppendHandler(RecvOpcodes.RELOG, LoginPacketHandlers.RELOG);
            Processor.AppendHandler(RecvOpcodes.ErrorLog, LoginPacketHandlers.ERROR_LOG);
            Processor.AppendHandler(RecvOpcodes.PlayerUpdate, LoginPacketHandlers.PLAYER_UPDATE);
        }

        public override bool Start()
        {
            Console.WriteLine($"正在启动 {Processor.Label} 监听端口: {Config.Port}");
            var result = base.Start();
            Console.WriteLine(result ? $"{Processor.Label} 启动成功" : $"{Processor.Label} 启动失败");
            return result;
        }

        public override void Stop()
        {
            Console.WriteLine($"正在停止 {Processor.Label}");
            base.Stop();
            Console.WriteLine($"{Processor.Label}已停止");
        }
    }
}