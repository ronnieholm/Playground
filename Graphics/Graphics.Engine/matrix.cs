using System;
using System.Runtime.CompilerServices;
using System.Text;

// Unlinke C++ templates, C# generics doesn't directly support specializing
// types based on primitive types. C# by design doesn't support a type
// constraint as "integral" that has +, -, *, / operations defined. A
// approximate solution exist, but based on runtime dispatch its slow:
//
// https://gist.github.com/klmr/314d05b66c72d62bd8a184514568e22f
//
// We don't have a urgent need to use anything but doubles for now.

namespace Graphics.Engine
{
    public class Matrix
    {
        // TODO: Possible use Span<T> to avoid bounds checking on array and
        // similar features making C# implementation slow compared to C++.
        readonly double[,] _matrix;

        public int Rows => _matrix.GetLength(0);
        public int Columns => _matrix.GetLength(1);

        public Matrix(int rows, int columns)
        {
            _matrix = new double[rows, columns];
        }

        public double this[int row, int column] 
        {
            get => _matrix[row, column];
            set => _matrix[row, column] = value;
        } 

        // Data most be passed row-wise.
        public Matrix(int rows, int columns, double[] data) : this(rows, columns)
        {
            if (data.Length != rows * columns)
                throw new Exception("Too little data");

            for (var i = 0; i < Rows; i++)
                for (var j = 0; j < Columns; j++)
                    this[i, j] = data[i * Columns + j];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Get(int row, int column) => _matrix[row, column];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int row, int column, double value) => _matrix[row, column] = value;
        
        public static Matrix operator+(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new Exception("Mismatched dimensions");

            var r = new Matrix(a.Rows, a.Columns);
            for (var i = 0; i < a.Rows; i++)
                for (var j = 0; j < a.Rows; j++)
                    r[i, j] = a[i, j] + b[i, j];
            return r;
        }

        public static Matrix operator-(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new Exception("Mismatched dimensions");

            var r = new Matrix(a.Rows, a.Columns);
            for (var i = 0; i < a.Rows; i++)
                for (var j = 0; j < a.Rows; j++)
                    r[i, j] = a[i, j] - b[i, j];
            return r;
        }

        public static Matrix operator*(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
                throw new Exception("Dimension mismatch");

            var r = new Matrix(a.Rows, b.Columns);
            for (var i = 0; i < a.Rows; i++)
            {
                for (var j = 0; j < b.Columns; j++)
                {
                    double cij = 0.0;
                    for (var k = 0; k < a.Columns; k++)
                        cij += a[i, k] * b[k, j];
                    r[i, j] = cij;
                }
            }
            return r;
        }

        public static Matrix operator/(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new Exception("Mismatched dimensions");

            var r = new Matrix(a.Rows, a.Columns);
            for (var i = 0; i < a.Rows; i++)
                for (var j = 0; j < a.Rows; j++)
                    r[i, j] = a[i, j] / b[i, j];
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
}