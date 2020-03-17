using System.Linq;
using Graphics.Engine;
using static SDL2.SDL;

namespace Graphics.Example
{
    public class TransformationEngine : GraphicsEngine
    {
        const int TranslateStep = 3;
        const double ScaleStep = 0.015;
        const double RotateStep = 0.02;

        Matrix[] _initialControlPoints;
        Matrix[] _transformedControlPoints;

        double _translateX;
        double _translateY;
        double _scaleX = 1;
        double _scaleY = 1;
        double _rotateAngle = 0;

        public TransformationEngine()
        {
            SetSquare();
        }

        private (double, double) CalculateCentroid(Matrix[] points)
        {
            // TODO: Add to blog post
            // https://math.stackexchange.com/questions/1801867/finding-the-centre-of-an-abritary-set-of-points-in-two-dimensions
            var xs = points[..^1].Select(p => p[0, 0]).Sum();
            var ys = points[..^1].Select(p => p[0, 1]).Sum();
            var n = points.Length - 1;
            return (xs / n, ys / n);
        }

        private void SetSquare()
        {
            var controlPoints = new[] 
            {
                new Matrix(1, 3, new[] { 200d, 200, 1 }),
                new Matrix(1, 3, new[] { 400d, 200, 1 }),
                new Matrix(1, 3, new[] { 400d, 400, 1 }),
                new Matrix(1, 3, new[] { 200d, 400, 1 }),
                new Matrix(1, 3, new[] { 200d, 200, 1 })
            };

            _initialControlPoints = controlPoints;
            _transformedControlPoints = new Matrix[_initialControlPoints.Length];
        }

        private void SetTriangle()
        {
            var controlPoints = new[] 
            {
                new Matrix(1, 3, new[] { 200d, 200, 1 }),
                new Matrix(1, 3, new[] { 400d, 200, 1 }),
                new Matrix(1, 3, new[] { 200d, 400, 1 }),
                new Matrix(1, 3, new[] { 200d, 200, 1 })
            };

            _initialControlPoints = controlPoints;
            _transformedControlPoints = new Matrix[_initialControlPoints.Length];            
        }

        public override void UpdateState()
        {
            // TODO: Abstract away so we query on F1, F2, and so on and only Pressed. No Down and so on
            var f1 = _keys[(int)SDL_Scancode.SDL_SCANCODE_F1].Down;
            var f2 = _keys[(int)SDL_Scancode.SDL_SCANCODE_F2].Down;
            var ctrl = (_keys[(int)SDL_Scancode.SDL_SCANCODE_LCTRL].Down || _keys[(int)SDL_Scancode.SDL_SCANCODE_RCTRL].Down);
            var shift = (_keys[(int)SDL_Scancode.SDL_SCANCODE_LSHIFT].Down || _keys[(int)SDL_Scancode.SDL_SCANCODE_RSHIFT].Down);
            var left = _keys[(int)SDL_Scancode.SDL_SCANCODE_LEFT].Down;
            var right = _keys[(int)SDL_Scancode.SDL_SCANCODE_RIGHT].Down;
            var up = _keys[(int)SDL_Scancode.SDL_SCANCODE_UP].Down;
            var down = _keys[(int)SDL_Scancode.SDL_SCANCODE_DOWN].Down;        

            if (_mouse.Left.Pressed)
                System.Console.WriteLine("Left mouse");

            if (f1 || f2)
            {
                if (f1)
                    SetSquare();
                if (f2)
                    SetTriangle();

                _translateX = 0;
                _translateY = 0;
                _scaleX = 1;
                _scaleY = 1;
                _rotateAngle = 0;
            }

            if (shift && left)
                _scaleX += ScaleStep * DeltaTime;
            if (shift && right)
                _scaleX -= ScaleStep * DeltaTime;
            if (shift && up)
                _scaleY += ScaleStep * DeltaTime;
            if (shift && down)
                _scaleY -= ScaleStep * DeltaTime;

            if (!(shift || ctrl) && left)
                _translateX -= TranslateStep * DeltaTime;
            if (!(shift || ctrl) && right)
                _translateX += TranslateStep * DeltaTime;
            if (!(shift || ctrl) && up)
                _translateY -= TranslateStep * DeltaTime;
            if (!(shift || ctrl) && down)
                _translateY += TranslateStep * DeltaTime;

            if (ctrl && up)
                _rotateAngle += RotateStep * DeltaTime;
            if (ctrl && down)
                _rotateAngle -= RotateStep * DeltaTime;
        }

        public override void RenderFrame()
        {            
            // We can either calculate  center of mass once and apply
            // transformations to it as if it were a control point of the shape
            // or we can calculate center of mass from transformed control
            // points.
            var (centerOfMassX, centerOfMassY) = CalculateCentroid(_initialControlPoints);
            var transform = new Transform2D()
                .Translate(-centerOfMassX, -centerOfMassY)
                .Scale(_scaleX, _scaleY)
                .Rotate(_rotateAngle)
                // Translation must happen last as scale and rotate shape is
                // centered around origin.
                .Translate(_translateX, _translateY)
                .Translate(centerOfMassX, centerOfMassY);

            var t = transform.Build();
            for (var i = 0; i < _initialControlPoints.Length; i++)
                _transformedControlPoints[i] = _initialControlPoints[i] * t;

            for (var i = 1; i < _transformedControlPoints.Length; i++)
                DrawLine(
                    (int)_transformedControlPoints[i - 1][0,0], 
                    (int)_transformedControlPoints[i - 1][0,1], 
                    (int)_transformedControlPoints[i][0,0], 
                    (int)_transformedControlPoints[i][0,1]);                  

            // Draw cross at centroid.
            var (x, y) = CalculateCentroid(_transformedControlPoints);
            DrawLine((int)x - 10, (int)y - 10, (int)x + 10, (int)y + 10);
            DrawLine((int)x + 10, (int)y - 10, (int)x - 10, (int)y + 10);

            DrawText(10, 10, $"Square : F1");
            DrawText(10, 35, $"Triangle : F2");
            DrawText(10, 60, $"Centroid: ({x:0},{y:0})");
            DrawText(10, 85, $"Tx (Left/Right): {_translateX:0}");
            DrawText(10, 110, $"Ty (Up/Down): {_translateY:0}");
            DrawText(10, 135, $"Sx (Shift + Left/Right): {_scaleX:0.0}");
            DrawText(10, 160, $"Sy (Shift + Up/Down): {_scaleY:0.0}");
            DrawText(10, 185, $"R (Ctrl + Up/Down):  {_rotateAngle:0.0}");            
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var engine = new TransformationEngine();
            engine.Initialize("2D transformations", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 512, 512);
            engine.Run();

            // TODO: Implement IDisposable pattern on engine.
            engine.Cleanup();
        }
    }
}