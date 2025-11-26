using HtmlAgilityPack;
using MaterialDesign;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using Polychan.Utils;
using SkiaSharp;
using System.Net;
using Polychan.API.Models;

namespace Polychan.Widgets;

public class PostWidgetContainer : Widget, IPaintHandler
{
    private static readonly Padding Padding = new(8);

    private bool m_loadedReplies;
    private bool m_showingReplies;

    private int m_treeIndex = 0;
    
    private readonly PostWidget m_postWidget;

    private NullWidget? m_repliesHolder;
    private PushButton? m_showRepliesButton;

    public Post ApiPost => m_postWidget.ApiPost;
    public List<string> ReferencedPosts => m_postWidget.ReferencedPosts;
    public PostWidget Test => m_postWidget; // @TEMP
    
    public PostWidgetContainer(Post post, Widget? parent = null) : base(parent)
    {
        Name = "PostWidgetContainer";

        this.Layout = new VBoxLayout
        {
            Padding = new(8),
            Spacing = 8
        };
        this.AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit);

        m_postWidget = new PostWidget(post, this)
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

        var pw = new Dictionary<int, PostWidgetContainer>(replies.Count);
        foreach (var item in replies)
        {
            var widget = new PostWidgetContainer(item.m_postWidget.ApiPost, m_repliesHolder)
            {
                Width = this.Width,
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                m_treeIndex = this.m_treeIndex + 1 // Alternating row colors for replies? Looks pretty cool ig
            };
            pw.Add(item.m_postWidget.ApiPost.No, widget);
        }
        ChanApp.MainWindow.Bruhhh(pw);
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

public class PostWidget : Widget, IMouseClickHandler, IPaintHandler
{
    private readonly PostThumbnail m_previewBitmap;
    private readonly Label m_nameLabel;
    private readonly Label m_dateLabel;
    private readonly Label? m_previewInfoLabel;
    private readonly Label m_postIdLabel;
    private readonly Label m_commentLabel;
    private readonly Label m_repliesLabel;

    public Post ApiPost { get; }
    public readonly List<string> ReferencedPosts = [];

    public PostWidget(API.Models.Post post, Widget? parent = null) : base(parent)
    {
        Name = "A Post widget!!!";
        ShouldCache = true;

        ApiPost = post;

        // UI Layout
        m_nameLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"name\">{post.Name}</span>",
            CatchCursorEvents = false,
        };

        m_dateLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"date\">{post.Now}</span>",
            CatchCursorEvents = false,
        };

        m_postIdLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"postID\">#{post.No}</span>",
            CatchCursorEvents = false,
        };

        m_repliesLabel = new Label(this);

        if (post.Tim != null)
        {
            m_previewInfoLabel = new Label(this)
            {
                X = 0,
                Y = 0,
            };
        }

        var rawComment = post.Com;
        var htmlEncoded = rawComment;
        var decoded = WebUtility.HtmlDecode(htmlEncoded);
        var commentInput = decoded;

        // sanitize html
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(decoded);

            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "a":
                        switch (node.GetAttributeValue("class", ""))
                        {
                            case "quotelink":
                                ReferencedPosts.Add(node.InnerHtml.TrimStart('>'));
                                if (node.InnerText == $">>{ChanApp.ChanClient.CurrentThread.No}")
                                {
                                    node.InnerHtml = $"{node.InnerHtml} (OP)";
                                }
                                break;
                        }
                        break;
                }
            }

            commentInput = doc.DocumentNode.OuterHtml;
            // Console.WriteLine(commentInput);
        }

        var commentY = m_nameLabel.Y + m_nameLabel.Height + 4;

        m_previewBitmap = new(ApiPost, this)
        {
            X = 0,
            Y = commentY,
        };

        m_commentLabel = new Label(this)
        {
            Y = commentY,

            Text = commentInput,
            WordWrap = true,

            Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Fixed),
            CatchCursorEvents = false,
        };

        SetHeight();
    }

    public void SetBitmapPreview(SKImage thumbnail)
    {
        m_previewBitmap.SetThumbnail(thumbnail);
        // Bytes and size info label
        var previewInfo = $"{ApiPost.Filename}{ApiPost.Ext} ({((long)ApiPost.Fsize!).FormatBytes()}, {thumbnail.Width}x{thumbnail.Height})";
        m_previewInfoLabel!.Text = $"<span class=\"date\">{previewInfo}</span>";

        SetHeight();
    }

    #region Widget events

    public void OnPaint(SKCanvas canvas)
    {
    }

    public bool OnMouseClick(MouseEvent evt)
    {
        if (evt.button == GUI.Input.MouseButton.Right)
        {
            var threadUrl = $"https://boards.4chan.org/{ChanApp.ChanClient.CurrentBoard}/thread/{ChanApp.ChanClient.CurrentThread.No}";
            var postUrl = $"{threadUrl}#p{ApiPost.No}";

            MenuPopup a = new(this);
            var m = new Menu(this);

            m.AddAction(MaterialIcons.Link, "Copy Post URL to Clipboard", () =>
            {
                Application.Clipboard.SetText(postUrl);
            });
            m.AddAction(MaterialIcons.Public, "Open Post in Browser", () =>
            {
                Application.OpenURL(postUrl);
            });
            m.AddAction(MaterialIcons.Feed, "Open Thread in Browser", () =>
            {
                Application.OpenURL(threadUrl);
            });

            m.AddSeparator();
            m.AddAction(MaterialIcons.Reply, "Reply", null);

            a.SetMenu(m);
            a.SetPosition(evt.globalX, evt.globalY);

            Console.WriteLine(evt.globalY);

            a.Show();
        }


        return true;
    }

    #endregion

    internal void SetReplies(string replies)
    {
        m_repliesLabel.Text = replies;
    }

    #region Private methods

    internal void OnResize()
    {
        // Fit thumbnail
        var spaceForText = 200;
        m_previewBitmap.FitToMaxWidth(this.Width - spaceForText);

        SetPositions();
        SetHeight();
    }

    internal void SetPositions()
    {
        m_commentLabel.X = (m_previewBitmap.Bitmap != null ? (m_previewBitmap.Width + 8) : 0);

        // m_dateLabel.X = Width - m_dateLabel.Width - Padding.Right;
        m_dateLabel.X = m_nameLabel.X + m_nameLabel.Width + 2;
        // m_postIDLabel.X = m_dateLabel.X + m_dateLabel.Width + 2;
        m_postIdLabel.X = this.Width - m_postIdLabel.Width;

        if (m_previewInfoLabel != null)
        {
            m_previewInfoLabel.Y = (m_previewBitmap.Bitmap != null ? (m_previewBitmap.Y + m_previewBitmap.Height + 8) : 0);
        }

        m_repliesLabel.X = m_dateLabel.X + m_dateLabel.Width + 2;
    }

    internal void SetHeight()
    {
        m_commentLabel.Width = this.Width - m_commentLabel.X;
        m_commentLabel.Height = m_commentLabel.MeasureHeightFromWidth(m_commentLabel.Width);

        int newHeight = 0;
        if (m_commentLabel.Height > m_previewBitmap.Height)
        {
            newHeight += m_commentLabel.Height + 4;
        }
        else
        {
            newHeight = m_previewBitmap.Height + 4;
        }

        this.Height = newHeight + m_nameLabel.Height + ((m_previewInfoLabel?.Height + 8) ?? 0);
    }

    #endregion
}