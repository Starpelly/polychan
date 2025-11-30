using Polychan.GUI.Widgets;
using Polychan.GUI;
using SkiaSharp;
using Polychan.App.Utils;

namespace Polychan.App.Widgets;

public class CommentThumbnail : Image, IPaintHandler, IMouseDownHandler, IMouseEnterHandler, IMouseLeaveHandler
{
    private const int MAX_IMAGE_WIDTH = 1280;

    private SKImage? m_thumbnailImage;
    private SKImage? m_fullImage;

    private bool m_usingThumbnail = true;
    private bool m_loadedFull = false;
    private bool m_triedLoadingFull = false;

    private readonly string m_fullUrl;
    private readonly string m_ext;

    private GifPlayer? m_gifPlayer;

    public CommentThumbnail(string fullSizedUrl, string ext, CommentWidgetContent parent) : base(parent)
    {
        m_fullUrl = fullSizedUrl;
        m_ext = ext;
        updateImage(null);
    }

    public void SetThumbnail(SKImage thumbnail)
    {
        m_thumbnailImage = thumbnail;

        updateImage(m_thumbnailImage);
    }

    public bool OnMouseDown(MouseEvent evt)
    {
        if (evt.button != GUI.Input.MouseButton.Left)
            return false;

        if (m_usingThumbnail)
        {
            if (!m_loadedFull)
            {
                loadFull();
            }
        }

        if (!m_loadedFull) return false;

        if (!m_usingThumbnail)
        {
            m_gifPlayer?.Stop();
        }
        else
        {
            m_gifPlayer?.Start();
        }

        m_usingThumbnail = !m_usingThumbnail;
        updateImage((m_usingThumbnail) ? m_thumbnailImage : m_fullImage);

        return true;
    }

    public new void OnPaint(SKCanvas canvas)
    {
        canvas.Save();

        base.OnPaint(canvas);

        canvas.Restore();

        using var paint = new SKPaint();
        paint.Color = Application.DefaultStyle.GetFrameColor();
        paint.IsStroke = true;
        canvas.DrawRoundRect(new SKRect(0, 0, Width - 1, Height - 1), 0, 0, paint);

        // Idk if we wanna update the gif while it isn't painted?
        m_gifPlayer?.Update();
    }

    public void OnMouseEnter()
    {
        MouseCursor.Set(MouseCursor.CursorType.Hand);
    }

    public void OnMouseLeave()
    {
        MouseCursor.Set(MouseCursor.CursorType.Arrow);
    }

    internal void FitToMaxWidth(int maxWidth)
    {
        if (Bitmap == null)
            return;

        var newWidth = Bitmap.Width;
        var newHeight = Bitmap.Height;

        // var fullPreviewWidth = m_fullImage?.Width ?? newWidth;
        if (maxWidth > newWidth)
        {
            maxWidth = newWidth;
        }

        if (newWidth > maxWidth || newWidth > CommentThumbnail.MAX_IMAGE_WIDTH)
        {
            newWidth = maxWidth;
            newHeight = (int)(((float)newWidth / Bitmap.Width) * Bitmap.Height);
        }

        Resize(newWidth, newHeight);
    }

    #region Private methods

    private void updateImage(SKImage? bitmap)
    {
        Bitmap = bitmap;

        if (Bitmap == null)
        {
            Resize(0, 0);
            return;
        }

        FitToMaxWidth(m_fullImage?.Width ?? MAX_IMAGE_WIDTH);

        (Parent as CommentWidgetContent)?.OnResize();
    }

    private void loadFull()
    {
        if (m_triedLoadingFull) return;
        m_triedLoadingFull = true;

        if (m_ext == ".gif")
        {
            m_gifPlayer = new GifPlayer();

            Console.WriteLine(m_fullUrl);
            _ = m_gifPlayer.LoadAsync(m_fullUrl, () =>
            {
                m_loadedFull = true;
                m_usingThumbnail = !m_usingThumbnail;

                // Fallback to the first frame in the gif for the full image
                m_fullImage = m_gifPlayer.CurrentImage;
            });

            m_gifPlayer.OnFrameChanged = () =>
            {
                if (!m_usingThumbnail)
                    updateImage(m_gifPlayer.CurrentImage);
            };
        }
        else
        {
            _ = DownloadAttachmentFromURLAsync(m_fullUrl, (thumbnail) =>
            {
                if (thumbnail != null)
                {
                    m_fullImage = thumbnail;

                    m_usingThumbnail = !m_usingThumbnail;
                    updateImage(m_fullImage);

                    m_loadedFull = true;
                }
            });
        }
    }
    
    public async Task DownloadAttachmentFromURLAsync(string url, Action<SKImage?> onComplete)
    {
        try
        {
            byte[] imageBytes = await ChanApp.ImageboardClient.HttpClient.GetByteArrayAsync(url);
            using var ms = new MemoryStream(imageBytes);
            var ret = SKImage.FromEncodedData(ms); // Decode into SKBitmap
            onComplete.Invoke(ret);
        }
        catch
        {
            onComplete?.Invoke(null);
            // return null; // Handle gracefully if image isn't available
        }
    }

    #endregion

    public override void Dispose()
    {
        base.Dispose();

        m_thumbnailImage?.Dispose();
        m_fullImage?.Dispose();
        m_gifPlayer?.Dispose();

        Console.WriteLine("Disposed Comment Thumbnail");
    }
}