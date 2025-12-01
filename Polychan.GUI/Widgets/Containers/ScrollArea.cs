using Polychan.Framework;
using Polychan.GUI.Layouts;

namespace Polychan.GUI.Widgets;

public class ScrollArea : Widget, IMouseWheelHandler
{
    public NullWidget ContentFrame { get; private set; }
    public ScrollBar VerticalScrollbar { get; private set; }

    private Widget? m_childWidget;
    public Widget? ChildWidget
    {
        get => m_childWidget;
        set
        {
            setWidget(value);
        }
    }

    private int m_newScrollY = 0;
    private float m_scrollPosY = 0.0f;

    /// <summary>
    /// If toggled on, the <see cref="ContentFrame"/> will be scrolled smoothly when the user "wheels" over the <see cref="ScrollArea"/>.
    /// </summary>
    public static bool SmoothScroll { get; set; } = true;

    public ScrollArea(Widget? parent = null) : base(parent)
    {
        Layout = new HBoxLayout
        {
            Spacing = 0,
        };

        ContentFrame = new NullWidget(this)
        {
            Name = "ScrollArea Content",
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding),
        };

        VerticalScrollbar = new ScrollBar(this)
        {
            X = 400,
            Y = 16,
            Width = 16,
            Height = 400,
            Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
            Minimum = 0,
            Maximum = 0
        };
        VerticalScrollbar.OnValueChanged += delegate (int value)
        {
            if (m_childWidget != null)
            {
                var minY = 0;

                if (ContentFrame.Layout != null)
                {
                    minY = -ContentFrame.Layout.Padding.Top;
                    value += minY;
                }

                m_newScrollY = value;
                if (!SmoothScroll)    
                    ContentFrame.ContentsPositions = new(ContentFrame.ContentsPositions.X, -value);
            }
        };
        VerticalScrollbar.OnSliderMoved += delegate (int value)
        {
            // We don't need to handle all this if we're scrolling instantly.
            if (!SmoothScroll)
                return;

            if (m_childWidget != null)
            {
                m_newScrollY = value;
                m_scrollPosY = m_newScrollY;

                ContentFrame.ContentsPositions = new(ContentFrame.ContentsPositions.X, -value);
                VerticalScrollbar.SetValueWithoutNotify(value);
            }
        };

        validate();
    }

    private void setWidget(Widget? newChildWidget)
    {
        if (m_childWidget != null)
        {
            m_childWidget.OnResized -= validate;
        }
        if (newChildWidget == null)
            return;

        m_childWidget = newChildWidget;
        newChildWidget.SetParent(ContentFrame);

        m_childWidget.OnResized += validate;

        validate();
    }

    public bool OnMouseScroll(MouseWheelEvent evt)
    {
        var newValue = m_newScrollY - evt.deltaY * VerticalScrollbar.PageStep / 4;
        newValue = Math.Clamp(newValue, VerticalScrollbar.Minimum, VerticalScrollbar.Maximum);
        VerticalScrollbar.Value = newValue;

        return false;
    }

    public override void OnUpdate(double dt)
    {
        if (!SmoothScroll)
            return;

        m_scrollPosY = Mathf.Lerp(m_scrollPosY, m_newScrollY, (float)dt * 10);
        ContentFrame.ContentsPositions = new(ContentFrame.ContentsPositions.X, -(int)m_scrollPosY);
        VerticalScrollbar.SetValueWithoutNotify((int)m_scrollPosY);
    }

    public override void OnPostLayout()
    {
        validate();
    }

    #region Private methods

    private void validate()
    {
        // Console.WriteLine("Validate");
        fitScrollbarsToContent();
    }

    private void fitScrollbarsToContent()
    {
        var maxY = 0;

        if (ContentFrame.Layout != null)
        {
            maxY = ContentFrame.Layout.Padding.Bottom * 2;
        }

        if (m_childWidget != null)
        {
            VerticalScrollbar.Minimum = 0;
            VerticalScrollbar.Maximum = Math.Max(0, (m_childWidget.Height - ContentFrame.Height) + maxY);
            VerticalScrollbar.PageStep = ContentFrame.Height;
        }

        VerticalScrollbar.Value = Math.Clamp(VerticalScrollbar.Value, VerticalScrollbar.Minimum, VerticalScrollbar.Maximum);
        VerticalScrollbar.Enabled = VerticalScrollbar.Maximum > VerticalScrollbar.Minimum;

        // So the reason it looks as if the list scrolls back up to the top when the window is resized (or equivalent)-
        // is because the layout for m_mainContentWidget is setting the position of the list in the Layout?.PositionsPass().
        // Dunno what to do about that, maybe create a flag or something?
    }

    #endregion
}