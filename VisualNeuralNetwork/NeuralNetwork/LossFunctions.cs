using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.NeuralNetwork
{
    public abstract class LossFunction
    {
        public abstract double ComputeLoss(Tensor predicted, Tensor target);
        public abstract Tensor ComputeGradient(Tensor predicted, Tensor target);
    }

    public class MSE : LossFunction
    {
        public override Tensor ComputeGradient(Tensor predicted, Tensor target)
        {
            return predicted - target;
        }

        public override double ComputeLoss(Tensor predicted, Tensor target)
        {
            Tensor diff = predicted - target;
            return (diff ^ 2).Mean().Data[0];
        }

        public override string ToString()
        {
            return "MSE";
        }
    }

    public class SoftmaxLoss : LossFunction
    {
        public override double ComputeLoss(Tensor predicted, Tensor target)
        {
            Tensor softmax = predicted.Softmax(axis: 1);
            Tensor loss = -1 * (target * softmax.Log()).Mean();
            return loss.Data[0];
        }

        public override Tensor ComputeGradient(Tensor predicted, Tensor target)
        {
            Tensor softmax = predicted.Softmax(axis: 1);
            Tensor grad = softmax - target;
            return grad;
        }

        public override string ToString()
        {
            return "Softmax";
        }
    }

    public class CrossEntropyLoss : LossFunction
    {
        public override double ComputeLoss(Tensor predicted, Tensor target)
        {
            var loss = -1 * (target * predicted.Log() + (1 - target) * (1 - predicted).Log()).Mean();
            return loss.Data[0];
        }

        public override Tensor ComputeGradient(Tensor predicted, Tensor target)
        {
            predicted = predicted.Softmax(1);
            double[] result = new double[predicted.Data.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = -(target.Data[i] / predicted.Data[i]) + (1 - target.Data[i]) / (1 - predicted.Data[i]);
            }
            return new Tensor(result, predicted.Shape);
        }

        public override string ToString()
        {
            return "CrossEntropy";
        }
    }
}
