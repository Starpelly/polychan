using System.Diagnostics;
using HtmlAgilityPack;
using MaterialDesign;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;
using Polychan.App.Utils;
using SkiaSharp;
using System.Net;

namespace Polychan.App.Widgets;

public class CommentWidgetContent : Widget, IPaintHandler, IMouseClickHandler
{
    private readonly CommentThumbnail? m_previewBitmap;
    private readonly Label m_nameLabel;
    private readonly Label m_dateLabel;
    private readonly Label? m_previewInfoLabel;
    private readonly Label m_postIdLabel;
    private readonly Label m_commentLabel;
    private readonly Label m_repliesLabel;

    public Imageboard.Comment ApiComment { get; }
    public readonly List<string> ReferencedPosts = [];

    public CommentWidgetContent(Imageboard.Comment comment, Widget? parent = null) : base(parent)
    {
        Name = "A Post widget!!!";
        ShouldCache = true;

        ApiComment = comment;
        
        // UI Layout
        m_nameLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"name\">{comment.AuthorName}</span>",
            CatchCursorEvents = false,
        };

        m_dateLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"date\">{comment.CreatedAt}</span>",
            CatchCursorEvents = false,
        };

        m_postIdLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"postID\">#{comment.Id}</span>",
            CatchCursorEvents = false,
        };

        m_repliesLabel = new Label(this);

        var rawComment = comment.RawCommentContent!;
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
                                // @HACK @TODO - this isn't necessarily true and needs to be an API bool instead.
                                if (node.InnerText == $">>{ApiComment.ThreadId}")
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

        if (comment.Attachment != null)
        {
            var thumbnailUrl = comment.Attachment.Url;
            var thumbnailExt = comment.Attachment.Ext;
            m_previewBitmap = new CommentThumbnail(thumbnailUrl, thumbnailExt, this)
            {
                X = 0,
                Y = commentY,
            };
            m_previewInfoLabel = new Label(this)
            {
                X = 0,
                Y = 0,
            };
        }

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
        Debug.Assert(ApiComment.Attachment != null);
        
        m_previewBitmap?.SetThumbnail(thumbnail);
        // Bytes and size info label
        var previewInfo = $"{ApiComment.Attachment.FileName}{ApiComment.Attachment.Ext} ({ApiComment.Attachment.FileSize.FormatBytes()}, {thumbnail.Width}x{thumbnail.Height})";
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
            var threadUrl = $"https://boards.4chan.org/{ApiComment.BoardId}/thread/{ApiComment.ThreadId}";
            var postUrl = $"{threadUrl}#p{ApiComment.Id}";

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
        m_previewBitmap?.FitToMaxWidth(this.Width - spaceForText);

        SetPositions();
        SetHeight();
    }

    internal void SetPositions()
    {
        m_commentLabel.X = (m_previewBitmap?.Bitmap != null ? (m_previewBitmap.Width + 8) : 0);

        // m_dateLabel.X = Width - m_dateLabel.Width - Padding.Right;
        m_dateLabel.X = m_nameLabel.X + m_nameLabel.Width + 2;
        // m_postIDLabel.X = m_dateLabel.X + m_dateLabel.Width + 2;
        m_postIdLabel.X = this.Width - m_postIdLabel.Width;

        if (m_previewInfoLabel != null)
        {
            m_previewInfoLabel.Y = m_previewBitmap!.Y + m_previewBitmap.Height + 8;
        }

        m_repliesLabel.X = m_dateLabel.X + m_dateLabel.Width + 2;
    }

    private void SetHeight()
    {
        m_commentLabel.Width = this.Width - m_commentLabel.X;
        m_commentLabel.Height = m_commentLabel.MeasureHeightFromWidth(m_commentLabel.Width);

        int newHeight = 0;
        if (m_previewBitmap != null)
        {
            if (m_commentLabel.Height > m_previewBitmap.Height)
            {
                newHeight += m_commentLabel.Height + 4;
            }
            else
            {
                newHeight = m_previewBitmap.Height + 4;
            }
        }
        else
        {
            newHeight += m_commentLabel.Height + 4;
        }
        
        if (m_previewInfoLabel != null)
        {
            newHeight += m_previewInfoLabel.Height;
            newHeight += 8;
        }

        this.Height = newHeight + m_nameLabel.Height;
    }

    #endregion
}