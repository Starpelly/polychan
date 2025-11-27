using SkiaSharp;
using Polychan.GUI.Styles;

namespace Polychan.GUI.Widgets;

public class PanelButton : GenericButton, IPaintHandler
{
    private const int TEXT_PADDING_W = 12;
    
    public readonly MenuAction Action;
    public string Text => Action.Text;
    public string? Icon => Action.Icon;
    
    public PanelButton(MenuAction action, Widget? parent = null) : base(parent)
    {
        base.HandCursor = false;
        this.Action = action;
        Width = (int)Application.DefaultFont.MeasureText(Text) + TEXT_PADDING_W;
    }

    public void OnPaint(SKCanvas canvas)
    {
        var option = new StyleOptionPanelButton
        {
            Text = this.Text,
            Icon = Icon
        };
        option.InitFrom(this);

        if (Hovering)
        {
            if (Pressed)
                option.State |= Style.StateFlag.Sunken;
            else
                option.State |= Style.StateFlag.MouseOver;
        }
        
        Application.DefaultStyle.DrawPanelButton(canvas, this, option);
    }

    protected override void OnClick()
    {
        base.OnClick();
        
        Action.Action?.Invoke();
    }
}