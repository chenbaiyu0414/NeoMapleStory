using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Server;
using System;
using System.Threading.Tasks;
using NeoMapleStory.Packet;

namespace NeoMapleStory.Game.Map
{
     public class MapleMapTimer
    {
        private readonly int _duration;
        private DateTime _startTime;
        private DateTime _predictedStopTime;
        public int MapToWarpTo { get; private set; } = -1;
        public int MinLevelToWarp { get; private set; }
        public int MaxLevelToWarp { get; private set; } = 256;
        public Task Sf0F { get; private set; }

        public MapleMapTimer(Task sfO, int newDuration, int mapToWarpToP, int minLevelToWarpP, int maxLevelToWarpP)
        {
            _duration = newDuration;
            _startTime = DateTime.Now;
            _predictedStopTime = DateTime.Now.AddSeconds(_duration);
            MapToWarpTo = mapToWarpToP;
            MinLevelToWarp = minLevelToWarpP;
            MaxLevelToWarp = maxLevelToWarpP;
            Sf0F = sfO;
        }

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
            long stopTimeStamp = _predictedStopTime.GetTimeMilliseconds();
            long currentTimeStamp = DateTime.Now.GetTimeMilliseconds();
            timeLeft = (int)(stopTimeStamp - currentTimeStamp) / 1000;
            return timeLeft;
        }
    }
}
