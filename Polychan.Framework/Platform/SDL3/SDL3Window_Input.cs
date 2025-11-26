using System.Numerics;
using Polychan.GUI.Input;
using SDL;

namespace Polychan.GUI.Framework.Platform.SDL3
{
    internal partial class SDL3Window
    {
        #region SDL Event Handling

        private ulong m_lastPreciseScroll;
        private const uint precise_scroll_debounce = 100;

        private void handleMouseWheelEvent(SDL_MouseWheelEvent evtWheel)
        {
            bool isPrecise(float f) => f % 1 != 0;

            bool precise;

            if (isPrecise(evtWheel.x) || isPrecise(evtWheel.y))
            {
                precise = true;
                m_lastPreciseScroll = evtWheel.timestamp;
            }
            else
            {
                precise = evtWheel.timestamp < m_lastPreciseScroll + precise_scroll_debounce;
            }

            // SDL reports horizontal scroll opposite of what we expect (in non-"natural" mode, scrolling to the right gives positive deltas while we want negative).
            MouseWheel?.Invoke(new Vector2(evtWheel.mouse_x, evtWheel.mouse_y), new Vector2(-evtWheel.x, evtWheel.y), precise);
        }

        private void handleMouseButtonEvent(SDL_MouseButtonEvent evtButton)
        {
            var button = mouseButtonFromEvent(evtButton.Button);
            var pos = new Vector2(evtButton.x, evtButton.y);

            switch (evtButton.type)
            {
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                    MouseDown?.Invoke(pos, button);
                    break;
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                    MouseUp?.Invoke(pos, button);
                    break;
            }
        }

        private void handleMouseMotionEvent(SDL_MouseMotionEvent evtMotion)
        {
            MouseMove?.Invoke(new System.Numerics.Vector2(evtMotion.x * Scale, evtMotion.y * Scale));
        }

        #endregion

        private MouseButton mouseButtonFromEvent(SDLButton button)
        {
            switch (button)
            {
                case SDLButton.SDL_BUTTON_LEFT:
                    return MouseButton.Left;

                case SDLButton.SDL_BUTTON_RIGHT:
                    return MouseButton.Right;

                case SDLButton.SDL_BUTTON_MIDDLE:
                    return MouseButton.Middle;

                case SDLButton.SDL_BUTTON_X1:
                    return MouseButton.Button1;

                case SDLButton.SDL_BUTTON_X2:
                    return MouseButton.Button2;

                default:
                    Console.WriteLine($"unknown mouse button: {button}, defaulting to left button");
                    return MouseButton.Left;
            }
        }

        #region Events

        public event Action<Vector2>? MouseMove;
        public event Action<Vector2, MouseButton>? MouseDown;
        public event Action<Vector2, MouseButton>? MouseUp;
        public event Action? MouseEntered;
        public event Action? MouseLeft;
        public event Action<Vector2, Vector2, bool>? MouseWheel;

        #endregion
    }
}
