using Imageboard;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.App.Widgets.Main;

public class BoardSelectorWidget : Widget
{
    public Action<BoardId>? OnSelect { get; set; }
    
    public BoardSelectorWidget(Widget parent) : base(parent)
    {
        Layout = new VBoxLayout()
        {
            Padding = new Padding(0),
            Spacing = 4
        };
        
        var scrollArea = new ScrollArea(this)
        {
            Fitting = FitPolicy.ExpandingPolicy,
        };
        scrollArea.ContentFrame.Layout = new HBoxLayout
        {
            Padding = new Padding(8),
        };

        var scrollContent = scrollArea.ChildWidget = new NullWidget(scrollArea.ContentFrame)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            AutoSizing = new SizePolicy(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
            Layout = new VBoxLayout
            {
                Padding = new Padding(0),
                Spacing = 4,
            },
            Name = "Boards Lists Holder"
        };
        
        foreach (var board in ChanApp.ImageboardClient.FourChanBoards)
        {
            _ = new PushButton(board.Key, scrollContent)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                OnClicked = () =>
                {
                    OnSelect?.Invoke(board.Key);
                }
            };
        }
    }
}