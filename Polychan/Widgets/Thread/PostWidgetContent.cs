using HtmlAgilityPack;
using MaterialDesign;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using Polychan.App.Utils;
using SkiaSharp;
using System.Net;

namespace Polychan.App.Widgets;

public class PostWidgetContent : Widget, IPaintHandler, IMouseClickHandler
{
    private readonly PostThumbnail m_previewBitmap;
    private readonly Label m_nameLabel;
    private readonly Label m_dateLabel;
    private readonly Label? m_previewInfoLabel;
    private readonly Label m_postIdLabel;
    private readonly Label m_commentLabel;
    private readonly Label m_repliesLabel;

    private readonly string m_board;
    private readonly FChan.Models.ThreadPosts m_thread;

    public FChan.Models.Post ApiPost { get; }
    public readonly List<string> ReferencedPosts = [];

    public PostWidgetContent(string board, FChan.Models.ThreadPosts thread, FChan.Models.Post post, Widget? parent = null) : base(parent)
    {
        Name = "A Post widget!!!";
        ShouldCache = true;

        ApiPost = post;
        m_board = board;
        m_thread = thread;
        
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
                                if (node.InnerText == $">>{m_thread.No}")
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

        var thumbnailUrl = $"https://{FChan.Domains.UserContent}/{board}/{post.Tim}{post.Ext}";
        var thumbnailExt = ApiPost.Ext;
        m_previewBitmap = new PostThumbnail(thumbnailUrl, thumbnailExt, this)
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
            var threadUrl = $"https://boards.4chan.org/{m_board}/thread/{m_thread.No}";
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

    private void SetHeight()
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