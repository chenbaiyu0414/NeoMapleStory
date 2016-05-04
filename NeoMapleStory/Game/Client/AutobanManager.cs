using NeoMapleStory.Core;
using NeoMapleStory.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoMapleStory.Game.Client
{
     public class AutobanManager
    {
        private class ExpirationEntry : IComparable<ExpirationEntry>
        {

            public readonly long MTime;
            public readonly int MAcc;
            public readonly int MPoints;

            public ExpirationEntry(long time, int acc, int points)
            {
                MTime = time;
                MAcc = acc;
                MPoints = points;
            }

            public int CompareTo(ExpirationEntry other)
            {
                return (int)(MTime - other.MTime);
            }
        }

        public static AutobanManager Instance { get; } = new AutobanManager();

        private readonly Dictionary<int, int> _mPoints = new Dictionary<int, int>();
        private readonly Dictionary<int, List<string>> _mReasons = new Dictionary<int, List<string>>();
        private readonly List<ExpirationEntry> _mExpirations = new List<ExpirationEntry>();
        private static readonly int AutobanPoints = 1000;

        public void Autoban(MapleClient c, string reason)
        {
            if (c.Player.GmLevel > 0)
            {
                return;
            }
            AddPoints(c, AutobanPoints, 0, reason);
        }

        public void broadcastMessage(MapleClient c)
        {
            //c.ChannelServer.getWorldInterface().broadcastGMMessage(null, PacketCreator.ServerNotice(PacketCreator.ServerMessageType.LightBlueText, $"{c.Character.CharacterName } 已被永久封号.").getBytes());

        }

        public void broadcastMessage(MapleClient c, string s)
        {
            //c.ChannelServer.getWorldInterface().broadcastMessage(null, PacketCreator.ServerNotice(PacketCreator.ServerMessageType.Notice, s).getBytes());
        }

        public void AddPoints(MapleClient c, int points, long expiration, string reason)
        {
            if (c.Player.GmLevel > 0)
            {
                return;
            }
            int acc = c.Player.AccountId;
            List<string> reasonList;
            if (_mPoints.ContainsKey(acc))
            {
                if (_mPoints[acc] >= AutobanPoints)
                {
                    return;
                }
                _mPoints.Add(acc, _mPoints[acc + points]);
                reasonList = _mReasons[acc];
                reasonList.Add(reason);
            }
            else {
                _mPoints.Add(acc, points);
                reasonList = new List<string>();
                reasonList.Add(reason);
                _mReasons.Add(acc, reasonList);
            }
            if (_mPoints[acc] >= AutobanPoints)
            {
                string name = c.Player.Name;
                StringBuilder banReason = new StringBuilder("Autoban for Character ");
                banReason.Append(name);
                banReason.Append(" (IP ");
                banReason.Append(c.SocketSession.RemoteEndPoint.Address.ToString());
                banReason.Append("): ");
                foreach (string s in _mReasons[acc])
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
                _mExpirations.Add(new ExpirationEntry(DateTime.Now.GetTimeMilliseconds() + expiration, acc, points));
            }
        }

        public void Run()
        {
            long now = DateTime.Now.GetTimeMilliseconds();
            foreach (ExpirationEntry e in _mExpirations)
            {
                if (e.MTime <= now)
                {
                    _mPoints.Add(e.MAcc, _mPoints[e.MAcc] - e.MPoints);
                }
                else {
                    return;
                }
            }
        }
    }
}
