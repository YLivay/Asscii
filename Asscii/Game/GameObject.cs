
using System;
using System.Diagnostics;

using Asscii.Graphics;

namespace Asscii.Game
{
    public class GameObject
    {
        public long ID { get; private set; }
        public double X;
        public double Y;
        public double AnimationSpeed;
        public double AnimationFrame;
        public Animation Animation;
        public int Depth;

        protected GameScene Scene { get; private set; }
        protected FastConsole Console
        {
            get { return Scene == null ? null : Scene.Console; }
        }

        public Random Random
        {
            get { return Scene == null ? null : Scene.Random; }
        }

        public GameObject() : this(0, 0, null) { }
        public GameObject(double x, double y) : this(x, y, null) { }
        public GameObject(double x, double y, Animation anim)
        {
            ID = -1;
            X = x;
            Y = y;
            Animation = anim;
            AnimationFrame = 0;
            AnimationSpeed = 1;
        }

        public void AddToScene(GameScene scene)
        {
            if (ID != -1)
                throw new InvalidOperationException("Object already added to scene");

            Scene = scene;
            ID = scene.NextObjectID();
            scene.Add(this);
        }

        public void Remove()
        {
            Scene.Remove(this);
        }

        public virtual void Created() { }
        public virtual void Removed() { }

        public virtual void Update(double deltaTime)
        {
            AnimationFrame += AnimationSpeed * deltaTime / Stopwatch.Frequency;
            AnimationFrame %= Animation.Frames.Count;
        }

        public virtual void Draw()
        {
            if (Animation != null)
                Animation.Draw(Console, (int)X, (int)Y, Depth, AnimationFrame);
        }
    }
}
