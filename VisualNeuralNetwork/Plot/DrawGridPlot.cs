//using Avalonia.Controls;
//using Avalonia;
//using Avalonia.Controls.Shapes;
//using Avalonia.Layout;
//using Avalonia.Media;
//using Avalonia.Media.Imaging;
//using Avalonia.Platform;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace VisualNeuralNetwork
//{
//    internal class DrawGridPlot<K> where K : unmanaged
//    {
//        public static WriteableBitmap Draw(ArraySegment<K> values, int width, int height, int NumColumns)
//        {
//            WriteableBitmap writeableBitmap = new WriteableBitmap(
//                new PixelSize(width, height),
//                new Vector(96, 96),
//                PixelFormat.Bgra8888,
//                AlphaFormat.Unpremul);

//            int rows = (values.Count + NumColumns - 1) / NumColumns;
//            double columnWidth = width / (double)NumColumns;
//            double rowHeight = height / (double)rows;
//            K value;
//            int colorValue;
//            double x = 0, y = -rowHeight;
//            int posX, posY;
//            writeableBitmap.Lock();

//            for (int i = 0; i < values.Count; i++)
//            {
//                if (i % NumColumns == 0)
//                {
//                    y += rowHeight;
//                    x = 0;
//                }

//                value = values[i];

//                if (value is byte b)
//                {
//                    colorValue = (int)(b << 16 | b << 8 | b | 0xff000000);
//                }
//                else if (value is int val)
//                {
//                    colorValue = val;
//                }
//                else
//                {
//                    continue;
//                }

//                posX = (int)x;
//                posY = (int)y;

//                //writeableBitmap.PaintRect(colorValue,
//                //        posX % writeableBitmap.Size.Width,
//                //        posY % writeableBitmap.Size.Height,
//                //        (int)(x + columnWidth) - posX,
//                //        (int)(y + rowHeight) - posY);

//                x += columnWidth;
//            }

//            return writeableBitmap;
//        }
//    }
//}
