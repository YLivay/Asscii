using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asscii.Utils;

using static Asscii.Graphics.FastConsole;

namespace Asscii.Graphics
{
    class DrawStep
    {
        public readonly ConsoleColor fg;
        public readonly ConsoleColor bg;
        public string String { get; set; }

        public DrawStep(ConsoleColor fg, ConsoleColor bg, string str) {
            this.fg = fg;
            this.bg = bg;
            String = str;
        }
    }

    public class Sprite : Drawable
    {
        private readonly DrawStep[] _steps;
        private readonly Array2D<CharInfo> _canvas;

        private Sprite(DrawStep[] steps) {
            _steps = steps;
            string[] lines = string.Join("", steps.Select(step => step.String).ToArray()).Split(new char[] { '\n' });
            Width = lines.Max(line => line.Length);
            Height = lines.Length;

            // Flattens the DrawSteps onto a canvas for faster rendering on FastConsole
            _canvas = new Array2D<CharInfo>(Width, Height);
            DrawUsingSteps(_canvas, new int?[Width * Height], 0, 0, 0);

            PivotX = Width / 2;
            PivotY = Height / 2;
        }

        public static Sprite Load(string filepath, int pivotX, int pivotY) {
            Sprite result = Load(filepath);
            result.PivotX = pivotX;
            result.PivotY = pivotY;
            return result;
        }

        // I needed a way to quickly and easily design "art" for my game in ascii.
        // After searching a bit I found a cool program called Playscii: https://imgur.com/z03UaAD
        // The issue was that the only export type that seemed to contain all the data I needed looks like: https://imgur.com/GGGS1yy
        // Found how these are parsed in this document: http://ascii-table.com/ansi-escape-sequences.php
        // So first mission is to parse these.
        public static Sprite Load(string filepath) {
            var text = System.IO.File.ReadAllText(filepath, Encoding.Default).Replace("\r", "");

            // \x1B\x5B is an ansi escape code, after which comes some attribute modifications like color, bold, underline, blink etc.
            // Since these attributes are only cleared when set to something else, I first try and split the file into "draw steps" where each step is
            // a serie of consecutive characters all having the same attribute. This strategy makes it faster to draw sprites on consoles where switching
            // attributes is time consuming (like the default C# console).
            //
            // Later in the code, these draw steps will be flattened on to a Array2D<CharInfo> which is faster to use when switching attributes is NOT Slow,
            // like the FastConsole implementation.
            var codes = text.Split(new string[] { "\x1B\x5B" }, StringSplitOptions.RemoveEmptyEntries);
            var steps = new List<DrawStep>();

            int i = 0;
            DrawStep lastStep = null;
            if (!text.StartsWith("\x1B\x5B")) {
                lastStep = new DrawStep(ConsoleColor.Gray, ConsoleColor.Black, codes[0]);
                steps.Add(lastStep);
                i += 1;
            }

            var mSplit = new char[] { 'm' };
            var modifierSplit = new char[] { ';' };
            for (; i < codes.Length; i++) {
                var codesAndText = codes[i].Split(mSplit, 2);

                ConsoleColor fg = ConsoleColor.Gray, bg = ConsoleColor.Black;

                var modifiers = codesAndText[0]
                    .Split(modifierSplit)
                    .Select(modifier => {
                        int a;
                        if (int.TryParse(modifier, out a))
                            return a;
                        return 0;
                    })
                    .ToArray();

                // The color scheme i was working on 
                bool fgLight = false, bgLight = false;
                foreach (var modifier in modifiers) {
                    switch (modifier) {
                        case 1:
                            fgLight = true;
                            break;

                        case 5:
                            bgLight = true;
                            break;

                        case 30:
                            fg = fgLight ? ConsoleColor.DarkGray : ConsoleColor.Black;
                            break;

                        case 31:
                            fg = fgLight ? ConsoleColor.Red : ConsoleColor.DarkRed;
                            break;

                        case 32:
                            fg = fgLight ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                            break;

                        case 33:
                            fg = fgLight ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                            break;

                        case 34:
                            fg = fgLight ? ConsoleColor.Blue : ConsoleColor.DarkBlue;
                            break;

                        case 35:
                            fg = fgLight ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta;
                            break;

                        case 36:
                            fg = fgLight ? ConsoleColor.Cyan : ConsoleColor.DarkCyan;
                            break;

                        case 37:
                            fg = fgLight ? ConsoleColor.White : ConsoleColor.Gray;
                            break;

                        case 40:
                            bg = bgLight ? ConsoleColor.DarkGray : ConsoleColor.Black;
                            break;

                        case 41:
                            bg = bgLight ? ConsoleColor.Red : ConsoleColor.DarkRed;
                            break;

                        case 42:
                            bg = bgLight ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                            break;

                        case 43:
                            bg = bgLight ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                            break;

                        case 44:
                            bg = bgLight ? ConsoleColor.Blue : ConsoleColor.DarkBlue;
                            break;

                        case 45:
                            bg = bgLight ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta;
                            break;

                        case 46:
                            bg = bgLight ? ConsoleColor.Cyan : ConsoleColor.DarkCyan;
                            break;

                        case 47:
                            bg = bgLight ? ConsoleColor.White : ConsoleColor.Gray;
                            break;

                        // Used by playscii to say "no character here"
                        case 29:
                        case 39:
                            break;

                        case 0:
                        default:
                            fgLight = false;
                            bgLight = false;
                            fg = ConsoleColor.Gray;
                            bg = ConsoleColor.Black;
                            break;
                    }
                }

                var trimmed = codesAndText[1].Trim(new char[] { '\n', '\0' });
                if (i > 0 && ((lastStep.fg == fg && lastStep.bg == bg) || trimmed.Length == 0)) {
                    lastStep.String += codesAndText[1];
                }
                else {
                    lastStep = new DrawStep(fg, bg, codesAndText[1]);
                    steps.Add(lastStep);
                }
            }
            return new Sprite(steps.ToArray());
        }

        public override void Draw(FastConsole console, int x, int y, int depth) => DrawUsingBuffer(console.Buffer, console.DepthMap, x, y, depth);

        public void DrawUsingBuffer(Array2D<CharInfo> targetCanvas, int?[] depthMap, int x, int y, int depth) {
            // Offset the drawing by the pivot.
            x -= PivotX;
            y -= PivotY;

            var targetWidth = targetCanvas.Width;
            var targetHeight = targetCanvas.Height;

            // If trying to draw the sprite out of bounds - do nothing.
            if (x <= -Width || x >= targetWidth || y <= -Height || y >= targetHeight)
                return;

            // Find the intersecting bounds of the target canvas.
            int minTargetPx = Math.Max(x, 0), maxTargetPx = Math.Min(x + Width, targetWidth);
            int minTargetPy = Math.Max(y, 0), maxTargetPy = Math.Min(y + Width, targetHeight);

            // Find the intersecting top-left corner of the sprite we're drawing.
            // We don't need to find the bottom-right because we'll be iterating over the
            // intersection size of the target canvas which should cover both.
            var minPx = x < 0 ? -x : 0;
            var minPy = y < 0 ? -y : 0;

            for (int targetPy = minTargetPy, py = minPy; targetPy < maxTargetPy; targetPy++, py++) {
                for (int targetPx = minTargetPx, px = minPx; targetPx < maxTargetPx; targetPx++, px++) {
                    int depthIdx = targetPx + targetPy * targetWidth;
                    if (depthMap[depthIdx].HasValue && depthMap[depthIdx] < depth)
                        continue;

                    var ch = _canvas[px, py];
                    if (ch.AsciiChar == 0)
                        continue;

                    targetCanvas[targetPx, targetPy] = ch;
                    depthMap[depthIdx] = depth;
                }
            }
        }

        public void DrawUsingSteps(Array2D<CharInfo> targetCanvas, int?[] depthMap, int x, int y, int depth) {
            // Offset the drawing by the pivot.
            x -= PivotX;
            y -= PivotY;

            int canvasWidth = targetCanvas.Width, canvasHeight = targetCanvas.Height, px = x, py = y;

            // If trying to draw the sprite out of bounds - do nothing.
            if (x <= -Width || x >= canvasWidth || y <= -Height || y >= canvasHeight)
                return;

            foreach (var step in _steps) {
                var lines = step.String.Split(new char[] { '\n' });
                for (var i = 0; i < lines.Length; i++) {
                    // If a line is too long, draw only part of the string.
                    var line = lines[i];
                    var startOffset = px < 0 ? -px : 0;
                    var endPx = px + line.Length;

                    if (endPx >= 0) {
                        var endOffset = Math.Max(0, endPx - canvasWidth);
                        px = Math.Max(px, 0);
                        line = line.Substring(startOffset, line.Length - endOffset - startOffset);
                        foreach (var ch in line) {
                            if (ch == '\0') {
                                px++;
                                continue;
                            }

                            if (px < 0 || py < 0)
                                continue;

                            var depthIdx = px + py * canvasWidth;
                            if (!depthMap[depthIdx].HasValue || depthMap[depthIdx] >= depth) {
                                targetCanvas[px, py] = new CharInfo(Charset.Chars[ch], step.fg, step.bg);
                                depthMap[depthIdx] = depth;
                            }

                            px++;
                        }
                    }
                    else {
                        px = endPx;
                    }

                    if (i != lines.Length - 1) {
                        py++;
                        px = x;
                        if (py == canvasHeight)
                            return;
                    }
                }
            }
        }
    }
}
