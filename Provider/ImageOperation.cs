using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace MSFSPopoutPanelManager.Provider
{
    public class ImageOperation
    {
        public static Bitmap TakeScreenShot(IntPtr windowHandle)
        {
            // Set window to foreground so nothing can hide the window
            PInvoke.SetForegroundWindow(windowHandle);
            Thread.Sleep(300);

            Rectangle rectangle;
            PInvoke.GetWindowRect(windowHandle, out rectangle);

            var left = rectangle.Left;
            var top = rectangle.Top;
            var width = rectangle.Right - rectangle.Left;
            var height = rectangle.Bottom - rectangle.Top;

            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(new Point(left, top), Point.Empty, rectangle.Size);
            }

            return bmp;
        }
    }
}
