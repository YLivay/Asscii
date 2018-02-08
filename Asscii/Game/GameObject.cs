
using System;

using Asscii.Graphics;

namespace Asscii.Game
{
    public class GameObject : GameComponent
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double AnimationSpeed { get; set; }
        public double AnimationFrame { get; set; }
        public Animation Animation { get; set; }
        public int Depth { get; set; }

        public GameObject() : this(0, 0, null) { }
        public GameObject(double x, double y) : this(x, y, null) { }
        public GameObject(double x, double y, Animation anim) : base() {
            X = x;
            Y = y;
            Animation = anim;
            AnimationFrame = 0;
            AnimationSpeed = 1;
        }

        public override void Update(double deltaTime) {
            AnimationFrame += AnimationSpeed * deltaTime;
            AnimationFrame %= Animation.Frames.Count;
        }

        public override void Draw() {
            if (Animation != null)
                Animation.Draw(Console, (int)X, (int)Y, Depth, AnimationFrame);
        }
    }
}
