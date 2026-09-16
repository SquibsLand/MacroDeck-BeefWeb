using BeefWeb.ConfigFlow;
using BeefWeb.Music;
using BeefWeb.Music.API;
using BeefWeb.Music.API.Responses;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.ConfigFlow;
using MacroDeck.Sdk.Events;
using MacroDeck.Sdk.MusicPlayer;
using MacroDeck.Sdk.Variables;
using static BeefWeb.BeefWebVaribles.Variables;
using ApiPlayer = BeefWeb.Music.API.Responses.Player.Player;
using ILogger = Serilog.ILogger;

namespace BeefWeb;

public sealed class BeefWebIntergration : IPluginIntegration, IMusicPlayerProvider, IConfigFlowProvider, IVariableProvider, IEventProvider
{

    private const string PlayerID = "player";
    private const string PlayerDisplayName = "BeefWeb";

    private readonly ILogger _logger;

    private IIntegrationContext? _context;

    public BeefWebIntergration(ILogger logger)
    {
        
        _logger = logger.ForContext<BeefWebIntergration>();
        Player = new BeefWebPlayer(this);
        Actions = new BeefWebActions(this, ResolvePlayer, GetInstances).Get();
        Events = new BeefWebEvents(GetContext);
    }

    public const string IntegrationId = "app.macro-deck.beefweb";

    public bool IsInitialized { get; private set; }
    internal IReadOnlyList<ActionParameterOption> InstanceOptions() => [.. GetInstances().Select(instance => new ActionParameterOption { Value = instance.Id, Label = instance.DisplayName })];

    public IReadOnlyList<IActionDefinition> Actions { get; }

    internal BeefWebPlayer Player { get; init; }
    internal BeefWebEvents Events { get; init; }
    public static bool VariablesDependOnConfiguration => true;

    public IReadOnlyList<VariableDefinition> Variables => BeefWebVaribles.Declare(PlayerID);

    public IReadOnlyList<EventDefinition> EventDefinitions => Events.GetEvents();

    public Task<DynamicOptionsResult> GetEventOptionsAsync(EventOptionsContext context, CancellationToken cancellationToken)
        => Task.FromResult(new DynamicOptionsResult { Options = InstanceOptions() });

    public async Task InitializeAsync(IIntegrationContext context)
    {

        _context = context;

        // A real integration reads its config entries here, after the user has been through the flow
        // below - the same division of labor as SpotifyIntegration.ConnectFromConfig. The sample keeps
        // to a single entry: the location name typed into the config flow's one field.
        var entries = await context.Config.GetEntriesAsync();
 
        if (entries.Count > 0)
        {

            var address = await context.Config.GetStringAsync(entries[0].Id, BeefWebConfigFlow.ServerFieldName);
            string? portString = await context.Config.GetStringAsync(entries[0].Id, BeefWebConfigFlow.PortFieldName);
            var type = await context.Config.GetStringAsync(entries[0].Id, BeefWebConfigFlow.PlayerTypeFieldName);

            if (!string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(portString) && !string.IsNullOrWhiteSpace(type) && int.TryParse(portString, out int port))
            {
                if (Enum.TryParse<PlayerType>(type, out PlayerType playerType))
                {
                    Player.init(address, port, playerType);
                }
                else
                {
                    _logger.Error($"Uknown player type of {type}, defaulting to Foobar");
                    Player.init(address, port, PlayerType.FOOBAR);
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

    internal IIntegrationContext? GetContext() => _context;

    public async ValueTask<VariableReading> ReadAsync(string name, CancellationToken cancellationToken = default)
    {
        string prefix = BeefWebVaribles.prefix;
        string withoutPrefix = name ?? string.Empty;
        if (withoutPrefix.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            withoutPrefix = withoutPrefix[prefix.Length..];
       
        string variableName = withoutPrefix;

        string keyPrefix = $"{PlayerID}-";

        if (withoutPrefix.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase)) 
            variableName = withoutPrefix[keyPrefix.Length..];

        IVariable? variable = BeefWebVaribles.TryGetVariable(variableName);
        if (variable is null) return VariableReading.Unavailable;
        Type dataType = variable.DataType;
        return variable.DataType switch
        {
            var t when t == typeof(ApiPlayer) && Player.LastPlayer is ApiPlayer player =>
                BeefWebVaribles.TryGet(variableName, player),
            var t when t == typeof(MusicPlayerState) =>
                BeefWebVaribles.TryGet(variableName, Player.LastValidState),
            var t when t == typeof(PlayQueueItem[]) =>
                BeefWebVaribles.TryGet(variableName, Player.LastPlayQueue),
            _ => VariableReading.Unavailable
        };
    }
}
