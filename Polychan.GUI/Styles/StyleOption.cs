using Polychan.GUI.Widgets;
using SkiaSharp;

namespace Polychan.GUI.Styles;

public class StyleOption
{
    public enum OptionType
    {
        Button,
        TitleBar
    }
    
    public Style.StateFlag State { get; set; }

    /// <summary>
    /// Initializes the State based on the specified widget.
    /// </summary>
    public void InitFrom(Widget widget)
    {
        State = Style.StateFlag.None;
        if (widget.Enabled)
            State |= Style.StateFlag.Enabled;
    }
}

public class StyleOptionComplex : StyleOption
{

}

public class StyleOptionButton : StyleOption
{
    public string Text { get; set; } = string.Empty;
}

public class StyleOptionPanelButton : StyleOption
{
    public string Text { get; set; } = string.Empty;
    public string? Icon { get; set; } = string.Empty;
}


public class StyleOptionScrollBar : StyleOptionComplex
{
    public ScrollBar.SubControl Hovered { get; set; }
    public ScrollBar.SubControl Pressed { get; set; }

    public ScrollBar.SubControl ActiveSubControls { get; set; }

    public Dictionary<ScrollBar.SubControl, SKRectI> SubControlRects { get; } = [];
}

public class StyleOptionShapedFrame : StyleOption
{
}