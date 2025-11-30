using Newtonsoft.Json;
using System.Net;
using System.Web;

namespace Imageboard;

public class Client
{
    private readonly HttpClientHandler m_httpsClientHandler;
    private readonly HttpClient m_httpClient;
    private readonly SemaphoreSlim m_throttler = new(8); // 8 concurrent downloads
    
    public HttpClient HttpClient => m_httpClient;
    public SemaphoreSlim Throttler => m_throttler;

    /// <summary>
    /// @TEMP @TODO - just for testing
    /// </summary>
    public readonly Dictionary<BoardId, Board> FourChanBoards;
    
    public Client(string cloudflareClearance, string fourchanPasskey)
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = { new Backends.JsonResponseConverter() }
        };
        
        m_httpsClientHandler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer()
        };

        // https://gitlab.com/catamphetamine/imageboard/-/blob/master/docs/engines/4chan.md#post-a-comment

        // COOKIES
        // WE AINT NO ROOKIES
        var cookieUri = new Uri("https://sys.4chan.org");
        m_httpsClientHandler.CookieContainer.Add(cookieUri, new Cookie("cf_clearance", cloudflareClearance));
        m_httpsClientHandler.CookieContainer.Add(cookieUri, new Cookie("4chan_pass", fourchanPasskey));

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
        
        FourChanBoards = GetBoardsAsync().Result;
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

    private async Task<Dictionary<BoardId, Board>> GetBoardsAsync()
    {
        var url = $"https://{Backends.FChan.Domains.Api}/boards.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = new Dictionary<BoardId, Board>();
        
        var fourResponse = JsonConvert.DeserializeObject<Backends.FChan.Responses.BoardsResponse>(json);
        foreach (var fboard in fourResponse.Boards)
        {
            var boardId = new BoardId(fboard.URL);
            Console.WriteLine(boardId);
            result.Add(boardId, new Board()
            {
                Id = boardId,
                Title = fboard.Title,
                Language = "en",
                ExplicitContent = fboard.Worksafe == 0,
            });
        }
        return result;
    }

    public async Task<Catalog> GetCatalogAsync(Board board)
    {
        var url = $"https://{Backends.FChan.Domains.Api}/{board.Id}/catalog.json";
        var json = await m_httpClient.GetStringAsync(url);

        var fourResponse = JsonConvert.DeserializeObject<List<Backends.FChan.Models.CatalogPage>>(json)!;

        var pages = new Catalog.Page[fourResponse.Count];
        for (var i = 0; i < fourResponse.Count; i++)
        {
            pages[i] = new Catalog.Page()
            {
                Id = fourResponse[i].Page,
                Threads = new Thread[fourResponse[i].Threads.Count]
            };
            for (var ii = 0; ii < fourResponse[i].Threads.Count; ii++)
            {
                var fourthread = fourResponse[i].Threads[ii];
                pages[i].Threads[ii] = new Thread()
                {
                    Id = new ThreadId(fourthread.No),
                    BoardId = board.Id,
                    Summary = true,
                    Title = fourthread.Sub,
                    CreatedAt = Utils.UnixToDateTime(fourthread.Time),
                    UpdatedAt = Utils.UnixToDateTime(fourthread.LastModified),
                    CommentsCount = fourthread.Replies ?? 0,
                    AttachmentsCount = fourthread.Images ?? 0,
                    CommentAttachmentsCount = fourthread.Images - 1 ?? 0,
                    Pinned = fourthread.Sticky == 1,
                    Locked = fourthread.Closed == 1,
                    RawCommentContent = new RawCommentContent(fourthread.Com),
                    Attachment = null
                };
                if (fourthread.Tim != null)
                {
                    pages[i].Threads[ii].Attachment = new Attachment()
                    {
                        Url = $"https://{Backends.FChan.Domains.UserContent}/{board.Id}/{fourthread.Tim}{fourthread.Ext}",
                        ThumbnailUrl = $"https://{Backends.FChan.Domains.UserContent}/{board.Id}/{fourthread.Tim}s.jpg",
                        FileName = fourthread.Filename,
                        FileSize = (long)fourthread.Fsize!,
                        Ext = fourthread.Ext,
                        Type = Utils.GetAttachmentTypeFromExtension(fourthread.Ext),
                    };
                }
            }
        }
        return new Catalog()
        {
            Board = board,
            Pages = pages
        };
    }

    public async Task<Thread> GetFullThreadAsync(Thread thread)
    {
        var url = $"https://{Backends.FChan.Domains.Api}/{thread.BoardId}/thread/{thread.Id}.json";
        var json = await m_httpClient.GetStringAsync(url);

        var fourResponse = JsonConvert.DeserializeObject<Backends.FChan.Models.ThreadPosts>(json)!;
        var comments = new Comment[fourResponse.Posts.Count];
        for (var i = 0; i < fourResponse.Posts.Count; i++)
        {
            var fourComment = fourResponse.Posts[i];
            comments[i] = new Comment()
            {
                Id = new CommentId(fourComment.No),
                ThreadId = thread.Id,
                BoardId = thread.BoardId,
                Title = fourComment.Sub,
                CreatedAt = Utils.UnixToDateTime(fourComment.Time),
                AuthorName = fourComment.Name,
                AuthorRole = fourComment.Capcode,
                RawCommentContent = new RawCommentContent(fourComment.Com),
            };
            if (fourComment.Tim != null)
            {
                comments[i].Attachment = new Attachment()
                {
                    Url = $"https://{Backends.FChan.Domains.UserContent}/{thread.BoardId}/{fourComment.Tim}{fourComment.Ext}",
                    ThumbnailUrl = $"https://{Backends.FChan.Domains.UserContent}/{thread.BoardId}/{fourComment.Tim}s.jpg",
                    FileName = fourComment.Filename,
                    FileSize = (long)fourComment.Fsize,
                    Ext = fourComment.Ext,
                    Type = Utils.GetAttachmentTypeFromExtension(fourComment.Ext),
                };
            }
        };
        var result = new Thread()
        {
            Id = new ThreadId(fourResponse.No),
            BoardId = thread.BoardId,
            Summary = false,
            Title = comments[0].Title,
            Comments = comments,
            CreatedAt = comments[0].CreatedAt,
            UpdatedAt = comments[0].UpdatedAt,
            CommentsCount = fourResponse.Posts[0].Replies ?? 0,
            AttachmentsCount = fourResponse.Posts[0].Images ?? 0,
            CommentAttachmentsCount = fourResponse.Posts[0].Images - 1 ?? 0,
            Pinned = fourResponse.Posts[0].Sticky == 1,
            Locked = fourResponse.Posts[0].Closed == 1,
            RawCommentContent = comments[0].RawCommentContent,
            Attachment = comments[0].Attachment,
        };
        return result;
    }
}