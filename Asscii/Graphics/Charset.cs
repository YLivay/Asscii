using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asscii.Graphics
{
    class Charset
    {
        public static readonly Encoding Encoding = Encoding.GetEncoding("DOS-862");

        private static IReadOnlyDictionary<char, byte> _chars;
        public static IReadOnlyDictionary<char, byte> Chars {
            get {
                if (_chars == null) {
                    byte b = 0;
                    byte[] charBytes = new byte[256];
                    do {
                        charBytes[b] = b++;
                    }
                    while (b != 0);
                    b = 0;
                    _chars = Charset.Encoding.GetChars(charBytes).ToDictionary(keyChar => keyChar, keyChar => b++);
                }
                return _chars;
            }
        }
    }
}
