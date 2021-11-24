using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.IO;

namespace MSFSPopoutPanelManager
{
    public class PanelAnalysisModule
    {
        private Form _form;

        public PanelAnalysisModule(Form form)
        {
            _form = form;
        }

        public void Analyze(WindowProcess simulatorProcess, int profileId, int panelsCount)
        {
            var panelsToBeIdentified = panelsCount;
            // Get all child windows 
            var processZero = GetProcessZero();

            // Move process zero childs back into simulator process
            MoveChildWindowsIntoSimulatorProcess(simulatorProcess, processZero);

            if (simulatorProcess.ChildWindows.Count > 0)
            {
                foreach(var customPopout in simulatorProcess.ChildWindows.FindAll(x => x.WindowType == WindowType.Undetermined))
                {
                    while (panelsToBeIdentified > 1)        // Do not have to separate the last panel
                    {
                        var coordinate = AnalyzeMergedPopoutWindows(customPopout.Handle);
                        if (!coordinate.IsEmpty)
                            SeparateUntitledPanel(customPopout.Handle, coordinate.X, coordinate.Y);

                        panelsToBeIdentified--;
                    }

                    panelsToBeIdentified = panelsCount;
                }
            }

            // Now all newly pop out windows are in process zero, move them into flight simulator process
            processZero = GetProcessZero();
            MoveChildWindowsIntoSimulatorProcess(simulatorProcess, processZero);

            // Analyze the content of the pop out panels
            AnalyzePopoutWindows(simulatorProcess, profileId);
        }

        private WindowProcess GetProcessZero()
        {
            // Get process with PID of zero (PID zero launches all the popout windows for MSFS)
            var process = Process.GetProcesses().ToList().Find(x => x.Id == 0);
            var processZero = new WindowProcess()
                            {
                                ProcessId = process.Id,
                                ProcessName = process.ProcessName,
                                Handle = process.MainWindowHandle
                            };

            GetChildWindows(processZero);

            return processZero;
        }

        private void GetChildWindows(WindowProcess process)
        {
            int classNameLength = 256;

            var childHandles = GetAllChildHandles(process.Handle);

            childHandles.ForEach(childHandle =>
            {
                StringBuilder className = new StringBuilder(classNameLength);
                PInvoke.GetClassName(childHandle, className, classNameLength);

                if (className.ToString() == "AceApp")
                {
                    process.ChildWindows.Add(new ChildWindow
                    {
                        ClassName = "AceApp",
                        Handle = childHandle,
                        Title = GetWindowTitle(childHandle)
                    });
                }
            });
        }

        private List<IntPtr> GetAllChildHandles(IntPtr parent)
        {
            var childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                PInvoke.EnumWindowProc childProc = new PInvoke.EnumWindowProc(EnumWindow);
                PInvoke.EnumChildWindows(parent, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            var gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList.Target == null)
                return false;

            var childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }

        private string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder title = new StringBuilder(1024);
            PInvoke.GetWindowText(hWnd, title, title.Capacity);

            return String.IsNullOrEmpty(title.ToString()) ? null : title.ToString();
        }

        private void MoveChildWindowsIntoSimulatorProcess(WindowProcess simulatorProcess, WindowProcess processZero)
        {
            // The popout windows such as PFD and MFD attached itself to main window for Process ID zero instead of the MSFS process. 
            // Moving these windows back into MSFS main window
            if (processZero != null)
            {
                // Clean up all existing simulator process child window data
                simulatorProcess.ChildWindows.RemoveAll(x => x.WindowType == WindowType.Custom_Popout || x.WindowType == WindowType.BuiltIn_Popout);

                foreach (var child in processZero.ChildWindows)
                {
                    int parentProcessId;
                    PInvoke.GetWindowThreadProcessId(child.Handle, out parentProcessId);

                    if (simulatorProcess != null && parentProcessId == simulatorProcess.ProcessId && !simulatorProcess.ChildWindows.Exists(x => x.Handle == child.Handle))
                    {
                        if (String.IsNullOrEmpty(child.Title))
                            child.WindowType = WindowType.Undetermined;
                        else if (child.Title.Contains("(Custom)"))
                            child.WindowType = WindowType.Custom_Popout;
                        else if (child.Title.Contains("Microsoft Flight Simulator"))
                            child.WindowType = WindowType.FlightSimMainWindow;
                        else if (!String.IsNullOrEmpty(child.Title))
                            child.WindowType = WindowType.BuiltIn_Popout;
                        else
                            child.WindowType = WindowType.Undetermined;

                        simulatorProcess.ChildWindows.Add(child);
                    }
                }
            }
        }

        public Point AnalyzeMergedPopoutWindows(IntPtr windowHandle)
        {
            float EXHAUSTIVE_TEMPLATE_MATCHING_SIMILARITY_THRESHOLD = 0.86f;

            var sourceImage = ImageOperation.TakeScreenShot(windowHandle, true);
            var templateImage = ImageOperation.GetExpandButtonImage(sourceImage.Height);

            var panelsStartingTop = GetPanelsStartingTop(windowHandle, sourceImage);

            if (panelsStartingTop > sourceImage.Height / 2)     // if usually the last panel occupied the entire window with no white menubar
                return Point.Empty;

            var panelsStartingLeft = GetPanelsStartingLeft(windowHandle, sourceImage, panelsStartingTop + 5);

            var templateImageRatios = GetExpandButtonHeightRatio(windowHandle, sourceImage, 1);

            var resizedSource = ImageOperation.CropImage(sourceImage, 0, panelsStartingTop, sourceImage.Width, sourceImage.Height / 12);  // add around 100px per 1440p resolution

            resizedSource.Save(FileManager.GetFilePathByType(FilePathType.PreprocessingData) + "source.png");

            var resizedTemplate = ImageOperation.ResizeImage(templateImage, templateImage.Width * templateImageRatios[0], templateImage.Height * templateImageRatios[0]);
            var point = ImageAnalysis.ExhaustiveTemplateMatchAnalysisAsync(resizedSource, resizedTemplate, EXHAUSTIVE_TEMPLATE_MATCHING_SIMILARITY_THRESHOLD, panelsStartingTop, panelsStartingLeft);
            
            if (point.IsEmpty)
            {
                resizedTemplate = ImageOperation.ResizeImage(templateImage, templateImage.Width * templateImageRatios[1], templateImage.Height * templateImageRatios[1]);
                point = ImageAnalysis.ExhaustiveTemplateMatchAnalysisAsync(resizedSource, resizedTemplate, EXHAUSTIVE_TEMPLATE_MATCHING_SIMILARITY_THRESHOLD, panelsStartingTop, panelsStartingLeft);
            }

            return point;
        }

        private List<double> GetExpandButtonHeightRatio(IntPtr windowHandle, Bitmap sourceImage, int numberOfRows, double percentFromLeft = 0.48)
        {
            var ratios = new List<double>();

            const int SW_MAXIMIZE = 3;
            PInvoke.ShowWindow(windowHandle, SW_MAXIMIZE);
            PInvoke.SetForegroundWindow(windowHandle);
            Thread.Sleep(200);

            Rect rect = new Rect();
            PInvoke.GetClientRect(windowHandle, out rect);

            // Get a snippet of 1 pixel wide vertical strip of windows. We will choose the strip left of center.
            // This is to determine when the actual panel's vertical pixel starts in the window. This will allow accurate sizing of the expand button image
            var clientWindowHeight = rect.Bottom - rect.Top;
            var left = Convert.ToInt32((rect.Right - rect.Left) * percentFromLeft);  // look at around 48% from the left
            var top = sourceImage.Height  - clientWindowHeight;

            // Using much faster image LockBits instead of GetPixel method
            unsafe
            {
                var stripData = sourceImage.LockBits(new Rectangle(left, top, 1, clientWindowHeight), ImageLockMode.ReadWrite, sourceImage.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(stripData.PixelFormat) / 8;
                int heightInPixels = stripData.Height;
                int widthInBytes = stripData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)stripData.Scan0;

                // Find the first white pixel (the panel title bar)
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

                            var unpopPanelSize = (clientWindowHeight - (y * 2)) / Convert.ToDouble(numberOfRows);

                            ratios.Add(unpopPanelSize / Convert.ToDouble(clientWindowHeight));      // 1 row of panel
                            ratios.Add(unpopPanelSize / 2 / Convert.ToDouble(clientWindowHeight));  // 2 rows of panel
                            return ratios;
                        }
                    }
                }

                sourceImage.UnlockBits(stripData);
            }

            return GetExpandButtonHeightRatio(windowHandle, sourceImage, numberOfRows, percentFromLeft - 0.01);
        }

        private int GetPanelsStartingTop(IntPtr windowHandle, Bitmap sourceImage, double percentFromLeft = 0.49)
        {
            const int SW_MAXIMIZE = 3;
            PInvoke.ShowWindow(windowHandle, SW_MAXIMIZE);
            PInvoke.SetForegroundWindow(windowHandle);
            Thread.Sleep(250);

            Rect rect = new Rect();
            PInvoke.GetClientRect(windowHandle, out rect);

            // Get a snippet of 1 pixel wide vertical strip of windows. We will choose the strip left of center.
            // This is to determine when the actual panel's vertical pixel starts in the window. This will allow accurate sizing of the template image
            var clientWindowHeight = rect.Bottom - rect.Top;
            var left = Convert.ToInt32((rect.Right - rect.Left) * percentFromLeft);  // look at around 49% from the left
            var top = sourceImage.Height - clientWindowHeight;

            if (top < 0 || left < 0)
                return -1;

            unsafe
            {
                var stripData = sourceImage.LockBits(new Rectangle(left, top, 1, clientWindowHeight), ImageLockMode.ReadWrite, sourceImage.PixelFormat);

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

            return GetPanelsStartingTop(windowHandle, sourceImage, percentFromLeft - 0.01);
        }

        private int GetPanelsStartingLeft(IntPtr windowHandle, Bitmap sourceImage, int top)
        {
            Rect rect = new Rect();
            PInvoke.GetClientRect(windowHandle, out rect);

            // Get a snippet of 1 pixel wide horizontal strip of windows
            var clientWindowWidth = rect.Right - rect.Left;

            unsafe
            {
                var stripData = sourceImage.LockBits(new Rectangle(0, top, clientWindowWidth, 1), ImageLockMode.ReadWrite, sourceImage.PixelFormat);

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

        private void SeparateUntitledPanel(IntPtr windowHandle, int x, int y)
        {
            const uint MOUSEEVENTF_LEFTDOWN = 0x02;
            const uint MOUSEEVENTF_LEFTUP = 0x04;

            var point = new Point { X = x, Y = y };
            Cursor.Position = new Point(point.X, point.Y);

            // Wait for mouse to get into position
            Thread.Sleep(500);

            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, point.X, point.Y, 0, 0);
            Thread.Sleep(200);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, point.X, point.Y, 0, 0);

            Cursor.Position = new Point(point.X + 50, point.Y + 50);
        }

        private void AnalyzePopoutWindows(WindowProcess simulatorProcess, int profileId)
        {
            var panelScores = new List<PanelScore>();

            // Get analysis template data for the profile
            var planeProfile = FileManager.ReadAllPlaneProfileData().Find(x => x.ProfileId == profileId);
            var templateData = FileManager.ReadAllAnalysisTemplateData().Find(x => x.TemplateName == planeProfile.AnalysisTemplateName);

            if(templateData == null)
            {
                CreateNewAnalysisTemplate(simulatorProcess, profileId);
                return;
            }

            // Load the template images for the selected profile
            var templates = new List<KeyValuePair<string, Bitmap>>();
            foreach (var template in templateData.Templates)
            {
                foreach (var imagePath in template.ImagePaths)
                {
                    templates.Add(new KeyValuePair<string, Bitmap>(template.PopoutName, ImageOperation.ConvertToFormat(new Bitmap(FileManager.LoadAsStream(FilePathType.AnalysisData, imagePath)), PixelFormat.Format24bppRgb)));
                }
            }

            var popouts = simulatorProcess.ChildWindows.FindAll(x => x.WindowType == WindowType.Undetermined);

            foreach (var popout in popouts)
            {
                popout.WindowType = WindowType.Custom_Popout;

                // Resize all untitled pop out panels to 800x600 and set it to foreground
                PInvoke.MoveWindow(popout.Handle, 0, 0, 800, 600, true);
                PInvoke.SetForegroundWindow(popout.Handle);

                Thread.Sleep(300);      // ** this delay is important to allow the window to go into focus before screenshot is taken

                var srcImage = ImageOperation.TakeScreenShot(popout.Handle, false);
                var srcImageBytes = ImageOperation.ImageToByte(srcImage);

                Parallel.ForEach(templates, new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 1.0)) }, template =>
                {
                    var src = ImageOperation.ByteToImage(srcImageBytes);
                    panelScores.Add(new PanelScore
                    {
                        WindowHandle = popout.Handle,
                        PanelName = template.Key,
                        Score = ImageAnalysis.ExhaustiveTemplateMatchAnalysisScore(src, template.Value, 0.85f)
                    });
                });
            }

            // Gets the highest matching score for template matches for each panel
            var panels = (from s in panelScores
                          group s by s.WindowHandle into g
                          select g.OrderByDescending(z => z.Score).FirstOrDefault()).ToList();

            // Set the pop out panel title bar text to identify it
            foreach (var panel in panels)
            {
                var title = $"{panel.PanelName} (Custom)";
                simulatorProcess.ChildWindows.Find(x => x.Handle == panel.WindowHandle).Title = title;
                PInvoke.SetWindowText(panel.WindowHandle, title);
            }
        }

        private void CreateNewAnalysisTemplate(WindowProcess simulatorProcess, int profileId)
        {
            var planeProfile = FileManager.ReadAllPlaneProfileData().Find(x => x.ProfileId == profileId);

            var popouts = simulatorProcess.ChildWindows.FindAll(x => x.WindowType == WindowType.Undetermined);
            var images = new List<Bitmap>();
            

            foreach (var popout in popouts)
            {
                popout.WindowType = WindowType.Custom_Popout;

                // Resize all untitled pop out panels to 800x600 and set it to foreground
                PInvoke.MoveWindow(popout.Handle, 0, 0, 800, 600, true);
                PInvoke.SetForegroundWindow(popout.Handle);

                Thread.Sleep(300);      // ** this delay is important to allow the window to go into focus before screenshot is taken

                var screenshot = ImageOperation.TakeScreenShot(popout.Handle, false);
                images.Add(screenshot);
            }

            var customAnalysisDataList = FileManager.ReadCustomAnalysisTemplateData();

            AnalysisData analysisData = new AnalysisData();
            analysisData.TemplateName = planeProfile.ProfileName;
            analysisData.IsUserTemplate = true;

            for (var i = 0; i < popouts.Count; i++)
            {
                var panelName = "Panel" + (i + 1);
                var imageName = @$"{panelName}.png";
                var imagePath = analysisData.TemplateImagePath;

                using (var memoryStream = new MemoryStream())
                {
                    images[i].Save(memoryStream, ImageFormat.Png);
                    FileManager.SaveFile(FilePathType.AnalysisData, imagePath, imageName, memoryStream);
                }

                analysisData.Templates.Add(new Template()
                {
                    PopoutId = i + 1,
                    PopoutName = panelName,
                    ImagePaths = new List<string>() { @$"{imagePath}/{imageName}" } 
                });

                var panelTitle = $"{panelName} (Custom)";
                simulatorProcess.ChildWindows.Find(x => x.Handle == popouts[i].Handle).Title = panelTitle;
                PInvoke.SetWindowText(popouts[i].Handle, panelTitle);
            }

            customAnalysisDataList.Add(analysisData);
            FileManager.WriteCustomAnalysisTemplateData(customAnalysisDataList);
        }

        private Bitmap CloneImage(Bitmap srcImage)
        {
            return srcImage.Clone(new Rectangle(0, 0, srcImage.Width, srcImage.Height), PixelFormat.Format24bppRgb);
        }
    }


    public class PanelScore
    {
        public IntPtr WindowHandle { get; set; }

        public string PanelName { get; set; }

        public float Score { get; set; }
    }
}
