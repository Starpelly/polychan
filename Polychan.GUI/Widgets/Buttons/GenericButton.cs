namespace Polychan.GUI.Widgets;

public class GenericButton : Widget, IMouseEnterHandler, IMouseLeaveHandler, IMouseDownHandler, IMouseUpHandler, IMouseClickHandler
{
    private bool m_hovering = false;
    private bool m_pressed = false;
    
    protected bool Hovering => m_hovering;
    protected bool Pressed => m_pressed;

    protected bool HandCursor = true;

    public Action? OnClicked;
    public Action? OnPressed;
    public Action? OnReleased;

    public GenericButton(Widget? parent = null) : base(parent)
    {
    }

    public void OnMouseEnter()
    {
        m_hovering = true;
        
        if (HandCursor)
            MouseCursor.Set(MouseCursor.CursorType.Hand);

        TriggerRepaint();
    }

    public void OnMouseLeave()
    {
        m_hovering = false;
        MouseCursor.Set(MouseCursor.CursorType.Arrow);

        TriggerRepaint();
    }

    public bool OnMouseDown(MouseEvent evt)
    {
        m_pressed = true;
        OnPressed?.Invoke();

        TriggerRepaint();

        return true;
    }

    public bool OnMouseUp(MouseEvent evt)
    {
        m_pressed = false;
        OnReleased?.Invoke();

        TriggerRepaint();

        return true;
    }

    public bool OnMouseClick(MouseEvent evt)
    {
        OnClicked?.Invoke();
        OnClick();
    
        TriggerRepaint();

        return true;
    }

    protected virtual void OnClick()
    {
        
    }
}