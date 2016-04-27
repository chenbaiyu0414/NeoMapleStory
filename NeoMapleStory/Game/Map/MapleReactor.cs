using NeoMapleStory.Core.IO;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using System;
using System.Drawing;
using NeoMapleStory.Core;

namespace NeoMapleStory.Game.Map
{
     public class MapleReactor : AbstractMapleMapObject
    {
        public int ReactorId { get; private set; }
        public byte State { get; set; }
        public int Delay { get; set; }
        public MapleMap Map { get; set; }
        public string ReactorName { get; set; }
        public bool IsTimerActive { get; set; }
        public bool IsAlive { get; set; }

        private readonly MapleReactorStats _stats;

        public MapleReactor(MapleReactorStats stats, int rid)
        {
            _stats = stats;
            ReactorId = rid;
            IsAlive = true;
        }


        public int ReactorType => _stats.GetType(State);

        public Tuple<int, int> ReactItem => _stats.GetReactItem(State);

        public Rectangle Area => new Rectangle(Position.X + _stats.Tl.X, Position.Y + _stats.Tl.Y, _stats.Br.X - _stats.Tl.X, _stats.Br.Y - _stats.Tl.Y);

        public OutPacket MakeDestroyData() => PacketCreator.DestroyReactor(this);


        public OutPacket MakeSpawnData() => PacketCreator.SpawnReactor(this);


        public void DelayedHitReactor(MapleClient c, int delay)
        {
            TimerManager.Instance.ScheduleJob(() =>
             {
                 HitReactor(c);
             }, delay);
        }

        public void HitReactor(MapleClient c)
        {
            HitReactor(0, 0, c);
        }

        public void HitReactor(int charPos, short stance, MapleClient c)
        {
            if (_stats.GetType(State) < 999 && _stats.GetType(State) != -1)
            {
                //type 2 = only hit from right (kerning swamp plants), 00 is air left 02 is ground left
                if (!(_stats.GetType(State) == 2 && (charPos == 0 || charPos == 2)))
                {
                    //get next state
                    State = _stats.GetNextState(State);

                    if (_stats.GetNextState(State) == 128)
                    {
                        //end of reactor
                        if (_stats.GetType(State) < 100)
                        { 
                            //reactor broken
                            if (Delay > 0)
                            {
                                Map.DestroyReactor(ObjectId);
                            }
                            else {//trigger as normal
                                Map.BroadcastMessage(PacketCreator.TriggerReactor(this, stance));
                            }
                        }
                        else { //item-triggered on final step
                            Map.BroadcastMessage(PacketCreator.TriggerReactor(this, stance));
                        }
                       // ReactorScriptManager.getInstance().act(c, this);
                    }
                    else { //reactor not broken yet
                        Map.BroadcastMessage(PacketCreator.TriggerReactor(this, stance));
                        if (State == _stats.GetNextState(State))
                        { //current state = next state, looping reactor
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
