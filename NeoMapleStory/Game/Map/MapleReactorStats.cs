using System;
using System.Collections.Generic;
using System.Drawing;

namespace NeoMapleStory.Game.Map
{
     public class MapleReactorStats
    {
        public Point Tl { get; set; }
        public Point Br { get; set; }
        private readonly Dictionary<byte, StateData> _stateInfo = new Dictionary<byte, StateData>();

        public void AddState(byte state, int type, Tuple<int, int> reactItem, byte nextState)
        {
            _stateInfo.Add(state, new StateData(type, reactItem, nextState));
        }

        public byte GetNextState(byte state)
        {
            StateData nextState;
            return _stateInfo.TryGetValue(state,out nextState) ? nextState.NextState : (byte)0;
        }

        public int GetType(byte state)
        {
            StateData nextState;
            if (_stateInfo.TryGetValue(state, out nextState))
            {
                return nextState.Type;
            }
            else {
                return -1;
            }
        }

        public Tuple<int, int> GetReactItem(byte state)
        {
            StateData nextState;
            if (_stateInfo.TryGetValue(state, out nextState))
            {
                return nextState.ReactItem;
            }
            else {
                return null;
            }
        }

        private class StateData
        {

            public int Type { get; }
            public Tuple<int, int> ReactItem { get; }
            public byte NextState { get; }

            public StateData(int type, Tuple<int, int> reactItem, byte nextState)
            {
                Type = type;
                ReactItem = reactItem;
                NextState = nextState;
            }
        }
    }
}
