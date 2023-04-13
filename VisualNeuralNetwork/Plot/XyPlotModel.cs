using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork
{
    internal class XyPlotModel
    {
        public float[] Y { get; }
        public float[]? X { get; }
        
        public XyPlotModel(float[] y, float[]? x = null)
        {
            Y = y;
            X = x;
        }
    }
}
