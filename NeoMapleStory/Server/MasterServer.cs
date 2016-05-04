using System;
using NeoMapleStory.Settings;
using SuperSocket.SocketBase.Config;
using System.Collections.Generic;
using NeoMapleStory.Core;
using System.Data.SqlClient;
using Quartz;

namespace NeoMapleStory.Server
{

    public sealed class MasterServer
    {
        public static MasterServer Instance { get; } = new MasterServer();
        public List<ChannelServer> ChannelServers { get; private set; } = new List<ChannelServer>(ServerSettings.ChannelCount);
        public LoginServer LoginServer { get; private set; }


        public MasterServer()
        {
            try
            {
                var loginConfig = new ServerConfig
                {
                    Port = ServerSettings.ServerPort,
                    Ip = "Any",
                    MaxConnectionNumber = ServerSettings.BacklogLimit,
                    Name = "登录服务器",
                };
                LoginServer = new LoginServer();
                LoginServer.Setup(loginConfig);

                for (int i = 0; i < ServerSettings.ChannelCount; i++)
                {
                    var channel = new ChannelServer(i);
                    channel.Setup("Any", ServerSettings.ChannelPort + i);
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

        public bool Start()
        {
            bool result = LoginServer.Start();
            ChannelServers.ForEach(x => { result = result && x.Start(); });
            TimerManager.Instance.Start();
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
