using NeoMapleStory.Core;
using NeoMapleStory.Game.Mob;
using System;
using System.Drawing;

namespace NeoMapleStory.Game.Map
{
    public class SpawnPoint
    {
        public Point Pos { get; private set; }

        private MapleMonster _monster;      
        private  long _nextPossibleSpawn;
        private int _mobTime;
        private InterLockedInt _spawnedMonsters = new InterLockedInt(0);
        private  bool _immobile;

        public SpawnPoint(MapleMonster monster, Point pos, int mobTime)
        {
            this._monster = monster;
            this.Pos = pos;
            this._mobTime = mobTime;
            _immobile = !monster.Stats.IsMobile;
            _nextPossibleSpawn = DateTime.Now.GetTimeMilliseconds();
        }

        public bool ShouldSpawn()
        {
            return ShouldSpawn(DateTime.Now.GetTimeMilliseconds());
        }

        private bool ShouldSpawn(long now)
        {
            if (_mobTime < 0)
            {
                return false;
            }
            if (((_mobTime != 0 || _immobile) && _spawnedMonsters.Value > 0) || _spawnedMonsters.Value > 2)
            {
                return false;
            }
            return _nextPossibleSpawn <= now;
        }

        public MapleMonster spawnMonster(MapleMap mapleMap)
        {
            MapleMonster mob = new MapleMonster(_monster);
            mob.Position = Pos;
            _spawnedMonsters.Increment();

            mob.listeners.Add((kak,args) =>
            {
                _nextPossibleSpawn = DateTime.Now.GetTimeMilliseconds();
                if (_mobTime > 0)
                {
                    _nextPossibleSpawn += _mobTime*1000;
                }
                else
                {
                    _nextPossibleSpawn +=args.monster.Stats.GetAnimationTime("die1");
                }
                _spawnedMonsters.Decrement();
            });

            mapleMap.spawnMonster(mob);
            if (_mobTime == 0) {
                _nextPossibleSpawn = DateTime.Now.GetTimeMilliseconds() + 5000;
            }
            return mob;
        }
    }
}
