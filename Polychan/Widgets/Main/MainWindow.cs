using System.Diagnostics;
using MaterialDesign;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using Polychan.App.Widgets.History;

namespace Polychan.App.Widgets.Main;

public class MainWindow : NormalWindow
{
    public enum SideBarOptions
    {
        Tabs,
        Saved,
        History,
        Search
    }

    public class CatalogPage : Widget
    {
        private SideBar.TabWidget? m_tab;

        private BoardSelectorWidget? m_emptySelector;
        private CatalogContentWidget? m_catalog;

        public CatalogPage(Widget parent) : base(parent)
        {
            Layout = new VBoxLayout();
        }

        public void ConnectTab(SideBar.TabWidget tab)
        {
            Debug.Assert(m_tab == null);
            m_tab = tab;
        }
        
        public void LoadEmpty()
        {
            Debug.Assert(m_emptySelector == null);
            m_emptySelector = new BoardSelectorWidget(this)
            {
                Fitting = FitPolicy.ExpandingPolicy,
                OnSelect = (boardId) =>
                {
                    var catalog = ChanApp.ImageboardClient.GetCatalogAsync(ChanApp.ImageboardClient.FourChanBoards[boardId]).GetAwaiter().GetResult();
                    LoadCatalog(catalog);
                }
            };
        }
        
        public void LoadCatalog(Imageboard.Catalog catalog)
        {
            m_emptySelector?.Dispose();
            m_catalog?.Dispose();
            
            m_catalog = new CatalogContentWidget(this)
            {
                Fitting = FitPolicy.ExpandingPolicy,
            };
            
            m_catalog.LoadCatalog(catalog);
            m_tab!.SetLabel($"/{catalog.Board.Id}/");
        }
    }

    private readonly Dictionary<SideBarOptions, Widget> m_pages = [];
    private readonly SideBar m_sideBar;

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

                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
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

            _ = new HLine(this)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
        }

        CentralWidget = new Widget(this)
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

            // SideBar
            {
                m_sideBar = new SideBar(this, mainHolder)
                {
                    Fitting = new FitPolicy(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                    Width = 140
                };
                CreateSeparator(mainHolder);
            }

            // BOARD PAGE
            {
                var boardPage = m_pages[SideBarOptions.Tabs] = new NullWidget(mainHolder)
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
                _ = new VLine(parent)
                {
                    Fitting = new FitPolicy(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                };
            }
        }

        switchPage(SideBarOptions.Tabs);
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
        _ = new HLine(parent)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            Height = 1
        };
        return w;
    }
    
    public CatalogPage CreateNewEmptyTab()
    {
        var page = m_pages[SideBarOptions.Tabs];
        var value = new CatalogPage(page)
        {
            Fitting = FitPolicy.ExpandingPolicy
        };
        var tab = m_sideBar.CreateTab($"None", value);
        value.ConnectTab(tab);
        
        value.LoadEmpty();
        return value;
    }

    public CatalogPage CreateNewTabWithCatalog(Imageboard.Catalog catalog)
    {
        var page = m_pages[SideBarOptions.Tabs];
        var value = new CatalogPage(page)
        {
            Fitting = FitPolicy.ExpandingPolicy
        };
        var tab = m_sideBar.CreateTab($"None", value);
        value.ConnectTab(tab);
        
        value.LoadCatalog(catalog);
        return value;
    }

    public void LoadPage_Tabs()
    {
        switchPage(SideBarOptions.Tabs);
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