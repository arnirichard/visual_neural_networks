using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
        public uint Color { get; private set; }
        public int MinPointsSpacing { get; private set; }
        public LinesDefinition(float value, float interval, bool solid, uint color, int minPointSpacing = 10)
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
        public float Y { get; }
        public float? X { get; }
        public int Index { get; }
        public int Number => Index + 1;
        public DataPoint(float y, float? x, int index)
        {
            Y = y;
            X = x;
            Index = index;
        }
    }

    public partial class XYPlot : UserControl
    {
        public static int Red = int.Parse("FFFF0000", System.Globalization.NumberStyles.HexNumber);
        public static int Orange = int.Parse("FFFF6A00", System.Globalization.NumberStyles.HexNumber);
        public static int Black = int.Parse("FF000000", System.Globalization.NumberStyles.HexNumber);
        public static int White = int.Parse("FFFFFFFF", System.Globalization.NumberStyles.HexNumber);
        public static int Beige = int.Parse("FFAAAAAA", System.Globalization.NumberStyles.HexNumber);
        public static int Grey = int.Parse("FF666666", System.Globalization.NumberStyles.HexNumber);
        public static int Blue = int.Parse("FF0000FF", System.Globalization.NumberStyles.HexNumber);

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

            grid.PointerMoved += Grid_PointerMoved;

            DataContextChanged += GridPlot_DataContextChanged;
        }

        private void Grid_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            Point p = e.GetPosition(this);   

            if(p.X > -1 && p.X < Bounds.Width && DataContext is XyPlotModel m)
            {
                int index = (int)(p.X / Bounds.Width * m.Y.Length);
                CurrentDataPoint = index > -1 && index < m.Y.Length
                    ? new DataPoint(m.Y[index], m.X?[index], index)
                    : null;
            }
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
            //if (data.Y.Length > width)
            //    width = (int)Math.Min(MaxWidth, data.Y.Length);

            WriteableBitmap? wbm = CreateBitmap(data, width, height);

            if (wbm != null)
            {
                image.Source = wbm;
            }
        }

        unsafe WriteableBitmap? CreateBitmap(XyPlotModel data, int width, int height)
        {
            try
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Bgra8888,
                    Avalonia.Platform.AlphaFormat.Unpremul);

                float yMin = data.Y.Min() * 1.1f;
                float yMax = data.Y.Max() * 1.1f;
                float yRange = yMax - yMin;
                uint color = (uint)Black;

                HashSet<int> points = new();

                // draw lines
                foreach (var line in HorizontalLines)
                    writeableBitmap.PaintHorizontalLines(line, yMin, yMax, points);

                // draw plot
                using (var buf = writeableBitmap.Lock())
                {
                    var ptr = (uint*)buf.Address;

                    int y, lasty=-1;
                    int sign;
                    float dpx = buf.Size.Width / (float)data.Y.Length;
                    int ipx = 0;
                    int toIpx;

                    for (int x = 0; x < data.Y.Length; x++)
                    {
                        y = (int)((1-(data.Y[x]-yMin)/yRange)*buf.Size.Height);

                        if (x == 0)
                        {
                            lasty = y;
                            ptr += y * buf.Size.Width;
                            if(y > -1 && y < buf.Size.Height)
                                *ptr = color;
                        }

                        sign = Math.Sign(y - lasty);
                        // plot vertical line from lasty to y
                        while (lasty != y)
                        {
                            lasty += sign;
                            ptr += buf.Size.Width * sign;
                            if (lasty > -1 && lasty < buf.Size.Height)
                                *ptr = color;
                        }
                        // plot horizontal line
                        toIpx = Math.Min((int)((x+1) * dpx), buf.Size.Width);

                        while (ipx < toIpx)
                        {
                            ptr += 1;
                            ipx++;
                            *ptr = color;
                        }
                    }
                }

                return writeableBitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
