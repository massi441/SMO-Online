using Core.Util;
using SMOO.Updater.Lib;

namespace SMOO.Updater;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Starting SMOO updater...");

            SMOOUpdater updater = new SMOOUpdater();

            await foreach (ProgressStatus status in updater.DownloadLatest())
            {
                if (status.IsFailed())
                {
                    Console.WriteLine($"Error: {status.Message}");
                    return;
                }
                else
                {
                    Console.WriteLine(status.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occured: {ex.Message}");
            return;
        }
    }
}
