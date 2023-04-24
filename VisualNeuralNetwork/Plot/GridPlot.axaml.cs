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
using System.Threading;
using System.Threading.Tasks;

namespace VisualNeuralNetwork
{
    public enum ColorChannel : int
    {
        None = -1,
        Red = 16,
        Green = 8,
        Blue = 0
    }

    public partial class GridPlot : UserControl
    {
        public static int Red = int.Parse("FFFF0000", System.Globalization.NumberStyles.HexNumber);
        public static int Orange = int.Parse("FFFF6A00", System.Globalization.NumberStyles.HexNumber);
        public static int Black = int.Parse("FF000000", System.Globalization.NumberStyles.HexNumber);
        public static int White = int.Parse("FFFFFFFF", System.Globalization.NumberStyles.HexNumber);
        public static int Beige = int.Parse("FFDDDDDD", System.Globalization.NumberStyles.HexNumber);
        public static int Blue = int.Parse("FF0000FF", System.Globalization.NumberStyles.HexNumber);

        public static readonly StyledProperty<int> NumColumnsProperty =
            AvaloniaProperty.Register<GridPlot, int>(nameof(NumColumns), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public int NumColumns
        {
            get { return GetValue(NumColumnsProperty); }
            set { SetValue(NumColumnsProperty, value); }
        }

        public ColorChannel Channel { get; set; } = ColorChannel.None;
        public bool GrayScale { get; set; }
        public int AddDisplayValue { get; set; }
        DateTime lastRedrawTime;

        public GridPlot()
        {
            InitializeComponent();

            grid.GetObservable(BoundsProperty).Subscribe(value =>
            {
                if (DataContext is ArraySegment<byte> values)
                {
                    _ = Redraw(values);
                }
                else if (DataContext is ArraySegment<double> dvalues)
                {
                    _ = Redraw(dvalues);
                }
            });

            this.GetObservable(NumColumnsProperty).Subscribe(value =>
            {
                if (DataContext is ArraySegment<byte> values)
                {
                    _ = Redraw(values);
                }
                else if (DataContext is ArraySegment<double> dvalues)
                {
                    _ = Redraw(dvalues);
                }
            });

            DataContextChanged += GridPlot_DataContextChanged;
        }

        private void GridPlot_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is ArraySegment<byte> values)
            {
                _ = Redraw(values);
            }
        }

        async Task Redraw(ArraySegment<byte> values)
        {
            if(grid.Bounds.Width == 0 || grid.Bounds.Height == 0 || NumColumns <= 0) 
                return;

            DateTime redrawTime = lastRedrawTime = DateTime.Now;

            int width = (int)grid.Bounds.Width;
            int height = (int)grid.Bounds.Height;

            //WriteableBitmap? wbitmap = await CreateBitmapAsync(values, width, height, NumColumns, redrawTime);
            WriteableBitmap? wbitmap = CreateBitmap(values, width, height, NumColumns, redrawTime);

            if (wbitmap != null && redrawTime == lastRedrawTime)
            {
                image.Source = wbitmap;
            }
        }

        async Task Redraw(ArraySegment<double> values)
        {
            if (grid.Bounds.Width == 0 || grid.Bounds.Height == 0 || NumColumns <= 0)
                return;

            DateTime redrawTime = lastRedrawTime = DateTime.Now;

            int width = (int)grid.Bounds.Width;
            int height = (int)grid.Bounds.Height;

            double max = values.Max();
            double min = values.Min();
            double range = max - min;

            byte[] bytes = range == 0
                ? new byte[values.Count]
                : values.Select(d => (byte)((d-min) / range * 255)).ToArray();
                 
            //WriteableBitmap? wbitmap = await CreateBitmapAsync(bytes, width, height, NumColumns, redrawTime);
            WriteableBitmap? wbitmap = CreateBitmap(bytes, width, height, NumColumns, redrawTime);

            if (wbitmap != null && redrawTime == lastRedrawTime)
            {
                image.Source = wbitmap;
            }
        }

        async Task<WriteableBitmap?> CreateBitmapAsync(ArraySegment<byte> values, int width, int height, int numColumns, DateTime redrawTime)
        {
            TaskCompletionSource<WriteableBitmap?> taskCompletionSource =
                new TaskCompletionSource<WriteableBitmap?>();

            ThreadPool.QueueUserWorkItem(delegate
            {
                taskCompletionSource.SetResult(CreateBitmap(values, width, height, numColumns, redrawTime));
            });
            
            return await taskCompletionSource.Task;
        }

        WriteableBitmap? CreateBitmap(ArraySegment<byte> values, int width, int height, int numColumns, DateTime redrawTime)
        {
            try
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Bgra8888,
                    Avalonia.Platform.AlphaFormat.Unpremul);

                int rows = (values.Count + numColumns - 1) / numColumns;
                double columnWidth = width / (double)numColumns;
                double rowHeight = height / (double)rows;
                int value;
                uint colorValue;
                long display;
                int channelValue = (int)Channel;
                double fontSize = height / numColumns * 0.3;
                double x = 0, y = -rowHeight;
                int posX, posY;

                for (int i = 0; i < values.Count; i++)
                {
                    if (lastRedrawTime != redrawTime)
                    {
                        return null;
                    }

                    if (i % numColumns == 0)
                    {
                        y += rowHeight;
                        x = 0;
                    }

                    value = values[i];

                    if (Channel != ColorChannel.None)
                    {
                        display = (value & (255 << channelValue)) >> channelValue;

                        if (GrayScale)
                        {
                            colorValue = (uint)(display << 16 | display << 8 | display | 0xff000000);
                        }
                        else
                        {
                            colorValue = (uint)((display << channelValue) | 0xff000000);
                        }
                    }
                    else
                    {
                        colorValue = (uint)value;
                    }

                    posX = (int)x;
                    posY = (int)y;

                    writeableBitmap.PaintRect(colorValue,
                        posX % width,
                        posY % height,
                        (int)Math.Ceiling(x + columnWidth) - posX,
                        (int)Math.Ceiling(y + rowHeight) - posY);

                    x += columnWidth;
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
