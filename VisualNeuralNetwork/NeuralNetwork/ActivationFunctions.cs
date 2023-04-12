using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.NeuralNetwork
{
    public abstract class ActivationFunction
    {
        public abstract double Activate(double x);
        public abstract double Derivative(double x);
    }

    public class ReLU : ActivationFunction
    {
        public override double Activate(double x)
        {
            return Math.Max(x, 0);
        }

        public override double Derivative(double x)
        {
            return x > 0 ? 1 : 0;
        }
    }

    public class Sigmoid : ActivationFunction
    {
        public override double Activate(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        public override double Derivative(double x)
        {
            return Activate(x) * (1 - Activate(x));
        }
    }   
}
