using static System.Math;

namespace Graphics.Engine
{
    public class Transform2D
    {
        public Matrix _transform;
        Matrix _operation = new Matrix(3, 3);
        bool _first = true;

        public Transform2D Translate(double tx, double ty)
        {
            _operation = new Matrix(3, 3, new double[]
            {
                1,  0,  0,
                0,  1,  0,
                tx, ty, 1
            });
            Multiply();
            return this;        
        }

        public Transform2D Scale(double sx, double sy)
        {
            _operation = new Matrix(3, 3, new double[]
            {
                sx, 0, 0,
                0, sy, 0,
                0, 0,  1
            });
            Multiply();
            return this;
        }

        public Transform2D Rotate(double theta)
        {
            _operation = new Matrix(3, 3, new double[]
            {
                Cos(theta),  Sin(theta), 0,
                -Sin(theta), Cos(theta), 0,
                0,           0,          1
            });
            Multiply();
            return this;
        }

        public Matrix Build() => _transform;

        private void Multiply() 
        {
            if (_first)
            {
                _transform = _operation;
                _first = false;
                return;
            }
            _transform = _transform * _operation;
        }
    }
}