using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;

namespace Polychan.App;

public class FourChanClient
{
    private readonly HttpClientHandler m_httpsClientHandler;
    private readonly HttpClient m_httpClient;
    private readonly SemaphoreSlim m_throttler = new(8); // 8 concurrent downloads

    public string CurrentBoard { get; set; } = string.Empty;
    public FChan.Models.Thread CurrentThread { get; set; }

    public FChan.Responses.BoardsResponse Boards;
    public FChan.Responses.CatalogResponse Catalog;

    public FourChanClient()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = { new FChan.Models.ModelConverter() }
        };
        
        m_httpsClientHandler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer()
        };

        // https://gitlab.com/catamphetamine/imageboard/-/blob/master/docs/engines/4chan.md#post-a-comment

        // COOKIES
        // WE AINT NO ROOKIES
        var cookieUri = new Uri("https://sys.4chan.org");
        m_httpsClientHandler.CookieContainer.Add(cookieUri, new Cookie("cf_clearance", ChanApp.Settings.Cookies.CloudflareClearance));
        m_httpsClientHandler.CookieContainer.Add(cookieUri, new Cookie("4chan_pass", ChanApp.Settings.Cookies.FourchanPasskey));

        m_httpClient = new HttpClient(m_httpsClientHandler);

        // The 4chan API requires a UserAgent or else it won't work.
        // Pretend to be a browser or else Cloudflare will refuse post requests
        m_httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");
        // m_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("My4ChanClient/1.0 (+https://github.com/Starpelly/Polychan)");

        // The 4chan API says we need this but adding it just causes requests to fail so I'm unconvinced.
        // string lastCheck = DateTime.UtcNow.AddHours(-1).ToString("R"); // "R" means RFC1123 pattern
        // m_httpClient.DefaultRequestHeaders.Add("If-Modified-Since", lastCheck);

        // Idk if this is required
        m_httpClient.DefaultRequestHeaders.Add("Referer", "https://boards.4chan.org/");

        // _ = testc();
    }

    private async Task testc()
    {
        var baseUrl = "https://sys.4chan.org/captcha";

        var uriBuilder = new UriBuilder(baseUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query["board"] = "v";
        query["thread_id"] = "713509556";

        uriBuilder.Query = query.ToString();
        var finalUrl = uriBuilder.ToString();

        try
        {
            var json = await m_httpClient.GetStringAsync(finalUrl);
            Console.WriteLine(json);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request failed: {ex.Message}");
        }
    }

    public async Task<FChan.Responses.BoardsResponse> GetBoardsAsync()
    {
        var url = $"https://{FChan.Domains.Api}/boards.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = JsonConvert.DeserializeObject<FChan.Responses.BoardsResponse>(json);
        return result;
    }

    public async Task<FChan.Responses.CatalogResponse> GetCatalogAsync()
    {
        var url = $"https://{FChan.Domains.Api}/{CurrentBoard}/catalog.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = JsonConvert.DeserializeObject<List<FChan.Models.CatalogPage>>(json);
        return new()
        {
            Pages = result!
        };
    }

    public async Task<FChan.Models.Thread> GetThreadPostsAsync(FChan.Models.PostId threadId)
    {
        var url = $"https://{FChan.Domains.Api}/{CurrentBoard}/thread/{threadId}.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = JsonConvert.DeserializeObject<FChan.Models.Thread>(json);
        return result;
    }

    public async Task<SKImage?> DownloadThumbnailFromPostURLAsync(FChan.Models.AttachmentId tim)
    {
        string url = $"https://{FChan.Domains.UserContent}/{CurrentBoard}/{tim}s.jpg";

        try
        {
            byte[] imageBytes = await m_httpClient.GetByteArrayAsync(url);
            using var ms = new MemoryStream(imageBytes);
            return SKImage.FromEncodedData(ms); // Decode into SKBitmap
        }
        catch
        {
            return null; // Handle gracefully if image isn't available
        }
    }

    public async Task DownloadAttachmentFromPostURLAsync(FChan.Models.Post post, Action<SKImage?> onComplete)
    {
        string url = $"https://{FChan.Domains.UserContent}/{CurrentBoard}/{post.Tim}{post.Ext}";

        try
        {
            byte[] imageBytes = await m_httpClient.GetByteArrayAsync(url);
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

    public async Task LoadThumbnailsAsync(IEnumerable<(FChan.Models.PostId thread, FChan.Models.AttachmentId? attachment)> imageIds, Action<FChan.Models.PostId, SKImage?> onComplete)
    {
        var tasks = imageIds.Select(async tim =>
        {
            await m_throttler.WaitAsync();
            try
            {
                if (tim.attachment != null)
                {
                    var img = await DownloadThumbnailFromPostURLAsync((FChan.Models.AttachmentId)tim.attachment);

                    if (img != null)
                    {
                        onComplete.Invoke(tim.thread, img);
                    }
                }
            }
            finally
            {
                m_throttler.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    public class GifFrame
    {
        public required SKImage Image;
        public int Delay; // in milliseconds
    }

    public async Task<List<GifFrame>> LoadGifFromUrlAsync(string url)
    {
        byte[] data = await m_httpClient.GetByteArrayAsync(url);
        using var stream = new MemoryStream(data);

        var frames = new List<GifFrame>();

        using var image = Image.Load<Rgba32>(stream);

        foreach (var frame in image.Frames)
        {
            // Delay (10ms units)
            int delay = 100;
            if (frame.Metadata.TryGetGifMetadata(out var gifMeta))
            {
                delay = gifMeta.FrameDelay * 10; // 1 unit = 10 ms
            }

            using var skBitmap = new SKBitmap(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            frame.CopyPixelDataTo(MemoryMarshal.Cast<byte, Rgba32>(skBitmap.GetPixelSpan()));

            frames.Add(new GifFrame
            {
                Image = SKImage.FromBitmap(skBitmap),
                Delay = delay
            });
        }

        return frames;
    }
}