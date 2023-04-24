using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data.Converters;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using VisualNeuralNetwork.NeuralNetwork;

namespace VisualNeuralNetwork.MNIST
{
    class DataContextWrapper : ViewModelBase
    {
        public object? DataContext { get; private set; }

        public DataContextWrapper(object? dataContext)
        {
            DataContext = dataContext;
        }

        public void SetDataContext(object dataContext)
        {
            DataContext = dataContext;
            this.RaisePropertyChanged("DataContext");
        }
    }

    public class DataContextConverter : IValueConverter
    {
        ObservableCollection<DataContextWrapper> dataContexts = new ObservableCollection<DataContextWrapper>();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Tensor arr)
            {
                if (arr.Shape.Length == 1)
                {
                    float[] y = arr.Data.Select(f => (float)f).ToArray();
                    return new XyPlotModel(y);
                }
                else if (arr.Shape[1] < 100)
                {
                    int index = 0;
                    for (int i = 0; i < arr.Shape[0]; i++)
                    {
                        float[] y = new float[arr.Shape[1]];
                        for (int j = 0; j < y.Length; j++)
                        {
                            y[j] = (float)arr.Data[index++];
                        }
                        if (i < dataContexts.Count)
                            dataContexts[i].SetDataContext(new XyPlotModel(y));
                        else
                            dataContexts.Add(new DataContextWrapper(new XyPlotModel(y)));
                    }
                    return dataContexts;
                }
                else
                {
                    //List<ArraySegment<double>> result = new();
                    int index = 0;
                    for (int i = 0; i < arr.Shape[0]; i++)
                    {
                        double[] y = new double[arr.Shape[1]];
                        for (int j = 0; j < y.Length; j++)
                        {
                            y[j] = arr.Data[index++];
                        }
                        if (i < dataContexts.Count)
                            dataContexts[i].SetDataContext(new ArraySegment<double>(y));
                        else
                            dataContexts.Add(new DataContextWrapper(new ArraySegment<double>(y)));
                    }

                    return dataContexts;
                }
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
            biasPlot.HorizontalLines.Add(new LinesDefinition(0, 1, true, (uint)XYPlot.Grey));
            biasPlot.HorizontalLines.Add(new LinesDefinition(0, 0.1f, false, (uint)XYPlot.Beige, minPointSpacing: 5));
            biasPlot.HorizontalLines.Add(new LinesDefinition(0, 0.01f, false, (uint)XYPlot.Beige, minPointSpacing: 5));
        }

        private void weightPlot_Loaded(object sender, System.EventArgs e)
        {
            if (sender is XYPlot plot)
            {
                if (plot.HorizontalLines.Count == 0)
                {
                    plot.HorizontalLines.Add(new LinesDefinition(0, 1, true, (uint)XYPlot.Grey));
                    plot.HorizontalLines.Add(new LinesDefinition(0, 0.1f, false, (uint)XYPlot.Beige, minPointSpacing: 5));
                    plot.HorizontalLines.Add(new LinesDefinition(0, 0.01f, false, (uint)XYPlot.Beige, minPointSpacing: 5));
                    plot.HorizontalLines.Add(new LinesDefinition(0, 0.001f, false, (uint)XYPlot.Beige, minPointSpacing: 5));

                    plot.PropertyChanged += Plot_PropertyChanged;
                    plot.IsVisible = plot.DataContext is XyPlotModel;
                }
            }
        }

        private void gridPlot_Loaded(object sender, System.EventArgs e)
        {
            if (sender is GridPlot plot)
            {
                plot.IsVisible = plot.DataContext is ArraySegment<double>;
            }
        }

        private void Plot_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == XYPlot.CurrentDataPointProperty && sender is XYPlot xyPlot && xyPlot.CurrentDataPoint != null &&
                weighs_ItemsControl?.ItemsSource != null)
            {
                int index = -1;

                foreach (var item in weighs_ItemsControl.ItemsSource)
                {
                    index++;

                    if (item  == xyPlot.DataContext)
                    {
                        break;
                    }
                }

                if (sender is XYPlot plot && plot.CurrentDataPoint != null)
                {
                    weight_TextBlock.Text = string.Format("Neuron {0}, Input {1}, Weight {2}",
                        index + 1,
                        plot.CurrentDataPoint.Number,
                        plot.CurrentDataPoint.Y);
                }
            }
        }
    }
}
