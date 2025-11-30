using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
namespace QueotaCustomSounds;

[MinimumApiVersion(338)]
public class QueotaCustomSoundsPlugin : BasePlugin, IPluginConfig<QueotaCustomSoundsConfig>
{
    public override string ModuleName => "Queota Custom Sounds";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "QUEOTA";

    public override string ModuleDescription =>
        "Custom sounds for a better CS2 experience on QUEOTA.club servers.";

    public QueotaCustomSoundsConfig Config { get; set; } = new QueotaCustomSoundsConfig();

    public override void Load(bool hotReload)
    {
        // Hook death event
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        
        Server.PrintToConsole(@"
  __   _  _  ____  __  ____  __      ___  __    _  _  ____ 
 /  \ / )( \(  __)/  \(_  _)/ _\    / __)(  )  / )( \(  _ \
(  O )) \/ ( ) _)(  O ) )( /    \ _( (__ / (_/\) \/ ( ) _ (
 \__\)\____/(____)\__/ (__)\_/\_/(_)\___)\____/\____/(____/
 
Loaded Queota Custom Sounds Plugin!
");
    }

    public void OnConfigParsed(QueotaCustomSoundsConfig config)
    {
        Config = config ?? new QueotaCustomSoundsConfig();
        Server.PrintToConsole($"Found {Config.Sounds?.Count ?? 0} sounds!");
    }

    public override void Unload(bool hotReload)
    {
        DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Server.PrintToConsole("Unloaded Queota Custom Sounds Plugin!");
    }

    /// <summary>
    /// Handle the player death event to detect Zeus kills.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        Server.PrintToConsole("[QueotaCustomSounds] Player death event detected.");
        if (ShouldSkipQueotaCustomSounds())
        {
            Server.PrintToConsole("[QueotaCustomSounds] Skipping sound because config is empty or no sounds are set.");
            return HookResult.Continue;
        }

        var attacker = @event.Attacker;
        var weapon = @event.Weapon;

        if (attacker == null || string.IsNullOrEmpty(weapon))
        {
            Server.PrintToConsole("[QueotaCustomSounds] No attacker or weapon, skipping sound.");
            return HookResult.Continue;
        }

        // Check if kill was with Zeus (weapon_taser)
        if (weapon == "taser")
        {
            BroadcastZeusSound();
        } else {
            Server.PrintToConsole("[QueotaCustomSounds] Not a Zeus kill, skipping sound.");
            Server.PrintToConsole($"[QueotaCustomSounds] Attacker: {attacker.PlayerName} ({attacker.SteamID})");
            Server.PrintToConsole($"[QueotaCustomSounds] Weapon: {weapon}");
        }

        return HookResult.Continue;
    }

    /// <summary>
    /// Config is empty, or no sounds are set, skip playing sounds.
    /// </summary>
    /// <returns>Config is invalid</returns>
    private bool ShouldSkipQueotaCustomSounds() => (Config?.Sounds == null || Config.Sounds.Count == 0);

    /// <summary>
    /// Get a random sound path from the config.
    /// </summary>
    /// <returns>Random sound path or empty string if no sounds available</returns>
    private string GetRandomSound()
    {
        if (Config?.Sounds == null || Config.Sounds.Count == 0)
        {
            return string.Empty;
        }
        return Config.Sounds[Random.Shared.NextDistinct(Config.Sounds.Count)];
    }

    /// <summary>
    /// Broadcast Zeus kill sound to all players.
    /// </summary>
    private void BroadcastZeusSound()
    {
        var soundPath = GetRandomSound();
        if (string.IsNullOrEmpty(soundPath))
        {
            Server.PrintToConsole("[QueotaCustomSounds] No valid sound available to play for Zeus kill.");
            return;
        }

        var players = Utilities.GetPlayers();
        if (players == null)
        {
            Server.PrintToConsole("[QueotaCustomSounds] No players found to broadcast Zeus kill sound.");
            return;
        }

        Server.PrintToConsole($"[QueotaCustomSounds] Playing Zeus kill sound \"{soundPath}\" to {players.Count} players.");

        foreach (var player in players)
        {
            if (player is { IsValid: true })
            {
                // Emit sound to each player
                // player.EmitSound(soundPath);
                Server.PrintToConsole($"[QueotaCustomSounds] Sending sound \"{soundPath}\" to player {player.PlayerName} ({player.SteamID})");
                player.ExecuteClientCommand($"play \"{soundPath}\"");
            }
        }
    }
}

public class QueotaCustomSoundsConfig : BasePluginConfig
{
    public List<string> Sounds { get; set; } = [];
}