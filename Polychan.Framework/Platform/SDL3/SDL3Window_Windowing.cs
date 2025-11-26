using System.Drawing;
using SDL;
using static SDL.SDL3;

namespace Polychan.GUI.Framework.Platform.SDL3
{
    internal partial class SDL3Window
    {
        private bool m_cursorInWindow = false;

        private Point m_position;

        public unsafe Point Position
        {
            get => m_position;
            set
            {
                m_position = value;
                ScheduleCommand(() => 
                SDL_SetWindowPosition(SDLWindowHandle, value.X, value.Y));
            }
        }

        private bool m_resizable;

        public unsafe bool Resizable
        {
            get => m_resizable;
            set
            {
                if (m_resizable == value)
                    return;

                m_resizable = value;
                ScheduleCommand(() => SDL_SetWindowResizable(SDLWindowHandle, value));
            }
        }

        private bool m_focused;

        public bool Focused
        {
            get => m_focused;
            set
            {
                if (value == m_focused)
                    return;

                m_isActive = m_focused = value;
                Console.WriteLine(value);
            }
        }

        private bool m_isActive;

        public bool IsActive => m_isActive;

        // This had another default value, but it creates like a white flash before the window draws the first frame.
        // So I shrunk it down to 0,0. Which still creates a pixel but it's less noticable I feel.
        // @Investigate
        private Size m_size = new(0, 0);

        public Size Size
        {
            get => m_size;
            set
            {
                if (value.Equals(m_size)) return;

                m_size = value;

                unsafe
                {
                    ScheduleCommand(() => SDL_SetWindowSize(SDLWindowHandle, m_size.Width, m_size.Height));
                }

                Resized?.Invoke();
            }
        }

        public Size MinSize
        {
            get => new(0, 0);
            set { }
        }

        public Size MaxSize
        {
            get => new(0, 0);
            set { }
        }

        private WindowState m_windowState = WindowState.Normal;
        private WindowState? m_pendingWindowState;

        public WindowState WindowState
        {
            get => m_windowState;
            set
            {
                if (m_pendingWindowState == null && m_windowState == value)
                    return;

                m_pendingWindowState = value;
            }
        }

        /// <summary>
        /// Updates <see cref="Size"/> and <see cref="Scale"/> according to SDL state.
        /// </summary>
        /// <returns>Whether the window size has been changed after updating.</returns>
        private unsafe void fetchWindowSize()
        {
            int w, h;
            SDL_GetWindowSize(SDLWindowHandle, &w, &h);

            int drawableW = w;

            // When minimised on windows, values may be zero.
            // If we receive zeroes for either of these, it seems safe to completely ignore them.
            if (w <= 0 || drawableW <= 0)
                return;

            Scale = (float)drawableW / w;
            Size = new Size(w, h);
        }

        #region SDL Event Handling

        private unsafe void handleWindowEvent(SDL_WindowEvent evtWindow)
        {
            switch (evtWindow.type)
            {
                case SDL_EventType.SDL_EVENT_WINDOW_MOVED:
                    // explicitly requery as there are occasions where what SDL has provided us with is not up-to-date.
                    int x, y;
                    SDL_GetWindowPosition(SDLWindowHandle, &x, &y);
                    var newPosition = new Point(x, y);

                    if (!newPosition.Equals(Position))
                    {
                        m_position = newPosition;
                        Moved?.Invoke(newPosition);
                    }
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED:
                    fetchWindowSize();
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_MOUSE_ENTER:
                    m_cursorInWindow = true;
                    MouseEntered?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_MOUSE_LEAVE:
                    m_cursorInWindow = false;
                    MouseLeft?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_RESTORED:
                case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED:
                    Focused = true;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_MINIMIZED:
                case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_LOST:
                    Focused = false;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                    ExitRequested?.Invoke();
                    break;
            }
        }

        #endregion

        private bool m_windowMaximised;

        /// <summary>
        /// Returns the drawable area, after scaling.
        /// </summary>
        public Size ClientSize => new((int)(Size.Width * Scale), (int)(Size.Height * Scale));

        public float Scale { get; private set; } = 1;

        /// <summary>
        /// Set when <see cref="UpdateWindowStateAndSize"/> is in progress to avoid <see cref="fetchWindowSize"/> being called with invalid data.
        /// </summary>
        /// <remarks>
        /// Since <see cref="UpdateWindowStateAndSize"/> is a multi-step process, intermediary windows size changes might be invalid.
        /// This is usually not a problem, but since <see cref="HandleEventFromFilter"/> runs out-of-band, invalid data might appear in those events.
        /// </remarks>
        private bool m_updatingWindowStateAndSize;

        #region Events

        public event Action? Resized;
        public event Action<WindowState>? WindowStateChanged;
        public event Action<Point>? Moved;

        #endregion
    }
}
