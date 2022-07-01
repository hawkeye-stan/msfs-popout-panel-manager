using MSFSPopoutPanelManager.Provider;
using System;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp
{
    internal static class WindowExtensions
    {
        public static void FixWindowMaximizeCropping(this Window window)
        {
            window.SourceInitialized += (s, e) =>
            {
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                var currentStyle = PInvoke.GetWindowLong(hwnd, PInvokeConstant.GWL_STYLE).ToInt64();

                PInvoke.SetWindowLong(hwnd, PInvokeConstant.GWL_STYLE, (uint)(currentStyle & ~PInvokeConstant.WS_MAXIMIZEBOX & ~PInvokeConstant.WS_MINIMIZEBOX));
            };
        }
    }
}
