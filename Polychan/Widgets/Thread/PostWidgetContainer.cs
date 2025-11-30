using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using SkiaSharp;

namespace Polychan.App.Widgets;

public class PostWidgetContainer : Widget, IPaintHandler
{
    private static readonly Padding Padding = new(8);

    private bool m_loadedReplies;
    private bool m_showingReplies;

    private int m_treeIndex = 0;

    private readonly PostsView m_view;
    private readonly PostWidget m_postWidget;

    private NullWidget? m_repliesHolder;
    private PushButton? m_showRepliesButton;

    private readonly string m_board;
    private readonly FChan.Models.ThreadPosts m_thread;

    public FChan.Models.Post ApiPost => m_postWidget.ApiPost;
    public List<string> ReferencedPosts => m_postWidget.ReferencedPosts;
    public PostWidget Widget => m_postWidget; // @TEMP
    
    public PostWidgetContainer(PostsView view, string board, FChan.Models.ThreadPosts thread, FChan.Models.Post post, Widget? parent = null) : base(parent)
    {
        m_view = view;
        m_board = board;
        m_thread = thread;
        
        Name = "PostWidgetContainer";

        this.Layout = new VBoxLayout
        {
            Padding = new Padding(8),
            Spacing = 8
        };
        this.AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit);

        m_postWidget = new PostWidget(board, thread, post, this)
        {
            Width = this.Width,
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };
    }

    public void SetReplies(List<PostWidgetContainer> replies)
    {
        if (replies.Count == 0) return;

        /*
        var repliesString = new StringBuilder();

        foreach (var widget in replies)
        {
            repliesString.Append($">{widget.m_postWidget.APIPost.No} ");
        }
        m_postWidget.SetReplies(repliesString.ToString());
        */

        string ViewRepliesStr()
        {
            return $"View replies ({replies.Count})";
        }

        string HideRepliesStr()
        {
            return "Hide replies";
        }

        m_showRepliesButton = new PushButton(ViewRepliesStr(), this)
        {
            X = Padding.Left,
            OnClicked = () =>
            {
                if (!m_showingReplies)
                {
                    m_showRepliesButton!.Text = HideRepliesStr();

                    if (!m_loadedReplies)
                        loadReplies(replies);
                    showReplies();
                }
                else
                {
                    m_showRepliesButton!.Text = ViewRepliesStr();
                    hideReplies();
                }
            }
        };
    }

    private void loadReplies(List<PostWidgetContainer> replies)
    {
        m_loadedReplies = true;

        m_repliesHolder = new NullWidget(this)
        {
            Width = this.Width,
            
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),

            Layout = new VBoxLayout
            {
                // Padding = new(32, 0, -8, 0),
                Padding = new(16, 0, 0, 0),
                Spacing = 4,
            },
        };

        var pw = new Dictionary<FChan.Models.PostId, PostWidgetContainer>(replies.Count);
        foreach (var item in replies)
        {
            var widget = new PostWidgetContainer(m_view, m_board, m_thread, item.m_postWidget.ApiPost, m_repliesHolder)
            {
                Width = this.Width,
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                m_treeIndex = this.m_treeIndex + 1 // Alternating row colors for replies? Looks pretty cool ig
            };
            pw.Add(item.m_postWidget.ApiPost.No, widget);
        }
        m_view.LoadPostPreviews(m_board, pw);
    }

    private void showReplies()
    {
        if (m_repliesHolder != null) m_repliesHolder.Visible = true;
        m_showingReplies = true;
    }

    private void hideReplies()
    {
        if (m_repliesHolder != null) m_repliesHolder.Visible = false;
        m_showingReplies = false;
    }

    #region Widget events

    public override void OnPostLayout()
    {
        m_postWidget.OnResize();
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();
        paint.Color = m_treeIndex % 2 == 0 ? Palette.Get(ColorRole.Base) : Palette.Get(ColorRole.AlternateBase);

        // canvas.DrawRect(0, 0, Width, Height, paint);
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), m_treeIndex == 0 ? 0 : 0), paint);

        if (m_treeIndex == 0)
            return;

        paint.IsStroke = true;
        paint.Color = Palette.Get(ColorRole.Base).Darker(1.4f);
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width - 1, Height - 1), m_treeIndex == 0 ? 0 : 0), paint);
    }

    #endregion
}