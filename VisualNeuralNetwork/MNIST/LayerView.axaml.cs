using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VisualNeuralNetwork.NeuralNetwork;

namespace VisualNeuralNetwork.MNIST
{
    public class DataContextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Tensor arr)
            {
                if (arr.Shape.Length == 1)
                {
                    float[] y = arr.Data.Select(f => (float)f).ToArray();
                    return new XyPlotModel(
                        y,
                        x: Enumerable.Range(1, y.Length + 1).Select(b => (float)b).ToArray()
                    );
                }

                List<XyPlotModel> result = new();
                int index = 0;
                for (int i = 0; i < arr.Shape[0]; i++)
                {
                    float[] y = new float[arr.Shape[1]];
                    for (int j = 0; j < y.Length; j++)
                    {
                        y[j] = (float)arr.Data[index++];
                    }
                    result.Add(new XyPlotModel(
                        y,    
                        x: Enumerable.Range(1, y.Length + 1).Select(b => (float)b).ToArray()
                    ));
                }

                return result;
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public partial class LayerView : UserControl
    {
        public LayerView()
        {
            InitializeComponent();
        }

        private void weightPlot_Loaded(object sender, System.EventArgs e)
        {
            if (sender is XYPlot plot)
            {
                if (plot.HorizontalLines.Count == 0)
                {
                    plot.HorizontalLines.Add(new LinesDefinition(0, 1, true, XYPlot.Black));

                    //var pd = DependencyPropertyDescriptor.FromProperty(Plot.CurrentDataPointProperty, typeof(Plot));

                    //pd.AddValueChanged(plot, OnCurrentValueChanged);
                }
            }
        }
    }
}
