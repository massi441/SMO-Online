using System.Diagnostics;
using Core.Github;
using Core.OS;
using Core.Util;

namespace SMOO.Updater.Lib;

public class SMOOUpdater
{
    private readonly GithubReleaseClient _githubClient;

    public static readonly string ReleaseUrl = "https://api.github.com/repos/massi441/SMO-Online/releases/latest";
    public static readonly string AppName = "SMOO";
    public static readonly string TempDirName = "DownloadTemp";
    public static readonly string ServerName = "SMOO";
    public static readonly string UpdaterName = "Updater";

    public SMOOUpdater()
    {
        _githubClient = new GithubReleaseClient(ReleaseUrl, AppName, GetReleaseName(), CreateTagConverter());
    }

    private static IGithubTagVersionConverter CreateTagConverter()
    {
        string tagPrefix = "v";
        return new GithhubTagVersionPrefixConvertor(tagPrefix);
    }

    public static bool StartUpdater()
    {
        return ProcessUtil.TryStartNewProcess(GetUpdaterPath());
    }

    public static void WipeUpdateTempDirFrom()
    {
        Process[] updaterProcesses = Process.GetProcessesByName(UpdaterName);
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
        int closedCount = ProcessUtil.CloseProcessInstances(ServerName);
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
            yield return ProgressStatus.InProgress("Extracting zip contents into SMOO folder...");

            string currentUpdaterName = FileUtil.GetExecutingFileName(); // Updater.exe
            string oldUpdaterFileName = currentUpdaterName + ".old"; // Updater.exe.old

            string currentUpdaterPath = FileUtil.PathFromRunningDir(currentUpdaterName);
            string oldUpdaterPath = FileUtil.PathFromRunningDir(oldUpdaterFileName);

            FileUtil.RenameFile(currentUpdaterPath, oldUpdaterPath);
            FileUtil.CopyDirectory(tempOutputPath, FileUtil.GetExecutingDirectory());

            string oldUpdaterTempPath = Path.Combine(tempOutputPath, oldUpdaterFileName);
            File.Move(oldUpdaterPath, oldUpdaterTempPath, overwrite: true); // Updater.exe.old gets moved to Temp.Updater.exe.old, then gets wiped by server on next startup

            yield return ProgressStatus.InProgress("Restarting server...");

            if (!RestartServer())
            {
                yield return ProgressStatus.Failure("Failed to restart SMOO server");
                yield break;
            }
        }

        yield return ProgressStatus.Completed();
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

    private static string GetUpdaterPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return FileUtil.PathFromRunningDir(UpdaterName + ".exe");
        }

        return FileUtil.PathFromRunningDir(UpdaterName);
    }

    private static string GetServerPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return FileUtil.PathFromRunningDir(ServerName + ".exe");
        }

        return FileUtil.PathFromRunningDir(ServerName);
    }
}