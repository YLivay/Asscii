namespace Asscii.Graphics
{
    public abstract class Drawable
    {
        public int PivotX { get; set; }
        public int PivotY { get; set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public abstract void Draw(FastConsole console, int x, int y, int depth);
    }
}
