using System;
using System.Drawing;
using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Map
{
    public class MapleReactor : AbstractMapleMapObject
    {
        private readonly MapleReactorStats m_stats;

        public MapleReactor(MapleReactorStats stats, int rid)
        {
            m_stats = stats;
            ReactorId = rid;
            IsAlive = true;
        }

        public int ReactorId { get; private set; }
        public byte State { get; set; }
        public int Delay { get; set; }
        public MapleMap Map { get; set; }
        public string ReactorName { get; set; }
        public bool IsTimerActive { get; set; }
        public bool IsAlive { get; set; }


        public int ReactorType => m_stats.GetType(State);

        public Tuple<int, int> ReactorItem => m_stats.GetReactItem(State);

        public Rectangle Area
            =>
                new Rectangle(Position.X + m_stats.Tl.X, Position.Y + m_stats.Tl.Y, m_stats.Br.X - m_stats.Tl.X,
                    m_stats.Br.Y - m_stats.Tl.Y);

        public OutPacket MakeDestroyData() => PacketCreator.DestroyReactor(this);


        public OutPacket MakeSpawnData() => PacketCreator.SpawnReactor(this);


        public void DelayedHitReactor(MapleClient c, int delay)
        {
            TimerManager.Instance.RunOnceTask(() => { HitReactor(c); }, delay);
        }

        public void HitReactor(MapleClient c)
        {
            HitReactor(0, 0, c);
        }

        public void HitReactor(int charPos, short stance, MapleClient c)
        {
            if (m_stats.GetType(State) < 999 && m_stats.GetType(State) != -1)
            {
                //type 2 = only hit from right (kerning swamp plants), 00 is air left 02 is ground left
                if (!(m_stats.GetType(State) == 2 && (charPos == 0 || charPos == 2)))
                {
                    //get next state
                    State = m_stats.GetNextState(State);

                    if (m_stats.GetNextState(State) == 0xFF)
                    {
                        //end of reactor
                        if (m_stats.GetType(State) < 100)
                        {
                            //reactor broken
                            if (Delay > 0)
                            {
                                Map.DestroyReactor(ObjectId);
                            }
                            else
                            {
//trigger as normal
                                Map.BroadcastMessage(PacketCreator.TriggerReactor(this, stance));
                            }
                        }
                        else
                        {
                            //item-triggered on final step
                            Map.BroadcastMessage(PacketCreator.TriggerReactor(this, stance));
                        }
                        // ReactorScriptManager.getInstance().act(c, this);
                    }
                    else
                    {
                        //reactor not broken yet
                        Map.BroadcastMessage(PacketCreator.TriggerReactor(this, stance));
                        if (State == m_stats.GetNextState(State))
                        {
                            //current state = next state, looping reactor
                            //  ReactorScriptManager.getInstance().act(c, this);
                        }
                    }
                }
            }
            else
            {
                State++;
                Map.BroadcastMessage(PacketCreator.TriggerReactor(this, stance));
                // ReactorScriptManager.getInstance().act(c, this);
            }
        }


        public override MapleMapObjectType GetType() => MapleMapObjectType.Reactor;

        public override void SendDestroyData(MapleClient client)
        {
            client.Send(MakeDestroyData());
        }

        public override void SendSpawnData(MapleClient client)
        {
            client.Send(MakeSpawnData());
        }
    }
}