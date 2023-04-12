using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.NeuralNetwork
{
    public abstract class Layer : ViewModelBase
    {
        public abstract Tensor Weights { get; }
        public abstract Tensor Biases { get; }
        public abstract ActivationFunction? Activation { get; }
        public abstract Tensor BackProp(Tensor input, Tensor lossGradient, double learningRate);
        public abstract Tensor PreActivation(Tensor input);
        public Func<double, double>? Activate => Activation != null ? Activation.Activate : null;
        public Func<double, double>? ActivationDerivative => Activation != null ? Activation.Derivative : null;
        public virtual Tensor FeedForward(Tensor input)
        {
            return PreActivation(input).Apply(Activate);
        }

        internal virtual void SyncWeights(Layer layer)
        {

        }
    }
}
