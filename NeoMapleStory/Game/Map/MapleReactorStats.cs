using System;
using System.Collections.Generic;
using System.Drawing;

namespace NeoMapleStory.Game.Map
{
    public class MapleReactorStats
    {
        private readonly Dictionary<byte, StateData> m_stateInfo = new Dictionary<byte, StateData>();
        public Point Tl { get; set; }
        public Point Br { get; set; }

        public void AddState(byte state, int type, Tuple<int, int> reactItem, byte nextState)
        {
            m_stateInfo.Add(state, new StateData(type, reactItem, nextState));
        }

        public byte GetNextState(byte state)
        {
            StateData nextState;
            return m_stateInfo.TryGetValue(state, out nextState) ? nextState.NextState : (byte) 0xFF;
        }

        public int GetType(byte state)
        {
            StateData nextState;
            if (m_stateInfo.TryGetValue(state, out nextState))
            {
                return nextState.Type;
            }
            return -1;
        }

        public Tuple<int, int> GetReactItem(byte state)
        {
            StateData nextState;
            if (m_stateInfo.TryGetValue(state, out nextState))
            {
                return nextState.ReactItem;
            }
            return null;
        }

        private class StateData
        {
            public StateData(int type, Tuple<int, int> reactItem, byte nextState)
            {
                Type = type;
                ReactItem = reactItem;
                NextState = nextState;
            }

            public int Type { get; }
            public Tuple<int, int> ReactItem { get; }
            public byte NextState { get; }
        }
    }
}