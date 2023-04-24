using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.NeuralNetwork
{
    class Network
    {
        private List<Layer> layers;
        LossFunction lossFunction;

        public Network(LossFunction lossFunction, List<Layer> layers)
        {
            this.layers = layers;
            this.lossFunction = lossFunction;
        }

        public IReadOnlyList<Layer> Layers => layers.AsReadOnly();

        public void SyncWeights(Network network)
        {
            for (int i = 0; i < network.layers.Count; i++)
            {
                layers[i].SyncWeights(network.layers[i]);
            }
        }

        public void SetLossFunction(LossFunction lossFunction)
        {
            this.lossFunction = lossFunction;
        }

        public void Train(Tensor input, Tensor target, double learningRate)
        {
            Tensor[] inputs = new Tensor[layers.Count];
            Tensor[] preactivations = new Tensor[layers.Count];
            Tensor output = input;
            Layer layer;

            for (int j = 0; j < layers.Count; j++)
            {
                layer = layers[j];
                inputs[j] = output;
                preactivations[j] = layer.PreActivation(output);
                output = preactivations[j].Apply(layer.Activate);
            }

            Tensor lossGradient = lossFunction.ComputeGradient(output, target);

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                layer = layers[i];
                if (layer.ActivationDerivative != null)
                {
                    lossGradient = lossGradient * preactivations[i].Apply(layer.ActivationDerivative);
                }
                lossGradient = layer.BackProp(inputs[i], lossGradient, learningRate);
            }

        }

        public Tensor FeedForward(Tensor input)
        {
            foreach (Layer layer in layers)
            {
                input = layer.FeedForward(input);
            }
            return input;
        }

        internal double[] FeedForward(ArraySegment<double> value)
        {
            return FeedForward(new Tensor(value)).Data;
        }

        internal double[] FeedForward(ArraySegment<byte> value)
        {
            return FeedForward(new Tensor(value.Select(v => v / 255.0).ToArray())).Data;
        }
    }
}
