using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.Provider
{
    public class PopoutSeparationManager
    {
        private const int RETRY_COUNT = 5;
        private IntPtr _simulatorHandle;
        private UserProfileData _profile;
        private List<PanelConfig> _panels;

        public PopoutSeparationManager(IntPtr simulatorHandle, UserProfileData profile)
        {
            _simulatorHandle = simulatorHandle;
            _profile = profile;
            _panels = new List<PanelConfig>();
        }

        public bool StartPopout()
        {
            // If enable, load the current viewport into custom view by Ctrl-Alt-0
            if (FileManager.ReadAppSettingData().UseAutoPanning)
            {
                var simualatorProcess = WindowManager.GetSimulatorProcess();
                if (simualatorProcess != null)
                {
                    InputEmulationManager.LoadCustomViewZero(simualatorProcess.Handle);
                    Thread.Sleep(500);
                }
            }

            Task<List<PanelConfig>> popoutPanelTask = Task<List<PanelConfig>>.Factory.StartNew(() =>
            {
                return ExecutePopoutSeparation();
            });

            popoutPanelTask.Wait();
            var popoutReslts = popoutPanelTask.Result;

            if (popoutReslts != null)
            {
                if (_profile.PanelConfigs.Count > 0)
                {
                    LoadAndApplyPanelConfigs(popoutReslts);
                    Logger.Status("Panels have been popped out succesfully. Previously saved panel settings have been applied.", StatusMessageType.Info);
                }
                else
                {
                    _profile.PanelConfigs = popoutReslts;
                    Logger.Status("Panels have been popped out succesfully. Please click 'Save Profile' once you're done making adjustment to the panels.", StatusMessageType.Info);
                }

                // If enable, center the view port by Ctrl-Space
                if (FileManager.ReadAppSettingData().UseAutoPanning)
                {
                    var simualatorProcess = WindowManager.GetSimulatorProcess();
                    if (simualatorProcess != null)
                    {
                        InputEmulationManager.CenterView(simualatorProcess.Handle);
                        Thread.Sleep(500);
                    }
                }

                return true;
            }

            return false;
        }

        public List<PanelConfig> ExecutePopoutSeparation()
        {
            _panels.Clear();

            // Must close out all existing custom pop out panels
            PInvoke.EnumWindows(new PInvoke.CallBack(EnumCustomPopoutCallBack), 0);
            if(_panels.Count > 0)
            {
                Logger.BackgroundStatus("Please close all existing panel pop outs before continuing.", StatusMessageType.Error);
                return null;
            }

            _panels.Clear();
            PInvoke.SetForegroundWindow(_simulatorHandle);

            try
            {
                for (var i = 0; i < _profile.PanelSourceCoordinates.Count; i++)
                {
                    PopoutPanel(_profile.PanelSourceCoordinates[i].X, _profile.PanelSourceCoordinates[i].Y);

                    if (i == 0)
                    {
                        int retry = 0;
                        while (retry < RETRY_COUNT)
                        {
                            PInvoke.EnumWindows(new PInvoke.CallBack(EnumCustomPopoutCallBack), 0);

                            if (GetPopoutPanelCountByType(PanelType.CustomPopout) == 0)
                                retry += 1;
                            else
                            {
                                var panel = GetCustomPopoutPanelByIndex(i);
                                panel.PanelName = $"Panel{i + 1} (Custom)";
                                PInvoke.SetWindowText(panel.PanelHandle, panel.PanelName);
                                break;
                            }
                        }

                        if (GetPopoutPanelCountByType(PanelType.CustomPopout) != i + 1)
                            throw new PopoutManagerException("Unable to pop out the first panel. Please align first panel's number circle and check if the first panel has already been popped out. Also please check for window obstruction. Process stopped.");
                    }
                    if (i >= 1)     // only separate with 2 or more panels
                    {
                        int retry = 0;
                        while (retry < RETRY_COUNT)
                        {
                            SeparatePanel(i, _panels[0].PanelHandle);
                            PInvoke.EnumWindows(new PInvoke.CallBack(EnumCustomPopoutCallBack), i);

                            if (GetPopoutPanelCountByType(PanelType.CustomPopout) != i + 1)
                                retry += 1;
                            else
                            {
                                // Panel has successfully popped out
                                var panel = GetCustomPopoutPanelByIndex(i);

                                PInvoke.MoveWindow(panel.PanelHandle, 0, 0, 800, 600, true);
                                panel.PanelName = $"Panel{i + 1} (Custom)";
                                PInvoke.SetWindowText(panel.PanelHandle, panel.PanelName);
                                break;
                            }
                        }

                        if (GetPopoutPanelCountByType(PanelType.CustomPopout) != i + 1)
                            throw new PopoutManagerException($"Unable to pop out panel number {i + 1}. Please align the panel's number circle and check if the panel has already been popped out. Also please check for window obstruction.");
                    }
                }

                // Performance validation, make sure the number of pop out panels is equal to the number of selected panel
                if (GetPopoutPanelCountByType(PanelType.CustomPopout) != _profile.PanelSourceCoordinates.Count)
                    throw new PopoutManagerException("Unable to pop out all panels. Please align all panel number circles with in-game panel locations. Also please check for window obstruction ");

                // Add the built-in pop outs (ie. ATC, VFR Map) to the panel list
                PInvoke.EnumWindows(new PInvoke.CallBack(EnumBuiltinPopoutCallBack), _profile.PanelSourceCoordinates.Count + 1);

                // Add the MSFS Touch Panel (My other github project) windows to the panel list
                PInvoke.EnumWindows(new PInvoke.CallBack(EnumMSFSTouchPanelPopoutCallBack), _profile.PanelSourceCoordinates.Count + 1);

                if (_panels.Count == 0)
                    throw new PopoutManagerException("No panels have been found. Please select or open at least one in-game panel or MSFS Touch Panel App's panel.");

                // Line up all the panels and fill in meta data
                for (var i = _panels.Count - 1; i >= 0; i--)
                {
                    var shift = _panels.Count - i - 1;
                    _panels[i].Top = shift * 30;
                    _panels[i].Left = shift * 30;
                    _panels[i].Width = 800;
                    _panels[i].Height = 600;

                    PInvoke.MoveWindow(_panels[i].PanelHandle, _panels[i].Top, _panels[i].Left, _panels[i].Width, _panels[i].Height, true);
                    PInvoke.SetForegroundWindow(_panels[i].PanelHandle);
                    Thread.Sleep(200);
                }

                return _panels;
            }
            catch(PopoutManagerException ex)
            {
                Logger.BackgroundStatus(ex.Message, StatusMessageType.Error);
                return null;
            }
            catch
            {
                throw;
            }
        }

        private void LoadAndApplyPanelConfigs(List<PanelConfig> popoutResults)
        {
            int index;
            popoutResults.ForEach(resultPanel =>
            {
                if (resultPanel.PanelType == PanelType.CustomPopout)
                {
                    index = _profile.PanelConfigs.FindIndex(x => x.PanelIndex == resultPanel.PanelIndex);
                    if (index > -1)
                        _profile.PanelConfigs[index].PanelHandle = resultPanel.PanelHandle;
                }
                else
                {
                    index = _profile.PanelConfigs.FindIndex(x => x.PanelName == resultPanel.PanelName);
                    if (index > -1)
                        _profile.PanelConfigs[index].PanelHandle = resultPanel.PanelHandle;
                    else
                        _profile.PanelConfigs.Add(resultPanel);
                }

            });

            _profile.PanelConfigs.RemoveAll(x => x.PanelHandle == IntPtr.Zero && x.PanelType == PanelType.BuiltInPopout);

            //_profile.PanelSettings.ForEach(panel =>
            Parallel.ForEach(_profile.PanelConfigs, panel =>
            {
                if (panel != null && panel.Width != 0 && panel.Height != 0)
                {
                    // Apply panel name
                    if (panel.PanelType == PanelType.CustomPopout)
                    {
                        PInvoke.SetWindowText(panel.PanelHandle, panel.PanelName);
                        Thread.Sleep(500);
                    }

                    // Apply locations
                    PInvoke.MoveWindow(panel.PanelHandle, panel.Left, panel.Top, panel.Width, panel.Height, true);
                    Thread.Sleep(1000);

                    // Apply always on top
                    if (panel.AlwaysOnTop)
                    {
                        WindowManager.ApplyAlwaysOnTop(panel.PanelHandle, true, new Rectangle(panel.Left, panel.Top, panel.Width, panel.Height));
                        Thread.Sleep(1000);
                    }

                    // Apply hide title bar
                    if (panel.HideTitlebar)
                    {
                        WindowManager.ApplyHidePanelTitleBar(panel.PanelHandle, true);
                    }
                }
            });
        }

        private int GetPopoutPanelCountByType(PanelType panelType)
        {
            return _panels.FindAll(x => x.PanelType == panelType).Count;
        }

        private PanelConfig GetCustomPopoutPanelByIndex(int index)
        {
            return _panels.Find(x => x.PanelType == PanelType.CustomPopout && x.PanelIndex == index + 1);
        }

        private void PopoutPanel(int x, int y)
        {
            InputEmulationManager.LeftClick(x, y);
            Thread.Sleep(200);
            InputEmulationManager.PopOutPanel(x, y);
        }

        private void SeparatePanel(int index, IntPtr hwnd)
        {
            // Resize all windows to 800x600 when separating and shimmy the panel
            // MSFS draws popout panel differently at different time for same panel
            PInvoke.MoveWindow(hwnd, -8, 0, 800, 600, true);
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(250);

            // Find the magnifying glass coordinate    
            var point = AnalyzeMergedWindows(hwnd);

            InputEmulationManager.LeftClick(point.X, point.Y);
        }

        public bool EnumCustomPopoutCallBack(IntPtr hwnd, int index)
        {
            var panelInfo = GetPanelWindowInfo(hwnd);

            if(panelInfo != null && panelInfo.PanelType == PanelType.CustomPopout)
            {
                if (!_panels.Exists(x => x.PanelHandle == hwnd))
                {
                    panelInfo.PanelIndex = index + 1;       // Panel index starts at 1
                    _panels.Add(panelInfo);
                }
            }

            return true;
        }

        public bool EnumBuiltinPopoutCallBack(IntPtr hwnd, int index)
        {
            var panelInfo = GetPanelWindowInfo(hwnd);

            if (panelInfo != null && panelInfo.PanelType == PanelType.BuiltInPopout)
            {
                if (!_panels.Exists(x => x.PanelHandle == hwnd))
                {
                    panelInfo.PanelIndex = index;
                    _panels.Add(panelInfo);
                }
            }

            return true;
        }

        public bool EnumMSFSTouchPanelPopoutCallBack(IntPtr hwnd, int index)
        {
            var panelInfo = GetPanelWindowInfo(hwnd);

            if (panelInfo != null && panelInfo.PanelType == PanelType.MSFSTouchPanel)
            {
                if (!_panels.Exists(x => x.PanelHandle == hwnd))
                {
                    panelInfo.PanelIndex = index;
                    _panels.Add(panelInfo);
                }
            }

            return true;
        }

        private PanelConfig GetPanelWindowInfo(IntPtr hwnd)
        {
            var className = PInvoke.GetClassName(hwnd);

            if (className == "AceApp")      // MSFS windows designation
            {
                var caption = PInvoke.GetWindowText(hwnd);

                var panelInfo = new PanelConfig();
                panelInfo.PanelHandle = hwnd;
                panelInfo.PanelName = caption;

                if (String.IsNullOrEmpty(caption) || caption.IndexOf("Custom") > -1)
                    panelInfo.PanelType = PanelType.CustomPopout;
                else if (caption.IndexOf("Microsoft Flight Simulator") > -1)        // MSFS main game window
                    return null;
                else
                    panelInfo.PanelType = PanelType.BuiltInPopout;

                return panelInfo;
            }
            else  // For MSFS Touch Panel window
            {
                var caption = PInvoke.GetWindowText(hwnd);

                var panelInfo = new PanelConfig();
                panelInfo.PanelHandle = hwnd;
                panelInfo.PanelName = caption;

                if (caption.IndexOf("MSFS Touch Panel |") > -1)
                {
                    panelInfo.PanelType = PanelType.MSFSTouchPanel;
                    return panelInfo;
                }
                else
                    return null;
            }

            return null;
        }

        private Point AnalyzeMergedWindows(IntPtr hwnd)
        {
            var sourceImage = ImageOperation.TakeScreenShot(hwnd);

            Rectangle rectangle;
            PInvoke.GetClientRect(hwnd, out rectangle);

            var panelMenubarTop = GetPanelMenubarTop(sourceImage, rectangle);
            if (panelMenubarTop > sourceImage.Height)  
                return Point.Empty;

            var panelMenubarBottom = GetPanelMenubarBottom(sourceImage, rectangle);
            if (panelMenubarTop > sourceImage.Height)
                return Point.Empty;

            var panelsStartingLeft = GetPanelMenubarStartingLeft(sourceImage, rectangle, panelMenubarTop + 5);

            // The center of magnifying glass icon is around (2.7 x height of menubar) to the right of the panel menubar starting left
            // But need to use higher number here to click the left side of magnifying glass icon because on some panel, the ratio is smaller
            var menubarHeight = panelMenubarBottom - panelMenubarTop;
            var magnifyingIconXCoor = panelsStartingLeft - Convert.ToInt32(menubarHeight * 3.2);        // ToDo: play around with this multiplier to find the best for all resolutions
            var magnifyingIconYCoor = panelMenubarTop + Convert.ToInt32(menubarHeight / 2);

            return new Point(magnifyingIconXCoor, magnifyingIconYCoor);
        }

        private int GetPanelMenubarTop(Bitmap sourceImage, Rectangle rectangle)
        {
            // Get a snippet of 1 pixel wide vertical strip of windows. We will choose the strip left of center.
            // This is to determine when the actual panel's vertical pixel starts in the window. This will allow accurate sizing of the template image
            var left = Convert.ToInt32((rectangle.Width) * 0.70);  // look at around 70% from the left
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
                            return y + top;
                        }
                    }
                }

                sourceImage.UnlockBits(stripData);
            }

            return -1;
        }

        private int GetPanelMenubarBottom(Bitmap sourceImage, Rectangle rectangle)
        {
            // Get a snippet of 1 pixel wide vertical strip of windows. We will choose the strip about 25% from the left of the window
            var left = Convert.ToInt32((rectangle.Width) * 0.25);  // look at around 25% from the left
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

                        if (red == 255 && green == 255 && blue == 255)
                        {
                            // found the top of menu bar
                            menubarBottom = y + top;
                        }
                        else if(menubarBottom > -1)     /// it is no longer white in color, we hit menubar bottom
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

        private int GetPanelMenubarStartingLeft(Bitmap sourceImage, Rectangle rectangle, int top)
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

                        if (red == 255 && green == 255 && blue == 255)
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
