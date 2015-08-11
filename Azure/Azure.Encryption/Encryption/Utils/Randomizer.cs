﻿#region

using System;

#endregion

namespace Azure.Encryption.Utils
{
    public class Randomizer
    {
        private static readonly Random Rand = new Random();

        public static Random GetRandom
        {
            get { return Rand; }
        }

        public static int Next() { return Rand.Next(); }

        public static int Next(int max) { return Rand.Next(max); }

        public static int Next(int min, int max) { return Rand.Next(min, max); }

        public static double NextDouble() { return Rand.NextDouble(); }

        public static byte NextByte() { return (byte) Next(0, 255); }

        public static byte NextByte(int max)
        {
            max = Math.Min(max, 255);
            return (byte) Next(0, max);
        }

        public static byte NextByte(int min, int max)
        {
            max = Math.Min(max, 255);
            return (byte) Next(Math.Min(min, max), max);
        }

        public static void NextBytes(byte[] toparse) { Rand.NextBytes(toparse); }
    }
}