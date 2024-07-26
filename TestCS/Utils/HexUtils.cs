using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;


namespace TestCS.Utils
{
    public static class HexUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int StrToHex(char c) => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => throw new ArgumentException($"Invalid hex character: {c}")
        };

        public static string HexStr(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
                return string.Empty;

            var result = new char[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                var value = data[i];
                result[i * 2] = GetHexChar(value >> 4);
                result[i * 2 + 1] = GetHexChar(value & 0xF);
            }

            return new string(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetHexChar(int value)
        {
            return (char)(value < 10 ? value + '0' : value - 10 + 'A');
        }
    }
}
