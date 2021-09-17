using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Tesseract;

namespace MSFSPopoutPanelManager
{
    public class AnalysisEngine
    {
        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 GetClassName(IntPtr hWnd, StringBuilder StrPtrClassName, Int32 nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);
        private const short SWP_NOZORDER = 0X4;
        private const int SWP_SHOWWINDOW = 0x0040;

        
        public AnalysisEngine() { }

        public event EventHandler<EventArgs<string>> OnStatusUpdated;
        public event EventHandler<EventArgs<Dictionary<string, string>>> OnOcrDebugged;

        public void Analyze(ref MainWindow simWindow, string profile)
        {
            MainWindow processZeroMainWindow = null;

            // Get process with PID of zero (PID zero launches all the popout windows for MSFS)
            foreach (var process in Process.GetProcesses())
            {
                if(process.Id == 0)
                    processZeroMainWindow = new MainWindow()
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        Title = "Process Zero",
                        Handle = process.MainWindowHandle
                    };
            }

            // Get all child windows 
            GetChildWindows(simWindow);
            GetChildWindows(processZeroMainWindow);

            // The popout windows such as PFD and MFD attached itself to main window for Process ID zero instead of the MSFS process. 
            // Moving these windows back into MSFS main window
            if (processZeroMainWindow != null)
            {
                foreach (var child in processZeroMainWindow.ChildWindowsData)
                {
                    int parentProcessId;
                    GetWindowThreadProcessId(child.Handle, out parentProcessId);

                    if (simWindow != null && parentProcessId == simWindow.ProcessId)
                        simWindow.ChildWindowsData.Add(child);
                }
            }

            int classNameLength = 256;
            foreach (var childWindow in simWindow.ChildWindowsData)
            {
                StringBuilder className = new StringBuilder(classNameLength);
                GetClassName(childWindow.Handle, className, classNameLength);
                childWindow.ClassName = className.ToString();
            }

            // Remove all child windows where class name is not 'AceApp'
            simWindow.ChildWindowsData.RemoveAll(x => x.ClassName != "AceApp" || (x.Title != null && x.Title.Contains("Microsoft Flight Simulator", StringComparison.CurrentCultureIgnoreCase)));

            if(simWindow.ChildWindowsData.Count > 0)
            {
                var ocrEvaluationData = FileManager.ReadProfileData().Find(x => x.Profile == profile);
                var ocrImageScale = ocrEvaluationData.OCRImageScale;
                Dictionary<string, string> debugInfo = new Dictionary<string, string>();

                foreach (var childWindow in simWindow.ChildWindowsData)
                {
                    // Figure out what windows is what?
                    if (String.IsNullOrEmpty(childWindow.Title))
                    {
                        // Theses are the windows with no system menu bar title (ie. PFD, MFD, FMS, etc)
                        // We need to take a screenshot and do OCR to try to figure them out
                        Rect rect = new Rect();
                        GetWindowRect(childWindow.Handle, out rect);

                        // Set window to be custom scale (eaiser to OCR) and set to foreground before taking screenshot image
                        var originalWidth = rect.Right - rect.Left;
                        var originalHeight = rect.Bottom - rect.Top;

                        var newWidth = Convert.ToInt32(originalWidth * ocrImageScale);
                        var newHeight = Convert.ToInt32(originalHeight * ocrImageScale);
                        rect.Right = rect.Left + newWidth;
                        rect.Bottom = rect.Top + newHeight;
                        SetWindowPos(childWindow.Handle, 0, rect.Left, rect.Top, newWidth, newHeight, SWP_NOZORDER | SWP_SHOWWINDOW);
                        SetForegroundWindow(childWindow.Handle);
                        Thread.Sleep(500);

                        var image = TakeScreenShot(rect);
                        SetWindowPos(childWindow.Handle, 0, rect.Left, rect.Top, originalWidth, originalHeight, SWP_NOZORDER | SWP_SHOWWINDOW);

                        // OCR the image into text
                        var imageText = OcrImage(image);
                        var popoutName = EvaluateImageText(imageText, ocrEvaluationData);
                        childWindow.Title = childWindow.Title ?? popoutName;
                        childWindow.PopoutType = PopoutType.Custom;

                        if(!debugInfo.TryAdd(popoutName ?? $"Failed Analysis - {childWindow.Handle.ToString()}", imageText))
                        {
                            debugInfo.Add($"{popoutName} - {childWindow.Handle.ToString()}", imageText);
                        }
                    }
                }

                // Output debug info
                if(debugInfo.Count > 0)
                    OnOcrDebugged?.Invoke(this, new EventArgs<Dictionary<string, string>>(debugInfo));

                // Remove all windows that cannot be identified
                simWindow.ChildWindowsData.RemoveAll(x => x.Title == null);
            }
            else
            {
                OnStatusUpdated?.Invoke(this, new EventArgs<string>("No pop out panels to analyze."));
            }
        }

        private void GetChildWindows(MainWindow mainWindow)
        {
            var childHandles = GetAllChildHandles(mainWindow.Handle);

            childHandles.ForEach(childHandle =>
            {
                mainWindow.ChildWindowsData.Add(new ChildWindow
                {
                    Handle = childHandle,
                    Title = GetWindowTitle(childHandle)
                });
            });
        }

        private string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder title = new StringBuilder(1024);
            GetWindowText(hWnd, title, title.Capacity);

            return String.IsNullOrEmpty(title.ToString()) ? null : title.ToString();
        }

        private List<IntPtr> GetAllChildHandles(IntPtr parent)
        {
            var childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(parent, childProc, pointerChildHandlesList);
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

        private byte[] TakeScreenShot(Rect rect)
        {
            var bounds = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(new System.Drawing.Point(bounds.Left, bounds.Top), System.Drawing.Point.Empty, bounds.Size);
            }

            var converter = new ImageConverter();
            var imageBytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));

            // Convert image to grayscale and invert the color to give tesseract a better image to analyze
            using(var img = SixLabors.ImageSharp.Image.Load(imageBytes))
            {
                img.Mutate(x => x.Invert().GaussianSharpen().Grayscale());
                var memoryStream = new MemoryStream();
                img.Save(memoryStream, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder());
                img.Save(@".\test.jpg");
                return memoryStream.ToArray();
            }
        }

        private string OcrImage(byte[] imageByteArray)
        {
            var ocrengine = new TesseractEngine(@".\tessdata", "eng", EngineMode.Default);
            var iamge = Pix.LoadFromMemory(imageByteArray);
            var result = ocrengine.Process(iamge);

            return result.GetText();
        }

        
        private string EvaluateImageText(string imageText, OcrEvalData ocrEvaluationData)
        {
            if(ocrEvaluationData != null)
            {
                var popoutNames = ocrEvaluationData.EvalData.Select(x => x.PopoutName).Distinct();

                foreach(var popoutName in popoutNames)
                {
                    if (ocrEvaluationData.EvalData.Find(x => x.PopoutName == popoutName).Data.Any(s => imageText.Contains(s.ToLower(), StringComparison.CurrentCultureIgnoreCase)))
                        return popoutName;
                }
            }

            return null;
        }
    }
}
