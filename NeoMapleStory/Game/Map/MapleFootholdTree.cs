using System;
using System.Collections.Generic;
using System.Drawing;

namespace NeoMapleStory.Game.Map
{
     public class MapleFootholdTree
    {
        private MapleFootholdTree _nw;
        private MapleFootholdTree _ne;
        private MapleFootholdTree _sw;
        private MapleFootholdTree _se;
        private readonly List<MapleFoothold> _footholds = new List<MapleFoothold>();
        public Point Point1 { get; set; }
        public Point Point2 { get; set; }
        private Point _center;
        private readonly int _depth;
        private static readonly int MaxDepth = 8;
        public int MaxDropX { get; private set; }
        public int MinDropX { get; private set; }

        public MapleFootholdTree(Point p1, Point p2)
        {
            Point1 = p1;
            Point2 = p2;
            _center = new Point((p2.X - p1.X) / 2, (p2.Y - p1.Y) / 2);
        }

        public MapleFootholdTree(Point p1, Point p2, int depth)
        {
            Point1 = p1;
            Point2 = p2;
            this._depth = depth;
            _center = new Point((p2.X - p1.X) / 2, (p2.Y - p1.Y) / 2);
        }

        public void Insert(MapleFoothold f)
        {
            if (_depth == 0)
            {
                if (f.Point1.X > MaxDropX)
                {
                    MaxDropX = f.Point1.X;
                }
                if (f.Point1.X < MinDropX)
                {
                    MinDropX = f.Point1.X;
                }
                if (f.Point2.X > MaxDropX)
                {
                    MaxDropX = f.Point2.X;
                }
                if (f.Point2.X < MinDropX)
                {
                    MinDropX = f.Point2.X;
                }
            }
            if (_depth == MaxDepth ||
                    (f.Point1.X >= Point1.X && f.Point2.X <= Point2.X &&
                    f.Point1.Y >= Point1.Y && f.Point2.Y <= Point2.Y))
            {
                _footholds.Add(f);
            }
            else {
                if (_nw == null)
                {
                    _nw = new MapleFootholdTree(Point1, _center, _depth + 1);
                    _ne = new MapleFootholdTree(new Point(_center.X, Point1.Y), new Point(Point2.X, _center.Y), _depth + 1);
                    _sw = new MapleFootholdTree(new Point(Point1.X, _center.Y), new Point(_center.X, Point2.Y), _depth + 1);
                    _se = new MapleFootholdTree(_center, Point2, _depth + 1);
                }
                if (f.Point2.X <= _center.X && f.Point2.Y <= _center.Y)
                {
                    _nw.Insert(f);
                }
                else if (f.Point1.X > _center.X && f.Point2.Y <= _center.Y)
                {
                    _ne.Insert(f);
                }
                else if (f.Point2.X <= _center.X && f.Point1.Y > _center.Y)
                {
                    _sw.Insert(f);
                }
                else {
                    _se.Insert(f);
                }
            }
        }

        private List<MapleFoothold> GetRelevants(Point p)
        {
            return GetRelevants(p, new List<MapleFoothold>());
        }

        private List<MapleFoothold> GetRelevants(Point p, List<MapleFoothold> list)
        {
            list.AddRange(_footholds);
            if (_nw != null)
            {
                if (p.X <= _center.X && p.Y <= _center.Y)
                {
                    _nw.GetRelevants(p, list);
                }
                else if (p.X > _center.X && p.Y <= _center.Y)
                {
                    _ne.GetRelevants(p, list);
                }
                else if (p.X <= _center.X && p.Y > _center.Y)
                {
                    _sw.GetRelevants(p, list);
                }
                else {
                    _se.GetRelevants(p, list);
                }
            }
            return list;
        }

        private MapleFoothold FindWallR(Point p1, Point p2)
        {
            MapleFoothold ret;
            foreach (MapleFoothold f in _footholds)
            {
                //if (f.isWall()) System.out.println(f.Point1.X + " " + f.Point2.X);
                if (f.IsWall() && f.Point1.X >= p1.X && f.Point1.X <= p2.X &&
                        f.Point1.Y >= p1.Y && f.Point2.Y <= p1.Y)
                {
                    return f;
                }
            }
            if (_nw != null)
            {
                if (p1.X <= _center.X && p1.Y <= _center.Y)
                {
                    ret = _nw.FindWallR(p1, p2);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if ((p1.X > _center.X || p2.X > _center.X) && p1.Y <= _center.Y)
                {
                    ret = _ne.FindWallR(p1, p2);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if (p1.X <= _center.X && p1.Y > _center.Y)
                {
                    ret = _sw.FindWallR(p1, p2);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if ((p1.X > _center.X || p2.X > _center.X) && p1.Y > _center.Y)
                {
                    ret = _se.FindWallR(p1, p2);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
            }
            return null;
        }

        public MapleFoothold FindWall(Point p1, Point p2)
        {
            if (p1.Y != p2.Y)
            {
                throw new ArgumentException();
            }
            return FindWallR(p1, p2);
        }

        public MapleFoothold FindBelow(Point p)
        {
            List<MapleFoothold> relevants = GetRelevants(p);
            // find fhs with matching x coordinates
            List<MapleFoothold> xMatches = new List<MapleFoothold>();
            foreach (MapleFoothold fh in relevants)
            {
                if (fh.Point1.X <= p.X && fh.Point2.X >= p.X)
                {
                    xMatches.Add(fh);
                }
            }
            xMatches.Sort();
            foreach (MapleFoothold fh in xMatches)
            {
                if (!fh.IsWall() && fh.Point1.Y != fh.Point2.Y)
                {
                    int calcY;
                    double s1 = Math.Abs(fh.Point2.Y - fh.Point1.Y);
                    double s2 = Math.Abs(fh.Point2.X - fh.Point1.X);
                    double s4 = Math.Abs(p.X - fh.Point1.X);
                    double alpha = Math.Atan(s2 / s1);
                    double beta = Math.Atan(s1 / s2);
                    double s5 = Math.Cos(alpha) * (s4 / Math.Cos(beta));
                    if (fh.Point2.Y < fh.Point1.Y)
                    {
                        calcY = fh.Point1.Y - (int)s5;
                    }
                    else {
                        calcY = fh.Point1.Y + (int)s5;
                    }
                    if (calcY >= p.Y)
                    {
                        return fh;
                    }
                }
                else if (!fh.IsWall())
                {
                    if (fh.Point1.Y >= p.Y)
                    {
                        return fh;
                    }
                }
            }
            return null;
        }
    }
}
