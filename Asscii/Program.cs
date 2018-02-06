using System;
using Asscii.Input;
using Asscii.Graphics;
using Asscii.Game;

namespace Asscii
{
    static class Sprites
    {
        public static readonly Sprite Player = Sprite.Load("Assets/player.ans");
        public static readonly Sprite Colors = Sprite.Load("Assets/test.ans");
    }

    static class Animations
    {
        public static readonly Animation Player = Animation.FromSprite(Sprites.Player);
        public static readonly Animation CircleParticle = Animation.Load("Assets", "circle_particle*.ans");
    }

    class Player : GameObject
    {
        const double Acceleration = 0.1;
        const double Deacceleration = 0.05;
        double SpeedX;
        double SpeedY;

        public override void Created()
        {
            Animation = Animations.Player;
            SpeedX = 0;
            SpeedY = 0;
        }

        public override void Update(double deltaTime)
        {
            bool moved = false;
            if (Keyboard.IsPressed(Key.Up))
            {
                SpeedY -= Acceleration;
                moved = true;
            }
            if (Keyboard.IsPressed(Key.Right))
            {
                SpeedX += Acceleration;
                moved = true;
            }
            if (Keyboard.IsPressed(Key.Left))
            {
                SpeedX -= Acceleration;
                moved = true;
            }
            if (Keyboard.IsPressed(Key.Down))
            {
                SpeedY += Acceleration;
                moved = true;
            }

            if (!moved)
            {
                double dist = Math.Sqrt(SpeedX * SpeedX + SpeedY * SpeedY);
                if (dist != 0)
                {
                    double diff = Math.Max(dist - Deacceleration, 0) / dist;
                    SpeedX *= diff;
                    SpeedY *= diff;
                }
            }
            else
            {
                while (Random.Next(0, 2) == 0)
                {
                    GameObject particle = new PlayerRocketParticle();
                    particle.X = X + Random.Next(-1, 2);
                    particle.Y = Y + 4;
                    Scene.Add(particle);
                }
            }

            X += SpeedX;
            Y += SpeedY;
        }
    }

    class PlayerRocketParticle : GameObject
    {
        public override void Created()
        {
            Animation = Animations.CircleParticle;
            AnimationSpeed = 10;
            Depth = 1;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (AnimationFrame >= 10)
            {
                Remove();
                return;
            }

            X += (Random.NextDouble() - 0.5);
            Y += Random.NextDouble() * 0.3;
        }
    }

    class StatusBar : GameObject
    {
        public override void Update(double deltaTime)
        {
            /*for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    Console.Buffer[j + 5, i + 5] = new FastConsole.CharInfo((byte)(i * 32 + j), ConsoleColor.White, ConsoleColor.Black);
                }
            }*/
            Console.Write(0, 0, -1, string.Format("Delta time: {0,7} FPS: {1,3}", deltaTime, Math.Round((double)System.Diagnostics.Stopwatch.Frequency / deltaTime)));
        }
    }

    class Scene1 : GameScene
    {
        public Scene1(short width, short height, short fps) : base(width, height, fps) {}

        public override void Init()
        {
            Add(new StatusBar());
            Add(new Player());
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            // I want a game screen of 16:9, but the character dimensions in the console is 8x12 pixels.
            // This means i'd have to multiply the width of the "logical" game screen by 12/8 = 1.5 to get the correct visible screen size.
            // I went with 80x45 which gives me a nice round 120x45
            GameScene scene = new Scene1(120, 45, 30);
            scene.Loop();
        }
    }
}
