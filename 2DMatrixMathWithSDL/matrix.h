#pragma once
#include <cassert>
#include <memory>

template <class T>
class Matrix
{
  public:
    Matrix<T>(int rows, int columns);
    Matrix<T>(int rows, int columns, T data[]);
    ~Matrix();

    T Get(uint row, uint column) const;
    void Set(int row, int column, const T value);
    void Set(T *data);
    void Print() const;

    std::unique_ptr<Matrix<T>> Multiply(const Matrix<T> &m) const;

    uint Rows() const { return _rows; }
    uint Columns() const { return _columns; }

  private:
    T *_matrix;
    uint _rows;
    uint _columns;

    int ToIndex(uint row, uint column) const;
    void CopyArrayToMatrix(const T data[]);
};

template <class T>
Matrix<T>::Matrix(int rows, int columns)
{
    std::cout << "constructor" << std::endl;

    _rows = rows;
    _columns = columns;

    // TODO: Are these arrays store row major or column major by C++? Fill with
    // 1, 2, 3, ... and inspect memory.

    // Given that we know the dimensions of the matrix, we could store the data
    // as a single-dimensional array and when getting and setting elements
    // transform the 2D coordinate into 1D. But why have C++ do that work for
    // us. Assuming C++ internal storage of the array is efficient with respect
    // to iterating it, the multidimensional approach translates nicely to 3D.

    // Arguments for storing as 1D array. Create a 2D array and passing it to Set
    // becomes cumbersome (my first attempt):
    //   double x[][2] = { { 1, 2 },
    //                     { 3, 4 } };
    // over
    //   double x[] = { 1, 2, 3, 4 };
    _matrix = new T[rows * columns];
}

template <class T>
Matrix<T>::Matrix(int rows, int columns, T data[])
    : Matrix(rows, columns)
{
    CopyArrayToMatrix(data);
}

template <class T>
Matrix<T>::~Matrix()
{
    std::cout << "destructor" << std::endl;
    if (_matrix)
    {
        delete _matrix;
    }
}

template <class T>
void Matrix<T>::CopyArrayToMatrix(const T data[])
{
    assert(data);

    // Copy data rather than "_matrix = data" or we risk double freeing matrix =
    // data after it's been freed by calling code.
    auto length = Rows() * Columns();
    for (auto i = 0; i < length; i++)
    {
        _matrix[i] = data[i];
    }
}

template <class T>
int Matrix<T>::ToIndex(uint row, uint column) const
{
    return row * _rows + column;
}

template <class T>
T Matrix<T>::Get(uint row, uint column) const
{
    return _matrix[ToIndex(row, column)];
}

template <class T>
void Matrix<T>::Set(int row, int column, const T value)
{
    _matrix[ToIndex(row, column)] = value;
}

template <class T>
void Matrix<T>::Set(T *data)
{
    CopyArrayToMatrix(data);
}

template <class T>
void Matrix<T>::Print() const
{
    for (int i = 0; i < _rows; i++)
    {
        for (int j = 0; j < _columns; j++)
        {
            std::cout << _matrix[ToIndex(i, j)] << "\t";
        }
        std::cout << std::endl;
    }
}

template <class T>
std::unique_ptr<Matrix<T>> Matrix<T>::Multiply(const Matrix<T> &b) const
{
    // When multiplying two matrices of dimensions a x b and c x d,
    // multiplication is allowed only if b = c. The resulting matrix has
    // dimensions a x d.
    assert(Columns() == b.Rows());

    auto c = std::make_unique<Matrix<T>>(Rows(), b.Columns());
    for (int i = 0; i < Rows(); i++)
    {
        for (int j = 0; j < b.Columns(); j++)
        {
            T cij = 0;
            for (int k = 0; k < Columns(); k++)
            {
                cij += Get(i, k) * b.Get(k, j);
            }
            c->Set(i, j, cij);
        }
    }
    return std::move(c);
}

// overload +
// overload -
// overload *
// T operator[](int nIndex) const