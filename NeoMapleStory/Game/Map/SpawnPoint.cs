using System;
using System.Drawing;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Mob;

namespace NeoMapleStory.Game.Map
{
    public class SpawnPoint
    {
        private readonly bool m_immobile;
        private readonly int m_mobTime;

        private readonly MapleMonster m_monster;
        private long m_nextPossibleSpawn;
        private readonly InterLockedInt m_spawnedMonsters = new InterLockedInt(0);

        public SpawnPoint(MapleMonster monster, Point pos, int mobTime)
        {
            m_monster = monster;
            Pos = pos;
            m_mobTime = mobTime;
            m_immobile = !monster.Stats.IsMobile;
            m_nextPossibleSpawn = DateTime.Now.GetTimeMilliseconds();
        }

        public Point Pos { get; }

        public bool ShouldSpawn()
        {
            return ShouldSpawn(DateTime.Now.GetTimeMilliseconds());
        }

        private bool ShouldSpawn(long now)
        {
            if (m_mobTime < 0)
            {
                return false;
            }
            if (((m_mobTime != 0 || m_immobile) && m_spawnedMonsters.Value > 0) || m_spawnedMonsters.Value > 2)
            {
                return false;
            }
            return m_nextPossibleSpawn <= now;
        }

        public MapleMonster SpawnMonster(MapleMap mapleMap)
        {
            var mob = new MapleMonster(m_monster) {Position = Pos};
            m_spawnedMonsters.Increment();

            mob.Listeners.Add((kak, args) =>
            {
                m_nextPossibleSpawn = DateTime.Now.GetTimeMilliseconds();
                if (m_mobTime > 0)
                {
                    m_nextPossibleSpawn += m_mobTime*1000;
                }
                else
                {
                    m_nextPossibleSpawn += args.Monster.Stats.GetAnimationTime("die1");
                }
                m_spawnedMonsters.Decrement();
            });

            mapleMap.SpawnMonster(mob);
            if (m_mobTime == 0)
            {
                m_nextPossibleSpawn = DateTime.Now.GetTimeMilliseconds() + 5000;
            }
            return mob;
        }
    }
}