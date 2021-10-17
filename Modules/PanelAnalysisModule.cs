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
                        var coordinate = AnalyzeMergedPopoutWindows(simulatorProcess.Handle, customPopout.Handle);
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

        public Point AnalyzeMergedPopoutWindows(IntPtr flightSimHandle, IntPtr windowHandle)
        {
            var resolution = GetWindowResolution(flightSimHandle);

            int EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR = 1;
            float EXHAUSTIVE_TEMPLATE_MATCHING_SIMILARITY_THRESHOLD = 0.9f;

            // This is to speed up pattern matching and still balance accuracy
            switch (resolution)
            {
                case FlightSimResolution.HD:
                    EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR = 2;
                    EXHAUSTIVE_TEMPLATE_MATCHING_SIMILARITY_THRESHOLD = 0.88f;
                    break;
                case FlightSimResolution.QHD:
                case FlightSimResolution.WQHD:
                case FlightSimResolution.UHD:
                    EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR = 3;
                    EXHAUSTIVE_TEMPLATE_MATCHING_SIMILARITY_THRESHOLD = 0.85f;
                    break;
            }
           

            var windowResolution = GetWindowResolution(flightSimHandle);
            var templateFileName = $"separation_button_{windowResolution}.png";

            var source = ImageOperation.ConvertToFormat(ImageOperation.TakeScreenShot(windowHandle, true), PixelFormat.Format24bppRgb);
            var template = ImageOperation.ConvertToFormat(new Bitmap(FileManager.LoadAsStream(FilePathType.PreprocessingData, templateFileName)), PixelFormat.Format24bppRgb);

            // Get the updated template image ratio based on how the initial window with all the popout panels in it are organized. This is used to resize the template image
            var templateImageRatio = GetTemplateToPanelHeightRatio(windowHandle, source, windowResolution, 1);

            if (templateImageRatio == -1)
            {
                return Point.Empty;
            }

            // Resize the source and template image to speed up exhaustive template matching analysis
            var sourceWidth = source.Width / EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR;
            var sourceHeight = source.Height / EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR;
            var templateWidth = template.Width * templateImageRatio / EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR;
            var templateHeight = template.Height * templateImageRatio / EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR;

            var resizedSource = ImageOperation.ResizeImage(source, sourceWidth, sourceHeight);
            resizedSource = ImageOperation.ConvertToFormat(resizedSource, PixelFormat.Format24bppRgb);
            
            var resizedTemplate = ImageOperation.ResizeImage(template, templateWidth, templateHeight);

            var point = ImageAnalysis.ExhaustiveTemplateMatchAnalysis(resizedSource, resizedTemplate, EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR, EXHAUSTIVE_TEMPLATE_MATCHING_SIMILARITY_THRESHOLD);

            if (point.IsEmpty)
            {
                template = ImageOperation.ConvertToFormat(new Bitmap(FileManager.LoadAsStream(FilePathType.PreprocessingData, templateFileName)), PixelFormat.Format24bppRgb);
                templateImageRatio = GetTemplateToPanelHeightRatio(windowHandle, source, windowResolution, 2);      // maybe there are 2 rows of panels in the merged pop out window
                templateWidth = template.Width * templateImageRatio / EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR;
                templateHeight = template.Height * templateImageRatio / EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR;
                resizedTemplate = ImageOperation.ResizeImage(template, templateWidth, templateHeight);

                point = ImageAnalysis.ExhaustiveTemplateMatchAnalysis(resizedSource, resizedTemplate, EXHAUSTIVE_TEMPLATE_MATCHING_SHRINK_FACTOR, EXHAUSTIVE_TEMPLATE_MATCHING_SIMILARITY_THRESHOLD);
            }

            return point;
        }

        private FlightSimResolution GetWindowResolution(IntPtr windowHandle)
        {
            var rect = new Rect();
            PInvoke.GetClientRect(windowHandle, out rect);

            switch (rect.Bottom)
            {
                case 1009:
                case 1080:
                    return FlightSimResolution.HD;
                case 1369:
                case 1440:
                    return FlightSimResolution.QHD;
                case 2089:
                case 2160:
                    return FlightSimResolution.UHD;
                default:
                    return FlightSimResolution.HD;
            }
        }

        private float GetTemplateToPanelHeightRatio(IntPtr windowHandle, Bitmap sourceImage, FlightSimResolution windowResolution, int numberOfRows)
        {
            const int SW_MAXIMIZE = 3;
            PInvoke.ShowWindow(windowHandle, SW_MAXIMIZE);
            PInvoke.SetForegroundWindow(windowHandle);
            Thread.Sleep(500);

            Rect rect = new Rect();
            PInvoke.GetClientRect(windowHandle, out rect);

            // Get a snippet of 1 pixel wide vertical strip of windows. We will choose the strip left of center.
            // This is to determine when the actual panel's vertical pixel starts in the window. This will allow accurate sizing of the template image
            var clientWindowHeight = rect.Bottom - rect.Top;
            var left = Convert.ToInt32((rect.Right - rect.Left) * 0.25);  // look at around 25% from the left
            var top = sourceImage.Height  - clientWindowHeight;

            if (top < 0 || left < 0)
                return -1;

            // Using much faster image LockBits instead of GetPixel method
            unsafe
            {
                var stripData = sourceImage.LockBits(new Rectangle(left, top, 1, clientWindowHeight), ImageLockMode.ReadWrite, sourceImage.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(stripData.PixelFormat) / 8;
                int heightInPixels = stripData.Height;
                int widthInBytes = stripData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)stripData.Scan0;

                // Find the first white pixel and have at least 4 more white pixels in a row (the panel title bar)
                const int WHITE_PIXEL_TO_COUNT = 4;
                int whitePixelCount = 0;

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
                            whitePixelCount++;

                            if (whitePixelCount == WHITE_PIXEL_TO_COUNT)
                            {
                                sourceImage.UnlockBits(stripData);
                                var unpopPanelSize = (clientWindowHeight - (y) * 2) / Convert.ToSingle(numberOfRows);
                                var currentRatio = unpopPanelSize / Convert.ToSingle(clientWindowHeight);
                                return currentRatio;
                            }
                        }
                        else
                        {
                            whitePixelCount = 0;
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
            Thread.Sleep(1000);

            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, point.X, point.Y, 0, 0);
            Thread.Sleep(200);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, point.X, point.Y, 0, 0);
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

                var srcImage = ImageOperation.ConvertToFormat(ImageOperation.TakeScreenShot(popout.Handle, false), PixelFormat.Format24bppRgb);
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
