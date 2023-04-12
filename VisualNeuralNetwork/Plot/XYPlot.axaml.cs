using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using DynamicData;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VisualNeuralNetwork
{
    public class LinesDefinition
    {
        public float Value { get; private set; }
        public float Interval { get; private set; }
        public bool Solid { get; private set; }
        public int Color { get; private set; }
        public int MinPointsSpacing { get; private set; }
        public LinesDefinition(float value, float interval, bool solid, int color, int minPointSpacing = 10)
        {
            Value = value;
            Interval = interval;
            Solid = solid;
            Color = color;
            MinPointsSpacing = minPointSpacing;
        }
    }

    public class DataPoint
    {
        public float X { get; }
        public float Y { get; }
        public int Index { get; }
        public DataPoint(float x, float y, int index)
        {
            X = x;
            Y = y;
            Index = index;
        }
    }

    public partial class XYPlot : UserControl
    {
        public static int Red = int.Parse("FFFF0000", System.Globalization.NumberStyles.HexNumber);
        public static int Orange = int.Parse("FFFF6A00", System.Globalization.NumberStyles.HexNumber);
        public static int Black = int.Parse("FF000000", System.Globalization.NumberStyles.HexNumber);
        public static int White = int.Parse("FFFFFFFF", System.Globalization.NumberStyles.HexNumber);
        public static int Beige = int.Parse("FFDDDDDD", System.Globalization.NumberStyles.HexNumber);
        public static int Blue = int.Parse("FF0000FF", System.Globalization.NumberStyles.HexNumber);

        public readonly List<LinesDefinition> VerticalLines = new();
        public readonly List<LinesDefinition> HorizontalLines = new();

        public static readonly StyledProperty<DataPoint?> CurrentDataPointProperty =
            AvaloniaProperty.Register<XYPlot, DataPoint?>(nameof(CurrentDataPoint));

        public DataPoint? CurrentDataPoint
        {
            get { return GetValue(CurrentDataPointProperty); }
            set { SetValue(CurrentDataPointProperty, value); }
        }

        public XYPlot()
        {
            InitializeComponent();

            grid.GetObservable(BoundsProperty).Subscribe(value =>
            {
                if (DataContext is XyPlotModel values)
                {
                    Redraw(values);
                }
            });

            DataContextChanged += GridPlot_DataContextChanged;
        }

        private void GridPlot_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is XyPlotModel values)
            {
                Redraw(values);
            }
        }

        void Redraw(XyPlotModel data)
        {
            if(grid.Bounds.Width == 0 || grid.Bounds.Height == 0)
                return;

            int width = (int)grid.Bounds.Width;
            int height = (int)grid.Bounds.Height;

            WriteableBitmap? wbm = CreateBitmap(data, width, height);

            if (wbm != null)
            {
                image.Source = wbm;
            }
        }

        WriteableBitmap? CreateBitmap(XyPlotModel data, int width, int height)
        {
            try
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Bgra8888,
                    Avalonia.Platform.AlphaFormat.Unpremul);

                // draw lines

                // draw plot
                    
                return writeableBitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
