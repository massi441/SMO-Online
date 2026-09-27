using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace SMOO.Server;

internal static class Configurator
{
    private static JsonSerializerOptions JsonOptions => new JsonSerializerOptions()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static ServerConfig Load()
    {
        ILogger logger = ServerLoggerFactory.Instance();

        ServerConfig config = new ServerConfig();

        string? configPath = GetConfigPath(logger);
        if (configPath == null)
        {
            return config;
        }

        if (!Path.Exists(configPath))
        {
            logger.LogInformation("No configuration found at {ConfigPath}, creating it.", configPath);
            try
            {
                string json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(configPath, json);
                logger.LogInformation("Successfully created JSON configuration file");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while trying to create the JSON configuration file");
            }

            return config;
        }

        logger.LogInformation("Loading configuration from {ConfigPath}", configPath);

        try
        {
            string json = File.ReadAllText(configPath);

            ServerConfig? deserialized = JsonSerializer.Deserialize<ServerConfig>(json, JsonOptions);
            if (deserialized != null)
            {
                config = deserialized;
            }
            else
            {
                logger.LogWarning("Loaded configuration was empty, using default as fallback");
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "The configuration was malformed, default configuration will be used");
        }

        logger.LogInformation("Using configuration for server: {Config}", config);

        return config;
    }

    private static string? GetConfigPath(ILogger logger)
    {
        try
        {
            return Path.Combine(Directory.GetCurrentDirectory(), Constants.ConfigFileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "Access not given to read configuration file");
            return null;
        }
    }
}