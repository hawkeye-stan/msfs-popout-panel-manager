using MSFSPopoutPanelManager.Shared;
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
            if (!PInvoke.IsWindow(windowHandle))
                throw new PopoutManagerException("Pop out windows were closed unexpectedly.");

            // Set window to foreground so nothing can hide the window
            PInvoke.SetForegroundWindow(windowHandle);
            Thread.Sleep(300);

            Rectangle rectangle;
            PInvoke.GetWindowRect(windowHandle, out rectangle);

            Rectangle clientRectangle;
            PInvoke.GetClientRect(windowHandle, out clientRectangle);

            // Take a screen shot by removing the titlebar of the window
            var left = rectangle.Left;
            var top = rectangle.Top + (rectangle.Height - clientRectangle.Height) - 8;      // 8 pixels adjustments
            
            var bmp = new Bitmap(clientRectangle.Width, clientRectangle.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(new Point(left, top), Point.Empty, rectangle.Size);
            }

            // Place the above image in the same canvas size as before
            Bitmap backingImage = new Bitmap(rectangle.Width, rectangle.Height);
            using (Graphics gfx = Graphics.FromImage(backingImage))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 0, 0)))
            {
                gfx.FillRectangle(brush, 0, 0, rectangle.Width, rectangle.Height);
                gfx.DrawImage(bmp, new Point(0, top));
            }

            return backingImage;
        }
    }
}
