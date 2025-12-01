using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.App.Widgets.Main;

public class CatalogContentWidget : NullWidget
{
    // private readonly TabsController m_postTabs;
        
    private Imageboard.Catalog? m_catalog;
    private Imageboard.Thread? m_currentThread;
        
    private ThreadView? m_threadView;
    private CatalogListView? m_catalogListView;
        
    public CatalogContentWidget(Widget parent) : base(parent)
    {
        Layout = new HBoxLayout();
            
        /*
        m_postTabs = new TabsController(this)
        {
            Fitting = FitPolicy.ExpandingPolicy
        };
        */
    }

    public void LoadCatalog(Imageboard.Catalog catalog)
    {
        m_catalog = catalog;
        m_catalogListView = new CatalogListView(catalog, this)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
        };
        m_catalogListView.T();
        m_catalogListView.OnItemClick = loadThread;
        
        // Separator between the catalog and the thread.
        _ = new VLine(this)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
        };
    }

    private void loadThread(ThreadTicketWidget ticket)
    {
        var threadId = ticket.ApiThread.Id;
                
        m_currentThread = ChanApp.ImageboardClient.GetFullThreadAsync(ticket.ApiThread).GetAwaiter().GetResult();
            
        m_threadView?.Dispose();
        m_threadView = new ThreadView(m_currentThread, this);
        // m_postTabs.AddTab(view, $"{threadId}");

        /*
        ChanApp.HistoryDb.SaveVisit(threadId, m_catalogListView.CurrentBoard,
            ticket.ApiThread.OriginalJson, m_catalogListView.Threads[threadId].PreviewImage.Bitmap.EncodedData.ToArray());
            */
    }
}