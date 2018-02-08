
using System;

using Asscii.Graphics;

namespace Asscii.Game
{
    public class GameComponent
    {
        public long ID { get; private set; }
        protected GameScene Scene { get; private set; }
        protected FastConsole Console {
            get { return Scene == null ? null : Scene.Console; }
        }

        public Random Random {
            get { return Scene == null ? null : Scene.Random; }
        }

        public GameComponent() {
            ID = -1;
        }

        public void AddToScene(GameScene scene) {
            if (ID != -1)
                throw new InvalidOperationException("Object already added to scene");

            Scene = scene;
            ID = scene.NextObjectID();
            scene.Add(this);
        }

        public void Remove() {
            Scene.Remove(this);
        }

        public virtual void Created() { }
        public virtual void Removed() { }

        public virtual void Update(double deltaTime) { }
        public virtual void Draw() { }
    }
}
