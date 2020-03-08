using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static SDL2.SDL;
using static SDL2.SDL_ttf;
using static System.Math;

namespace SDLWithCS
{
    // ----------------- Matrix ---------------------------
    public interface ICalculator
    {        
    }

    public interface ICalculator<T> : ICalculator
    {
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, T b);
        T Divide(T a, T b);            
    }

    static class Calculators
    {
        public static readonly Dictionary<Type, ICalculator> calculators = new Dictionary<Type, ICalculator>()
        {
            { typeof(int), new IntCalculator() },
            { typeof(double), new DoubleCalculator() }
        };

        public static ICalculator<T> GetInstance<T>() => (ICalculator<T>)calculators[typeof(T)];

        class IntCalculator : ICalculator<int> 
        {
            public int Add(int a, int b) => a + b;
            public int Subtract(int a, int b) => a - b;
            public int Multiply(int a, int b) => a * b;
            public int Divide(int a, int b) => a / b;
        }

        class DoubleCalculator : ICalculator<double>
        {
            public double Add(double a, double b) => a + b;
            public double Subtract(double a, double b) => a - b;
            public double Multiply(double a, double b) => a * b;
            public double Divide(double a, double b) => a / b;
        }
    }

    public class Matrix<T>
    {
        // TODO: Possible use Span<T> to avoid bounds checking on array and similar features
        // making C# implementation very slow compared to C++.

        static readonly ICalculator<T> Calculator = Calculators.GetInstance<T>();
        readonly T[,] _matrix;

        public int Rows => _matrix.GetLength(0);
        public int Columns => _matrix.GetLength(1);

        public Matrix(int rows, int columns)
        {
            _matrix = new T[rows, columns];
        }

        public T this[int row, int column] 
        {
            get => _matrix[row, column];
            set => _matrix[row, column] = value;
        } 

        // Data most be passed row-wise.
        public Matrix(int rows, int columns, T[] data) : this(rows, columns)
        {
            if (data.Length != rows * columns)
                throw new Exception("Too little data");

            for (var i = 0; i < Rows; i++)
                for (var j = 0; j < Columns; j++)
                    this[i, j] = data[i * Columns + j];
        }

        public T Get(int row, int column) => _matrix[row, column];
        public void Set(int row, int column, T value) => _matrix[row, column] = value;
        
        public static Matrix<T> operator+(Matrix<T> a, Matrix<T> b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new Exception("Mismatched dimensions");

            var r = new Matrix<T>(a.Rows, a.Columns);
            for (var i = 0; i < a.Rows; i++)
                for (var j = 0; j < a.Rows; j++)
                    r[i, j] = Calculator.Add(a[i, j], b[i, j]);

            return r;
        }

        public static Matrix<T> operator-(Matrix<T> a, Matrix<T> b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new Exception("Mismatched dimensions");

            var r = new Matrix<T>(a.Rows, a.Columns);
            for (var i = 0; i < a.Rows; i++)
                for (var j = 0; j < a.Rows; j++)
                    r[i, j] = Calculator.Subtract(a[i, j], b[i, j]);

            return r;
        }

        public static Matrix<T> operator*(Matrix<T> a, Matrix<T> b)
        {
            if (a.Columns != b.Rows)
                throw new Exception("Dimension mismatch");

            var r = new Matrix<T>(a.Rows, b.Columns);
            for (var i = 0; i < a.Rows; i++)
            {
                for (var j = 0; j < b.Columns; j++)
                {
                    T cij = default;
                    for (var k = 0; k < a.Columns; k++)
                        cij = Calculator.Add(cij, Calculator.Multiply(a[i, k], b[k, j]));
                    r[i, j] = cij;
                }
            }

            return r;
        }

        public static Matrix<T> operator/(Matrix<T> a, Matrix<T> b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new Exception("Mismatched dimensions");

            var r = new Matrix<T>(a.Rows, a.Columns);
            for (var i = 0; i < a.Rows; i++)
                for (var j = 0; j < a.Rows; j++)
                    r[i, j] = Calculator.Divide(a[i, j], b[i, j]);

            return r;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < _matrix.GetLength(0); i++)
            {
                if (i != 0)
                    sb.Append("\n");

                for (var j = 0; j < _matrix.GetLength(1); j++)
                {
                    if (j != 0) 
                        sb.Append(", ");
                    sb.Append(this[i, j]);
                }
            }

            return sb.ToString();            
        }
    }
    // ----------------- Matrix ---------------------------

    public class Transform2D
    {
        public Matrix<double> Transform { get; private set; }
        Matrix<double> _operation = new Matrix<double>(3, 3);
        bool _first = true;

        public void Translate(double tx, double ty)
        {
            _operation = new Matrix<double>(3, 3, new double[]
            {
                1,  0,  0,
                0,  1,  0,
                tx, ty, 1
            });
            Multiply();            
        }

        public void Scale(double sx, double sy)
        {
            _operation = new Matrix<double>(3, 3, new double[]
            {
                sx, 0, 0,
                0, sy, 0,
                0, 0,  1
            });
            Multiply();
        }

        public void Rotate(double theta)
        {
            _operation = new Matrix<double>(3, 3, new double[]
            {
                Cos(theta),  Sin(theta), 0,
                -Sin(theta), Cos(theta), 0,
                0,           0,          1
            });
            Multiply();
        }

        private void Multiply() 
        {
            if (_first)
            {
                Transform = _operation;
                _first = false;
                return;
            }

            Transform = Transform * _operation;
        }
    }

    // ----------------- Graphics engine ------------------
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
        protected int _frameCounter;
        protected Key[] _keys = new Key[NumKeys];
        protected Mouse _mouse = new Mouse();

        string _title;
        int _frameCount;

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

        public void Initialize(string title, int x, int y, int width, int height)
        {
            Sdl(SDL_Init(SDL_INIT_EVERYTHING));
            _window = Sdl(SDL_CreateWindow(title, x, y, width, height, 0));
            _renderer = Sdl(SDL_CreateRenderer(_window, 0, 0));
            Sdl(TTF_Init());
            _font = Sdl(TTF_OpenFont("FreeSans.ttf", 18));
            Sdl(SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 0));
            Running = true;
        }

        public void Run()
        {
            while (Running)
            {
                HandleEvents();
                UpdateState();
                RenderFrame();
            }
        }

        public void HandleEvents()
        {
            Sdl(SDL_PollEvent(out SDL_Event e));
            switch (e.type)
            {                     
                case SDL_EventType.SDL_QUIT:
                    Running = false;
                    break;
            }

            HandleKeyboard();
            HandleMouse();
        }

        public void UpdateState()
        {
            OnUpdateState();            
        }

        public virtual void OnUpdateState()
        {
        }

        public virtual void OnRenderFrame(long elapsedTime)
        {            
        }

        public void RenderFrame()
        {
            Sdl(SDL_RenderClear(_renderer));
            //RenderFrameCounter();

            SDL_SetWindowTitle(_window, "foobar");
            _frameCount++;

            OnRenderFrame(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            SDL_RenderPresent(_renderer);
        }

        private void HandleKeyboard()
        {
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
        }

        private void HandleMouseButton(ref Button b, uint state)
        {
            bool down = state > 0;
            bool wasDown = b.Down;
            b.Down = down;
            b.Pressed = down && !wasDown;
            b.Released = !down && wasDown;
        }

        private void HandleMouse()
        {
            var state = SDL_GetMouseState(out int x, out int y);
            _mouse.X = x;
            _mouse.Y = y;

            HandleMouseButton(ref _mouse.Left, state & SDL_BUTTON(SDL_BUTTON_LEFT));
            HandleMouseButton(ref _mouse.Middle, state & SDL_BUTTON(SDL_BUTTON_MIDDLE));
            HandleMouseButton(ref _mouse.Right, state & SDL_BUTTON(SDL_BUTTON_RIGHT));
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

        // private void RenderFrameCounter()
        // {
        //     SDL_Rect rect = new SDL_Rect();
        //     IntPtr texture = IntPtr.Zero;
        //     var buffer = $"{_frameCounter}";
        //     GetTextureAndRect(10, 10, buffer, _font, ref texture, ref rect);

        //     if (SDL_RenderCopy(_renderer, texture, IntPtr.Zero, ref rect) != 0)
        //         throw new Exception(SDL_GetError());

        //     SDL_DestroyTexture(texture);
        // }

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
            SDL_Rect rect = new SDL_Rect();
            IntPtr texture = IntPtr.Zero;
            GetTextureAndRect(x, y, text, _font, ref texture, ref rect);

            if (SDL_RenderCopy(_renderer, texture, IntPtr.Zero, ref rect) != 0)
                throw new Exception(SDL_GetError());

            SDL_DestroyTexture(texture);            
        }
    }
    // ----------------- Graphics engine ------------------
}