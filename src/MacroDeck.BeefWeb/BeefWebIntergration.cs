using MacroDeck.BeefWeb.ConfigFlow;
using MacroDeck.BeefWeb.Music;
using MacroDeck.BeefWeb.Music.API;
using MacroDeck.Localization;
using MacroDeck.Plugin.Hosting;
using MacroDeck.Plugin.Hosting.Integrations;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.ConfigFlow;
using MacroDeck.Sdk.Events;
using MacroDeck.Sdk.MusicPlayer;
using MacroDeck.Sdk.Variables;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroDeck.BeefWeb;

public sealed class BeefWebIntergration : IPluginIntegration, IMusicPlayerProvider, IConfigFlowProvider, IVariableProvider
{

    private const string PlayerID = "beefweb";
    private const string PlayerDisplayName = "BeefWeb";

    public IReadOnlyList<ProvidedVariable> ProvidedVariables => BeefWebVaribles.Declare(PlayerID);

    private readonly ILogger _logger;

    private IIntegrationContext? _context;

    public BeefWebIntergration(ILogger logger)
    {
        
        _logger = logger.ForContext<BeefWebIntergration>();
        Player = new BeefWebPlayer();
        Actions = new BeefWebActions(this, ResolvePlayer, GetInstances).Get();
    }

    public const string IntegrationId = "app.macro-deck.beefweb";

    public bool IsInitialized { get; private set; }
    internal IReadOnlyList<ActionParameterOption> InstanceOptions() => [.. GetInstances().Select(instance => new ActionParameterOption { Value = instance.Id, Label = instance.DisplayName })];

    public IReadOnlyList<IActionDefinition> Actions { get; }

    internal BeefWebPlayer Player { get; init; }
    public static bool VariablesDependOnConfiguration => true;

    public async Task InitializeAsync(IIntegrationContext context)
    {

        _context = context;

        // A real integration reads its config entries here, after the user has been through the flow
        // below - the same division of labor as SpotifyIntegration.ConnectFromConfig. The sample keeps
        // to a single entry: the location name typed into the config flow's one field.
        var entries = await context.Config.GetEntriesAsync();
        _logger.Error($"{entries[0].Title}");
        if (entries.Count > 0)
        {

            var address = await context.Config.GetStringAsync(entries[0].Id, BeefWebConfigFlow.ServerFieldName);
            string? portString = await context.Config.GetStringAsync(entries[0].Id, BeefWebConfigFlow.PortFieldName);
            var type = await context.Config.GetStringAsync(entries[0].Id, BeefWebConfigFlow.PlayerTypeFieldName);

            if (!string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(portString) && !string.IsNullOrWhiteSpace(type))
            {
                if (Enum.TryParse<PlayerType>(type, out PlayerType playerType))
                {

                }
                else
                {
                    _logger.Error($"Uknown player type of {type}, defaulting to Foobar");
                    Player.init(address, int.Parse(portString), PlayerType.FOOBAR);
                }
            }
            else
            {
                IsInitialized = false;
                _logger.Error($"Beefweb failed to init");
            }

        }

        IsInitialized = true;

        _logger.Information("BeefWeb Integration Initialized");
    }

    public Task ShutdownAsync()
    {
        IsInitialized = false;
        _context = null;
        return Task.CompletedTask;
    }

    public async Task<object?> GetValueAsync(string name, CancellationToken cancellationToken)
    {
        string prefix = BeefWebVaribles.prefix;
        string withoutPrefix = name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? name[prefix.Length..] : name;
        string variableName = withoutPrefix;

        var keyPrefix = $"{PlayerID}_";
        var remainder = withoutPrefix[keyPrefix.Length..];
        variableName = remainder;

        if (Player is null) return null;
        return BeefWebVaribles.Get(variableName, Player.LastValidState);

    }

    // ----- IMusicPlayerProvider -----
    public IMusicPlayer? GetPlayer(string instanceId) => string.Equals(instanceId, PlayerID, StringComparison.Ordinal) ? Player : null;

    public IReadOnlyList<MusicPlayerInstance> GetInstances() => [new MusicPlayerInstance(PlayerID, PlayerDisplayName)];
    private IMusicPlayer? ResolvePlayer(string? instanceId)
        => GetPlayer(PlayerID);
    internal async Task<IReadOnlyList<ActionParameterOption>?> GetPlaylistOptions()
    {
        if (Player == null) return null;
        var playlists = await Player.GetPlaylists();
        if (playlists is null) return null;
        List<ActionParameterOption> options = [];
        foreach (var item in playlists)
        {
            options.Add(new ActionParameterOption { Value = item.Id, Label = item.Title });
        }
        return options;
    }
    internal async Task<IReadOnlyList<ActionParameterOption>?> GetPlaylistItemsOptions(string pid)
    {
        if (Player is null) return null;
        var items = await Player.GetPlaylistItems(pid);
        if (items is null) return null;
        List<ActionParameterOption> options = [];
        foreach (var item in items)
        {
            options.Add(new ActionParameterOption { Value = item.Id, Label = item.Title });
        }
        return options;
    }

    public IConfigFlow CreateConfigFlow() => new BeefWebConfigFlow();
}
