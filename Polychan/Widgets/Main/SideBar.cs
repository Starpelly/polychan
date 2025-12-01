using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using SkiaSharp;

namespace Polychan.App.Widgets.Main;

public class SideBar : Widget, IPaintHandler
{
    private readonly MainWindow m_mainWindow;
    private readonly List<TabWidget> m_tabs = [];
    private readonly Widget m_tabListContainer;
    private TabWidget? m_selectedTab;

    public class TabWidget : Widget, IPaintHandler, IMouseDownHandler
    {
        private SideBar m_sideBar;
        
        public required string Label;
        public required Widget Content;

        public bool Selected;

        public TabWidget(SideBar sideBar, Widget parent) : base(parent)
        {
            m_sideBar = sideBar;
            Height = 30;
        }

        public void OnPaint(SKCanvas canvas)
        {
            using var paint = new SKPaint();

            var bgColor = Selected ? Application.Palette.Get(ColorRole.Text).Darker(2) : SKColors.Transparent;
            var txColor = Selected
                ? Application.Palette.Get(ColorRole.Text)
                : Application.Palette.Get(ColorRole.Text);
            
            if (Selected)
            {
                paint.Color = bgColor;
                canvas.DrawRect(new SKRect(0, 0, Width, Height), paint);
            }

            paint.Color = txColor;
            canvas.DrawText(Label, new SKPoint(8, (Height * 0.5f) + (4)), SKTextAlign.Left, Application.DefaultFont, paint);
        }

        public bool OnMouseDown(MouseEvent evt)
        {
            m_sideBar.SwitchTab(this);
            return true;
        }

        public void SetLabel(string label)
        {
            this.Label = label;
        }
    }
    
    public SideBar(MainWindow window, Widget? parent = null) : base(parent)
    {
        m_mainWindow = window;
        
        Layout = new VBoxLayout()
        {
            Padding = new Padding(8),
            Spacing = 4
        };

        m_tabListContainer = new ShapedFrame(this)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding),
            Layout = new VBoxLayout()
            {
                Spacing = 2,
            }
        };

        new PushButton("New Tab", this)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            OnClicked = () =>
            {
                m_mainWindow.CreateNewEmptyTab();
            }
        };

        void selectable(string text, Action? onClick)
        {
            return;
            var pushButton = new PushButton(text, this)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                OnClicked = onClick
            };
        }

        selectable("Tabs", m_mainWindow.LoadPage_Tabs);
        selectable("Saved", null);
        selectable("History", m_mainWindow.LoadPage_History);
        selectable("Search", null);
    }

    public TabWidget CreateTab(string title, Widget content)
    {
        foreach (var tab in m_tabs)
        {
            tab.Content.Visible = false;
            tab.Selected = false;
        }

        var widget = new TabWidget(this, m_tabListContainer)
        {
            Label = title,
            Content = content,
            Selected = true,

            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };
        m_tabs.Add(widget);
        
        // Weird separator?
        /*
        new HLine(m_tabListContainer)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };
        */

        return widget;
    }

    public void SwitchTab(TabWidget widget)
    {
        foreach (var tab in m_tabs)
        {
            if (widget == tab)
            {
                tab.Content.Visible = true;
                tab.Selected = true;
            }
            else
            {
                tab.Content.Visible = false;
                tab.Selected = false;
            }
        }
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        paint.Color = Application.Palette.Get(ColorRole.Window).Darker(1.1f);
        canvas.DrawRect(new SKRect(0, 0, Width, Height), paint);
    }
}