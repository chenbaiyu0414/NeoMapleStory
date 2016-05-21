using System;
using System.Collections.Generic;
using System.Drawing;

namespace NeoMapleStory.Game.Map
{
    public class MapleFootholdTree
    {
        private static readonly int MaxDepth = 8;
        private readonly int m_depth;
        private readonly List<MapleFoothold> m_footholds = new List<MapleFoothold>();
        private Point m_center;
        private MapleFootholdTree m_ne;
        private MapleFootholdTree m_nw;
        private MapleFootholdTree m_se;
        private MapleFootholdTree m_sw;

        public MapleFootholdTree(Point p1, Point p2)
        {
            Point1 = p1;
            Point2 = p2;
            m_center = new Point((p2.X - p1.X)/2, (p2.Y - p1.Y)/2);
        }

        public MapleFootholdTree(Point p1, Point p2, int depth)
        {
            Point1 = p1;
            Point2 = p2;
            m_depth = depth;
            m_center = new Point((p2.X - p1.X)/2, (p2.Y - p1.Y)/2);
        }

        public Point Point1 { get; set; }
        public Point Point2 { get; set; }
        public int MaxDropX { get; private set; }
        public int MinDropX { get; private set; }

        public void Insert(MapleFoothold f)
        {
            if (m_depth == 0)
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
            if (m_depth == MaxDepth ||
                (f.Point1.X >= Point1.X && f.Point2.X <= Point2.X &&
                 f.Point1.Y >= Point1.Y && f.Point2.Y <= Point2.Y))
            {
                m_footholds.Add(f);
            }
            else
            {
                if (m_nw == null)
                {
                    m_nw = new MapleFootholdTree(Point1, m_center, m_depth + 1);
                    m_ne = new MapleFootholdTree(new Point(m_center.X, Point1.Y), new Point(Point2.X, m_center.Y),
                        m_depth + 1);
                    m_sw = new MapleFootholdTree(new Point(Point1.X, m_center.Y), new Point(m_center.X, Point2.Y),
                        m_depth + 1);
                    m_se = new MapleFootholdTree(m_center, Point2, m_depth + 1);
                }
                if (f.Point2.X <= m_center.X && f.Point2.Y <= m_center.Y)
                {
                    m_nw.Insert(f);
                }
                else if (f.Point1.X > m_center.X && f.Point2.Y <= m_center.Y)
                {
                    m_ne.Insert(f);
                }
                else if (f.Point2.X <= m_center.X && f.Point1.Y > m_center.Y)
                {
                    m_sw.Insert(f);
                }
                else
                {
                    m_se.Insert(f);
                }
            }
        }

        private List<MapleFoothold> GetRelevants(Point p)
        {
            return GetRelevants(p, new List<MapleFoothold>());
        }

        private List<MapleFoothold> GetRelevants(Point p, List<MapleFoothold> list)
        {
            list.AddRange(m_footholds);
            if (m_nw != null)
            {
                if (p.X <= m_center.X && p.Y <= m_center.Y)
                {
                    m_nw.GetRelevants(p, list);
                }
                else if (p.X > m_center.X && p.Y <= m_center.Y)
                {
                    m_ne.GetRelevants(p, list);
                }
                else if (p.X <= m_center.X && p.Y > m_center.Y)
                {
                    m_sw.GetRelevants(p, list);
                }
                else
                {
                    m_se.GetRelevants(p, list);
                }
            }
            return list;
        }

        private MapleFoothold FindWallR(Point p1, Point p2)
        {
            MapleFoothold ret;
            foreach (var f in m_footholds)
            {
                //if (f.isWall()) System.out.println(f.Point1.X + " " + f.Point2.X);
                if (f.IsWall() && f.Point1.X >= p1.X && f.Point1.X <= p2.X &&
                    f.Point1.Y >= p1.Y && f.Point2.Y <= p1.Y)
                {
                    return f;
                }
            }
            if (m_nw != null)
            {
                if (p1.X <= m_center.X && p1.Y <= m_center.Y)
                {
                    ret = m_nw.FindWallR(p1, p2);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if ((p1.X > m_center.X || p2.X > m_center.X) && p1.Y <= m_center.Y)
                {
                    ret = m_ne.FindWallR(p1, p2);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if (p1.X <= m_center.X && p1.Y > m_center.Y)
                {
                    ret = m_sw.FindWallR(p1, p2);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                if ((p1.X > m_center.X || p2.X > m_center.X) && p1.Y > m_center.Y)
                {
                    ret = m_se.FindWallR(p1, p2);
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
            var relevants = GetRelevants(p);
            // find fhs with matching x coordinates
            var xMatches = new List<MapleFoothold>();
            foreach (var fh in relevants)
            {
                if (fh.Point1.X <= p.X && fh.Point2.X >= p.X)
                {
                    xMatches.Add(fh);
                }
            }
            xMatches.Sort();
            foreach (var fh in xMatches)
            {
                if (!fh.IsWall() && fh.Point1.Y != fh.Point2.Y)
                {
                    int calcY;
                    double s1 = Math.Abs(fh.Point2.Y - fh.Point1.Y);
                    double s2 = Math.Abs(fh.Point2.X - fh.Point1.X);
                    double s4 = Math.Abs(p.X - fh.Point1.X);
                    var alpha = Math.Atan(s2/s1);
                    var beta = Math.Atan(s1/s2);
                    var s5 = Math.Cos(alpha)*(s4/Math.Cos(beta));
                    if (fh.Point2.Y < fh.Point1.Y)
                    {
                        calcY = fh.Point1.Y - (int) s5;
                    }
                    else
                    {
                        calcY = fh.Point1.Y + (int) s5;
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