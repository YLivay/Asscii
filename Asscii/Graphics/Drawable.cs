namespace Asscii.Graphics
{
    public abstract class Drawable
    {
        public int PivotX;
        public int PivotY;
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public abstract void Draw(FastConsole console, int x, int y, int depth);
    }
}
