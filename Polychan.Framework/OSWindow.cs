using SDL;
using System.Diagnostics;
using Polychan.GUI.Framework.Utils;
using Polychan.GUI.Framework.Platform;
using Polychan.GUI.Framework.Platform.SDL3;
using Polychan.GUI.Framework.Platform.Windows;

namespace Polychan.Framework;

public class OSWindow : IDisposable
{
    public readonly IWindow Window;
    
    public unsafe SDL_Window* SDLWindowHandle => ((SDL3Window)Window).SDLWindowHandle;
    public SDL_WindowID SDLWindowID => ((SDL3Window)Window).SDLWindowID;

    public OSWindow()
    {
        switch (RuntimeInfo.OS)
        {
            case RuntimeInfo.Platform.Windows:
                Debug.Assert(OperatingSystem.IsWindows());
                Window = new SDL3WindowsWindow();
                break;
            default:
                throw new InvalidOperationException($"Could not find a suitable window for the selected operating system ({RuntimeInfo.OS})");
        }
    }

    public static void PollEvents()
    {
        SDL3Window.pollSDLEvents();
    }

    public void RunCommands()
    {
        (Window as SDL3Window)!.RunCommands();
    }

    public void Dispose()
    {
        Window.Dispose();
        GC.SuppressFinalize(this);
    }
}