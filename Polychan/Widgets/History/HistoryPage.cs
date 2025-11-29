using MaterialDesign;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.App.Widgets.History;

public class HistoryPage : Widget
{
    public HistoryPage(Widget? parent = null) : base(parent)
    {
        this.Layout = new VBoxLayout();
        
        var toolbar = new ToolBar(this)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };
        toolbar.AddAction(new MenuAction(MaterialIcons.Refresh, "Refresh", null));
        toolbar.AddAction(new MenuAction(MaterialIcons.Delete, "Clear History", null));
        
        new HLine(this)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };
        
        var history = ChanApp.HistoryDb.LoadHistory();
        foreach (var thread in history)
        {
            /*
            new Label(this)
            {
                Text = thread.Title ?? "NO TITLE",
            };
            */
        }
    }
}