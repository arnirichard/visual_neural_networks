using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using DynamicData;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        DateTime lastRedrawTime;

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
                    _ = Redraw(values);
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
                _ = Redraw(values);
            }
        }

        async Task Redraw(XyPlotModel data)
        {
            if(grid.Bounds.Width == 0 || grid.Bounds.Height == 0)
                return;

            DateTime redrawTime = lastRedrawTime = DateTime.Now;

            int width = (int)grid.Bounds.Width;
            int height = (int)grid.Bounds.Height;

            WriteableBitmap? wbm = await CreateBitmapAsync(data, width, height, redrawTime);
            //WriteableBitmap? wbm = CreateBitmap(data, width, height, redrawTime);

            if (wbm != null)
            {
                image.Source = wbm;
            }
        }

        async Task<WriteableBitmap?> CreateBitmapAsync(XyPlotModel data, int width, int height, DateTime redrawTime)
        {
            TaskCompletionSource<WriteableBitmap?> taskCompletionSource =
                new TaskCompletionSource<WriteableBitmap?>();

            ThreadPool.QueueUserWorkItem(delegate
            {
                taskCompletionSource.SetResult(CreateBitmap(data, width, height, redrawTime));

            });

            return await taskCompletionSource.Task;
        }

        WriteableBitmap? CreateBitmap(XyPlotModel data, int width, int height, DateTime redrawTime)
        {
            try
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Unpremul);

                float yMin = data.Y.Min();
                float yMax = data.Y.Max();
                float yRange = yMax - yMin;
                yMax += yRange * 0.1f;
                yMin -= yRange * 0.1f;
                yRange = yMax - yMin;
                uint color = (uint)Black;

                HashSet<int> points = new();

                // draw lines

                // draw plot
                unsafe
                {
                    using (ILockedFramebuffer buf = writeableBitmap.Lock())
                    {
                        foreach (var line in HorizontalLines)
                            buf.PaintHorizontalLines(line, yMin, yMax, points);

                        uint* ptr = (uint*)buf.Address;

                        int y, lasty = -1;
                        int sign;
                        float dpx = buf.Size.Width / (float)data.Y.Length;
                        int ipx = 0;
                        int toIpx;
                        int counterX = 0, counterY = 0;

                        for (int x = 0; x < data.Y.Length; x++)
                        {
                            if (lastRedrawTime != redrawTime)
                            {
                                return null;
                            }

                            y = (int)((1 - (data.Y[x] - yMin) / yRange) * buf.Size.Height);

                            if (x == 0)
                            {
                                lasty = y;
                                ptr += y * buf.Size.Width;
                                counterY += y;
                                if (y > -1 && y < buf.Size.Height)
                                    *ptr = color;
                            }

                            sign = Math.Sign(y - lasty);
                            // plot vertical line from lasty to y
                            while (lasty != y)
                            {
                                lasty += sign;
                                counterY += sign;
                                ptr += buf.Size.Width * sign;
                                if (lasty > -1 && lasty < buf.Size.Height)
                                    *ptr = color;
                            }
                            // plot horizontal line
                            toIpx = Math.Min((int)((x + 1) * dpx), buf.Size.Width);

                            while (ipx < toIpx)
                            {
                                counterX++;
                                ptr += 1;
                                ipx++;
                                if (y > -1 && y < buf.Size.Height)
                                    *ptr = color;
                            }
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
