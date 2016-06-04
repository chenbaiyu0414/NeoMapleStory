using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NeoMapleStory.Core;
using NeoMapleStory.Settings;
using SuperSocket.SocketBase.Config;

namespace NeoMapleStory.Server
{
    public sealed class MasterServer:IDisposable
    {
        public MasterServer()
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
                LoginServer = new LoginServer();
                LoginServer.Setup(loginConfig);

                for (var i = 0; i < ServerSettings.ChannelCount; i++)
                {
                    var channel = new ChannelServer(i);
                    channel.Setup("127.0.0.1", ServerSettings.ChannelPort + i);
                    ChannelServers.Add(channel);
                }

                using (var con = DbConnectionManager.Instance.GetConnection())
                {
                    con.Open();
                }
                Console.WriteLine("数据库连接成功");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"数据库连接出错!原因:{ex.Message}");
            }
        }

        public static MasterServer Instance { get; } = new MasterServer();
        public List<ChannelServer> ChannelServers { get; } = new List<ChannelServer>(ServerSettings.ChannelCount);
        public LoginServer LoginServer { get; }

        public void Dispose()
        {
            LoginServer.Dispose();
        }

        public bool Start()
        {
            var result = LoginServer.Start();
            ChannelServers.ForEach(x => { result = result && x.Start(); });
            return result;
        }

        public void Stop()
        {
            LoginServer.Stop();
            ChannelServers.ForEach(x => x.Stop());
            TimerManager.Instance.Stop();
        }

        
    }
}