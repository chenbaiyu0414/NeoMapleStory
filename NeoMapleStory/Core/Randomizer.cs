using System;

namespace NeoMapleStory.Core
{
    public static class Randomizer
    {
        private static readonly Random MRandomer = new Random();

        public static double NextDouble() => MRandomer.NextDouble();

        public static int Next() => MRandomer.Next();

        public static int Next(int maxValue) => MRandomer.Next(maxValue);

        public static int Next(int minValue, int maxValue) => MRandomer.Next(minValue, maxValue);

        public static void NextBytes(byte[] buffer) => MRandomer.NextBytes(buffer);
    }
}
