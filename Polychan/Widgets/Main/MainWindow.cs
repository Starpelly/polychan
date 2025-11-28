using MaterialDesign;
using SkiaSharp;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using Polychan.App.Widgets;
using Microsoft.Data.Sqlite;

namespace Polychan.App;

public class MainWindow : NormalWindow
{
    public enum SideBarOptions
    {
        Boards,
        Saved,
        History,
        Search
    }
    
    private readonly Dictionary<SideBarOptions, NullWidget> m_pages = [];
    
    private readonly CatalogListView m_catalogListView;
    private readonly TabsController m_postTabs;
    
    private static readonly string DbPath = 
        Path.Combine(Settings.GetAppFolder(), "4chan.db");

    private readonly ThreadHistoryDatabase m_historyDb;

    class ThreadHistoryEntry
    {
        public long Id { get; set; }
        public int ThreadId { get; set; }
        public string Board { get; set; }
        public string? Title { get; set; }
        public DateTime VisitedAt { get; set; }
    }

    class ThreadHistoryDatabase
    {
        private readonly string m_connectionString;

        public ThreadHistoryDatabase(string dbPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            m_connectionString = $"Data Source={dbPath}";
        }

        public void Initialize()
        {
            using var conn = new SqliteConnection(m_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                CREATE TABLE IF NOT EXISTS thread_history (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    thread_id INTEGER NOT NULL,
                    board TEXT NOT NULL,
                    title TEXT,
                    visited_at TEXT NOT NULL,
                    UNIQUE(thread_id, board)
                ); 
                """;

            cmd.ExecuteNonQuery();
        }

        public void SaveVisit(int threadId, string board, string? title)
        {
            using var conn = new SqliteConnection(m_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                INSERT INTO thread_history (thread_id, board, title, visited_at)
                VALUES ($id, $board, $title, $ts)
                ON CONFLICT(thread_id, board) DO UPDATE SET
                    title = $title,
                    visited_at = $ts;
                """;
            cmd.Parameters.AddWithValue("$id", threadId);
            cmd.Parameters.AddWithValue("board", board);
            cmd.Parameters.AddWithValue("$title", title ?? "");
            cmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
            
            cmd.ExecuteNonQuery();
        }

        public List<ThreadHistoryEntry> LoadHistory()
        {
            var results = new List<ThreadHistoryEntry>();
            using var conn = new SqliteConnection(m_connectionString);
            conn.Open();
            
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                SELECT id, thread_id, board, title, visited_at
                FROM thread_history
                ORDER BY visited_at DESC;
                """;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new ThreadHistoryEntry
                {
                    Id = reader.GetInt64(0),
                    ThreadId = reader.GetInt32(1),
                    Board = reader.GetString(2),
                    Title = reader.IsDBNull(3) ? null : reader.GetString(3),
                    VisitedAt = DateTime.Parse(reader.GetString(4))
                });
            }

            return results;
        }
    }
    
    public MainWindow()
    {
        m_historyDb = new ThreadHistoryDatabase(DbPath);
        m_historyDb.Initialize();
        
        Layout = new VBoxLayout();

        void OpenSettings()
        {
            new SettingsDialog(this).Show();
        }

        void Refresh()
        {
            ChanApp.LoadCatalog(ChanApp.Client.CurrentBoard);
        }

        void ShowAbout()
        {
            new AboutDialog(this).Show();
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
                new(MaterialIcons.DoorFront, "Exit", () => {
                    this.Dispose();
                }),
            ]);
            AddMenu("Actions", [
                new(MaterialIcons.Refresh, "Refresh All", Refresh),
            ]);
            AddMenu("Tools", [
                new(MaterialIcons.Cloud, "Thread Downloader"),
                new(MaterialIcons.Terminal, "Toggle System Console"),
            ]);
            AddMenu("Help", [
                new(MaterialIcons.Public, "Website", () => {
                    Application.OpenURL("https://polychan.net");
                }),
                new(MaterialIcons.ImportContacts, "Wiki", () => {
                    Application.OpenURL("https://github.com/Starpelly/Polychan/wiki");
                }),
                new("")
                {
                    IsSeparator = true,
                },
                new(MaterialIcons.Code, "Source Code", () => {
                    Application.OpenURL("https://github.com/Starpelly/Polychan");
                }),

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
                Padding = new(16)
            };

            Widget mainHolder = CentralWidget;
            mainHolder = new ShapedFrame(CentralWidget)
            {
                Fitting = FitPolicy.ExpandingPolicy,
                Layout = new HBoxLayout
                {
                }
            };

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
                var boardPage = m_pages[SideBarOptions.Boards] = new NullWidget(mainHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy,
                    Layout = new HBoxLayout()
                };
                
                // Threads list
                m_catalogListView = new CatalogListView(boardPage);
            
                CreateSeparator(boardPage);

                m_postTabs = new TabsController(boardPage)
                {
                    Fitting = FitPolicy.ExpandingPolicy
                };
            }
            
            // HISTORY PAGE
            {
                var historyPage = m_pages[SideBarOptions.History] = new NullWidget(mainHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy,
                    Visible = false,
                    // Layout = new HBoxLayout()
                    Layout = new VBoxLayout()
                };

                var toolbar = new ToolBar(historyPage)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
                };
                toolbar.AddAction(new MenuAction(MaterialIcons.Refresh, "Refresh", null));
                toolbar.AddAction(new MenuAction(MaterialIcons.Delete, "Clear History", null));
                new HLine(historyPage)
                {
                    Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
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
        
        switchPage(SideBarOptions.Boards);
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

    public void LoadBoardCatalog(string board)
    {
        m_catalogListView.LoadCatalog(board);
        m_catalogListView.T();
    }

    public void LoadThreadPosts(string threadId)
    {
        m_historyDb.SaveVisit(int.Parse(threadId), ChanApp.Client.CurrentBoard,ChanApp.Client.CurrentThread.Posts[0].Sub);
        
        var view = new PostsView(threadId, m_postTabs);
        m_postTabs.AddTab(view, threadId);
    }
    
    public void LoadPage_Board()
    {
        switchPage(SideBarOptions.Boards);
    }

    public void LoadPage_History()
    {
        switchPage(SideBarOptions.History);
        
        var history = m_historyDb.LoadHistory();
        foreach (var thread in history)
        {
            new Label(m_pages[SideBarOptions.History])
            {
                Text = thread.Title ?? "NO TITLE",
            };
        }
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