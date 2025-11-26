using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Polychan.GUI.Framework.Platform.SDL3;
using Polychan.GUI.Framework.Platform.Windows.Native;

namespace Polychan.GUI.Framework.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    internal class SDL3WindowsWindow : SDL3Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_SYSMENU = 0x00080000;

        private const int seticon_message = 0x0080;
        private const int icon_big = 1;
        private const int icon_small = 0;

        private const int large_icon_size = 256;
        private const int small_icon_size = 16;

        private Icon? m_smallIcon;
        private Icon? m_largeIcon;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ScreenToClient(nint hWnd, ref Point point);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ClientToScreen(nint hWnd, ref Point point);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(nint hwnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("user32.dll", SetLastError = true)]
        static extern nint GetClassLongPtr(nint hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern nint SetClassLongPtr(nint hWnd, int nIndex, nint dwNewLong);

        const int GCL_STYLE = -26;
        const int CS_DROPSHADOW = 0x00020000;

        internal override void OnCreate(WindowFlags wf)
        {
            int style = GetWindowLong(WindowHandle, GWL_STYLE);
            var oldStyle = style;

            if (wf.HasFlag(WindowFlags.Dialog))
            {
                // Removes the "minimize" and "maximize" buttons on the window.
                style &= ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX;
            }

            if (wf.HasFlag(WindowFlags.SysMenu))
            {
                // style &= WS_SYSMENU;
            }

            if (wf.HasFlag(WindowFlags.Modal))
            {
                // Disables input for the parent window, acting as a "true modal".
                if (ParentWindow != null)
                {
                    EnableWindow(ParentWindow.WindowHandle, false);
                }
            }

            if (oldStyle != style)
            {
                SetWindowLong(WindowHandle, GWL_STYLE, style);
            }
        }

        internal override void OnClose(WindowFlags wf)
        {
            if (wf.HasFlag(WindowFlags.Modal))
            {
                // Enables input for the parent window if we disabled it before.
                if (ParentWindow != null)
                {
                    EnableWindow(ParentWindow.WindowHandle, true);
                }
            }
        }

        /// <summary>
        /// On Windows, SDL will use the same image for both large and small icons (scaled as necessary).
        /// This can look bad if scaling down a large image, so we use the Windows API directly so as
        /// to get a cleaner icon set than SDL can provide.
        /// If called before the window has been created, or we do not find two separate icon sizes, we fall back to the base method.
        /// </summary>
        internal override void SetIconFromGroup(IconGroup iconGroup)
        {
            m_smallIcon = iconGroup.CreateIcon(small_icon_size, small_icon_size);
            m_largeIcon = iconGroup.CreateIcon(large_icon_size, large_icon_size);

            nint windowHandle = WindowHandle;

            if (windowHandle == nint.Zero || m_largeIcon == null || m_smallIcon == null)
                base.SetIconFromGroup(iconGroup);
            else
            {
                SetIconNative(m_smallIcon, m_largeIcon);
            }
        }

        internal void SetIconNative(Icon smallIcon, Icon bigIcon)
        {
            SendMessage(WindowHandle, seticon_message, icon_small, smallIcon.Handle);
            SendMessage(WindowHandle, seticon_message, icon_big, bigIcon.Handle);
        }

        internal override void CopyIconFromOther(SDL3Window other)
        {
            if (other is not SDL3WindowsWindow window)
                throw new Exception("How did you do this?");

            if (window.m_smallIcon == null || window.m_largeIcon == null)
                return;

            SetIconNative(window.m_smallIcon, window.m_largeIcon);
        }

        internal void ResetDropShadow()
        {
            DisableDropShadowViaClassStyle(WindowHandle);
        }

        internal void AddDropShadow()
        {
            TryEnableDropShadowViaClassStyle(WindowHandle);
            // AddDropShadow(WindowHandle);
        }

        static void AddDropShadow(nint hwnd)
        {
            if (DwmIsCompositionEnabled(out bool enabled) == 0 && enabled)
            {
                var margins = new MARGINS
                {
                    cxLeftWidth = 1,
                    cxRightWidth = 1,
                    cyTopHeight = 1,
                    cyBottomHeight = 1
                };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
        }

        static nint OriginalWindowStylePTR;

        static void TryEnableDropShadowViaClassStyle(nint hwnd)
        {
            if (OriginalWindowStylePTR == 0)
                OriginalWindowStylePTR = GetClassLongPtr(hwnd, GCL_STYLE);

            SetClassLongPtr(hwnd, GCL_STYLE, (nint)(OriginalWindowStylePTR.ToInt64() | CS_DROPSHADOW));
        }

        static void DisableDropShadowViaClassStyle(nint hwnd)
        {
            if (OriginalWindowStylePTR == 0)
                return;

            SetClassLongPtr(hwnd, GCL_STYLE, OriginalWindowStylePTR);
        }
    }
}
