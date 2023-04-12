using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.MNIST
{
    internal static class Utils
    {
        public static ArraySegment<byte> GetImage(this ImageClass imageClass, int index, int imageDimension, ImageProcessing imageProcessing)
        {
            var image = imageClass.GetImage(index);

            if (imageProcessing == ImageProcessing.Stretch)
            {
                image = image.StretchImage(imageDimension);
            }
            else if (imageProcessing == ImageProcessing.Center)
            {
                image = image.CenterImage(imageDimension);
            }

            return image.ToArray();
        }

        public static ArraySegment<byte> CenterImage(this ArraySegment<byte> im, int width)
        {
            int height = im.Count / width;
            (int left, int right, int top, int bottom) = FindMargins(im, width);

            int shiftRight = (width - right - left) / 2;
            int shiftDown = (height - bottom - top) / 2;

            if (shiftRight == 0 && shiftDown == 0)
                return im;

            byte[] result = new byte[im.Count];
            int copyToY, copyToX;

            for (int copyFromY = Math.Max(0, -shiftDown); copyFromY < Math.Min(height, -shiftDown + height); copyFromY++)
            {
                copyToY = copyFromY + shiftDown;

                for (int copyFromX = Math.Max(0, -shiftRight); copyFromX < Math.Min(width, -shiftRight + width); copyFromX++)
                {
                    copyToX = copyFromX + shiftRight;
                    Array.Copy(im.Array!,
                        im.Offset + copyFromY * width + copyFromX,
                        result,
                        copyToY * width + copyToX,
                        width - Math.Max(copyFromX, copyToX));
                }
            }

            return new ArraySegment<byte>(result);
        }

        public static ArraySegment<byte> StretchImage(this ArraySegment<byte> im, int width)
        {
            int height = im.Count / width;
            (int left, int right, int top, int bottom) = FindMargins(im, width);

            // the image left-right, top-bottom must be mapped to new image without margins

            int actualWidth = right - left + 1;
            int actualHeight = bottom - top + 1;

            while (actualWidth < 9)
            {
                right = Math.Min(width - 1, right + 1);
                left = Math.Max(0, left - 1);
                actualWidth = right - left + 1;
            }

            // ratio of new pixels vs old pixels
            double wdpp = actualWidth / (double)width;
            double hdpp = actualHeight / (double)height;

            byte[] image = new byte[im.Count];
            double oldX, oldY = top;
            double xw1, xw2, yw1, yw2;
            int index = 0;
            int x1, x2, y1, y2;

            // y,x in new picture
            for (int y = 0; y < height; y++)
            {
                oldX = left;
                for (int x = 0; x < width; x++)
                {
                    xw1 = 1 - (oldX - (int)oldX);
                    xw2 = oldX + wdpp - (int)(oldX + wdpp);
                    yw1 = 1 - (oldY - (int)oldY);
                    yw2 = oldY + hdpp - (int)(oldY + hdpp);
                    x1 = (int)oldX;
                    x2 = (int)(oldX + wdpp);
                    if (x2 > right)
                        x2--;
                    y1 = (int)oldY;
                    y2 = (int)(oldY + hdpp);
                    if (y2 > bottom)
                        y2--;
                    // Copy from max 4 points
                    image[index] = (byte)((
                            yw1 * (xw1 * im[width * y1 + x1] + xw2 * im[width * y1 + x2]) / (xw1 + xw2)
                        + yw2 * (xw1 * im[width * y2 + x1] + xw2 * im[width * y2 + x2]) / (xw1 + xw2))
                        / (yw1 + yw2));
                    index++;
                    oldX += wdpp;
                }
                oldY += hdpp;
            }

            return new ArraySegment<byte>(image);
        }

        static (int left, int right, int top, int bottom) FindMargins(this ArraySegment<byte> im, int width)
        {
            int height = im.Count / width;

            int left = width;
            int right = 0;
            int top = height;
            int bottom = 0;
            int index = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (im[index] > 127)
                    {
                        if (x <= left)
                            left = x;
                        if (x >= right)
                            right = x;
                        if (y <= top)
                            top = y;
                        if (y >= bottom)
                            bottom = y;
                    }

                    index++;
                }
            }

            return (left, right, top, bottom);
        }
    }
}
