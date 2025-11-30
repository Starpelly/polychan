using System.Diagnostics;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.App.Widgets;

public class CatalogListView : Widget
{
    public string? CurrentBoard { get; private set; }
    
    public IReadOnlyDictionary<FChan.Models.PostId, ThreadTicketWidget> Threads => m_threadWidgets;
    public Action<ThreadTicketWidget>? OnItemClick { get; set; }
    
    private readonly FChan.Responses.CatalogResponse m_catalogResponse;
    private readonly Dictionary<FChan.Models.PostId, ThreadTicketWidget> m_threadWidgets = [];
    
    private readonly ScrollArea m_scrollArea;
    private readonly Label m_boardTitleLabel;
    
    public CatalogListView(FChan.Responses.CatalogResponse catalogResponse, Widget? parent = null) : base(parent)
    {
        m_catalogResponse = catalogResponse;
        
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
        this.Width = 400;
        this.Layout = new VBoxLayout();

        m_boardTitleLabel = MainWindow.TabInfoWidgetThing(threadsListHolder);

        m_scrollArea = new ScrollArea(threadsListHolder)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding),
        };
        m_scrollArea.ContentFrame.Layout = new HBoxLayout
        {
        };
        m_scrollArea.ChildWidget = new NullWidget(m_scrollArea.ContentFrame)
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
        this.CurrentBoard = board;
        
        m_threadWidgets.Clear();

        // m_boardTitleLabel.Text = $"<span class=\"header\">/{board}/ - {ChanApp.Client.Boards.Boards.Find(c => c.URL == board).Title}</span>";

        void LoadPage(FChan.Models.CatalogPage page)
        {
            foreach (var thread in page.Threads)
            {
                var widget = new ThreadTicketWidget(thread, m_scrollArea.ChildWidget)
                {
                    Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 50,
                };
                widget.OnItemClick = () =>
                {
                    OnItemClick?.Invoke(widget);
                };
                m_threadWidgets.Add(thread.No, widget);
            }
        }

        // loadPage(m_chanClient.Catalog.Pages[0]);
        // return;
        foreach (var page in m_catalogResponse.Pages)
        {
            LoadPage(page);
        }
    }
    
    public void T()
    {
        Debug.Assert(CurrentBoard != null);
        
        // @NOTE
        // I'm too tired to figure this out right now, but thread widgets won't look right if thumbnails are loaded "immediately"
        // AGH idk....
        // -pelly

        // Load thumbnails for threads
        var tuples = m_threadWidgets.Select(c => (c.Key, c.Value.ApiThread.Tim));
        _ = ChanApp.Client.LoadThumbnailsAsync(CurrentBoard, tuples, (postId, image) =>
        {
            if (image != null)
            {
                m_threadWidgets[postId].SetBitmapPreview(image);
            }
        });
    }
    
    private void clearThreads()
    {
        foreach (var widget in m_threadWidgets)
        {
            widget.Value.Dispose(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_threadWidgets.Clear();
        m_scrollArea.VerticalScrollbar.Value = 0;
    }
}