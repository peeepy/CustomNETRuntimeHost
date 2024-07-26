using System;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace TestCS.Memory
{
    public readonly struct Signature
    {
        private readonly char[] m_Signature;
        public readonly int SignatureByteLength;

        public Signature(string signature)
        {
            m_Signature = signature.ToCharArray();
            SignatureByteLength = signature.Count(c => c != ' ' && c != '\0' && c != '?') +
                                  signature.Count(c => c == '?') / 2;
        }

        public ReadOnlySpan<char> Get() => m_Signature;
        public int Length => m_Signature.Length;
    }

    public interface IPattern
    {
        string Name { get; }
        ReadOnlySpan<byte?> Signature { get; }
    }

    public sealed class Pattern : IPattern
    {
        private readonly string m_Name;
        private readonly byte?[] m_Signature;

        public Pattern(string name, string signatureString)
        {
            m_Name = name;
            var signature = new Signature(signatureString);
            m_Signature = new byte?[signature.SignatureByteLength];

            int pos = 0;
            for (int i = 0; i < signature.Length; i++)
            {
                char c = signature.Get()[i];
                if (c == ' ') continue;
                if (c == '\0') break;
                if (c == '?')
                {
                    if (i + 1 < signature.Length && signature.Get()[i + 1] == '?')
                        i++;
                    m_Signature[pos++] = null;
                    continue;
                }
                m_Signature[pos++] = (byte)(Utils.HexUtils.StrToHex(c) << 4 | Utils.HexUtils.StrToHex(signature.Get()[++i]));
            }
        }

        public string Name => m_Name;
        public ReadOnlySpan<byte?> Signature => m_Signature;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Name}: {{ ");
            foreach (var byte_ in Signature)
            {
                sb.Append(byte_.HasValue ? $"{byte_:X2} " : "?? ");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}