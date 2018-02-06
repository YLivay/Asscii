using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Asscii.Graphics
{
    public class Animation : Drawable
    {
        private Sprite[] _frames;
        public IReadOnlyCollection<Sprite> Frames => _frames;

        public Animation(Sprite[] frames) {
            _frames = (Sprite[])frames.Clone();
            Width = frames.Max(frame => frame.Width);
            Height = frames.Max(frame => frame.Height);

            PivotX = Width / 2;
            PivotY = Height / 2;
        }

        public static Animation Load(string dir, string framePattern) {
            var r = new Regex(framePattern.Replace("*", @"(\d+)"));
            var frameFiles = System.IO.Directory.GetFiles(dir, framePattern, System.IO.SearchOption.TopDirectoryOnly)
                .OrderBy(file => {
                    file = file.Substring(dir.Length).TrimStart(System.IO.Path.DirectorySeparatorChar);
                    Match m = r.Match(file);
                    return int.Parse(m.Groups[1].Value);
                })
                .ToArray();

            return Load(frameFiles);
        }

        public static Animation Load(string[] frameFiles) => new Animation(frameFiles.Select(file => Sprite.Load(file, 0, 0)).ToArray());
        public static Animation FromSprite(Sprite sprite) => new Animation(new Sprite[] { sprite });

        public override void Draw(FastConsole console, int x, int y, int depth) => Draw(console, x, y, depth, 0);

        public void Draw(FastConsole console, int x, int y, int depth, double frame) {
            var drawnFrame = (int)frame % _frames.Length;
            var sprite = _frames[drawnFrame];
            sprite.Draw(console, x - PivotX + sprite.PivotX, y - PivotY + sprite.PivotY, depth);
        }
    }
}
