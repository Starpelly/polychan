using Polychan.API.Models;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.App.Widgets;

public class CatalogListView : Widget
{
    private readonly Dictionary<Int64, ThreadTicketWidget> m_threadWidgets = [];
    
    private readonly ScrollArea? m_threadsListWidget;
    private readonly Label m_boardTitleLabel;
    
    public IReadOnlyDictionary<Int64, ThreadTicketWidget> Threads => m_threadWidgets;
    
    public CatalogListView(Widget? parent = null) : base(parent)
    {
        /*
        var threadsListHolder = new NullWidget(mainHolder)
        {
            Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
            Width = 400,

            Layout = new VBoxLayout
            {
            }
        };
        */

        var threadsListHolder = this;
        this.Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding);
        this.Width = 400;
        this.Layout = new VBoxLayout();

        m_boardTitleLabel = MainWindow.TabInfoWidgetThing(threadsListHolder);

        m_threadsListWidget = new ScrollArea(threadsListHolder)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding),
        };
        m_threadsListWidget.ContentFrame.Layout = new HBoxLayout
        {
        };
        m_threadsListWidget.ChildWidget = new NullWidget(m_threadsListWidget.ContentFrame)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
            Layout = new VBoxLayout
            {
                Spacing = 1,
            },
            Name = "Threads List Holder"
        };
    }

    public void LoadCatalog(string board)
    {
        m_threadWidgets.Clear();

        if (m_threadsListWidget == null)
            return;
        m_boardTitleLabel.Text = $"<span class=\"header\">/{board}/ - {ChanApp.Client.Boards.Boards.Find(c => c.URL == board).Title}</span>";

        void LoadPage(CatalogPage page)
        {
            foreach (var thread in page.Threads)
            {
                var widget = new ThreadTicketWidget(thread, m_threadsListWidget.ChildWidget)
                {
                    Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 50,
                };
                m_threadWidgets.Add(thread.No, widget);
            }
        }

        // loadPage(m_chanClient.Catalog.Pages[0]);
        // return;
        foreach (var page in ChanApp.Client.Catalog.Pages)
        {
            LoadPage(page);
        }
    }
    
    public void T()
    {
        // @NOTE
        // I'm too tired to figure this out right now, but thread widgets won't look right if thumbnails are loaded "immediately"
        // AGH idk....
        // -pelly

        // Load thumbnails for threads
        var tuples = m_threadWidgets.Select(c => (c.Key, c.Value.ApiThread.Tim));
        _ = ChanApp.Client.LoadThumbnailsAsync(tuples, (postId, image) =>
        {
            if (image != null)
            {
                m_threadWidgets[postId].SetBitmapPreview(image);
            }
        });
    }
    
    private void clearThreads()
    {
        if (m_threadsListWidget == null)
            return;

        foreach (var widget in m_threadWidgets)
        {
            widget.Value.Dispose(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_threadWidgets.Clear();
        m_threadsListWidget.VerticalScrollbar.Value = 0;
    }
}