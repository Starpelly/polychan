using SkiaSharp;
using Polychan.GUI.Styles;

namespace Polychan.GUI.Widgets;

public class PushButton : GenericButton, IPaintHandler
{
    private const int TEXT_PADDING_W = 16;
    private const int TEXT_PADDING_H = 16;

    private string m_text = string.Empty;
    public string Text
    {
        get => m_text;
        set
        {
            m_text = value;
            updateSize();
        }
    }

    public PushButton(string text, Widget? parent = null) : base(parent)
    {
        Text = text;
    }

    public void OnPaint(SKCanvas canvas)
    {
        var option = new StyleOptionButton
        {
            Text = Text
        };
        option.InitFrom(this);

        if (Hovering)
        {
            if (Pressed)
                option.State |= Style.StateFlag.Sunken;
            else
                option.State |= Style.StateFlag.MouseOver;
        }
        
        Application.DefaultStyle.DrawPushButton(canvas, this, option);
    }

    private void updateSize()
    {
        Resize((int)Application.DefaultFont.MeasureText(m_text) + TEXT_PADDING_W, (int)Application.DefaultFont.Size + 2 + TEXT_PADDING_H);
    }
}