using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork
{
    internal static class WriteableBitmapExt
    {
        internal static unsafe void PaintRect(this WriteableBitmap writeableBitmap, uint color, int x, int y, int width, int height)
        {
            x = Math.Max(0, x);
            y = Math.Max(0, y);

            int x2 = Math.Min(x + height, writeableBitmap.PixelSize.Width);
            int y2 = Math.Min(y + width, writeableBitmap.PixelSize.Height);

            height = y2 - y;
            width = x2 - x;

            if (width <= 0 || height <= 0)
                return;

            using (var buf = writeableBitmap.Lock())
            {
                var ptr = (uint*)buf.Address;

                ptr += y * buf.Size.Width + x;

                for (int _y = 0; _y < height; _y++)
                {
                    for (int _x = 0; _x < width; _x++)   
                    {
                        *ptr = color;
                        ptr += 1;
                    }
                    ptr += buf.Size.Width - width;
                }
            }
        }

        internal static void PaintHorizontalLines(this ILockedFramebuffer buf,
            LinesDefinition linesDefinition, float minY, float maxY , HashSet<int> points)
        {
            float range = maxY - minY;
            var spacing = buf.Size.Height *
                (linesDefinition.Interval > 0 ? linesDefinition.Interval : range)
                / range;

            if (range > 0 && spacing >= linesDefinition.MinPointsSpacing)
            {
                float val = linesDefinition.Value;

                if(val > minY && linesDefinition.Interval != 0) 
                {
                    val -= (int)((linesDefinition.Value - minY) / linesDefinition.Interval) * linesDefinition.Interval;
                }
                int pos;

                int lastPos = buf.Size.Height;

                while (val < maxY)
                {
                    if (val > minY)
                    {
                        var ratio = (val - minY) / range;
                        pos = (int)((1 - ratio) * buf.Size.Height);
                        if (!points.Contains(pos) &&
                            Math.Abs(lastPos - pos) > linesDefinition.MinPointsSpacing &&
                            buf.PaintHorizontalLine(linesDefinition.Color,
                            pos,
                            linesDefinition.Solid ? (int)buf.Size.Width : 8,
                            linesDefinition.Solid ? 0 : 6))
                        {
                            lastPos = pos;
                            points.Add(pos);
                        }
                    }

                    val += linesDefinition.Interval <= 0 ? int.MaxValue : linesDefinition.Interval;
                }
            }
        }

        internal unsafe static bool PaintHorizontalLine(this ILockedFramebuffer buf, uint color, int y,
            int solid = int.MaxValue, int gaps = 0)
        {
            if (y < 0 || y >= buf.Size.Height)
                return false;

            var ptr = (uint*)buf.Address;
            int width = buf.Size.Width;

            ptr += y * buf.Size.Width;

            int i = 0;
            int iTo;

            while (true)
            {
                iTo = Math.Min(i + solid, width);

                for (; i < iTo; i++)
                {
                    *ptr = color;
                    ptr++;
                }

                if (iTo < width && gaps > 0)
                {
                    ptr  += gaps;
                    i += gaps;
                }

                if (i >= width)
                    break;
            }

            return true;
        }
    }
}
