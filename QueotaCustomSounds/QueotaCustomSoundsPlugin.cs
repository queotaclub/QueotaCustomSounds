using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;

namespace QueotaCustomSounds;

[MinimumApiVersion(80)]
public class QueotaCustomSoundsPlugin : BasePlugin, IPluginConfig<QueotaCustomSoundsConfig>
{
    public override string ModuleName => "Queota Custom Sounds";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "QUEOTA";

    public override string ModuleDescription =>
        "Custom sounds for a better CS2 experience on QUEOTA.club servers.";

    public required QueotaCustomSoundsConfig Config { get; set; }

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
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
        Config = config;
        Server.PrintToConsole($"Found {Config.Sounds.Count} sounds!");
    }

    public override void Unload(bool hotReload)
    {
        DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
        Server.PrintToConsole("Unloaded Queota Custom Sounds Plugin!");
    }

    /// <summary>
    /// Handle the round end event.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (ShouldSkipQueotaCustomSounds())
        {
            return HookResult.Continue;
        }

        var players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            var sound = GetSoundPathForPlayer(player, (CsTeam)@event.Winner);
            PlaySoundForPlayer(player, sound);
        }

        return HookResult.Continue;
    }

    /// <summary>
    /// Config is empty, or no sounds are set, skip playing the end sounds.
    /// </summary>
    /// <returns>Config is invalid</returns>
    private bool ShouldSkipQueotaCustomSounds() => (Config.Sounds.Count == 0);

    /// <summary>
    /// Get a random sound path from the config.
    /// </summary>
    /// <param name="player">Player that sounds needs to play for</param>
    /// <param name="winningTeam">Winning team of the round</param>
    private string GetSoundPathForPlayer(CCSPlayerController player, CsTeam winningTeam)
    {
        return Config.Sounds[Random.Shared.NextDistinct(Config.Sounds.Count)];
    }

    /// <summary>
    /// Send a client command to play a sound from a workshop item for the given player.
    /// Check the README.md of this project for more info about creating the necessary workshop item.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="path">Path to sound file in workshop items</param>
    private static void PlaySoundForPlayer(CCSPlayerController player, string path) =>
        player.ExecuteClientCommand($"play \"{path}\"");
    }

public class QueotaCustomSoundsConfig : BasePluginConfig
{
    public List<string> Sounds { get; set; } = [];
}