using System.Collections.Generic;
using System.Collections.Specialized;
using NeoMapleStory.Server;

namespace NeoMapleStory.Game.Script.Event
{
    public class EventManager
    {
        public ChannelServer ChannelServer { get; private set; }
        public NameValueCollection props { get; } = new NameValueCollection();
        public string Name { get; private set; }

        private readonly Dictionary<string, EventInstanceManager> _instances = new Dictionary<string, EventInstanceManager>();
       
        public EventManager(ChannelServer cserv,string name)
        {
            //this.iv = iv;
            this.ChannelServer = cserv;
            this.Name = name;
        }

        //public void cancel()
        //{
        //    this.iv.invokeFunction("cancelSchedule", new object[] { null });
        //}

        //public void schedule(string methodName, int delay)
        //{
        //    TimerManager.Instance.ScheduleJob(() =>
        //    {
        //        iv.invokeFunction(methodName, (object)null);

        //    }, delay);
        //}

        //public string scheduleAtTimestamp(string methodName, long timestamp)
        //{
        //    return TimerManager.Instance.ScheduleJobAtTimeStamp(() =>
        //    {
        //        iv.invokeFunction(methodName, (object)null);
        //    }, timestamp);
        //}


        //public EventInstanceManager getInstance(string name) => _instances[name];


        //public List<EventInstanceManager> getInstances() => _instances.Values.ToList();


        //public EventInstanceManager newInstance(string name)
        //{
        //    EventInstanceManager ret = new EventInstanceManager(this, name);
        //    if (_instances.ContainsKey(name))
        //        _instances[name] = ret;
        //    else
        //        _instances.Add(name, ret);
        //    return ret;
        //}

        //public void disposeInstance(string name) => this._instances.Remove(name);


        //public void startInstance(MapleParty party, MapleMap map)
        //{

        //    EventInstanceManager eim =
        //        (EventInstanceManager)this.iv.invokeFunction("setup", new object[] { (object)null });
        //    eim.registerParty(party, map);
        //}

        //public void startInstance(MapleParty party, MapleMap map, bool partyid)
        //{

        //    EventInstanceManager eim;
        //    if (partyid)
        //    {
        //        eim =  (EventInstanceManager)this.iv.invokeFunction("setup", new object[] { party.PartyId });
        //    }
        //    else
        //    {
        //        eim = (EventInstanceManager)this.iv.invokeFunction("setup", new object[] { (object)null });
        //    }

        //    eim.registerParty(party, map);

        //}

        //public void startInstance(MapleSquad squad, MapleMap map)
        //{

        //    EventInstanceManager eim = (EventInstanceManager)this.iv.invokeFunction("setup", new object[] { squad.Leader.Id });
        //    eim.registerSquad(squad, map);

        //}

        //public void startInstance(EventInstanceManager eim, string leader)
        //{
        //    this.iv.invokeFunction("setup", new object[] { eim });
        //    eim.props.Add("leader", leader);
        //}

        //public EventInstanceManager startEventInstance(MapleParty party, MapleMap map, bool partyid)
        //{

        //    EventInstanceManager eim;
        //    if (partyid)
        //    {
        //        eim =
        //            (EventInstanceManager)
        //                (EventInstanceManager)this.iv.invokeFunction("setup", new object[] { party.PartyId });
        //    }
        //    else
        //    {
        //        eim =
        //            (EventInstanceManager)
        //                (EventInstanceManager)this.iv.invokeFunction("setup", new object[] { (object)null });
        //    }

        //    eim.registerParty(party, map);
        //    return eim;

        //}
    }
}
