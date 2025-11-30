using Polychan.App.Widgets;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.App;

public class PostsView : Widget
{
    private readonly Dictionary<FChan.Models.PostId, PostWidgetContainer> m_postWidgets = [];
    
    private readonly ScrollArea? m_postsListWidget;
    private readonly Label m_threadTitleLabel;
    
    public PostsView(string board, FChan.Models.ThreadPosts threadPosts, FChan.Models.PostId threadId, Widget? parent = null) : base(parent)
    {
        Name = "Posts View";
        CatchCursorEvents = false;

        Fitting = FitPolicy.ExpandingPolicy;
        Layout = new VBoxLayout();
        
        m_threadTitleLabel = MainWindow.TabInfoWidgetThing(this);
        m_threadTitleLabel.Text = $"<span class=\"header\">/{board}/{threadId}/ - {threadPosts.Posts[0].Sub}</span>";

        m_postsListWidget = new ScrollArea(this)
        {
            Fitting = FitPolicy.ExpandingPolicy,
            Name = "Main Content Holder"
        };
        m_postsListWidget.ContentFrame.Layout = new HBoxLayout
        {
        };
        m_postsListWidget.ChildWidget = new NullWidget(m_postsListWidget.ContentFrame)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            AutoSizing = new SizePolicy(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
            Layout = new VBoxLayout
            {
                Spacing = 1,
            },
            Name = "Posts Lists Holder"
        };
        
        foreach (var post in threadPosts.Posts)
        {
            var widget = new PostWidgetContainer(this, board, threadPosts, post, m_postsListWidget.ChildWidget)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            m_postWidgets.Add(post.No, widget);
        }

        LoadPostPreviews(board, m_postWidgets);
    }
    
    public void LoadPostPreviews(string board, Dictionary<FChan.Models.PostId, PostWidgetContainer> widgetsToUpdate)
    {
        var refPosts = new Dictionary<FChan.Models.PostId, List<PostWidgetContainer>>();

        foreach (var key in widgetsToUpdate.Keys)
        {
            refPosts.Add(key, []);
        }

        foreach (var widget in m_postWidgets)
        {
            foreach (var refID in widget.Value.ReferencedPosts)
            {
                if (long.TryParse(refID, out var lid))
                {
                    var id = new FChan.Models.PostId(lid);
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
        var tuples = widgetsToUpdate.Select(c => (c.Key, c.Value.ApiPost.Tim));
        _ = ChanApp.Client.LoadThumbnailsAsync(board, tuples, (postId, image) =>
        {
            if (image != null)
            {
                widgetsToUpdate[postId].Widget.SetBitmapPreview(image);
            }
        });
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
}