﻿using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Asscii.Graphics;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Asscii.Game
{
    public abstract class GameScene
    {
        private long _nextObjectId = 0;
        private ConcurrentDictionary<long, GameComponent> _components;
        private ManualResetEvent _mre;

        public FastConsole Console { get; private set; }
        public short Width { get; private set; }
        public short Height { get; private set; }
        public float FPS { get; set; }

        public readonly Random Random = new Random();

        public GameScene(short width, short height, short fps) {
            Width = width;
            Height = height;
            FPS = fps;
        }

        public virtual void Init() { }

        protected virtual void Update(double deltaTime) {
            foreach (var obj in _components.Values) {
                obj.Update(deltaTime);
            }
        }

        protected virtual void Render() {
            foreach (var obj in _components.Values) {
                obj.Draw();
            }
            Console.Render();
        }

        public void Loop() {
            System.Console.OutputEncoding = Charset.Encoding;
            System.Console.CursorVisible = false;
            System.Console.SetWindowSize(Width, Height);
            System.Console.SetBufferSize(Width, Height);

            Console = new FastConsole(Width, Height);
            _components = new ConcurrentDictionary<long, GameComponent>();

            Init();

            _mre = new ManualResetEvent(false);

            Task.Run(() => {
                var sw = new Stopwatch();
                sw.Start();

                long currentFrameTime = 0, lastFrameProcessingTime;

                while (true) {
                    lastFrameProcessingTime = sw.ElapsedMilliseconds;

                    var msPerFrame = 1000.0 / FPS;
                    var timeToWait = (int)(msPerFrame - lastFrameProcessingTime);
                    if (timeToWait > 0) {
                        Thread.Sleep(timeToWait);
                    }

                    currentFrameTime = sw.ElapsedMilliseconds;
                    sw.Restart();

                    Update(currentFrameTime / 1000.0);
                    Render();
                }
            });

            _mre.WaitOne();
        }

        public virtual void Exit() {
            if (_mre != null) {
                _mre.Set();
                _mre.Close();
            }
        }

        public long NextObjectID() {
            return _nextObjectId++;
        }

        public void Add(GameComponent obj) {
            if (obj.ID == -1)
                obj.AddToScene(this);
            else {
                _components.TryAdd(obj.ID, obj);
                obj.Created();
            }
        }

        public void Remove(GameComponent obj) {
            if (obj.ID != -1) {
                _components.TryRemove(obj.ID, out obj);
                obj.Removed();
            }
        }
    }
}
