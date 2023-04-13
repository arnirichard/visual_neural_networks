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

        public GridPlot()
        {
            InitializeComponent();

            grid.GetObservable(BoundsProperty).Subscribe(value =>
            {
                if (DataContext is ArraySegment<byte> values)
                {
                    Redraw(values);
                }
                else if (DataContext is ArraySegment<double> dvalues)
                {
                    Redraw(dvalues);
                }
            });

            this.GetObservable(NumColumnsProperty).Subscribe(value =>
            {
                if (DataContext is ArraySegment<byte> values)
                {
                    Redraw(values);
                }
                else if (DataContext is ArraySegment<double> dvalues)
                {
                    Redraw(dvalues);
                }
            });

            DataContextChanged += GridPlot_DataContextChanged;
        }

        private void GridPlot_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is ArraySegment<byte> values)
            {
                Redraw(values);
            }
        }

        void Redraw(ArraySegment<byte> values)
        {
            if(grid.Bounds.Width == 0 || grid.Bounds.Height == 0 || NumColumns <= 0) 
                return;

            int width = (int)grid.Bounds.Width;
            int height = (int)grid.Bounds.Height;

            WriteableBitmap? wbitmap = CreateBitmap(values, width, height); // DrawGridPlot.Draw<byte>(values, width, height);

            if (wbitmap != null)
            {
                IsVisible = true;
                image.Source = wbitmap;
            }
        }

        void Redraw(ArraySegment<double> values)
        {
            if (grid.Bounds.Width == 0 || grid.Bounds.Height == 0 || NumColumns <= 0)
                return;

            int width = (int)grid.Bounds.Width;
            int height = (int)grid.Bounds.Height;

            double max = values.Max();
            double min = values.Min();
            double range = max - min;

            byte[] bytes = range == 0
                ? new byte[values.Count]
                : values.Select(d => (byte)(d / range * 255)).ToArray();
                 
            WriteableBitmap? wbitmap = CreateBitmap(bytes, width, height);

            if (wbitmap != null)
            {
                image.Source = wbitmap;
            }
        }

        WriteableBitmap? CreateBitmap(ArraySegment<byte> values, int width, int height)
        {
            try
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Bgra8888,
                    Avalonia.Platform.AlphaFormat.Unpremul);

                int rows = (values.Count + NumColumns - 1) / NumColumns;
                double columnWidth = width / (double)NumColumns;
                double rowHeight = height / (double)rows;
                int value;
                uint colorValue;
                long display;
                int channelValue = (int)Channel;
                canvas.Children.Clear();
                double fontSize = height / NumColumns * 0.3;
                int posX, posY;
                double x = 0, y = -rowHeight;

                for (int i = 0; i < values.Count; i++)
                {
                    if (i % NumColumns == 0)
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
