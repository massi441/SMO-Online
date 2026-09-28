using Core.Github;
using Microsoft.Extensions.Logging;
using SMOO.Server;
using SMOO.Updater.Lib;

namespace SMOO;

class Program
{
    static async Task Main(string[] args)
    {
        SMOOUpdater.WipeUpdateTempDirFrom();

        if (await RequestUpdate())
        {
            return;
        }

        await Run();
    }

    private static async Task Run()
    {
        try
        {
            ServerLogger logger = ServerLoggerFactory.Instance();

            ServerConfig config = Configurator.Load();

            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            using SMOOServer server = ServerBuilder.Build(config, cancellationTokenSource.Token);

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            };

            await server.Run(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while the server was running: {ex.Message}");
        }
    }

    private static async Task<bool> RequestUpdate()
    {
        ServerLogger logger = ServerLoggerFactory.Instance();

        try
        {

            SMOOUpdater updater = new SMOOUpdater();

            GithubUpdateCheck updateCheck = await updater.CheckUpdate();
            if (updateCheck.IsNeedUpdate())
            {
                Console.WriteLine($"You are currently on version {updateCheck.CurrentVersion} of SMOO, but version {updateCheck.LatestVersion} is available.");
                Console.Write("Would you like to download it? (y): ");

                string? input = Console.ReadLine();

                if (input == "y" && SMOOUpdater.StartUpdater())
                {
                    Console.WriteLine("Launching updater");
                    return true;
                }
            }
            else
            {
                logger.LogInformation("Current SMOO version ({CurrentVersion}) is up to date", updateCheck.CurrentVersion);
            }

        }
        catch (Exception ex)
        {
            logger.LogWarning("An error occured while looking for an update: {Message}", ex.Message);
        }

        return false;
    }
}
