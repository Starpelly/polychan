using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Polychan.GUI.Input;

namespace Polychan.GUI.Framework.Platform.Windows;

internal class WindowsWindow : IWindow
{
    const string POPUP_CLASS = "PopupWindowClass";

    const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
    const int WS_VISIBLE = 0x10000000;
    const int WS_POPUP = unchecked((int)0x80000000);
    const int WS_BORDER = 0x00800000;

    const int WS_EX_TOOLWINDOW = 0x00000080;
    const int WS_EX_TOPMOST = 0x00000008;

    const int CW_USEDEFAULT = unchecked((int)0x80000000);
    const int WM_DESTROY = 0x0002;
    const int WM_RBUTTONDOWN = 0x0204;
    const int WM_LBUTTONDOWN = 0x0201;

    const int CS_DROPSHADOW = 0x00020000;

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName,
        int dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [DllImport("user32.dll")]
    static extern void PostQuitMessage(int nExitCode);

    const int IDC_ARROW = 32512;
    const int SW_SHOWNOACTIVATE = 4;

    delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    struct WNDCLASS
    {
        public uint style;
        public WndProc lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int x;
        public int y;
    }

    static IntPtr PopupWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static bool s_init = false;
    public static void Init()
    {
        if (s_init) return;
        s_init = true;

        WNDCLASS popupWc = new WNDCLASS
        {
            style = CS_DROPSHADOW,
            lpfnWndProc = PopupWndProc,
            hInstance = GetModuleHandle(null),
            lpszClassName = POPUP_CLASS,
            hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW)
        };
        RegisterClass(ref popupWc);
    }

    public IntPtr WindowPtr { get; private set; }

    public static IntPtr CreatePopup(IntPtr parent, int x, int y, int width, int height)
    {
        Init();

        var ptr = CreateWindowEx(
            WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            POPUP_CLASS,
            null,
            WS_POPUP | WS_BORDER,
            x, y,
            width, height,
            parent,
            IntPtr.Zero,
            GetModuleHandle(null),
            IntPtr.Zero);
        return ptr;
    }

    public void Create(IWindow? parent, WindowFlags flags)
    {

    }

    public void Close()
    {
    }

    public void Raise()
    {
    }

    public void Show()
    {
    }

    public void Hide()
    {
    }

    public void CopyIconFromWindow(IWindow window)
    {
    }

    public void SetIconFromStream(Stream imageStream)
    {
    }

    public void Dispose()
    {
    }

    public event Action? ExitRequested;
    public event Action? Exited;
    public event Action? Resized;
    public event Action<Vector2> MouseMove;
    public event Action<Vector2, MouseButton> MouseDown;
    public event Action<Vector2, MouseButton> MouseUp;
    public event Action? MouseEntered;
    public event Action? MouseLeft;
    public event Action<Vector2, Vector2, bool> MouseWheel;
    public event Action<WindowState> WindowStateChanged;
    public event Action<Point> Moved;

    public bool IsActive => throw new NotImplementedException();

    public WindowState WindowState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Size ClientSize => throw new NotImplementedException();

    public Point Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Size Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public float Scale => throw new NotImplementedException();

    public Size MinSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Size MaxSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool Resizable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}