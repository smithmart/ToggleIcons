using System.Runtime.InteropServices;
using System.Text;

public class Toggler {
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_COMMAND = 0x111;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    public static string GetWindowText(IntPtr hWnd) {
        int size = GetWindowTextLength(hWnd);
        if (size++ > 0) {
            var builder = new StringBuilder(size);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }
        return String.Empty;
    }

    public static IEnumerable<IntPtr> FindWindowsWithClass(string className) {
        List<IntPtr> windows = new List<IntPtr>();
        EnumWindows(delegate (IntPtr wnd, IntPtr param) {
            StringBuilder cl = new StringBuilder(256);
            GetClassName(wnd, cl, cl.Capacity);
            if (cl.ToString() == className && GetWindowText(wnd) == "")
                windows.Add(wnd);
            return true;
        }, IntPtr.Zero);
        return windows;
    }

    public static void ToggleDesktopIcons() {
        var toggleDesktopCommand = new IntPtr(0x7402);

        // SHELLDLL_DefView is a direct child of Progman on Win7, Win10, and Win11.
        // FindWindow requires the window title; passing null returns zero.
        IntPtr progman = FindWindow("Progman", "Program Manager");
        IntPtr hWnd = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);

        // Fallback: some wallpaper apps reparent SHELLDLL_DefView under a WorkerW.
        if (hWnd == IntPtr.Zero) {
            foreach (IntPtr workerW in FindWindowsWithClass("WorkerW")) {
                hWnd = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (hWnd != IntPtr.Zero) break;
            }
        }

        if (hWnd != IntPtr.Zero)
            SendMessage(hWnd, WM_COMMAND, toggleDesktopCommand, IntPtr.Zero);
    }
}
