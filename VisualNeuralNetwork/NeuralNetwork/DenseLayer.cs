using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.NeuralNetwork
{
    public class DenseLayer : Layer
    {
        readonly int _inputSize;
        readonly int _outputSize;
        Tensor _weights;
        Tensor _biases;
        ActivationFunction? _activation;

        public DenseLayer(int inputSize, int outputSize, ActivationFunction? activation)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            _weights = new Tensor(Rand.Next(inputSize * outputSize), new[] { _outputSize, _inputSize });
            _biases = new Tensor(Rand.Next(outputSize), new[] { outputSize });
            _activation = activation;
        }
        public int OutputSize => _outputSize;
        public int InputSize => _inputSize;
        public override Tensor Weights => _weights;
        public override Tensor Biases => _biases;
        public override ActivationFunction? Activation => _activation;

        internal override void SyncWeights(Layer layer)
        {
            if (layer is DenseLayer dl)
            {
                if (_weights.Shape.SequenceEqual(dl._weights.Shape))
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _weights = dl.Weights;
                        this.RaisePropertyChanged(nameof(Weights));
                    });
                }
                if (_biases.Shape.SequenceEqual(dl._biases.Shape))
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _biases = dl.Biases;
                        this.RaisePropertyChanged(nameof(Biases));
                    });
                }
            }
        }

        public override Tensor PreActivation(Tensor input)
        {
            if (input.Shape.Length != 2 || input.Shape[1] != _inputSize)
            {
                throw new ArgumentException("Invalid shape for FeedForward");
            }

            int size = _outputSize * input.Shape[0];
            double[] data = new double[size];
            int dataIndex = 0, inputIndex;
            int weightIndex;

            for (int i = 0; i < input.Shape[0]; i++)
            {
                weightIndex = 0;
                for (int j = 0; j < _outputSize; j++)
                {
                    inputIndex = i * _inputSize;
                    double sum = 0;
                    for (int k = 0; k < _inputSize; k++)
                    {
                        sum += input.Data[inputIndex++] * _weights.Data[weightIndex++];
                    }
                    data[dataIndex++] = sum + _biases[j];
                }
            }
            return new Tensor(data, new int[] { input.Shape[0], _outputSize });
        }

        public override Tensor FeedForward(Tensor input)
        {
            ActivationFunction? activation = _activation;

            if (activation != null)
            {
                if (input.Shape.Length != 2 || input.Shape[1] != _inputSize)
                {
                    throw new ArgumentException("Invalid shape for FeedForward");
                }

                int batchSize = input.Shape[0];
                double[] outputData = new double[batchSize * _outputSize];
                int outputDataIndex = 0, inputDataIndex;
                int weightIndex;
                double sum;

                for (int i = 0; i < batchSize; i++)
                {
                    weightIndex = 0;
                    for (int j = 0; j < _outputSize; j++)
                    {
                        inputDataIndex = i * _inputSize;
                        sum = 0;
                        for (int k = 0; k < _inputSize; k++)
                        {
                            sum += input.Data[inputDataIndex++] * _weights.Data[weightIndex++];
                        }
                        outputData[outputDataIndex++] = activation.Activate(sum + _biases[j]);
                    }
                }

                return new Tensor(outputData, new int[] { batchSize, _outputSize });
            }

            return PreActivation(input);
        }

        public override Tensor BackProp(Tensor input, Tensor lossGradient, double learningRate)
        {
            Tensor dLdW = lossGradient.Transpose().Dot(input, 1.0 / lossGradient.Shape[0]);
            Tensor dLdB = lossGradient.Mean();

            // Calculate gradient of loss with respect to input of this layer
            Tensor dLdX = lossGradient.Dot(_weights);

            // Update weights and biases
            _weights -= learningRate * dLdW;
            _biases -= learningRate * dLdB;

            Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(Weights));
                this.RaisePropertyChanged(nameof(Biases));
            });

            return dLdX;
        }
    }
}
