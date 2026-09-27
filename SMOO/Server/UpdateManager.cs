using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SMOO.Server;

/// <summary>
/// Manages the current 
/// </summary>
internal class UpdateManager
{
    public static async Task<UpdateCheck> CheckUpdate()
    {
        Version currentVersion = Assembly.GetEntryAssembly()?.GetName()?.Version 
            ?? throw new Exception("The SMOO version was not found on the current installation, either reinstall SMOO manually or contact the developper if this issue still persists");

        HttpClient githubClient = new HttpClient();

        githubClient.DefaultRequestHeaders.UserAgent.ParseAdd("SMOO");

        string repoUrl = "https://api.github.com/repos/massi441/SMO-Online/releases/latest";

        string body = await githubClient.GetStringAsync(repoUrl);
        GithubRelease? release = JsonSerializer.Deserialize<GithubRelease>(body);
        if (release == null)
        {
            return new UpdateCheck(currentVersion);
        }

        if (!Version.TryParse(release.Version, out Version? latestVersion)) {
            return new UpdateCheck(currentVersion);
        }

        return new UpdateCheck(currentVersion, latestVersion);
    }
}

internal class GithubRelease
{
    private readonly string VersionTagPrefix = "v";

    [JsonInclude, JsonPropertyName("tag_name")]
    public required string VersionTag { get; set; } = string.Empty;

    public string Version => VersionTag[VersionTagPrefix.Length..];
}

internal readonly struct UpdateCheck
{
    public Version CurrentVersion { get; init; } = null!;
    public Version LatestVersion { get; init; } = null!;
    
    public UpdateCheck(Version currentVersion)
    {
        CurrentVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);
    }

    public UpdateCheck(Version currentVersion, Version latestVersion) : this(currentVersion)
    {
        LatestVersion = latestVersion;
    }

    public bool IsNeedUpdate()
    {
        return CurrentVersion != null && LatestVersion != null && LatestVersion > CurrentVersion;
    }
}
