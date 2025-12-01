using System.Diagnostics;
using Polychan.App.Widgets.Main;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.App.Widgets;

public class ThreadView : Widget
{
    private readonly Dictionary<Imageboard.CommentId, CommentWidget> m_postWidgets = [];
    
    private readonly ScrollArea? m_postsListWidget;
    private readonly Label m_threadTitleLabel;
    
    public ThreadView(Imageboard.Thread fullThread, Widget? parent = null) : base(parent)
    {
        Name = "Posts View";
        CatchCursorEvents = false;

        Fitting = FitPolicy.ExpandingPolicy;
        Layout = new VBoxLayout();
        
        m_threadTitleLabel = MainWindow.TabInfoWidgetThing(this);
        m_threadTitleLabel.Text = $"<span class=\"header\">/{fullThread.BoardId}/{fullThread.Id}/ - {fullThread.Title}</span>";

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
        
        Debug.Assert(fullThread.Summary == false);
        foreach (var comment in fullThread.Comments!)
        {
            var widget = new CommentWidget(this, comment, m_postsListWidget.ChildWidget)
            {
                Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            m_postWidgets.Add(comment.Id, widget);
        }

        CommentPostPreviews(m_postWidgets);
    }
    
    public void CommentPostPreviews(Dictionary<Imageboard.CommentId, CommentWidget> widgetsToUpdate)
    {
        var refPosts = new Dictionary<Imageboard.CommentId, List<CommentWidget>>();

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
                    var id = new Imageboard.CommentId(lid);
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
        var tuples = widgetsToUpdate.Select(c => (c.Key, SmallUrl: c.Value.ApiPost.Attachment?.ThumbnailUrl));
        _ = Utils.HttpHelpers.LoadThumbnailsAsync(tuples, (postId, image) =>
        {
            if (image != null)
            {
                widgetsToUpdate[postId].Content.SetBitmapPreview(image);
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