using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.NeuralNetwork
{
    internal static class Utils
    {
        public static int Product(this int[] a)
        {
            int result = 1;

            for (int i = 0; i < a.Length; i++)
            {
                result *= a[i];
            }

            return result;
        }

        public static int[] RemoveAxis(this int[] array, int index)
        {
            int[] result = new int[array.Length - 1];

            if (index > 0)
                Array.Copy(array, result, index - 1);
            if (index < array.Length - 1)
                Array.Copy(array, index + 1, result, index, array.Length - index - 1);

            return result;
        }

        public static int MaxIndex(this double[] a, int indexFrom = 0, int? length = null)
        {
            int result = -1;
            double max = double.MinValue;
            int indexTo = length == null ? a.Length : indexFrom + length.Value;
            for (int i = indexFrom; i < indexTo; i++)
            {
                if (a[i] > max)
                {
                    max = a[i];
                    result = i;
                }
            }
            return result;
        }
    }
}
