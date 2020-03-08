using System.Collections.Generic;
using static SDL2.SDL;

// https://gist.github.com/klmr/314d05b66c72d62bd8a184514568e22f
// https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/olcPGEX_Graphics2D.h
// https://www.youtube.com/watch?v=fdAOPHgW7qM

namespace SDLWithCS
{
    public class TrigEngine : GraphicsEngine
    {
        readonly List<Matrix<double>> _startingPoints = new List<Matrix<double>>();
        double _translateX;
        double _translateY;
        double _scaleX = 1;
        double _scaleY = 1;
        double _rotateAngle = 0;

        public TrigEngine()
        {
            _startingPoints.Add(new Matrix<double>(1, 3, new double[] { 200, 200, 1 }));
            _startingPoints.Add(new Matrix<double>(1, 3, new double[] { 400, 200, 1 }));
            _startingPoints.Add(new Matrix<double>(1, 3, new double[] { 400, 400, 1 }));
            _startingPoints.Add(new Matrix<double>(1, 3, new double[] { 200, 400, 1 }));            
            _startingPoints.Add(new Matrix<double>(1, 3, new double[] { 200, 200, 1 }));
        }

        public override void OnUpdateState()
        {
            _frameCounter++;
            if (_mouse.Left.Pressed)
                System.Console.WriteLine("Left mouse");

            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_ESCAPE].Pressed)
            {
                _translateX = 0;
                _translateY = 0;
                _scaleX = 1;
                _scaleY = 1;
                _rotateAngle = 0;
            }

            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_LSHIFT].Down && _keys[(int)SDL_Scancode.SDL_SCANCODE_UP].Down)
            {
                _scaleX += 0.001;
                _scaleY += 0.001;
                return;
            }

            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_LSHIFT].Down && _keys[(int)SDL_Scancode.SDL_SCANCODE_DOWN].Down)
            {
                _scaleX -= 0.001;
                _scaleY -= 0.001;
                return;
            }

            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_LCTRL].Down && _keys[(int)SDL_Scancode.SDL_SCANCODE_DOWN].Down)
            {
                _rotateAngle += 0.001;
                return;
            }

            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_LCTRL].Down && _keys[(int)SDL_Scancode.SDL_SCANCODE_UP].Down)
            {
                _rotateAngle -= 0.001;
                return;
            }

            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_LEFT].Down)
                _translateX -= 0.1;
            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_RIGHT].Down)
                _translateX += 0.1;
            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_UP].Down)
                _translateY -= 0.1;
            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_DOWN].Down)
                _translateY += 0.1;

            if (_keys[(int)SDL_Scancode.SDL_SCANCODE_C].Pressed)
                _rotateAngle += 0.1;

        }

        public override void OnRenderFrame(long elapsedTime)
        {
            List<Matrix<double>> updatedPoints = new List<Matrix<double>>();
            var transform = new Transform2D();
            transform.Translate(-300, -300);
            transform.Scale(_scaleX, _scaleY);
            transform.Rotate(_rotateAngle);
            transform.Translate(_translateX, _translateY);
            transform.Translate(300, 300);

            var t = transform.Transform;
            foreach (var p in _startingPoints)
                updatedPoints.Add(p * t);

            for (var i = 1; i < updatedPoints.Count; i++)
                DrawLine(
                    (int)updatedPoints[i - 1][0,0], 
                    (int)updatedPoints[i - 1][0,1], 
                    (int)updatedPoints[i][0,0], 
                    (int)updatedPoints[i][0,1]);
            
            DrawText(10, 10, $"Tx: {_translateX}");
            DrawText(10, 35, $"Ty: {_translateY}");
            DrawText(10, 60, $"Sx: {_scaleX}");
            DrawText(10, 85, $"Sy: {_scaleY}");
            DrawText(10, 110, $"r:  {_rotateAngle}");
        }
    }

    class Program
    {
        const int Fps = 60;
        const int FrameDelay = 1000 / Fps;

        static void Main(string[] args)
        {
            var m1 = new Matrix<double>(2, 2, new double[] { 1,2,3,4 });
            var m2 = new Matrix<double>(2, 2, new double[] { 1,2,3,4 });
            var m3 = m1 * m2;

            var v1 = new Matrix<double>(1, 3, new double[] { 0, 0, 1 });
            System.Console.WriteLine(v1);
            System.Console.WriteLine();

            var t = new Transform2D();
            //t.Rotate(45);
            t.Translate(4, 5);
            t.Scale(7, 8);
            System.Console.WriteLine(t.Transform);
            System.Console.WriteLine();

            var v2 = v1 * t.Transform;
            System.Console.WriteLine(v2);

            // var m6 = t.Transform;
            // System.Console.WriteLine(m6);

            var engine = new TrigEngine();
            engine.Initialize("Hello", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 512, 512);
            engine.Run();
        }
    }
}
