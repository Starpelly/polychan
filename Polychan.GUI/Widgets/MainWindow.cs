using SkiaSharp;

namespace Polychan.GUI.Widgets;

public class MainWindow : WindowWidget, IPaintHandler, IResizeHandler
{
    public MenuBar? MenuBar { get; set; }
    public Widget? CentralWidget = null;

    public MainWindow(Widget? parent = null) : base(WindowType.Window, parent)
    {

    }

    #region Events

    public void OnPaint(SKCanvas canvas)
    {
        canvas.Clear(EffectivePalette.Get(ColorGroup.Active, ColorRole.Window));
    }

    public void OnResize(int width, int height)
    {
        MenuBar?.Resize(width, MenuBar.Height);
    }

    public void SetIconFromStream(Stream imageStream)
    {
        m_nativeWindow?.WindowHolder.Window.SetIconFromStream(imageStream);
    }

    #endregion

    #region Private methods

    private void layout()
    {
        SKRect available = new SKRect(0, 0, Width, Height);

        if (MenuBar != null)
        {
            MenuBar.Resize(Width, MenuBar.Height);
            available.Top += MenuBar.Height;
        }

        if (CentralWidget != null)
        {

        }
    }
    
    #endregion
}