using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;

namespace QueotaCustomSounds;

[MinimumApiVersion(370)]
public class QueotaCustomSoundsPlugin : BasePlugin, IPluginConfig<QueotaCustomSoundsConfig>
{
    public override string ModuleName => "Queota Custom Sounds";
    public override string ModuleVersion => "0.0.2";
    public override string ModuleAuthor => "QUEOTA";

    public override string ModuleDescription =>
        "Custom sounds for a better CS2 experience on QUEOTA.club servers.";

    public QueotaCustomSoundsConfig Config { get; set; } = new QueotaCustomSoundsConfig();

    static QueotaCustomSoundsPlugin()
    {
        // ponytail: static self-check only; upgrade = real unit test if weapon aliases grow
        if (!IsZeusWeapon("taser") || !IsZeusWeapon("weapon_taser") || !IsZeusWeapon("ZEUS")
            || IsZeusWeapon("ak47"))
        {
            throw new InvalidOperationException("[QueotaCustomSounds] IsZeusWeapon self-check failed");
        }
    }

    public override void Load(bool hotReload)
    {
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
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var weapon = @event.Weapon;

        if (attacker is not { IsValid: true } || string.IsNullOrEmpty(weapon))
        {
            return HookResult.Continue;
        }

        if (!IsZeusWeapon(weapon))
        {
            return HookResult.Continue;
        }

        Server.PrintToConsole(
            $"[QueotaCustomSounds] Zeus kill by {attacker.PlayerName} ({attacker.SteamID}), weapon=\"{weapon}\".");

        // Chat is the reliable signal; sound is best-effort and must not gate the announce.
        AnnounceZeusKill();
        BroadcastZeusSound();

        return HookResult.Continue;
    }

    /// <summary>
    /// player_death.weapon is usually "taser"; accept weapon_taser / zeus too.
    /// </summary>
    internal static bool IsZeusWeapon(string weapon)
    {
        var w = weapon.Trim();
        if (w.StartsWith("weapon_", StringComparison.OrdinalIgnoreCase))
        {
            w = w[7..];
        }

        return w.Equals("taser", StringComparison.OrdinalIgnoreCase)
            || w.Equals("zeus", StringComparison.OrdinalIgnoreCase);
    }

    private static void AnnounceZeusKill()
    {
        Server.ExecuteCommand(
            "say ϟ ϟ ϟ Ta eM ShOcK ϟ ϟ ϟ, NeWbA?? PiSoU nO FiO, PaEzÃo??? AihH AiHH AhhDDHhhhh");
    }

    /// <summary>
    /// Get a random sound path from the config.
    /// </summary>
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
        var configured = GetRandomSound();
        if (string.IsNullOrEmpty(configured))
        {
            Server.PrintToConsole("[QueotaCustomSounds] No valid sound available to play for Zeus kill.");
            return;
        }

        // play via client command broke after CS updates (ExecuteClientCommand only runs
        // FCVAR_CLIENT_CAN_EXECUTE cmds). EmitSound needs the soundevent name from .vsndevts
        // (workshop generator uses the file basename, e.g. "lasier-martins").
        var soundEvent = ToSoundEventName(configured);

        var players = Utilities.GetPlayers();
        if (players == null || players.Count == 0)
        {
            Server.PrintToConsole("[QueotaCustomSounds] No players found to broadcast Zeus kill sound.");
            return;
        }

        var filter = new RecipientFilter();
        CCSPlayerController? emitter = null;
        foreach (var player in players)
        {
            if (player is not { IsValid: true })
            {
                continue;
            }

            filter.Add(player);
            emitter ??= player;
        }

        if (emitter == null || filter.Count == 0)
        {
            Server.PrintToConsole("[QueotaCustomSounds] No valid players to receive Zeus kill sound.");
            return;
        }

        Server.PrintToConsole(
            $"[QueotaCustomSounds] Playing Zeus kill soundevent \"{soundEvent}\" (config \"{configured}\") to {filter.Count} players.");

        emitter.EmitSound(soundEvent, filter);
    }

    /// <summary>
    /// Map config entry to a soundevent name.
    /// Accepts either "lasier-martins" or legacy "sounds/queota_sounds/lasier-martins.vsnd".
    /// </summary>
    private static string ToSoundEventName(string configured)
    {
        var name = configured.Replace('\\', '/').Trim();
        if (name.EndsWith(".vsnd", StringComparison.OrdinalIgnoreCase))
        {
            name = name[..^5];
        }

        var slash = name.LastIndexOf('/');
        return slash >= 0 ? name[(slash + 1)..] : name;
    }
}

public class QueotaCustomSoundsConfig : BasePluginConfig
{
    public List<string> Sounds { get; set; } = [];
}