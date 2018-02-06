using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Threading;

using Asscii.Graphics;
using System.Collections.Concurrent;

namespace Asscii.Game
{
    public abstract class GameScene
    {
        private long nextObjectId = 0;
        private ConcurrentDictionary<long, GameObject> objects;

        public FastConsole Console { get; private set; }
        public short Width { get; private set; }
        public short Height { get; private set; }
        public short Fps { get; private set; }

        public long TicksPerFrame
        {
            get { return Stopwatch.Frequency / Fps; }
        }

        private long carryOverFrameTime;
        private Stopwatch frameSw;

        public readonly Random Random = new Random();

        public GameScene(short width, short height, short fps)
        {
            Width = width;
            Height = height;
            Fps = fps;
        }

        public virtual void Init() { }

        public void Update(long deltaTime)
        {
            foreach (GameObject obj in objects.Values)
            {
                // Add elapsed tick because the updates themselves may take time, and frameSw gets reset right before running the updates
                obj.Update(deltaTime + frameSw.ElapsedTicks);
            }
        }

        public void Render()
        {
            foreach (GameObject obj in objects.Values)
            {
                obj.Draw();
            }
            Console.Render();
        }

        public void Loop()
        {
            System.Console.OutputEncoding = Encoding.Default;
            System.Console.CursorVisible = false;
            System.Console.SetWindowSize(Width, Height);
            System.Console.SetBufferSize(Width, Height);

            Console = new FastConsole(Width, Height);
            objects = new ConcurrentDictionary<long, GameObject>();

            Init();

            carryOverFrameTime = 0;
            frameSw = new Stopwatch();
            frameSw.Start();

            while (true)
            {
                long elapsed = WaitForNextFrame();

                Update(elapsed);
                Render();
            }
        }

        private long WaitForNextFrame()
        {
            long elapsed = 0;
            while (true)
            {
                elapsed = frameSw.ElapsedTicks + carryOverFrameTime;
                if (elapsed > TicksPerFrame)
                    break;

                Thread.Sleep(0);
            }
            carryOverFrameTime = elapsed % TicksPerFrame;
            frameSw.Restart();
            return elapsed;
        }

        public long NextObjectID()
        {
            return nextObjectId++;
        }

        public void Add(GameObject obj)
        {
            if (obj.ID == -1)
                obj.AddToScene(this);
            else
            {
                objects.TryAdd(obj.ID, obj);
                obj.Created();
            }
        }

        public void Remove(GameObject obj)
        {
            if (obj.ID != -1) {
                objects.TryRemove(obj.ID, out obj);
                obj.Removed();
            }
        }
    }
}
