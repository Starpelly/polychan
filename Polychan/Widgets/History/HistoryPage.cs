using MaterialDesign;
using Newtonsoft.Json;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using SkiaSharp;

namespace Polychan.App.Widgets.History;

public class HistoryPage : Widget
{
    private readonly List<ThreadTicketWidget> m_widgets = [];
    
    public HistoryPage(Widget? parent = null) : base(parent)
    {
        this.Layout = new VBoxLayout();
        
        /*
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
        */

        var scroll = new ScrollArea(this)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding),
        };
        scroll.ContentFrame.Layout = new HBoxLayout
        {
        };
        scroll.ChildWidget = new NullWidget(scroll.ContentFrame)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
            Layout = new VBoxLayout
            {
                Spacing = 1,
            },
            Name = "History List Holder"
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
            var model = JsonConvert.DeserializeObject<FChan.Models.CatalogThread>(thread.Json)!;
            var ticket = new ThreadTicketWidget(model, scroll.ChildWidget)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                Height = 50
            };
            m_widgets.Add(ticket);

            if (thread.Thumbnail != null)
            {
                using var ms = new MemoryStream(thread.Thumbnail);
                var bitmap = SKImage.FromEncodedData(ms); // Decode into SKBitmap
                ticket.SetBitmapPreview(bitmap);
            }
        }
    }

    public void OnVisible()
    {
        // @TODO
        // We shouldn't have to do this, I should debug this in the morning. For some reason layouts aren't being updated
        // when we either turn visible or whatever else... :/
        foreach (var ticket in m_widgets)
        {
            ticket.OnPostLayout();
        } 
    }
}