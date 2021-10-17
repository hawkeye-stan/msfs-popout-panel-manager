using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace MSFSPopoutPanelManager
{
    public class ImageOperation
    {
        public static byte[] ImageToByte(Bitmap image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }

        public static Bitmap ByteToImage(byte[] bytes)
        {
            return new Bitmap(new MemoryStream(bytes));
        }

        public static Bitmap ResizeImage(Bitmap sourceImage, float width, float height)
        {
            return new ResizeBilinear(Convert.ToInt32(width), Convert.ToInt32(height)).Apply(sourceImage);
        }

        public static Bitmap CropImage(Bitmap sourceImage, int x, int y, int width, int height)
        {
            Rectangle crop = new Rectangle(x, y, width, height);

            var bmp = new Bitmap(crop.Width, crop.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(sourceImage, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        public static Bitmap ConvertToFormat(Bitmap image, PixelFormat format)
        {
            var copy = new Bitmap(image.Width, image.Height, format);
            using (Graphics gr = Graphics.FromImage(copy))
            {
                gr.DrawImage(image, new Rectangle(0, 0, copy.Width, copy.Height));
            }
            return copy;
        }

        public static Bitmap TakeScreenShot(IntPtr windowHandle, bool maximized)
        {
            if (maximized)
            {
                const int SW_MAXIMIZE = 3;
                PInvoke.ShowWindow(windowHandle, SW_MAXIMIZE);
            }

            // Set window to foreground so nothing can hide the window
            PInvoke.SetForegroundWindow(windowHandle);
            Thread.Sleep(500);

            var rect = new Rect();
            PInvoke.GetWindowRect(windowHandle, out rect);

            var left = rect.Left;
            var top = rect.Top;
            var right = rect.Right;
            var bottom = rect.Bottom;

            var bounds = new Rectangle(left, top, right - left, bottom - top);
            var bitmap = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return bitmap;
        }

        public static Bitmap HighLightMatchedPattern(Bitmap sourceImage, List<Rect> rectBoxes)
        {
            // Highlight the match in the source image
            var data = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadWrite, sourceImage.PixelFormat);

            foreach (Rect rectBox in rectBoxes)
            {
                Rectangle rect = new Rectangle(rectBox.Left, rectBox.Top, rectBox.Width, rectBox.Height);
                Drawing.Rectangle(data, rect, Color.Red);
            }

            sourceImage.UnlockBits(data);
            sourceImage.Save(@".\debug.png");

            return sourceImage;
        }
    }
}
