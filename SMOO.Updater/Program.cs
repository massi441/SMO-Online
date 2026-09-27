using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SMOO.Updater;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting SMOO updater...");

        int closedCount = CloseProcessInstances("SMOO");
        if (closedCount > 0)
        {
            Console.WriteLine($"Closed {closedCount} of SMOO");
        }

        Console.WriteLine("Fetching latest release from github");

        // TODO: Extract fetching logic in shared lib

        string repoUrl = "https://api.github.com/repos/massi441/SMO-Online/releases/latest";
        HttpClient githubClient = new HttpClient();
        githubClient.DefaultRequestHeaders.UserAgent.TryParseAdd("SMOO");

        try
        {
            string body = await githubClient.GetStringAsync(repoUrl);
            try
            {
                GithubRelease? githubRelease = JsonSerializer.Deserialize<GithubRelease>(body, new JsonSerializerOptions()
                {
                    RespectNullableAnnotations = true,
                });

                if (githubRelease == null)
                {
                    Console.WriteLine("Failed to deserialize release response from github");
                    return;
                }

                // TODO: Validate against current version

                if (await FindAndDownloadRelease(githubRelease))
                {
                    TryStartServer();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize release response from github: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch latest release from github: {ex.Message}");
        }

        Console.Write("Press any key to exit: ");
        Console.ReadKey();
    }

    private static async Task<bool> FindAndDownloadRelease(GithubRelease githubRelease)
    {
        foreach (PlatformRelease platformRelease in githubRelease.PlatformReleases)
        {
            if (!IsCurrentPlatformRelease(platformRelease))
            {
                continue;
            }

            HttpClient downloadClient = new HttpClient();
            downloadClient.DefaultRequestHeaders.UserAgent.TryParseAdd("SMOO");

            try
            {
                Console.WriteLine($"Found release {githubRelease.Version}, downloading it from {platformRelease.DownloadUrl}...");

                using Stream fileStream = await downloadClient.GetStreamAsync(platformRelease.DownloadUrl);

                Console.WriteLine("Successfully downloaded release, unzipping it...");

                string updaterName = Path.GetFileName(Environment.ProcessPath) ?? throw new Exception("Could not get the name of the currently running updater");
                string oldUpdaterName = updaterName + ".old";

                File.Move(Path.Combine(AppContext.BaseDirectory, updaterName), oldUpdaterName, overwrite: true);

                ZipFile.ExtractToDirectory(fileStream, AppContext.BaseDirectory, overwriteFiles: true);

                Console.WriteLine("Successfully extracted release to disk");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while downloading the release: {ex.Message}");
            }
        }

        return false;
    }

    private static bool IsCurrentPlatformRelease(PlatformRelease release)
    {
        if (OperatingSystem.IsWindows())
        {
            return release.Name == "SMOO-win-x64.zip";
        }

        return false;
    }

    // TODO: Move this util into Core once Core is setup

    /// <summary>
    /// Counts and closes all instances of a given process 
    /// </summary>
    /// <param name="processName">The name of the process, without the extension</param>
    /// <returns>The count of instances of the process that were closes</returns>
    private static int CloseProcessInstances(string processName)
    {
        int closedCount = 0;
        Process[] instances = Process.GetProcessesByName(processName);
        foreach (Process process in instances)
        {
            process.Kill();
            closedCount++;
        }

        return closedCount;
    }

    private static void TryStartServer()
    {
        Console.Write("Press y to start the server, or enter to exit: ");
        string? input = Console.ReadLine();

        if (input == "y")
        {

        }
    }
}

// TODO: Move to shared lib

internal class GithubRelease
{
    [JsonPropertyName("tag_name")]
    public required string Version { get; set; } = string.Empty;

    [JsonPropertyName("assets")]
    public required List<PlatformRelease> PlatformReleases { get; set; }

}

internal class PlatformRelease
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public required string DownloadUrl { get; set; }
}
