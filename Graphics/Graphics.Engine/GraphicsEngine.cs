using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

// Inspired by olcPixelGameEngine:
// https://github.com/OneLoneCoder/olcPixelGameEngine

namespace Graphics.Engine
{
    // TODO: Expose custom set of enums representing key and not SDL ones.
    public class GraphicsEngine
    {
        protected struct Key
        {
            public bool Down;
            public bool Pressed;  // Goes from not down to down
            public bool Released; // goes from down to not down
        }

        protected struct Button
        {
            public bool Down;
            public bool Pressed;  // Goes from not down to down
            public bool Released; // goes from down to not down
        }

        protected struct Mouse
        {
            public Button Left;
            public Button Middle;
            public Button Right;
            public int X;
            public int Y;
        }

        public bool Running { get; private set; }

        IntPtr _window;
        IntPtr _renderer;
        IntPtr _font;
        
        const int NumKeys = 512;
        protected Key[] _keys = new Key[NumKeys];
        protected Mouse _mouse = new Mouse();

        public string Title { get; private set; }
        public double DeltaTime { get; private set; }

        public int _targetTicksPerSecond;
        public bool _delayFrameRendering;

        private int Sdl(int code)
        {
            if (code < 0)
                throw new Exception("SDL pooped itself: " + SDL_GetError());
            
            // Wrapping a function such as SDL_PollEvent we need the return
            // value to exit the poll loop. In most other cases the return value
            // can be ignores.
            return code;
        }

        private IntPtr Sdl(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new Exception("SDL pooped itself: " + SDL_GetError());
            return ptr;
        }

        public void Initialize(string title, int x, int y, int width, int height, bool delayRendering = true, int targetTicksPerSecond = 60)
        {
            Sdl(SDL_Init(SDL_INIT_EVERYTHING));
            _window = Sdl(SDL_CreateWindow(title, x, y, width, height, 0));
            _renderer = Sdl(SDL_CreateRenderer(_window, 0, SDL_RendererFlags.SDL_RENDERER_ACCELERATED /*| SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC*/));
            Sdl(TTF_Init());
            _font = Sdl(TTF_OpenFont("../Graphics.Engine/FreeSans.ttf", 18));
            Sdl(SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 0));
            Running = true;
            Title = title;
            _targetTicksPerSecond = targetTicksPerSecond;
            _delayFrameRendering = delayRendering;
            DeltaTime = 1;            
        }

        public void Run()
        {
            double TargetTimePerTick = 1000.0 / _targetTicksPerSecond;
            while (Running)
            {
                var start = SDL_GetTicks();
                HandleInput();
                UpdateState();
                EngineRenderFrame();
                
                // Artificial frame render delay
                //SDL_Delay(100);

                var frameRenderTime = SDL_GetTicks() - start;
                
                if (_delayFrameRendering)
                {
                    var delay = TargetTimePerTick - frameRenderTime;
                    if (delay > 0)
                    {
                        SDL_Delay(Convert.ToUInt32(delay));
                        DeltaTime = 1;
                    }
                }
                else
                    DeltaTime = (frameRenderTime / 1000.0) * _targetTicksPerSecond;
            }
        }

        public void HandleInput()
        {
            Sdl(SDL_PollEvent(out SDL_Event e));
            switch (e.type)
            {                     
                case SDL_EventType.SDL_QUIT:
                    Running = false;
                    break;
            }

            var arrayPtr = Sdl(SDL_GetKeyboardState(out int numKeys));
            Debug.Assert(NumKeys == numKeys);

            var keyState = new byte[NumKeys];
            Marshal.Copy(arrayPtr, keyState, 0, NumKeys);

            for (var i = 0; i < NumKeys; i++)
            {
                bool down = keyState[i] != 0;
                bool wasDown = _keys[i].Down;
                _keys[i].Down = down;
                _keys[i].Pressed = down && !wasDown;
                _keys[i].Released = !down && wasDown;
            }

            var state = SDL_GetMouseState(out int x, out int y);
            _mouse.X = x;
            _mouse.Y = y;

            HandleMouseButton(ref _mouse.Left, state & SDL_BUTTON(SDL_BUTTON_LEFT));
            HandleMouseButton(ref _mouse.Middle, state & SDL_BUTTON(SDL_BUTTON_MIDDLE));
            HandleMouseButton(ref _mouse.Right, state & SDL_BUTTON(SDL_BUTTON_RIGHT));
        }

        public virtual void UpdateState()
        {
        }

        public virtual void RenderFrame()
        {
        }

        public void EngineRenderFrame()
        {
            Sdl(SDL_RenderClear(_renderer));
            // TODO: Computer FPS property by way of 1000 / Delta time?
            SDL_SetWindowTitle(_window, $"{Title} - {DeltaTime:0.00} - {(_targetTicksPerSecond / DeltaTime):0}");
            RenderFrame();
            SDL_RenderPresent(_renderer);
        }

        private void HandleMouseButton(ref Button b, uint state)
        {
            bool down = state > 0;
            bool wasDown = b.Down;
            b.Down = down;
            b.Pressed = down && !wasDown;
            b.Released = !down && wasDown;
        }

        private void GetTextureAndRect(int x, int y, string text, IntPtr ttfFont, ref IntPtr sdlTexture, ref SDL_Rect rect)
        {
            var textColor = new SDL_Color { r = 255, g = 255, b = 255, a = 0 };
            var surface = Sdl(TTF_RenderText_Solid(ttfFont, text, textColor));
            sdlTexture = Sdl(SDL_CreateTextureFromSurface(_renderer, surface));
            var surface2 = (SDL_Surface)Marshal.PtrToStructure(surface, typeof(SDL_Surface));
            var textWidth = surface2.w;
            var textHeight = surface2.h;
            SDL_FreeSurface(surface);
            rect.x = x;
            rect.y = y;
            rect.w = textWidth;
            rect.h = textHeight;
        }

        public void Cleanup()
        {
            SDL_DestroyRenderer(_renderer);
            SDL_DestroyWindow(_window);
            TTF_CloseFont(_font);
            SDL_Quit();
        }

        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            Sdl(SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 0));
            Sdl(SDL_RenderDrawLine(_renderer, x1, y1, x2, y2));
            Sdl(SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 0));
        }

        public void DrawText(int x, int y, string text)
        {
            var rect = new SDL_Rect();
            var texture = IntPtr.Zero;
            GetTextureAndRect(x, y, text, _font, ref texture, ref rect);
            Sdl(SDL_RenderCopy(_renderer, texture, IntPtr.Zero, ref rect));
            SDL_DestroyTexture(texture);            
        }
    }
}