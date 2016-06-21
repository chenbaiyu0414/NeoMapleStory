using System;
using System.Collections.Generic;
using NeoMapleStory.Core;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Settings;
using SuperSocket.SocketBase.Config;

namespace NeoMapleStory.Server
{
    public sealed class MasterServer:IDisposable
    {
        public static MasterServer Instance { get; } = new MasterServer();
        public List<ChannelServer> ChannelServers { get; } = new List<ChannelServer>(ServerSettings.ChannelCount);
        public LoginServer LoginServer { get; } = new LoginServer();
        private bool isLaunched = false;

        public void Dispose()
        {
            LoginServer.Dispose();
        }

        public bool Start()
        {
            try
            {
                var loginConfig = new ServerConfig
                {
                    Port = ServerSettings.ServerPort,
                    Ip = "127.0.0.1",
                    MaxConnectionNumber = ServerSettings.BacklogLimit,
                    Name = "登录服务器",
                    IdleSessionTimeOut = 30
                };
                LoginServer.Setup(loginConfig);

                for (var i = 0; i < ServerSettings.ChannelCount; i++)
                {
                    var channel = new ChannelServer(i);
                    channel.Setup("127.0.0.1", ServerSettings.ChannelPort + i);
                    ChannelServers.Add(channel);
                }

                bool result = LoginServer.Start();
                ChannelServers.ForEach(x => { result = result && x.Start(); });
                isLaunched = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public void Stop()
        {
            if (isLaunched)
            {
                LoginServer.Stop();
                ChannelServers.ForEach(x => x.Stop());
                TimerManager.Instance.Stop();
            }      
        }

        
    }
}