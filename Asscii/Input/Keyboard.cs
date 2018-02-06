namespace Asscii.Input
{
    // Keycodes map from https://msdn.microsoft.com/en-us/library/dd375731(v=vs.85).aspx
    public enum Key : int
    {
        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,

        Z = 0x5A,
        X = 0x58,

        Escape = 0x1B,
    }

    public static class Keyboard
    {
        // GetKeyState from https://msdn.microsoft.com/en-us/library/ms646301(v=VS.85).aspx
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetKeyState(int key);

        public static bool IsPressed(Key key)
        {
            return (GetKeyState((int)key) & 0x8000) != 0;
        }
    }
}
