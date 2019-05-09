using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace VSIXShowHidePreviewLabel.Helpers
{
    public static class WpfWindowHelper
    {
        public static Window GetMainWindowOfCurrentProcess()
        {
            var process = Process.GetCurrentProcess();
            var hwndSource = HwndSource.FromHwnd(process.MainWindowHandle);
            var mainWindow = (Window)hwndSource.RootVisual;
            return mainWindow;
        }
    }
}
