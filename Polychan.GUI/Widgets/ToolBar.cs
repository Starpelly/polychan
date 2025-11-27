using Polychan.GUI.Layouts;
using Polychan.GUI.Styles;
using SkiaSharp;

namespace Polychan.GUI.Widgets;


public class ToolBar : Widget, IPaintHandler
{
    public enum Orientation
    {
        Top,
        Left,
        Right,
        Bottom
    }
    
    private const int TOOL_BAR_HEIGHT = 59;

    private readonly List<PanelButton> m_actions = [];
    
    public ToolBar(Widget? parent = null) : base(parent)
    {
        this.Layout = new HBoxLayout()
        {
            Align = HBoxLayout.VerticalAlignment.Center,
            Spacing = 4,
            Padding = new Padding(2)
        };
        Height = TOOL_BAR_HEIGHT;
    }
    
    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();
        
        paint.Color = EffectivePalette.Get(ColorRole.Window);
        canvas.DrawRect(0, 0, Width, Height, paint);
    }

    public void AddAction(MenuAction action)
    {
        var btn = new PanelButton(action, this);
        btn.Fitting = new FitPolicy(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding);
        m_actions.Add(btn);
    }

    public void AddSeparator()
    {
        new VLine(this)
        {
            // Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
            // @TODO - not dynamic, but looks better IMO
            Height = this.Height - 8
        };
    }
}