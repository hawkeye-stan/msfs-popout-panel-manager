using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace MSFSPopoutPanelManager.Orchestration
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class PanelAnalyzer
    {
        public static Point GetMagnifyingGlassIconCoordinate(IntPtr hwnd)
        {
            var sourceImage = TakeScreenShot(hwnd);

            if (sourceImage == null)
                return new Point(0, 0);

            Rectangle rectangle = WindowActionManager.GetClientRect(hwnd);

            var panelMenubarTop = GetPanelMenubarTop(sourceImage, rectangle);
            if (panelMenubarTop > sourceImage.Height)
                return Point.Empty;

            var panelMenubarBottom = GetPanelMenubarBottom(sourceImage, rectangle);
            if (panelMenubarBottom > sourceImage.Height)
                return Point.Empty;

            var panelsStartingLeft = GetPanelMenubarStartingLeft(sourceImage, rectangle, panelMenubarTop + 5);

            // The center of magnifying glass icon is around (2.7 x height of menubar) to the right of the panel menubar starting left
            // But need to use higher number here to click the left side of magnifying glass icon because on some panel, the ratio is smaller
            var menubarHeight = panelMenubarBottom - panelMenubarTop;
            var magnifyingIconXCoor = panelsStartingLeft - Convert.ToInt32(menubarHeight * 2.7);        // ToDo: play around with this multiplier to find the best for all resolutions
            var magnifyingIconYCoor = panelMenubarTop + Convert.ToInt32(menubarHeight / 2.2);

            return new Point(magnifyingIconXCoor, magnifyingIconYCoor);
        }

        private static Bitmap TakeScreenShot(IntPtr windowHandle)
        {
            if (!WindowActionManager.IsWindow(windowHandle))
                return null;

            // Set window to foreground so nothing can hide the window
            PInvoke.SetForegroundWindow(windowHandle);
            Thread.Sleep(300);

            Rectangle rectangle = WindowActionManager.GetWindowRect(windowHandle);

            Rectangle clientRectangle = WindowActionManager.GetClientRect(windowHandle);

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
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 0, 0)))
                {
                    gfx.FillRectangle(brush, 0, 0, rectangle.Width, rectangle.Height);
                    gfx.DrawImage(bmp, new Point(0, top));
                }
            }

            return backingImage;
        }

        private static int GetPanelMenubarTop(Bitmap sourceImage, Rectangle rectangle)
        {
            // Get a snippet of 1 pixel wide vertical strip of windows. We will choose the strip left of center.
            // This is to determine when the actual panel's vertical pixel starts in the window. This will allow accurate sizing of the template image
            var left = Convert.ToInt32((rectangle.Width) * 0.70);  // look at around 70% from the left

            if (left < 0)
                return -1;

            unsafe
            {
                var stripData = sourceImage.LockBits(new Rectangle(left, 0, 1, rectangle.Height), ImageLockMode.ReadWrite, sourceImage.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(stripData.PixelFormat) / 8;
                int heightInPixels = stripData.Height;
                int widthInBytes = stripData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)stripData.Scan0;

                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * stripData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int red = currentLine[x + 2];
                        int green = currentLine[x + 1];
                        int blue = currentLine[x];

                        if (red == 255 && green == 255 && blue == 255)
                        {
                            sourceImage.UnlockBits(stripData);
                            return y;
                        }
                    }
                }

                sourceImage.UnlockBits(stripData);
            }

            return -1;
        }

        private static int GetPanelMenubarBottom(Bitmap sourceImage, Rectangle rectangle)
        {
            // Get a snippet of 1 pixel wide vertical strip of windows. We will choose the strip about 70% from the left of the window
            var left = Convert.ToInt32((rectangle.Width) * 0.7);  // look at around 70% from the left
            var top = sourceImage.Height - rectangle.Height;

            if (top < 0 || left < 0)
                return -1;

            unsafe
            {
                var stripData = sourceImage.LockBits(new Rectangle(left, top, 1, rectangle.Height), ImageLockMode.ReadWrite, sourceImage.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(stripData.PixelFormat) / 8;
                int heightInPixels = stripData.Height;
                int widthInBytes = stripData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)stripData.Scan0;

                int menubarBottom = -1;

                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * stripData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int red = currentLine[x + 2];
                        int green = currentLine[x + 1];
                        int blue = currentLine[x];

                        if (red > 250 && green > 250 && blue > 250)     // allows the color to be a little off white (ie. Fenix A30 EFB)
                        {
                            // found the top of menu bar
                            menubarBottom = y + top;
                        }
                        else if (menubarBottom > -1)     /// it is no longer white in color, we hit menubar bottom
                        {
                            sourceImage.UnlockBits(stripData);
                            return menubarBottom;
                        }
                    }
                }

                sourceImage.UnlockBits(stripData);
            }

            return -1;
        }

        private static int GetPanelMenubarStartingLeft(Bitmap sourceImage, Rectangle rectangle, int top)
        {
            unsafe
            {
                var stripData = sourceImage.LockBits(new Rectangle(0, top, rectangle.Width, 1), ImageLockMode.ReadWrite, sourceImage.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(stripData.PixelFormat) / 8;
                int widthInPixels = stripData.Width;
                int heightInBytes = stripData.Height * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)stripData.Scan0;

                for (int x = 0; x < widthInPixels; x++)
                {
                    byte* currentLine = ptrFirstPixel - (x * bytesPerPixel);
                    for (int y = 0; y < heightInBytes; y = y + bytesPerPixel)
                    {
                        int red = currentLine[y + 2];
                        int green = currentLine[y + 1];
                        int blue = currentLine[y];

                        if (red > 250 && green > 250 && blue > 250)     // allows the color to be a little off white (ie. Fenix A30 EFB)
                        {
                            sourceImage.UnlockBits(stripData);
                            return sourceImage.Width - x;
                        }
                    }
                }

                sourceImage.UnlockBits(stripData);
            }

            return -1;
        }
    }
}
