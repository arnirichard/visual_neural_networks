using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.NeuralNetwork
{
    class Rand
    {
        static Random random = new Random();

        public static double[] Next(int n)
        {
            double[] result = new double[n];
            for (int i = 0; i < result.Length; i++)
                result[i] = random.NextDouble() - 0.5;
            return result;
        }

        public static int[] NextIntArray(int upperBound, int length)
        {
            int[] result = new int[length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = NextInt(upperBound);
            }
            return result;
        }

        public static int NextInt(int upperBound)
        {
            return random.Next(upperBound);
        }

        public static double NextDouble()
        {
            return random.NextDouble();
        }
    }
}
