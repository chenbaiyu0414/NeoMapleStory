using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NeoMapleStory.Core
{
    public static class MethodExtension
    {
        public static double DistanceSquare(this Point point, Point otherPoint)
        {
            double px = otherPoint.X - point.X;
            double py = otherPoint.Y - point.Y;
            return px * px + py * py;
        }
        public static void Shuffle<T>(this List<T> list)
        {
            Random rng = new Random();
            list.OrderBy(x => rng.Next());
        }
    }
}
