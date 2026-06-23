using System.Text.Json;

namespace SMOO.Server;

internal static class Configurator
{
    private static JsonSerializerOptions JsonOptions => new JsonSerializerOptions()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static ServerConfig Load()
    {
        ServerConfig config = new ServerConfig();

        string? configPath = GetConfigPath();
        if (configPath == null)
        {
            return config;
        }

        if (!Path.Exists(configPath))
        {
            Console.WriteLine($"No configuration found at {configPath}, creating it.");
            try
            {
                string json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(configPath, json);
                Console.WriteLine("Successfully created json configuration file");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured while trying to create the JSON configuration file: {ex.Message}");
            }

            return config;
        }

        Console.WriteLine($"Loading configuration from {configPath}");

        try
        {
            string json = File.ReadAllText(configPath);

            ServerConfig? deserialized = JsonSerializer.Deserialize<ServerConfig>(json);
            if (deserialized != null)
            {
                config = deserialized;
            }
            else
            {
                Console.WriteLine("Loaded configuration was empty, using default as fallback");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"The configuration was malformed, default configuration will be used. Error: {ex.Message}");
        }

        Console.WriteLine($"Using configuration for server: {config}");

        return config;
    }

    private static string? GetConfigPath()
    {
        try
        {
            return Path.Combine(Directory.GetCurrentDirectory(), Constants.ConfigFileName);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Access not given to read configuration file");
            return null;
        }
    }
}
