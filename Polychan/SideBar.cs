using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using SkiaSharp;

namespace Polychan.App;

public class SideBar : Widget, IPaintHandler
{
    private MainWindow m_mainWindow;
    
    public SideBar(MainWindow window, Widget? parent = null) : base(parent)
    {
        m_mainWindow = window;
        
        Layout = new VBoxLayout()
        {
            Padding = new Padding(8),
            Spacing = 2
        };

        void selectable(string text, Action? onClick)
        {
            var pushButton = new PushButton(text, this)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                OnClicked = onClick
            };
        }

        selectable("Board", m_mainWindow.LoadPage_Board);
        selectable("Saved", null);
        selectable("History", m_mainWindow.LoadPage_History);
        selectable("Search", null);
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        paint.Color = Application.Palette.Get(ColorRole.Window).Darker(1.1f);
        canvas.DrawRect(new SKRect(0, 0, Width, Height), paint);
    }
}