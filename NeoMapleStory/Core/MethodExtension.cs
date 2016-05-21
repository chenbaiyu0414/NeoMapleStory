using System;
using System.Collections.Generic;
using System.Drawing;

namespace NeoMapleStory.Core
{
    public static class MethodExtension
    {
        public static double DistanceSquare(this Point point, Point otherPoint)
        {
            double px = otherPoint.X - point.X;
            double py = otherPoint.Y - point.Y;
            return px*px + py*py;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            var rng = new Random();
            for (var i = list.Count; i > 1; i--)
                Swap(list, i - 1, rng.Next(i));
        }

        private static void Swap<T>(List<T> list, int i, int j)
        {
            var temp = list[j];
            list[j] = list[i];
            list[i] = temp;
        }
    }
}