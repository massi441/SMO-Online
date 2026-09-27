using System.Diagnostics;
using Core.Github;
using Core.OS;
using Core.Util;

namespace SMOO.Updater;

public class SMOOUpdater
{
    private readonly GithubReleaseClient _githubClient;

    public static readonly string ReleaseUrl = "https://api.github.com/repos/massi441/SMO-Online/releases/latest";
    public static readonly string AppName = "SMOO";
    public static readonly string TempDirName = "DownloadTemp";

    public SMOOUpdater()
    {
        _githubClient = new GithubReleaseClient(ReleaseUrl, AppName, GetReleaseName(), CreateTagConverter());
    }

    private static IGithubTagVersionConverter CreateTagConverter()
    {
        string tagPrefix = "v";
        return new GithhubTagVersionPrefixConvertor(tagPrefix);
    }

    private static string GetUpdaterPath()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, ReflectionUtil.GetAssemblyNameOf<SMOOUpdater>());
        if (OperatingSystem.IsWindows())
        {
            return filePath + ".exe";
        }

        return filePath;
    }

    private static string GetServerPath()
    {
        string serverName = "SMOO";
        string filePath = Path.Combine(AppContext.BaseDirectory, serverName);
        if (OperatingSystem.IsWindows())
        {
            return filePath + ".exe";
        }

        return filePath;
    }

    public static bool StartUpdater()
    {
        return ProcessUtil.TryStartNewProcess(GetUpdaterPath());
    }

    public static void CloseUpdater()
    {
        Process[] updaterProcesses = Process.GetProcessesByName(ReflectionUtil.GetAssemblyNameOf<SMOOUpdater>());
        foreach (Process updater in updaterProcesses)
        {
            updater.WaitForExit();
        }


        FileUtil.DeleteDirIfExists(FileUtil.PathFromRunningDir(TempDirName));
    }

    public Task<GithubUpdateCheck> CheckUpdate()
    {
        return _githubClient.CheckUpdateFor<SMOOUpdater>();
    }

    public async IAsyncEnumerable<ProgressStatus> DownloadLatest()
    {
        int closedCount = ProcessUtil.CloseProcessInstances("SMOO");
        if (closedCount > 0)
        {
            yield return ProgressStatus.InProgress($"Closed {closedCount} instances of SMOO server");
        }

        string tempOutputPath = FileUtil.PathFromRunningDir(TempDirName);

        await foreach (ProgressStatus status in _githubClient.DownloadLatestFor<SMOOUpdater>(tempOutputPath)) // the CI always set the same version for the server and the updater so using the version of the updater is fine here
        {
            yield return status;

            if (status.IsFailed())
            {
                yield break;
            }
        }

        // the temp dir might not be created if the version is already up to date
        if (Directory.Exists(tempOutputPath))
        {
            string currentUpdaterName = FileUtil.PathFromRunningDir(FileUtil.GetExecutingFileName());
            string newUpdaterFileName = currentUpdaterName + ".old";

            FileUtil.RenameFile(currentUpdaterName, newUpdaterFileName);
            FileUtil.CopyDirectory(tempOutputPath, FileUtil.GetRunningDir());

            yield return ProgressStatus.InProgress("Restarting server...");

            if (!RestartServer())
            {
                yield return ProgressStatus.Failure("Failed to restart SMOO server");
                yield break;
            }
        }

        yield return ProgressStatus.Success();
    }

    private static bool RestartServer()
    {
        return ProcessUtil.TryStartNewProcess(GetServerPath());
    }

    private static string GetReleaseName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "SMOO-win-x64.zip";
        }

        throw new Exception("Unsupported platform");
    }
}
