using Newtonsoft.Json;

namespace FChan.Models;

/// <summary>
/// A full thread, consisting of the OP and all replies.
/// </summary>
public class Thread : Model
{
    [JsonIgnore]
    public PostId No => Posts[0].No;

    [JsonProperty("posts")]
    public List<Post> Posts = [];
}

/// <summary>
/// A single post within a thread, including the OP.
/// </summary>
public class Post : Model
{
    [JsonProperty("no")]
    public PostId No;

    [JsonProperty("resto")]
    public int Resto;

    [JsonProperty("sticky")]
    public int? Sticky;

    [JsonProperty("closed")]
    public int? Closed;

    [JsonProperty("now")]
    public string Now = string.Empty;

    [JsonProperty("time")]
    public int Time;

    [JsonProperty("name")]
    public string Name = string.Empty;

    [JsonProperty("trip")]
    public string Trip = string.Empty;

    [JsonProperty("id")]
    public string Id = string.Empty;

    [JsonProperty("capcode")]
    public string Capcode = string.Empty;

    [JsonProperty("country")]
    public string Country = string.Empty;

    [JsonProperty("country_name")]
    public string CountryName = string.Empty;

    [JsonProperty("board_flag")]
    public string BoardFlag = string.Empty;

    [JsonProperty("flag_name")]
    public string FlagName = string.Empty;

    [JsonProperty("sub")]
    public string Sub = string.Empty;

    [JsonProperty("com")]
    public string Com = string.Empty;

    /// <summary>
    /// Unix timestamp + microtime that an image was uploaded
    /// This can also be used to grab the image attached to the post.
    /// </summary>
    [JsonProperty("tim")]
    public AttachmentId? Tim;

    [JsonProperty("filename")]
    public string Filename = string.Empty;

    [JsonProperty("ext")]
    public string Ext = string.Empty;

    [JsonProperty("fsize")]
    public int? Fsize;

    [JsonProperty("md5")]
    public string Md5 = string.Empty;

    [JsonProperty("w")]
    public int? W;

    [JsonProperty("h")]
    public int? H;

    [JsonProperty("tn_w")]
    public int? TnW;

    [JsonProperty("tn_h")]
    public int? TnH;

    [JsonProperty("filedeleted")]
    public int? FileDeleted;

    [JsonProperty("spoiler")]
    public int? Spoiler;

    [JsonProperty("custom_spoiler")]
    public int? CustomSpoiler;

    [JsonProperty("replies")]
    public int? Replies;

    [JsonProperty("images")]
    public int? Images;

    [JsonProperty("bumplimit")]
    public int? BumpLimit;

    [JsonProperty("imagelimit")]
    public int? ImageLimit;

    [JsonProperty("tag")]
    public string Tag = string.Empty;

    [JsonProperty("semantic_url")]
    public string SemanticUrl = string.Empty;

    [JsonProperty("since4pass")]
    public int? Since4Pass;

    [JsonProperty("unique_ips")]
    public int? UniqueIps;

    [JsonProperty("m_img")]
    public int? MobileImage;

    [JsonProperty("archived")]
    public int? Archived;

    [JsonProperty("archived_on")]
    public int? ArchivedOn;
}