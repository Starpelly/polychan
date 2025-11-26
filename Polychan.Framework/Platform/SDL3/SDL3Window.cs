using Polychan.GUI.Framework.Extensions.ImageExtensions;
using SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polychan.GUI.Framework.Platform.Windows;
using Polychan.GUI.Framework.Threading;
using Polychan.GUI.Framework.Utils;
using static SDL.SDL3;

namespace Polychan.GUI.Framework.Platform.SDL3
{
    internal abstract unsafe partial class SDL3Window : IWindow
    {
        internal SDL_Window* SDLWindowHandle { get; private set; } = null;
        internal SDL_WindowID SDLWindowID { get; private set; }

        /// <summary>
        /// Returns true if the window has been created.
        /// Returns false if the window has not yet been created, or has been closed.
        /// </summary>
        public bool Exists { get; private set; }

        private const int default_width = 1366;
        private const int default_height = 768;

        private const int default_icon_size = 256;

        private WindowFlags m_winFlags = WindowFlags.None;

        private static readonly Dictionary<SDL_WindowID, SDL3Window> s_openedWindows = [];

        private string m_title = string.Empty;

        /// <summary>
        /// Scheduler for actions to run before the next event loop.
        /// </summary>
        private readonly Scheduler m_commandScheduler = new();

        /// <summary>
        /// Gets and sets the window title.
        /// </summary>
        public string Title
        {
            get => m_title;
            set
            {
                m_title = value;
                ScheduleCommand(() => SDL_SetWindowTitle(SDLWindowHandle, m_title));
            }
        }

        /// <summary>
        /// Whether the current display server is Wayland.
        /// </summary>
        internal bool IsWayland => SDL_GetCurrentVideoDriver() == "wayland";

        /// <summary>
        /// Gets the native window handle as provided by the operating system.
        /// </summary>
        public IntPtr WindowHandle
        {
            get
            {
                if (SDLWindowHandle == null)
                    return IntPtr.Zero;

                var props = SDL_GetWindowProperties(SDLWindowHandle);

                switch (RuntimeInfo.OS)
                {
                    case RuntimeInfo.Platform.Windows:
                        return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);

                    case RuntimeInfo.Platform.Linux:
                        if (IsWayland)
                            return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER, IntPtr.Zero);

                        if (SDL_GetCurrentVideoDriver() == "x11")
                            return new IntPtr(SDL_GetNumberProperty(props, SDL_PROP_WINDOW_X11_WINDOW_NUMBER, 0));

                        return IntPtr.Zero;

                    case RuntimeInfo.Platform.macOS:
                        return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_COCOA_WINDOW_POINTER, IntPtr.Zero);

                    case RuntimeInfo.Platform.iOS:
                        return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_UIKIT_WINDOW_POINTER, IntPtr.Zero);

                    case RuntimeInfo.Platform.Android:
                        return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_ANDROID_WINDOW_POINTER, IntPtr.Zero);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Represents a handle to this <see cref="SDL3Window"/> instance, used for unmanaged callbacks.
        /// </summary>
        protected ObjectHandle<SDL3Window> ObjectHandle { get; private set; }

        protected SDL3Window()
        {
            ObjectHandle = new ObjectHandle<SDL3Window>(this, GCHandleType.Normal);

            SDL_SetLogPriority(SDL_LogCategory.SDL_LOG_CATEGORY_ERROR, SDL_LogPriority.SDL_LOG_PRIORITY_DEBUG);
            SDL_SetEventFilter(&eventFilter, ObjectHandle.Handle);
        }

        protected SDL3Window? ParentWindow;

        public void Create(IWindow? parent, WindowFlags wf)
        {
            m_winFlags = wf;

            SDL3Window? parentWindow = null;
            if (parent != null)
            {
                if (parent is not SDL3Window)
                {
                    throw new Exception("Uhhhh lmao???");
                }
                parentWindow = parent as SDL3Window;
            }

            var flags = SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY |
                        SDL_WindowFlags.SDL_WINDOW_HIDDEN;

            if (wf.HasFlag(WindowFlags.OpenGL))
            {
                flags |= SDL_WindowFlags.SDL_WINDOW_OPENGL;
            }

            if (wf.HasFlag(WindowFlags.Resizable))
            {
                flags |= SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            }
            if (wf.HasFlag(WindowFlags.Popup))
            {
                flags |= SDL_WindowFlags.SDL_WINDOW_POPUP_MENU;
                flags |= SDL_WindowFlags.SDL_WINDOW_TRANSPARENT;
            }

            if (flags.HasFlag(SDL_WindowFlags.SDL_WINDOW_POPUP_MENU) || flags.HasFlag(SDL_WindowFlags.SDL_WINDOW_TOOLTIP))
            {
                if (parentWindow == null)
                    throw new Exception("Popup and tool menus NEED to have parents!");

                const bool createNativeWindow = false;

                if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows && createNativeWindow)
                {
                    var parentHandle = parentWindow.WindowHandle;
                    var ptr = WindowsWindow.CreatePopup(parentHandle, 0, 0, Size.Width, Size.Height);
                    if (ptr == IntPtr.Zero)
                    {
                        throw new Exception("uhhh");
                    }

                    SDL_PropertiesID props = SDL_CreateProperties();
                    SDL_SetPointerProperty(props, SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER, ptr);
                    // SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_FLAGS_NUMBER, (long)flags);
                    // SDL_SetPointerProperty(props, SDL_PROP_WINDOW_CREATE_PARENT_POINTER, (nint)parentWindow.SDLWindowHandle);

                    SDLWindowHandle = SDL_CreateWindowWithProperties(props);
                    SDL_DestroyProperties(props);

                    if (SDLWindowHandle == null)
                    {
                        throw new Exception(SDL_GetError());
                    }
                }
                else
                {
                    SDLWindowHandle = SDL_CreatePopupWindow(parentWindow.SDLWindowHandle, 0, 0, Size.Width, Size.Height, flags);
                }
            }
            else
            {
                SDLWindowHandle = SDL_CreateWindow(m_title, Size.Width, Size.Height, flags);
            }
            SDLWindowID = SDL_GetWindowID(SDLWindowHandle);

            if (SDLWindowHandle == null)
            {
                Console.WriteLine(SDL_GetError());
            }

            if (parentWindow != null)
            {
                SDL_SetWindowParent(SDLWindowHandle, parentWindow.SDLWindowHandle);
            }

            Exists = true;

            s_openedWindows.Add(SDLWindowID, this);
            SDL_AddEventWatch(&eventWatch, ObjectHandle.Handle);

            ParentWindow = parentWindow;

            OnCreate(wf);
        }

        /// <summary>
        /// Forcibly closes the window immediately. Call order be damned.
        /// </summary>
        public void Close()
        {
            Exists = false;
            
            if (SDLWindowHandle != null)
            {
                OnClose(m_winFlags);

                SDL_DestroyWindow(SDLWindowHandle);
                SDLWindowHandle = null;
            }
        }

        public void Raise()
        {
            ScheduleCommand(() =>
            {
                var flags = SDL_GetWindowFlags(SDLWindowHandle);

                if (flags.HasFlag(SDL_WindowFlags.SDL_WINDOW_MINIMIZED))
                    SDL_RestoreWindow(SDLWindowHandle);

                SDL_RaiseWindow(SDLWindowHandle);
            });
        }

        public void Show()
        {
            ScheduleCommand(() => SDL_ShowWindow(SDLWindowHandle));
        }

        public void Hide()
        {
            ScheduleCommand(() => SDL_HideWindow(SDLWindowHandle));
        }

        #region SDL Event Handling

        protected void ScheduleCommand(Action action) => m_commandScheduler.Add(action, false);

        private const int events_per_peep = 64;
        private static readonly SDL_Event[] events = new SDL_Event[events_per_peep];

        internal void RunCommands()
        {
            m_commandScheduler.Update();
        }

        /// <summary>
        /// Poll for all pending events.
        /// </summary>
        public static void pollSDLEvents()
        {
            SDL_PumpEvents();

            int eventsRead;

            do
            {
                eventsRead = SDL_PeepEvents(events, SDL_EventAction.SDL_GETEVENT, SDL_EventType.SDL_EVENT_FIRST, SDL_EventType.SDL_EVENT_LAST);
                for (int i = 0; i < eventsRead; i++)
                {
                    var windowID = events[i].window.windowID;

                    if (s_openedWindows.TryGetValue(windowID, out var window))
                    {
                        window.HandleEvent(events[i]);
                    }
                }
            } while (eventsRead == events_per_peep);
        }

        internal void HandleEvent(SDL_Event e)
        {
            if (e.Type >= SDL_EventType.SDL_EVENT_WINDOW_FIRST && e.Type <= SDL_EventType.SDL_EVENT_WINDOW_LAST)
            {
                handleWindowEvent(e.window);
                return;
            }

            switch (e.Type)
            {
                /*
                case SDL_EventType.SDL_EVENT_QUIT:
                    ExitRequested?.Invoke();
                    break;
                */
                    // @HACK
                /*
                case SDL_EventType.SDL_EVENT_KEY_DOWN:
                    if (e.key.key == SDL_Keycode.SDLK_F2)
                    {
                        Application.DebugDrawing = !Application.DebugDrawing;
                    }
                    break;
                */
            }
        }

        /// <summary>
        /// Handles <see cref="SDL_Event"/>s fired from the SDL event filter.
        /// </summary>
        /// <remarks>
        /// As per SDL's recommendation, application events should always be handled via the event filter.
        /// See: https://wiki.libsdl.org/SDL3/SDL_EventType#android_ios_and_winrt_events
        /// </remarks>
        /// <returns>A <c>bool</c> denoting whether to keep the event. <c>false</c> will drop the event.</returns>
        protected virtual bool HandleEventFromFilter(SDL_Event e)
        {
            SDL_WindowID windowId = e.window.windowID;
            
            switch (e.Type)
            {
                case SDL_EventType.SDL_EVENT_TERMINATING:
                    ExitRequested?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_DID_ENTER_BACKGROUND:
                    // Suspended?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_WILL_ENTER_FOREGROUND:
                    // Resumed?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_LOW_MEMORY:
                    // LowOnMemory?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                    handleMouseMotionEvent(e.motion);
                    return false;

                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                    handleMouseButtonEvent(e.button);
                    return false;

                case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    handleMouseWheelEvent(e.wheel);
                    return false;
            }

            return true;
        }


        protected void HandleEventFromWatch(SDL_Event evt)
        {
            switch (evt.Type)
            {
                case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                    // polling via SDL_PollEvent blocks on resizes (https://stackoverflow.com/a/50858339)
                    if (!m_updatingWindowStateAndSize)
                    {
                        // bool isUserResizing = SDL_GetGlobalMouseState(null, null).HasFlag(SDL_MouseButtonFlags.SDL_BUTTON_LMASK);
                        fetchWindowSize();
                    }
                    break;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static SDLBool eventFilter(IntPtr userdata, SDL_Event* eventPtr)
        {
            /*
            var handle = new ObjectHandle<SDL3Window>(userdata);
            if (handle.GetTarget(out SDL3Window window))
            {
                Console.WriteLine(window.SDLWindowID);
                return window.HandleEventFromFilter(*eventPtr);
            }
            */

            var windowID = eventPtr->window.windowID;
            if (s_openedWindows.TryGetValue(windowID, out var window))
            {
                return window.HandleEventFromFilter(*eventPtr);
            }

            return true;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static SDLBool eventWatch(IntPtr userdata, SDL_Event* eventPtr)
        {
            /*
            var handle = new ObjectHandle<SDL3Window>(userdata);
            if (handle.GetTarget(out SDL3Window window))
                window.HandleEventFromWatch(*eventPtr);
            */

            var windowID = eventPtr->window.windowID;
            if (s_openedWindows.TryGetValue(windowID, out var window))
            {
                window.HandleEventFromWatch(*eventPtr);
            }

            return true;
        }

        #endregion

        internal virtual void OnCreate(WindowFlags wf)
        {
        }

        internal virtual void OnClose(WindowFlags wf)
        {
        }

        private IconGroup? m_iconGroup;

        public void CopyIconFromWindow(IWindow window)
        {
            if (window is not SDL3Window sdlwindow)
                throw new Exception("Window is not of type SDL3! Realistically, you should never be switching these to begin with...?");

            CopyIconFromOther(sdlwindow);
        }

        internal virtual void CopyIconFromOther(SDL3Window window)
        {
            // Idk yet lol
        }

        public void SetIconFromStream(Stream imageStream)
        {
            using (var ms = new MemoryStream())
            {
                imageStream.CopyTo(ms);
                ms.Position = 0;

                try
                {
                    SetIconFromImage(Image.Load<Rgba32>(ms.GetBuffer()));
                }
                catch
                {
                    if (IconGroup.TryParse(ms.GetBuffer(), out var iconGroup))
                    {
                        m_iconGroup = iconGroup;
                        SetIconFromGroup(iconGroup);
                    }
                }
            }
        }

        internal virtual void SetIconFromGroup(IconGroup iconGroup)
        {
            // LoadRawIcon returns raw PNG data if available, which avoids any Windows-specific pinvokes
            byte[]? bytes = iconGroup.LoadRawIcon(default_icon_size, default_icon_size);
            if (bytes == null)
                return;

            SetIconFromImage(Image.Load<Rgba32>(bytes));
        }

        internal virtual void SetIconFromImage(Image<Rgba32> iconImage) => setSDLIcon(iconImage);

        private void setSDLIcon(Image<Rgba32> image)
        {
            var pixelMemory = image.CreateReadOnlyPixelMemory();
            var imageSize = image.Size;

            var pixelSpan = pixelMemory.Span;

            SDL_Surface* surface;

            fixed (Rgba32* ptr = pixelSpan)
            {
                var pixelFormat = SDL_GetPixelFormatForMasks(32, 0xff, 0xff00, 0xff0000, 0xff000000);
                surface = SDL_CreateSurfaceFrom(imageSize.Width, imageSize.Height, pixelFormat, new IntPtr(ptr), imageSize.Width * 4);
            }

            SDL_SetWindowIcon(SDLWindowHandle, surface);
            SDL_DestroySurface(surface);
        }

        #region Events

        /// <summary>
        /// Invoked when the window close (X) button or another platform-native exit action has been pressed.
        /// </summary>
        public event Action? ExitRequested;

        /// <summary>
        /// Invoked when the window is about to close.
        /// </summary>
        public event Action? Exited;

        #endregion

        public void Dispose()
        {
            Close();
            s_openedWindows.Remove(SDLWindowID);

            ObjectHandle.Dispose();
        }
    }
}
