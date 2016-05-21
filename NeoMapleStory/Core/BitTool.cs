using System;

namespace NeoMapleStory.Core
{
    public static class BitTool
    {
        public static string GetHexStr(byte[] bytes) => BitConverter.ToString(bytes).Replace('-', ' ');
    }
}