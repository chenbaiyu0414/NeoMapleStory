using System;
using System.Collections.Generic;
using System.Text;
using NeoMapleStory.Core;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Client
{
    public class AutobanManager
    {
        private static readonly int AutobanPoints = 1000;
        private readonly List<ExpirationEntry> m_mExpirations = new List<ExpirationEntry>();

        private readonly Dictionary<int, int> m_mPoints = new Dictionary<int, int>();
        private readonly Dictionary<int, List<string>> m_mReasons = new Dictionary<int, List<string>>();

        public static AutobanManager Instance { get; } = new AutobanManager();

        public void Autoban(MapleClient c, string reason)
        {
            if (c.Player.GmLevel > 0)
            {
                return;
            }
            AddPoints(c, AutobanPoints, 0, reason);
        }

        public void BroadcastMessage(MapleClient c)
        {
            //c.ChannelServer.getWorldInterface().broadcastGMMessage(null, PacketCreator.ServerNotice(PacketCreator.ServerMessageType.LightBlueText, $"{c.Character.CharacterName } 已被永久封号.").getBytes());
        }

        public void BroadcastMessage(MapleClient c, string s)
        {
            //c.ChannelServer.getWorldInterface().broadcastMessage(null, PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Notice, s).getBytes());
        }

        public void AddPoints(MapleClient c, int points, long expiration, string reason)
        {
            if (c.Player.GmLevel > 0)
            {
                return;
            }
            var acc = c.Player.AccountId;
            List<string> reasonList;
            if (m_mPoints.ContainsKey(acc))
            {
                if (m_mPoints[acc] >= AutobanPoints)
                {
                    return;
                }
                m_mPoints.Add(acc, m_mPoints[acc + points]);
                reasonList = m_mReasons[acc];
                reasonList.Add(reason);
            }
            else
            {
                m_mPoints.Add(acc, points);
                reasonList = new List<string>();
                reasonList.Add(reason);
                m_mReasons.Add(acc, reasonList);
            }
            if (m_mPoints[acc] >= AutobanPoints)
            {
                var name = c.Player.Name;
                var banReason = new StringBuilder("Autoban for Character ");
                banReason.Append(name);
                banReason.Append(" (IP ");
                banReason.Append(c.SocketSession.RemoteEndPoint.Address);
                banReason.Append("): ");
                foreach (var s in m_mReasons[acc])
                {
                    banReason.Append(s);
                    banReason.Append(", ");
                }
                if (c.Player.GmLevel == 0)
                {
                    //c.ChannelServer.getWorldInterface().broadcastGMMessage(null, PacketCreator.ServerNotice( PacketCreator.ServerMessageType.LightBlueText, $"[系统警告] { name } 检测到使用非法程序.如继续使用将封号处理!").getBytes());
                }
                return;
            }
            if (expiration > 0)
            {
                m_mExpirations.Add(new ExpirationEntry(DateTime.Now.GetTimeMilliseconds() + expiration, acc, points));
            }
        }

        public void Run()
        {
            var now = DateTime.Now.GetTimeMilliseconds();
            foreach (var e in m_mExpirations)
            {
                if (e.MTime <= now)
                {
                    m_mPoints.Add(e.MAcc, m_mPoints[e.MAcc] - e.MPoints);
                }
                else
                {
                    return;
                }
            }
        }

        private class ExpirationEntry : IComparable<ExpirationEntry>
        {
            public readonly int MAcc;
            public readonly int MPoints;

            public readonly long MTime;

            public ExpirationEntry(long time, int acc, int points)
            {
                MTime = time;
                MAcc = acc;
                MPoints = points;
            }

            public int CompareTo(ExpirationEntry other)
            {
                return (int) (MTime - other.MTime);
            }
        }
    }
}