using MaterialDesign;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using SkiaSharp;
using Polychan.API.Models;
using Polychan.Widgets;

namespace Polychan;

public class PolychanWindow : MainWindow, IResizeHandler, IMouseDownHandler
{
    private readonly List<ThreadWidget> m_threadWidgets = [];
    private readonly Dictionary<int, PostWidgetContainer> m_postWidgets = [];

    private ScrollArea? m_threadsListWidget;
    private ScrollArea? m_postsListWidget;

    public readonly SKFont IconsFont;

    private readonly Label m_boardTitleLabel;
    private readonly Label m_threadTitleLabel;

    public PolychanWindow() : base()
    {
        Layout = new VBoxLayout
        {
        };

        void OpenSettings()
        {
            new PreferencesDialog(this).Show();
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
                new(MaterialIcons.Refresh, "Refresh All"),
            ]);
            AddMenu("Tools", [
                new(MaterialIcons.Cloud, "Thread Downloader"),
                new(MaterialIcons.Terminal, "Toggle System Console"),
            ]);
            AddMenu("Help", [
                new(MaterialIcons.Public, "Website", () => {
                    Application.OpenURL("https://boxsubmus.com");

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

                new(MaterialIcons.Info, "About Polychan", () => {
                    new AboutDialog(this).Show();
                })
            ]);
        }
        
        // Setup ToolBar
        {
            ToolBar = new ToolBar(this)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            ToolBar.AddAction(new MenuAction(MaterialIcons.Refresh, "Refresh"));
            ToolBar.AddSeparator();
            ToolBar.AddAction(new MenuAction(MaterialIcons.Settings, "Settings", OpenSettings));

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
                // Padding = new(4)
            };

            Widget mainHolder = CentralWidget;
            /*
            mainHolder = new ShapedFrame(this)
            {
                Fitting = FitPolicy.ExpandingPolicy,
                Layout = new HBoxLayout
                {
                }
            };
            */

            // Boards list
            if (false)
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

                foreach (var board in ChanApp.ChanClient.Boards.Boards)
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

            // CreateSeparator();

            // Threads list
            {
                var threadsListHolder = new NullWidget(mainHolder)
                {
                    Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                    Width = 400,

                    Layout = new VBoxLayout
                    {
                    }
                };

                TabInfoWidgetThing(out m_boardTitleLabel, threadsListHolder);

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

            CreateSeparator();

            // Main content
            if (true)
            {
                var postsListHolder = new NullWidget(mainHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy,

                    Layout = new VBoxLayout
                    {
                    }
                };

                TabInfoWidgetThing(out m_threadTitleLabel, postsListHolder);

                m_postsListWidget = new ScrollArea(postsListHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy,
                    Name = "Main Content Holder"
                };
                m_postsListWidget.ContentFrame.Layout = new HBoxLayout
                {
                };
                m_postsListWidget.ChildWidget = new NullWidget(m_postsListWidget.ContentFrame)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                    Layout = new VBoxLayout
                    {
                        Spacing = 1,
                    },
                    Name = "Posts Lists Holder"
                };
            }

            void TabInfoWidgetThing(out Label w, Widget parent)
            {
                // @TODO
                // Add anchor points
                var bg = new Rect(Palette.Get(ColorRole.Window), parent)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 48,

                    Layout = new HBoxLayout
                    {
                        Padding = new(12, 8)
                    }
                };
                w = new Label(bg)
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
            }

            void CreateSeparator()
            {
                new VLine(mainHolder)
                {
                    Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                };
            }
        }
    }

    private Dictionary<long, ThreadWidget> m_threadIds = new();

    public void LoadBoardCatalog(string board)
    {
        clearThreads();
        clearPosts();

        m_threadIds.Clear();

        if (m_threadsListWidget == null)
            return;
        m_boardTitleLabel.Text = $"<span class=\"header\">/{board}/ - {ChanApp.ChanClient.Boards.Boards.Find(c => c.URL == board).Title}</span>";

        m_threadIds = new Dictionary<long, ThreadWidget>();
        void loadPage(CatalogPage page)
        {
            foreach (var thread in page.Threads)
            {
                var widget = new ThreadWidget(thread, m_threadsListWidget.ChildWidget)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 50,
                };
                m_threadWidgets.Add(widget);

                if (thread.Tim != null && thread.Tim > 0)
                {
                    m_threadIds.Add((long)thread.Tim, widget);
                }
            }
        }

        // loadPage(m_chanClient.Catalog.Pages[0]);
        // return;
        foreach (var page in ChanApp.ChanClient.Catalog.Pages)
        {
            loadPage(page);
        }
    }

    public void T()
    {
        // @NOTE
        // I'm too tired to figure this out right now, but thread widgets won't look right if thumbnails are loaded "immediately"
        // AGH idk....
        // -pelly

        // Load thumbnails for threads
        _ = ChanApp.ChanClient.LoadThumbnailsAsync(m_threadIds.Keys, (long tim, SKImage? image) =>
        {
            if (image != null)
            {
                m_threadIds[tim].SetBitmapPreview(image);
            }
        });
    }

    public void LoadThreadPosts(string threadID)
    {
        if (m_postsListWidget == null)
            return;

        clearPosts();

        m_threadTitleLabel.Text = $"<span class=\"header\">{ChanApp.ChanClient.CurrentThread.Posts[0].Sub}</span>";


        var imageIDs = new Dictionary<long, PostWidgetContainer>();

        for (var i = 0; i < ChanApp.ChanClient.CurrentThread.Posts.Count; i++)
        {
            var post = ChanApp.ChanClient.CurrentThread.Posts[i];
            var widget = new PostWidgetContainer(post, m_postsListWidget.ChildWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            m_postWidgets.Add(post.No, widget);

            if (post.Tim != null && post.Tim > 0)
            {
                imageIDs.Add((long)post.Tim, widget);
            }
        }

        Bruhhh(m_postWidgets);

        // Load thumbnails for posts
        _ = ChanApp.ChanClient.LoadThumbnailsAsync(imageIDs.Keys, (long tim, SKImage? image) =>
        {
            if (image != null)
            {
                imageIDs[tim].Test.SetBitmapPreview(image);
            }
        });
    }

    public void Bruhhh(Dictionary<int, PostWidgetContainer> widgetsToUpdate)
    {
        var refPosts = new Dictionary<int, List<PostWidgetContainer>>();
        var imageIDs = new Dictionary<long, PostWidgetContainer>();

        foreach (var key in widgetsToUpdate.Keys)
        {
            refPosts.Add(key, []);

            var post = widgetsToUpdate[key].ApiPost;
            if (post.Tim != null && post.Tim > 0)
            {
                imageIDs.Add((long)post.Tim, widgetsToUpdate[key]);
            }
        }

        foreach (var widget in m_postWidgets)
        {
            foreach (var refID in widget.Value.ReferencedPosts)
            {
                if (int.TryParse(refID, out var id))
                {
                    // if (postWidgets.TryGetValue(id, out var value))
                    if (widgetsToUpdate.ContainsKey(id))
                    {
                        // list.Add(value);
                        refPosts[id].Add(widget.Value);
                    }
                    else
                    {
                        // @NOTE
                        // So this indicates the post comes from another thread
                        // I need to add a case to handle this!
                    }
                }
            }
        }


        foreach (var post in refPosts)
        {
            widgetsToUpdate[post.Key].SetReplies(post.Value);
        }

        // Load thumbnails for posts
        _ = ChanApp.ChanClient.LoadThumbnailsAsync(imageIDs.Keys, (long tim, SKImage? image) =>
        {
            if (image != null)
            {
                imageIDs[tim].Test.SetBitmapPreview(image);
            }
        });
    }

    private void clearThreads()
    {
        if (m_threadsListWidget == null)
            return;

        foreach (var widget in m_threadWidgets)
        {
            widget.Dispose(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_threadWidgets.Clear();
        m_threadsListWidget.VerticalScrollbar.Value = 0;
    }

    private void clearPosts()
    {
        if (m_postsListWidget == null)
            return;

        foreach (var widget in m_postWidgets)
        {
            widget.Value.Dispose(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_postWidgets.Clear();
        m_postsListWidget.VerticalScrollbar.Value = 0;
    }

    public new void OnResize(int width, int height)
    {
        // Console.Clear();
        // Console.WriteLine("\x1b[3J");

        base.OnResize(width, height);

        var menubarHeight = MenuBar != null ? MenuBar.Height : 0;

        /*
        m_centralWidget.Y = menubarHeight;
        m_centralWidget.Width = width;
        m_centralWidget.Height = height - menubarHeight;
        */
    }

    public bool OnMouseDown(MouseEvent evt)
    {
        return true;
    }
}