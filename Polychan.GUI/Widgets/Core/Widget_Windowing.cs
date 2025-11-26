using Polychan.GUI.Input;

namespace Polychan.GUI.Widgets;

public partial class Widget
{
    private static Widget? s_openPopupMenu = null;

    private void handleMouseEnter()
    {
        if (!m_hovered)
        {
            m_hovered = true;
            (this as IMouseEnterHandler)?.OnMouseEnter();
        }
    }

    private void handleMouseLeave()
    {
        if (m_hovered)
        {
            m_hovered = false;
            (this as IMouseLeaveHandler)?.OnMouseLeave();
        }
    }

    private void onNativeWindowMouseEvent(int mouseX, int mouseY, MouseEventType type, MouseButton button, int deltaX = 0, int deltaY = 0)
    {
        if (Config.POPUPS_MAKE_WINDOWS)
        {
            if (type == MouseEventType.Down)
            {
                Console.WriteLine(mouseY);

                if (s_openPopupMenu != null && !s_openPopupMenu.HitTestLocal(mouseX, mouseY))
                {
                    (s_openPopupMenu as MenuPopup)?.Submit();
                    s_openPopupMenu = null;
                }
            }
        }

        Widget? hovered = null;

        // Check popups first
        if (!Config.POPUPS_MAKE_WINDOWS)
        {
            var find = findHoveredPopupWidget(mouseX, mouseY, true);
            if (find != null)
            {
                hovered = find.findHoveredWidget(mouseX, mouseY, true);
            }
        }
        if (hovered == null)
        {
            hovered = findHoveredWidget(mouseX, mouseY, true);

            // Close any active popup (because we aren't hovering over it)
            if (type == MouseEventType.Down && s_openPopupMenu != null && s_openPopupMenu != hovered && s_openPopupMenu is MenuPopup popup)
            {
                var dothing = true;

                if (hovered == popup.Menu)
                {
                    dothing = false;
                }

                if (dothing)
                {
                    if (popup.RequestClose())
                        s_openPopupMenu = null;
                }
            }
        }

        if (hovered != m_lastHovered)
        {
            m_lastHovered?.handleMouseLeave();
            hovered?.handleMouseEnter();
            m_lastHovered = hovered;
        }

        // Console.WriteLine($"{this.Name}, {hovered?.Name}, ({mouseX}, {mouseY})");

        // If there's a mouse grabber, it always receives input!
        if (s_mouseGrabber != null && s_mouseGrabber.Enabled)
        {
            var (lx, ly) = getLocalPosition(s_mouseGrabber, mouseX, mouseY);

            var mouseEvent = new MouseEvent()
            {
                x = lx,
                y = ly,
                globalX = mouseX,
                globalY = mouseY,
                button = button
            };
            var scrollEvent = new MouseWheelEvent()
            {
                x = lx,
                y = ly,
                globalX = mouseX,
                globalY = mouseY,
                deltaX = deltaX,
                deltaY = deltaY,
            };

            switch (type)
            {
                case MouseEventType.Move:
                    (s_mouseGrabber as IMouseMoveHandler)?.OnMouseMove(lx, ly);
                    break;

                case MouseEventType.Down:
                    (s_mouseGrabber as IMouseDownHandler)?.OnMouseDown(mouseEvent);
                    break;

                case MouseEventType.Up:
                    (s_mouseGrabber as IMouseUpHandler)?.OnMouseUp(mouseEvent);

                    if (s_mouseGrabber == hovered)
                    {
                        (s_mouseGrabber as IMouseClickHandler)?.OnMouseClick(mouseEvent);
                    }

                    s_mouseGrabber = null;
                    break;

                case MouseEventType.Wheel:
                    (s_mouseGrabber as IMouseWheelHandler)?.OnMouseScroll(scrollEvent);
                    break;
            }

            return;
        }

        // No grabber - do regular hit testing.
        if (hovered != null)
        {
            switch (type)
            {
                case MouseEventType.Down:
                    if (bubbleMouseEvent(hovered, type, button, mouseX, mouseY, deltaX, deltaY))
                    {
                        s_mouseGrabber = hovered;
                    }
                    break;

                case MouseEventType.Up:
                    bool upHandled = bubbleMouseEvent(hovered, type, button, mouseX, mouseY, deltaX, deltaY);

                    if (s_mouseGrabber == hovered && upHandled)
                    {
                        var (lx, ly) = getLocalPosition(hovered, mouseX, mouseY);

                        var mouseEvent = new MouseEvent()
                        {
                            x = lx,
                            y = ly,
                            globalX = mouseX,
                            globalY = mouseY,
                            button = button
                        };

                        (hovered as IMouseClickHandler)?.OnMouseClick(mouseEvent);
                    }

                    s_mouseGrabber = null;
                    break;

                default:
                    bubbleMouseEvent(hovered, type, button, mouseX, mouseY, deltaX, deltaY);
                    break;
            }
        }
    }

    private bool bubbleMouseEvent(Widget widget, MouseEventType type, MouseButton button, int globalX, int globalY, int dx = 0, int dy = 0)
    {
        while (widget != null)
        {
            if (!widget.Enabled)
            {
                widget = widget.Parent!;
                continue;
            }

            var (lx, ly) = getLocalPosition(widget, globalX, globalY);

            var mouseEvent = new MouseEvent()
            {
                x = lx,
                y = ly,
                globalX = globalX,
                globalY = globalY,
                button = button
            };
            var scrollEvent = new MouseWheelEvent()
            {
                x = lx,
                y = ly,
                globalX = globalX,
                globalY = globalY,
                deltaX = dx,
                deltaY = dy,
            };

            bool handled = type switch
            {
                MouseEventType.Move => (widget as IMouseMoveHandler)?.OnMouseMove(lx, ly) ?? false,
                MouseEventType.Down => (widget as IMouseDownHandler)?.OnMouseDown(mouseEvent) ?? false,
                MouseEventType.Up => (widget as IMouseUpHandler)?.OnMouseUp(mouseEvent) ?? false,
                MouseEventType.Wheel => (widget as IMouseWheelHandler)?.OnMouseScroll(scrollEvent) ?? false,
                _ => false
            };

            if (handled) return true;
            if (widget.IsWindow) return false;

            widget = widget.Parent!;
        }

        return false;
    }

    private void onNativeWindowResizeEvent(int w, int h)
    {
        {
            m_width = w;
            m_height = h;

            // This is fine because a native window can only exist on top level widgets and thus,
            // can't be in a layout!
            if (m_nativeWindow != null)
            {
                m_nativeWindow.WindowHolder.Window.Size = new System.Drawing.Size(m_width, m_height);
            }

            dispatchResize();
            // callResizeEvents();
        }
        m_nativeWindow!.CreateFrameBuffer(w, h);

        Application.CurrentFrame++;
        TriggerRepaint();

        LayoutQueue.Flush();
        RenderTopLevel(Application.DebugDrawing);
    }
}