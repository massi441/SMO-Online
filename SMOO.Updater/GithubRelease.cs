using System.Text.Json.Serialization;

namespace SMOO.Updater;

/// <summary>
/// Represents an API response from a Github Release
/// </summary>
internal class GithubRelease
{
    private static readonly string VersionTagPrefix = "v";


    [JsonInclude, JsonPropertyName("tag_name")]
    public required string VersionTag { get; set; } = string.Empty;


    [JsonPropertyName("assets")]
    public required List<ReleaseAsset> ReleaseAssets { get; set; }

    /// <summary>
    /// Returns the version of the release with the 'v' prefix stripped from the version tag
    /// </summary>
    public string Version => VersionTag[VersionTagPrefix.Length..];

}

/// <summary>
/// Represents a specific asset as part of a github release
/// </summary>
internal class ReleaseAsset
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public required string DownloadUrl { get; set; }
}
