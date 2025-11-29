using Newtonsoft.Json;

namespace Polychan.API.Models;

/// <summary>
/// Represents a page in the 4chan board catalog.
/// </summary>
public class CatalogPage
{
    /// <summary>
    /// Page number in the catalog.
    /// </summary>
    [JsonProperty("page")]
    public int Page { get; set; }

    /// <summary>
    /// List of threads on this page.
    /// </summary>
    [JsonProperty("threads")]
    public List<CatalogThread> Threads { get; set; } = [];
}

/// <summary>
/// Represents a thread or a post in the catalog.
/// </summary>
public class CatalogThread
{
    /// <summary>The numeric post ID.</summary>
    [JsonProperty("no")]
    public long No { get; set; }

    /// <summary>Thread ID this post is replying to. Zero for OP.</summary>
    [JsonProperty("resto")]
    public int Resto { get; set; }

    /// <summary>Indicates if the thread is stickied (1 = yes).</summary>
    [JsonProperty("sticky")]
    public int? Sticky { get; set; }

    /// <summary>Indicates if the thread is closed (1 = yes).</summary>
    [JsonProperty("closed")]
    public int? Closed { get; set; }

    /// <summary>Date and time string the post was made (EST/EDT timezone).</summary>
    [JsonProperty("now")]
    public string Now { get; set; } = string.Empty;

    /// <summary>UNIX timestamp the post was created.</summary>
    [JsonProperty("time")]
    public int Time { get; set; }

    /// <summary>Name user posted with (usually "Anonymous").</summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>User's tripcode, if used.</summary>
    [JsonProperty("trip")]
    public string Trip { get; set; } = string.Empty;

    /// <summary>Poster ID (8 characters), if present.</summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Poster's capcode identifier.</summary>
    [JsonProperty("capcode")]
    public string Capcode { get; set; } = string.Empty;

    /// <summary>ISO 3166-1 alpha-2 country code.</summary>
    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>Poster's country name.</summary>
    [JsonProperty("country_name")]
    public string CountryName { get; set; } = string.Empty;

    /// <summary>Subject text of the original post.</summary>
    [JsonProperty("sub")]
    public string Sub { get; set; } = string.Empty;

    /// <summary>Comment in HTML-escaped format.</summary>
    [JsonProperty("com")]
    public string Com { get; set; } = string.Empty;

    /// <summary>
    /// Unix timestamp + microtime that an image was uploaded
    /// This can also be used to grab the image attached to the post.
    /// </summary>
    [JsonProperty("tim")]
    public AttachmentID? Tim { get; set; }

    /// <summary>Original filename as uploaded.</summary>
    [JsonProperty("filename")]
    public string Filename { get; set; } = string.Empty;

    /// <summary>File extension of the uploaded attachment.</summary>
    [JsonProperty("ext")]
    public string Ext { get; set; } = string.Empty;

    /// <summary>Size of the file in bytes.</summary>
    [JsonProperty("fsize")]
    public int? Fsize { get; set; }

    /// <summary>Base64 encoded MD5 hash of the file.</summary>
    [JsonProperty("md5")]
    public string Md5 { get; set; } = string.Empty;

    /// <summary>Width of the uploaded image.</summary>
    [JsonProperty("w")]
    public int? W { get; set; }

    /// <summary>Height of the uploaded image.</summary>
    [JsonProperty("h")]
    public int? H { get; set; }

    /// <summary>Thumbnail width.</summary>
    [JsonProperty("tn_w")]
    public int? TnW { get; set; }

    /// <summary>Thumbnail height.</summary>
    [JsonProperty("tn_h")]
    public int? TnH { get; set; }

    /// <summary>Whether the file was deleted (1 = yes).</summary>
    [JsonProperty("filedeleted")]
    public int? FileDeleted { get; set; }

    /// <summary>Whether the image was spoilered (1 = yes).</summary>
    [JsonProperty("spoiler")]
    public int? Spoiler { get; set; }

    /// <summary>Custom spoiler ID (1–10), if set.</summary>
    [JsonProperty("custom_spoiler")]
    public int? CustomSpoiler { get; set; }

    /// <summary>Number of replies omitted from preview.</summary>
    [JsonProperty("omitted_posts")]
    public int? OmittedPosts { get; set; }

    /// <summary>Number of image replies omitted from preview.</summary>
    [JsonProperty("omitted_images")]
    public int? OmittedImages { get; set; }

    /// <summary>Total number of replies in the thread.</summary>
    [JsonProperty("replies")]
    public int? Replies { get; set; }

    /// <summary>Total number of image replies in the thread.</summary>
    [JsonProperty("images")]
    public int? Images { get; set; }

    /// <summary>Indicates if the thread reached bump limit (1 = yes).</summary>
    [JsonProperty("bumplimit")]
    public int? BumpLimit { get; set; }

    /// <summary>Indicates if the image limit was reached (1 = yes).</summary>
    [JsonProperty("imagelimit")]
    public int? ImageLimit { get; set; }

    /// <summary>UNIX timestamp of last modification to the thread.</summary>
    [JsonProperty("last_modified")]
    public int? LastModified { get; set; }

    /// <summary>Category tag (used on /f/ board only).</summary>
    [JsonProperty("tag")]
    public string Tag { get; set; } = string.Empty;

    /// <summary>SEO-friendly URL slug for the thread.</summary>
    [JsonProperty("semantic_url")]
    public string SemanticUrl { get; set; } = string.Empty;

    /// <summary>Year of 4chan pass purchase (if set).</summary>
    [JsonProperty("since4pass")]
    public int? Since4Pass { get; set; }

    /// <summary>Number of unique posters in the thread.</summary>
    [JsonProperty("unique_ips")]
    public int? UniqueIps { get; set; }

    /// <summary>Whether a mobile-optimized image exists (1 = yes).</summary>
    [JsonProperty("m_img")]
    public int? MobileImage { get; set; }

    /// <summary>List of most recent replies to the thread.</summary>
    [JsonProperty("last_replies")]
    public List<Thread> LastReplies { get; set; } = [];
}