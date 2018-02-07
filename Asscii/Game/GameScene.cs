using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Threading;

using Asscii.Graphics;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Asscii.Game
{
    public abstract class GameScene
    {
        private long _nextObjectId = 0;
        private ConcurrentDictionary<long, GameObject> _objects;
        private ManualResetEvent _mre;

        public FastConsole Console { get; private set; }
        public short Width { get; private set; }
        public short Height { get; private set; }
        public short Fps { get; private set; }

        public long TicksPerFrame {
            get { return Stopwatch.Frequency / Fps; }
        }

        private long _carryOverFrameTime;
        private Stopwatch _frameSw;

        public readonly Random Random = new Random();

        public GameScene(short width, short height, short fps) {
            Width = width;
            Height = height;
            Fps = fps;
        }

        public virtual void Init() { }

        public void Update(long deltaTime) {
            foreach (var obj in _objects.Values) {
                // Add elapsed tick because the updates themselves may take time, and frameSw gets reset right before running the updates
                obj.Update(deltaTime + _frameSw.ElapsedTicks);
            }
        }

        public void Render() {
            foreach (var obj in _objects.Values) {
                obj.Draw();
            }
            Console.Render();
        }

        public void Loop() {
            System.Console.OutputEncoding = Encoding.Default;
            System.Console.CursorVisible = false;
            System.Console.SetWindowSize(Width, Height);
            System.Console.SetBufferSize(Width, Height);

            Console = new FastConsole(Width, Height);
            _objects = new ConcurrentDictionary<long, GameObject>();

            Init();

            _carryOverFrameTime = 0;
            _frameSw = new Stopwatch();
            _frameSw.Start();

            _mre = new ManualResetEvent(false);

            Task.Run(async () => {
                while (true) {
                    var elapsed = await WaitForNextFrame();

                    Update(elapsed);
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

        private async Task<long> WaitForNextFrame() {
            var elapsed = _frameSw.ElapsedTicks + _carryOverFrameTime;
            if (elapsed < TicksPerFrame) {
                var msPerFrame = 1000 / (double)Fps;
                var waitFor = (int)(msPerFrame - _frameSw.ElapsedMilliseconds);
                await Task.Delay(waitFor);

                elapsed = TicksPerFrame;
            }
            _carryOverFrameTime = elapsed % TicksPerFrame;
            _frameSw.Restart();
            return elapsed;
        }

        public long NextObjectID() {
            return _nextObjectId++;
        }

        public void Add(GameObject obj) {
            if (obj.ID == -1)
                obj.AddToScene(this);
            else {
                _objects.TryAdd(obj.ID, obj);
                obj.Created();
            }
        }

        public void Remove(GameObject obj) {
            if (obj.ID != -1) {
                _objects.TryRemove(obj.ID, out obj);
                obj.Removed();
            }
        }
    }
}
