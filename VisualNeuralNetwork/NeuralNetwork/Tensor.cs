using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.NeuralNetwork
{
    public class Tensor
    {
        private readonly double[] _data;
        private readonly int[] _shape;

        public Tensor(int[] shape)
        {
            _data = new double[shape.Product()];
            _shape = shape;
        }

        public Tensor(double[] data, int[] shape)
        {
            _data = data;
            _shape = shape;
        }

        public Tensor(ArraySegment<double> values)
        {
            _data = values.ToArray();
            _shape = new int[] { 1, _data.Length };
        }

        public double[] Data => _data;
        public int[] Shape => _shape;

        public double this[params int[] indices]
        {
            get
            {
                if (indices.Length < _shape.Length)
                {
                    throw new ArgumentException("Indices length must be at least the number of tensor dimensions");
                }

                return _data[GetIndex(indices)];
            }
            set
            {
                _data[GetIndex(indices)] = value;
            }
        }

        public int GetIndex(params int[] indices)
        {
            int index = 0;
            int stride = 1;
            int shift = indices.Length - _shape.Length;
            for (int i = _shape.Length - 1; i >= 0; i--)
            {
                if (indices[i + shift] < 0 || indices[i + shift] >= _shape[i])
                {
                    throw new ArgumentException($"Index out of range for dimension {i}: {indices[i]}");
                }

                index += indices[i + shift] * stride;
                stride *= _shape[i];
            }
            return index;
        }

        public Tensor Transpose()
        {
            if (_shape.Length != 2)
                throw new InvalidOperationException("Transpose is only defined for 2-dimensional tensors");

            int rows = _shape[0];
            int cols = _shape[1];
            double[] transposedData = new double[_data.Length];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    transposedData[j * rows + i] = _data[i * cols + j];
                }
            }

            return new Tensor(transposedData, new int[] { cols, rows });
        }

        public Tensor GetSubTensor(int[] indexes, int axis = 0)
        {
            if (axis >= Shape.Length)
            {
                throw new ArgumentException("Axis out of range");
            }
            if (indexes.Any(i => i < 0 || i >= Shape[axis]))
            {
                throw new ArgumentException("Index out of range");
            }

            int[] newShape = (int[])Shape.Clone();
            newShape[axis] = indexes.Length;
            Tensor subTensor = new Tensor(newShape);

            int subTensorIndex = 0;
            int subTensorStride = subTensor.Shape.Skip(axis + 1).Aggregate((acc, val) => acc * val);
            int sourceStride = Shape.Skip(axis + 1).Aggregate((acc, val) => acc * val);
            int index;

            for (int i = 0; i < indexes.Length; i++)
            {
                index = indexes[i];
                int startIndex = index * sourceStride;
                for (int j = 0; j < subTensorStride; j++)
                {
                    subTensor.Data[subTensorIndex++] = Data[startIndex + j];
                }
            }

            return subTensor;
        }

        public Tensor Softmax(int axis)
        {
            if (axis < 0 || axis >= _shape.Length)
            {
                throw new ArgumentException("Invalid axis");
            }

            int dim = _shape[axis];

            // Create a new tensor to hold the softmax output
            Tensor output = new Tensor(_shape);

            // Loop over the specified axis
            for (int i = 0; i < _data.Length; i += dim)
            {
                double[] slice = new double[dim];

                // Copy the slice into a separate array
                Array.Copy(_data, i, slice, 0, dim);

                // Compute the softmax of the slice
                double[] softmax = Softmax(slice);

                // Copy the softmax back into the output tensor
                Array.Copy(softmax, 0, output._data, i, dim);
            }

            return output;
        }

        private double[] Softmax(double[] slice)
        {
            // Used to prevent numerical overflow
            double max = slice.Max();

            double sum = 0;
            for (int i = 0; i < slice.Length; i++)
            {
                sum += Math.Exp(slice[i] - max);
            }

            double[] result = new double[slice.Length];
            for (int i = 0; i < slice.Length; i++)
            {
                result[i] = Math.Exp(slice[i] - max) / sum;
            }

            return result;
        }

        public static Tensor operator -(Tensor a, double b)
        {
            return a.Apply(x => x - b);
        }

        public static Tensor operator -(double b, Tensor a)
        {
            return a.Apply(x => b - x);
        }

        public static Tensor operator -(Tensor a)
        {
            return a.Apply(v => -v);
        }

        public static Tensor operator +(Tensor a, Tensor b)
        {
            return ApplyTensorsOperation(a, b, (a, b) => a + b);
        }

        public static Tensor operator -(Tensor a, Tensor b)
        {
            return ApplyTensorsOperation(a, b, (a, b) => a - b);
        }

        public static Tensor operator /(Tensor a, Tensor b)
        {
            return ApplyTensorsOperation(a, b, (a, b) => a / b);
        }

        public static Tensor operator *(Tensor a, Tensor b)
        {
            return ApplyTensorsOperation(a, b, (a, b) => a * b);
        }

        private static Tensor ApplyTensorsOperation(Tensor a, Tensor b, Func<double, double, double> f)
        {
            if (!a.Shape.SequenceEqual(b.Shape))
            {
                throw new ArgumentException("Tensors must have the same shape.");
            }

            var result = new Tensor(a.Shape);

            for (int i = 0; i < result.Data.Length; i++)
            {
                result.Data[i] = f(a.Data[i], b.Data[i]);
            }

            return result;
        }

        public Tensor Log()
        {
            return Apply(Math.Log);
        }

        public static Tensor operator *(Tensor a, double b)
        {
            return a.Apply(v => v * b);
        }

        public static Tensor operator *(double a, Tensor b)
        {
            return b * a;
        }

        public static Tensor operator /(Tensor a, double b)
        {
            return a.Apply(v => v / b);
        }

        public static Tensor operator /(double a, Tensor b)
        {
            return b.Apply(v => a / v);
        }

        public static Tensor operator ^(Tensor a, double exponent)
        {
            return a.Apply(v => Math.Pow(v, exponent));
        }

        public Tensor Mean(int axis = 0)
        {
            if (axis < 0 || axis >= _shape.Length)
            {
                throw new ArgumentException("Invalid axis specified");
            }

            int[] newShape = _shape.RemoveAxis(axis);
            double[] newData = new double[newShape.Product()];

            if (_shape[axis] == 1)
            {
                Array.Copy(_data, newData, newData.Length);
            }
            else
            {
                int axisSize = _shape[axis];
                int blockSize = newData.Length;
                int[] blockIndices = GetBlockIndices(axis, axisSize, blockSize);

                for (int i = 0; i < newData.Length; i++)
                {
                    double sum = 0.0;
                    for (int j = 0; j < blockIndices.Length; j++)
                    {
                        sum += _data[blockIndices[j] + i];
                    }
                    newData[i] = sum / axisSize;
                }
            }

            return new Tensor(newData, newShape);
        }

        private int[] GetBlockIndices(int axis, int axisSize, int blockSize)
        {
            int[] blockIndices = new int[axisSize];
            for (int i = 0; i < axisSize; i++)
            {
                blockIndices[i] = i * blockSize;
            }
            return blockIndices;
        }

        public Tensor Apply(Func<double, double>? f)
        {
            if (f != null)
            {
                double[] newData = new double[_data.Length];
                for (int i = 0; i < _data.Length; i++)
                {
                    newData[i] = f(_data[i]);
                }
                return new Tensor(newData, _shape);
            }
            return this;
        }

        public Tensor Dot(Tensor other, double mult = 1)
        {
            if (_shape.Length > 2 || other.Shape.Length > 2 || _shape[_shape.Length - 1] != other.Shape[0])
            {
                throw new ArgumentException("Invalid shape for matrix multiplication");
            }

            bool applyMultiplication = mult != 1;
            int m = _shape.Length > 1 ? _shape[0] : 1;
            int n = other.Shape[1];
            int p = _shape[_shape.Length - 1];

            double[] resultData = new double[m * n];
            int index, otherIndex, resultIndex;

            for (int i = 0; i < m; i++)
            {
                resultIndex = i * n;
                for (int j = 0; j < n; j++)
                {
                    double sum = 0;
                    index = i * p;
                    otherIndex = j; //00a other.GetIndex(0, j);
                    for (int k = 0; k < p; k++)
                    {
                        sum += Data[index++] * other.Data[otherIndex];
                        otherIndex += n;
                    }
                    if (applyMultiplication)
                        sum *= mult;
                    resultData[resultIndex++] = sum;
                }
            }

            return new Tensor(resultData, new int[] { m, n });
        }
    }
}
