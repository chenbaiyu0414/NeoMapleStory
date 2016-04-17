﻿using System;
using System.Drawing;

namespace NeoMapleStory.Game.Map
{
     public class MapleFoothold : IComparable<MapleFoothold>
    {
        public Point Point1 { get; private set; }
        public Point Point2 { get; private set; }
        public int FootholdId { get; private set; }
        public int NextFootholdId { get; set; }
        public int PrevFootholdId { get; set; }

        public MapleFoothold(Point p1, Point p2, int id)
        {
            Point1 = p1;
            Point2 = p2;
            FootholdId = id;
        }

        public bool IsWall() => Point1.X == Point2.X;

        public int CompareTo(MapleFoothold other)
        {
            if (Point2.Y < other.Point1.Y)
                return -1;
            else if (Point1.Y > other.Point2.Y)
                return 1;
            else
                return 0;
        }
    }
}
