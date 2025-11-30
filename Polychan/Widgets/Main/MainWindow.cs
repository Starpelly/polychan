using System.Diagnostics;
using MaterialDesign;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using Polychan.App.Widgets;
using Polychan.App.Widgets.History;

namespace Polychan.App;

public class MainWindow : NormalWindow
{
    public enum SideBarOptions
    {
        BoardTabs,
        Saved,
        History,
        Search
    }

    class Tab : NullWidget
    {
        private readonly string m_board;
        private readonly CatalogListView m_catalogListView;
        private readonly TabsController m_postTabs;
        
        private FChan.Models.ThreadPosts? m_threadPosts;

        public Tab(string board, Widget parent) : base(parent)
        {
            Layout = new HBoxLayout();
            
            var catalogApiResponse = ChanApp.Client.GetCatalogAsync(board).GetAwaiter().GetResult();
            
            m_board = board;
            m_catalogListView = new CatalogListView(catalogApiResponse, this)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
            };
            m_postTabs = new TabsController(this)
            {
                Fitting = FitPolicy.ExpandingPolicy
            };
            
            m_catalogListView.LoadCatalog(board);
            m_catalogListView.T();

            m_catalogListView.OnItemClick = loadThread;
        }

        private void loadThread(ThreadTicketWidget ticket)
        {
            Debug.Assert(m_catalogListView.CurrentBoard != null);
            var threadId = ticket.ApiThread.No;
                
            m_threadPosts = ChanApp.Client.GetThreadPostsAsync(m_board, threadId).GetAwaiter().GetResult();
                
            var view = new PostsView(m_board, m_threadPosts, threadId, m_postTabs);
            m_postTabs.AddTab(view, $"{threadId}");

            ChanApp.HistoryDb.SaveVisit(threadId, m_catalogListView.CurrentBoard,
                ticket.ApiThread.OriginalJson, m_catalogListView.Threads[threadId].PreviewImage.Bitmap.EncodedData.ToArray());
        }
    }

    private readonly Dictionary<SideBarOptions, Widget> m_pages = [];
    private readonly List<Tab> m_tabs = [];

    public MainWindow()
    {
        Layout = new VBoxLayout();

        void OpenSettings()
        {
            new SettingsDialog(this).Show();
        }

        void Refresh()
        {
        }

        void ShowAbout()
        {
            new AboutDialog(this).Show();
        }

        void DownloadThread()
        {
        }

        // Setup MenuBar
        {
            MenuBar = new(this)
            {
                Width = this.Width,
                ScreenPosition = MenuBar.Orientation.Top,

                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };

            void AddMenu(string title, MenuAction[] items)
            {
                var menu = MenuBar.AddMenu(title);
                foreach (var item in items)
                {
                    menu.AddAction(item);
                }
            }

            AddMenu("File", [
                new(MaterialIcons.Settings, "Preferences", OpenSettings),
                new("")
                {
                    IsSeparator = true,
                },
                new(MaterialIcons.DoorFront, "Exit", () => { this.Dispose(); }),
            ]);
            AddMenu("Actions", [
                new(MaterialIcons.Refresh, "Refresh All", Refresh),
            ]);
            AddMenu("Tools", [
                new(MaterialIcons.Cloud, "Thread Downloader"),
                new(MaterialIcons.Terminal, "Toggle System Console"),
            ]);
            AddMenu("Help", [
                new(MaterialIcons.Public, "Website", () => { Application.OpenURL("https://polychan.net"); }),
                new(MaterialIcons.ImportContacts, "Wiki",
                    () => { Application.OpenURL("https://github.com/Starpelly/Polychan/wiki"); }),
                new("")
                {
                    IsSeparator = true,
                },
                new(MaterialIcons.Code, "Source Code",
                    () => { Application.OpenURL("https://github.com/Starpelly/Polychan"); }),

                new(MaterialIcons.Info, "About Polychan", ShowAbout)
            ]);
        }

        // Setup ToolBar
        {
            ToolBar = new ToolBar(this)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            ToolBar.AddAction(new MenuAction(MaterialIcons.Add, "New"));
            ToolBar.AddAction(new MenuAction(MaterialIcons.FolderOpen, "Open"));
            ToolBar.AddAction(new MenuAction(MaterialIcons.Download, "Download", DownloadThread));
            ToolBar.AddSeparator();
            ToolBar.AddAction(new MenuAction(MaterialIcons.Refresh, "Refresh", Refresh));
            ToolBar.AddSeparator();
            ToolBar.AddAction(new MenuAction(MaterialIcons.Settings, "Settings", OpenSettings));
            ToolBar.AddAction(new MenuAction(MaterialIcons.Info, "About", ShowAbout));

            new HLine(this)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
        }

        CentralWidget = new(this)
        {
            Fitting = FitPolicy.ExpandingPolicy
        };

        // Setup UI
        {
            CentralWidget!.Layout = new HBoxLayout
            {
                // Padding = new(16)
            };

            Widget mainHolder = CentralWidget;
            /*
            mainHolder = new ShapedFrame(CentralWidget)
            {
                Fitting = FitPolicy.ExpandingPolicy,
                Layout = new HBoxLayout
                {
                }
            };
            */

            // Boards list
            /*
            {
                var boardsListHolder = new NullWidget(mainHolder)
                {
                    Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                    Width = 158,

                    Layout = new VBoxLayout { }
                };

                var m_boardsListWidget = new ScrollArea(boardsListHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy
                };
                // m_boardsListWidget.VerticalScrollbar.Visible = false;

                m_boardsListWidget.ContentFrame.Layout = new HBoxLayout
                {
                };

                m_boardsListWidget.ChildWidget = new NullWidget(m_boardsListWidget.ContentFrame)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                    Layout = new VBoxLayout
                    {
                        Padding = new(8),
                        Spacing = 4,
                    },
                    Name = "Boards Lists Holder"
                };

                foreach (var board in ChanApp.Client.Boards.Boards)
                {
                    new PushButton(board.Title, m_boardsListWidget.ChildWidget)
                    {
                        Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                        OnClicked = () =>
                        {
                            ChanApp.LoadCatalog(board.URL);
                        }
                    };
                }
            }
            */

            // SideBar
            {
                new SideBar(this, mainHolder)
                {
                    Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                    Width = 140
                };
                CreateSeparator(mainHolder);
            }

            // BOARD PAGE
            {
                var boardPage = m_pages[SideBarOptions.BoardTabs] = new NullWidget(mainHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy,
                    Layout = new HBoxLayout()
                };

                // Threads list
                /*
                m_catalogListView = new CatalogListView(boardPage);

                CreateSeparator(boardPage);

                m_postTabs = new TabsController(boardPage)
                {
                    Fitting = FitPolicy.ExpandingPolicy
                };
                */
            }

            // HISTORY PAGE
            {
                var historyPage = m_pages[SideBarOptions.History] = new HistoryPage(mainHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy,
                    Visible = false,
                };
            }

            void CreateSeparator(Widget parent)
            {
                new VLine(parent)
                {
                    Fitting = new FitPolicy(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                };
            }
        }

        switchPage(SideBarOptions.BoardTabs);
    }

    public static Label TabInfoWidgetThing(Widget parent)
    {
        // @TODO
        // Add anchor points
        var bg = new Rect(Application.Palette.Get(ColorRole.Window), parent)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            Height = 48,

            Layout = new HBoxLayout
            {
                Padding = new(12, 8)
            }
        };
        var w = new Label(bg)
        {
            Fitting = FitPolicy.ExpandingPolicy,
            Anchor = Label.TextAnchor.CenterLeft,
        };

        // Separator
        new HLine(parent)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            Height = 1
        };
        return w;
    }

    public void NewTab(string board)
    {
        var tabParent = m_pages[SideBarOptions.BoardTabs];
        var tab = new Tab(board, tabParent)
        {
            Fitting = FitPolicy.ExpandingPolicy,
        };
        
        m_tabs.Add(tab);
    }
    
    public void LoadThreadPosts(FChan.Models.ThreadPosts threadPosts, FChan.Models.CatalogThread catalogThread, FChan.Models.PostId threadId)
    {
        /*
        var view = new PostsView(threadId, m_postTabs);
        m_postTabs.AddTab(view, $"{threadId}");

        ChanApp.HistoryDb.SaveVisit(threadId, ChanApp.Client.CurrentBoard,
            catalogThread.OriginalJson, m_catalogListView.Threads[threadId].PreviewImage.Bitmap.EncodedData.ToArray());
            */
    }

    public void LoadPage_Board()
    {
        switchPage(SideBarOptions.BoardTabs);
    }

    public void LoadPage_History()
    {
        switchPage(SideBarOptions.History);

        var historyPage = (HistoryPage)m_pages[SideBarOptions.History];
        // historyPage.OnVisible();
    }

    private void switchPage(SideBarOptions option)
    {
        foreach (var page in m_pages)
        {
            if (page.Key == option)
                page.Value.Visible = true;
            else
                page.Value.Visible = false;
        }
    }
}