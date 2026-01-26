using System.IO;
using System.Text.Json;

namespace CS2Cheat.Utils;

public class ConfigManager
{
    private const string ConfigFile = "config.json";
    public bool BombTimer { get; set; }
    public bool EspAimCrosshair { get; set; }
    public bool EspBox { get; set; }
    public bool EspHealthBar { get; set; }   
    public bool EspPlayerName { get; set; }  
    public bool SkeletonEsp { get; set; }
    public bool TeamCheck { get; set; }
    public bool EspFlags { get; set; }
    public bool EspAmmoBar { get; set; }
    public bool DrawWeaponText { get; set; }
    public bool DrawWeaponIcon { get; set; }
    public bool EspSound { get; set; }
    public bool EspChams { get; set; }
    public bool EspSnaplines { get; set; }

   








    public static ConfigManager Load()
    {
        try
        {
            if (!File.Exists(ConfigFile))
            {
                var defaultOptions = Default();
                Save(defaultOptions);
                return defaultOptions;
            }

            var json = File.ReadAllText(ConfigFile);
            return JsonSerializer.Deserialize<ConfigManager>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Default();
        }
        catch (JsonException) { return Default(); }
    }

    public static void Save(ConfigManager options)
    {
        try
        {
            var json = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFile, json);
        }
        catch { }
    }

    public static ConfigManager Default()
    {
        return new ConfigManager
        {
            BombTimer = false,
            EspAimCrosshair = false,
            EspBox = true,
            EspHealthBar = true,
            EspPlayerName = true,   
            SkeletonEsp = true,
            TeamCheck = true,
            EspFlags = true,
            DrawWeaponText = false,
            DrawWeaponIcon = true,
            EspAmmoBar = false,
            EspSound = true,
            EspChams = false,
            EspSnaplines = false,
        };
    }
}