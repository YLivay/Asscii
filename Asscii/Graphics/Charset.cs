using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asscii.Graphics
{
    class Charset
    {
        private static IReadOnlyDictionary<char, byte> chars;

        public static IReadOnlyDictionary<char, byte> Chars
        {
            get
            {
                if (chars == null)
                {
                    byte b = 0;
                    byte[] charBytes = new byte[256];
                    do
                    {
                        charBytes[b] = b++;
                    }
                    while (b != 0);
                    b = 0;
                    chars = Encoding.Default.GetChars(charBytes).ToDictionary(keyChar => keyChar, keyChar => b++);
                }
                return chars;
            }
        }
    }
}
