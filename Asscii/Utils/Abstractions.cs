namespace Asscii.Utils
{
    public class Array2D<T>
    {
        public readonly T[] Raw;
        public readonly int Width;
        public readonly int Height;

        public Array2D(int width, int height) : this(width, height, new T[width * height]) { }
        public Array2D(int width, int height, T[] array)
        {
            Raw = array;
            Width = width;
            Height = height;
        }

        public T this[int i]
        {
            get { return Raw[i]; }
            set { Raw[i] = value; }
        }

        public T this[int x, int y]
        {
            get { return Raw[y * Width + x]; }
            set { Raw[y * Width + x] = value; }
        }
    }
}
