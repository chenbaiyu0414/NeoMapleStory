using System;
using System.Threading.Tasks;
using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public class MapleMapTimer
    {
        private readonly int m_duration;
        private DateTime m_predictedStopTime;
        private DateTime m_startTime;

        public MapleMapTimer(Task sfO, int newDuration, int mapToWarpToP, int minLevelToWarpP, int maxLevelToWarpP)
        {
            m_duration = newDuration;
            m_startTime = DateTime.Now;
            m_predictedStopTime = DateTime.Now.AddSeconds(m_duration);
            MapToWarpTo = mapToWarpToP;
            MinLevelToWarp = minLevelToWarpP;
            MaxLevelToWarp = maxLevelToWarpP;
            Sf0F = sfO;
        }

        public int MapToWarpTo { get; private set; } = -1;
        public int MinLevelToWarp { get; private set; }
        public int MaxLevelToWarp { get; private set; } = 256;
        public Task Sf0F { get; private set; }

        public OutPacket CreateSpawnData()
        {
            return PacketCreator.GetClock(GetTimeLeft());
        }

        public void SendSpawnData(MapleClient c)
        {
            c.Send(CreateSpawnData());
        }

        public int GetTimeLeft()
        {
            int timeLeft;
            var stopTimeStamp = m_predictedStopTime.GetTimeMilliseconds();
            var currentTimeStamp = DateTime.Now.GetTimeMilliseconds();
            timeLeft = (int) (stopTimeStamp - currentTimeStamp)/1000;
            return timeLeft;
        }
    }
}