using System.Diagnostics;
using Polychan.GUI;
using Polychan.GUI.Input;
using Polychan.GUI.Widgets;
using SkiaSharp;
using System.Net;
using MaterialDesign;

namespace Polychan.App.Widgets;

public class ThreadTicketWidget : Widget, IPaintHandler, IPostPaintHandler, IMouseEnterHandler, IMouseLeaveHandler, IMouseDownHandler, IMouseClickHandler
{
    private const int MAX_IMAGE_WIDTH = 75;
    private static readonly Padding Padding = new(8);

    private readonly Image m_previewImage;
    private readonly Label? m_subjectLabel;
    private readonly Label m_commentLabel;

    private bool m_hovering = false;
    
    public Image PreviewImage => m_previewImage;
    public Imageboard.Thread ApiThread { get; }

    public Action? OnItemClick { get; set; }

    public ThreadTicketWidget(Imageboard.Thread thread, Widget? parent = null) : base(parent)
    {
        ApiThread = thread;
        Name = "A thread widget!";
        ShouldCache = true;

        m_previewImage = new Image(this)
        {
            X = Padding.Left,
            Y = Padding.Top,

            CatchCursorEvents = false
        };

        var rawComment = thread.RawCommentContent ?? string.Empty;
        var htmlEncoded = rawComment;
        var decoded = WebUtility.HtmlDecode(htmlEncoded);

        if (!string.IsNullOrEmpty(thread.Title))
        {
            m_subjectLabel = new Label(this)
            {
                X = Padding.Left,
                Y = Padding.Top,
                
                Text = $"<span class=\"name\">{thread.Title}</span>",
                
                WordWrap = true,
                CatchCursorEvents = false,
            };
        }

        m_commentLabel = new Label(this)
        {
            X = Padding.Left,
            Y = Padding.Top + (m_subjectLabel != null ? m_subjectLabel.Height : 0),

            Text = decoded,
            
            WordWrap = true,
            CatchCursorEvents = false,
            ShouldCache = false
        };
        
        // updateLayout();
    }

    public void SetBitmapPreview(SKImage image)
    {
        m_previewImage.Bitmap = image;

        var newWidth = image.Width;
        var newHeight = image.Height;

        if (newWidth > MAX_IMAGE_WIDTH)
        {
            newWidth = MAX_IMAGE_WIDTH;
            newHeight = (int)(((float)newWidth / image.Width) * image.Height);
        }

        m_previewImage.Width = newWidth;
        m_previewImage.Height = newHeight;

        updateLayout();
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        paint.Color = m_hovering ? Application.DefaultStyle.GetButtonHoverColor().Lighter(1.5f) : Palette.Get(ColorRole.Button);
        canvas.DrawRect(new(0, 0, Width, Height), paint);
    }

    public void OnPostPaint(SKCanvas canvas)
    {
        Padding textPadding = new(8);

        using var paint = new SKPaint();

        // Kinda like a proto immediate mode UI?
        // @TODO - I think I should expand this into its own system so we can do immediate drawing more easily in the program.
        // Especially for widgets that don't need interaction but still need layouts.
        var icons = new List<(float ix, float iy, float lx, float ly, string icon, string? label)>();
        var iconX = 0;
        void drawIconText(string icon, string? label = null)
        {
            var iconWidth = Application.FontIcon.MeasureText(icon);
            var labelWidth = label != null ? Application.DefaultFont.MeasureText(label) : 0;
            var spacing = 4;
            
            icons.Add(new (
                // icon pos
                iconX, Application.FontIcon.Size - 2,
                // label pos
                iconX + iconWidth + spacing, Application.DefaultFont.Size - 1,
                icon, label));

            iconX += (int)iconWidth;
            if (label != null)
            {
                iconX += (int)(labelWidth + spacing);
                iconX += 10;
            }
            else
            {
                iconX += 4;
            }
        }
        
        Debug.Assert(ApiThread.CreatedAt != null);
        
        if (ApiThread.Pinned)
            drawIconText(MaterialDesign.MaterialIcons.PushPin);
        if (ApiThread.Locked)
            drawIconText(MaterialDesign.MaterialIcons.Lock);
        drawIconText(MaterialDesign.MaterialIcons.AccessTime, timeAgo((DateTime)ApiThread.CreatedAt));
        drawIconText(MaterialDesign.MaterialIcons.ModeComment, ApiThread.CommentsCount.ToString());
        drawIconText(MaterialDesign.MaterialIcons.Image, ApiThread.AttachmentsCount.ToString());

        var metaRect = new SKRectI(0, 0, iconX + 8 + 4, (int)Application.DefaultFont.Size + (textPadding.Top + textPadding.Bottom));
        metaRect.Left = Width - metaRect.Width;
        metaRect.Right = Width + 1;
        metaRect = metaRect.SetY(Height - metaRect.Height);
        // metaRect.Left = Width - metaRect.Right;

        using var roundRect = new SKRoundRect(metaRect);
        roundRect.SetRectRadii(metaRect,
        [
            new SKPoint(0, 0),
            new SKPoint(),
            new SKPoint(),
            new SKPoint(),
        ]);

        // paint.IsAntialias = true;
        paint.Color = Palette.Get(ColorRole.Window).Lighter(1.1f);
        canvas.DrawRoundRect(roundRect, paint);

        paint.IsStroke = true;
        paint.Color = Palette.Get(ColorRole.Window);
        canvas.DrawRoundRect(roundRect, paint);
        paint.IsAntialias = false;

        canvas.Save();
        canvas.Translate(metaRect.Left + textPadding.Left, metaRect.Top + textPadding.Top);

        paint.IsStroke = false;
        paint.Color = Palette.Get(ColorRole.Text);

        /*
        var iconX = 0;
        void drawIconText(string icon, string? label = null)
        {
            var iconWidth = Application.FontIcon.MeasureText(icon);
            var labelWidth = label != null ? Application.DefaultFont.MeasureText(label) : 0;
            var spacing = 4;

            canvas.DrawText(icon, new SKPoint(iconX, Application.FontIcon.Size - 2), Application.FontIcon, paint);
            if (label != null)
                canvas.DrawText(label, new SKPoint(iconX + iconWidth + spacing, Application.DefaultFont.Size - 1), Application.DefaultFont, paint);

            iconX += (int)(iconWidth + labelWidth + spacing + 8);
        }
        */

        foreach (var icon in icons)
        {
            canvas.DrawText(icon.icon, new SKPoint(icon.ix, icon.iy), Application.FontIcon, paint);
            if (icon.label != null)
                canvas.DrawText(icon.label, new SKPoint(icon.lx, icon.ly), Application.DefaultFont, paint);
        }

        canvas.Restore();
    }

    private string unixToTimeAgo(long unixTime)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        var dateTime = dateTimeOffset.UtcDateTime;

        return timeAgo(dateTime);
    }

    private string timeAgo(DateTime time)
    {
        var now = DateTime.UtcNow;
        var diff = now - time;

        if (diff.TotalSeconds < 60)
        {
            return $"{(int)diff.TotalSeconds} second{(diff.Seconds != 1 ? "s" : "")} ago";
        }
        else if (diff.TotalMinutes < 60)
        {
            return $"{(int)diff.TotalMinutes} minute{(diff.Minutes != 1 ? "s" : "")} ago";
        }
        else if (diff.TotalHours < 24)
        {
            return $"{(int)diff.TotalHours} hour{(diff.Hours != 1 ? "s" : "")} ago";
        }
        else
        {
            return $"{(int)diff.TotalDays} day{(diff.Days != 1 ? "s" : "")} ago";
        }
    }

    public override void OnPostLayout()
    {
        updateLayout();
    }

    public void OnMouseEnter()
    {
        m_hovering = true;
        MouseCursor.Set(MouseCursor.CursorType.Hand);

        TriggerRepaint();
    }

    public void OnMouseLeave()
    {
        m_hovering = false;
        MouseCursor.Set(MouseCursor.CursorType.Arrow);
        
        TriggerRepaint();
    }

    public bool OnMouseDown(MouseEvent evt)
    {
        if (evt.button == MouseButton.Left)
        {
            OnItemClick?.Invoke();
        }

        return true;
    }

    #region Private methods

    private void updateLayout()
    {
        int newHeight = m_previewImage.Height;

        var labelX = Padding.Left + (m_previewImage.Bitmap != null ? (m_previewImage.Width + 8) : 0);

        if (m_subjectLabel != null)
        {
            m_subjectLabel.X = labelX;
            m_subjectLabel.Width = Width - m_subjectLabel.X - Padding.Right;
            m_subjectLabel.Height = m_subjectLabel.MeasureHeightFromWidth(m_subjectLabel.Width);
        }
        
        m_commentLabel.X = labelX;
        m_commentLabel.Y = m_subjectLabel != null ? (m_subjectLabel.Y + m_subjectLabel.Height) : m_commentLabel.Y;
        m_commentLabel.Width = Width - m_commentLabel.X - Padding.Right;
        m_commentLabel.Height = m_commentLabel.MeasureHeightFromWidth(m_commentLabel.Width);

        var subjectHeight = m_subjectLabel?.Height ?? 0;
        var commentHeight = m_commentLabel.Height;
        
        if (subjectHeight + commentHeight > newHeight)
        {
            newHeight = subjectHeight + commentHeight;
        }

        // newHeight = Math.Max(100, newHeight);
        Height = newHeight + Padding.Top /*+ m_nameLabel.Height*/ + Padding.Bottom + 24;
    }

    #endregion

    public bool OnMouseClick(MouseEvent evt)
    {
        if (evt.button == MouseButton.Right)
        {
            var threadUrl = $"https://boards.4chan.org/{ApiThread.BoardId}/thread/{ApiThread.Id}";

            MenuPopup a = new(this);
            var m = new Menu(this);

            m.AddAction(MaterialIcons.Public, "Open Thread in Browser", () => { Application.OpenURL(threadUrl); });

            m.AddSeparator();
            
            m.AddAction(MaterialIcons.Link, "Copy Thread URL to Clipboard",
                () => { Application.Clipboard.SetText(threadUrl); });

            if (ApiThread.Attachment != null)
            {
                m.AddSeparator();
                m.AddAction(MaterialIcons.Link, "Copy Image Link to Clipboard",
                    () => { Application.Clipboard.SetText(ApiThread.Attachment.Url); });
            }

            a.SetMenu(m);
            a.SetPosition(evt.globalX, evt.globalY);

            Console.WriteLine(evt.globalY);

            a.Show();
        }

        return true;
    }
}