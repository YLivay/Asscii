using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

using Asscii.Utils;

namespace Asscii.Graphics
{
    public class FastConsole
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteConsoleOutput(
            SafeFileHandle hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y) {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        private struct CharUnion
        {
            [FieldOffset(0)]
            public char UnicodeChar;
            [FieldOffset(0)]
            public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)]
            private CharUnion _ch;
            [FieldOffset(2)]
            public short Attributes;

            public static readonly IReadOnlyDictionary<ConsoleColor, short> Color2Attribute = new Dictionary<ConsoleColor, short>()
            {
                // Darker colors (luminous bit is off)
                { ConsoleColor.Black,       0x00 },
                { ConsoleColor.DarkBlue,    0x01 },
                { ConsoleColor.DarkGreen,   0x02 },
                { ConsoleColor.DarkCyan,    0x03 },
                { ConsoleColor.DarkRed,     0x04 },
                { ConsoleColor.DarkMagenta, 0x05 },
                { ConsoleColor.DarkYellow,  0x06 },
                { ConsoleColor.Gray,        0x07 },

                // Lighter colors (luminous bit is on)
                { ConsoleColor.DarkGray,    0x08 },
                { ConsoleColor.Blue,        0x09 },
                { ConsoleColor.Green,       0x0a },
                { ConsoleColor.Cyan,        0x0b },
                { ConsoleColor.Red,         0x0c },
                { ConsoleColor.Magenta,     0x0d },
                { ConsoleColor.Yellow,      0x0e },
                { ConsoleColor.White,       0x0f },
            };

            // For reverse lookups
            public static readonly IReadOnlyDictionary<short, ConsoleColor> Attribute2Color = Color2Attribute.ToDictionary(pair => pair.Value, pair => pair.Key);

            public CharInfo(char ch, ConsoleColor fg, ConsoleColor bg) : this(new CharUnion { UnicodeChar = ch }, fg, bg) { }
            public CharInfo(byte ch, ConsoleColor fg, ConsoleColor bg) : this(new CharUnion { AsciiChar = ch }, fg, bg) { }
            private CharInfo(CharUnion ch, ConsoleColor fg, ConsoleColor bg) {
                _ch = ch;
                Attributes = 0;
                ForegroundColor = fg;
                BackgroundColor = bg;
            }

            public byte AsciiChar {
                get { return _ch.AsciiChar; }
            }

            public char UnicodeChar {
                get { return _ch.UnicodeChar; }
            }

            public ConsoleColor ForegroundColor {
                get { return Attribute2Color[(short)(Attributes & 0x0f)]; }
                set { Attributes |= Color2Attribute[value]; }
            }

            public ConsoleColor BackgroundColor {
                get { return Attribute2Color[(short)((Attributes & 0xf0) >> 4)]; }
                set { Attributes |= (short)(Color2Attribute[value] << 4); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        private static SafeFileHandle _h;

        public static void Init() {
            if (_h != null && !_h.IsInvalid && !_h.IsClosed)
                return;

            _h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            if (_h.IsInvalid)
                throw new Exception("Failed to get handle to the console buffer");
        }

        public Array2D<CharInfo> Buffer {
            get;
            private set;
        }

        public int?[] DepthMap {
            get;
            private set;
        }

        private Coord _size;
        private Coord _pos;
        private readonly Coord _origin = new Coord { X = 0, Y = 0 };

        public FastConsole() : this((short)Console.WindowWidth, (short)Console.WindowHeight) { }
        public FastConsole(short width, short height) : this(width, height, 0, 0) { }
        public FastConsole(short width, short height, short x, short y) {
            Init();
            Buffer = new Array2D<CharInfo>(width, height);
            DepthMap = new int?[width * height];
            _size = new Coord() { X = width, Y = height };
            _pos = new Coord() { X = x, Y = y };
        }

        public short X {
            get { return _pos.X; }
            set { _pos.X = value; }
        }

        public short Y {
            get { return _pos.Y; }
            set { _pos.Y = value; }
        }

        public short Width {
            get { return _size.X; }
        }

        public short Height {
            get { return _size.Y; }
        }

        public void Write(short x, short y, int depth, string str) {
            if (x + str.Length < 0 || x >= Width || y < 0 || y >= Height)
                return;

            str = str.Substring(Math.Max(-str.Length, 0));
            short px = Math.Max(x, (short)0);
            foreach (var ch in str) {
                int depthIdx = px + y * Buffer.Width;
                if (!DepthMap[depthIdx].HasValue || DepthMap[depthIdx] >= depth) {
                    Buffer[px, y] = new CharInfo(Charset.Chars[ch], ConsoleColor.White, ConsoleColor.Black);
                    DepthMap[depthIdx] = depth;
                }

                px++;
                if (px >= Width)
                    break;
            }
        }

        public void Render() {
            var rect = new SmallRect { Left = _pos.X, Top = _pos.Y, Right = (short)(_pos.X + _size.X), Bottom = (short)(_pos.Y + _size.Y) };
            WriteConsoleOutput(_h, Buffer.Raw, _size, _origin, ref rect);
            Buffer = new Array2D<CharInfo>(_size.X, _size.Y);
            Array.Clear(DepthMap, 0, DepthMap.Length);
        }
    }
}
