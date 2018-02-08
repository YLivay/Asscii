namespace Asscii.Graphics
{
    public interface IDrawable
    {
        void Draw(FastConsole console, int x, int y, int depth);
    }

    public abstract class Drawable : IDrawable
    {
        public int PivotX { get; set; }
        public int PivotY { get; set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public abstract void Draw(FastConsole console, int x, int y, int depth);
    }
}
