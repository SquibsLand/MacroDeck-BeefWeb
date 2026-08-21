using MacroDeck.BeefWeb.ConfigFlow;
using MacroDeck.BeefWeb.Music;
using MacroDeck.BeefWeb.Music.API;
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

public sealed class BeefWebIntergration : IIntegration, IVariableProvider, IEventProvider,
    IConfigFlowProvider, IIntegrationIconProvider, IMusicPlayerProvider
{

    private const string PlayerID = "beefweb";
    private const string PlayerDisplayName = "BeefWeb";

    private readonly byte[] _icon;

    private readonly IPluginCatalogNotifier _catalogNotifier;
    private readonly ILogger _logger = Log.ForContext<BeefWebIntergration>();

    private IIntegrationContext? _context;

    public BeefWebIntergration(IPluginCatalogNotifier catalogNotifier,
        PluginMetadata metadata,
        IHostEnvironment environment)
    {
        _catalogNotifier = catalogNotifier;
        _icon = LoadIcon(metadata, environment);
        Player = new BeefWebPlayer();
        Actions = new BeefWebActions(this, ResolvePlayer, GetInstances).Get();
    }

    public const string IntegrationId = "app.macro-deck.beefweb";
    public string Id => IntegrationId;

    public string Name => "Sample";

    public string Version => "1.0.0";

    public bool IsInitialized { get; private set; }

    public IReadOnlyList<IActionDefinition> Actions { get; }

    internal BeefWebPlayer Player { get; }
    public bool VariablesDependOnConfiguration => true;

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

    // ----- IVariableProvider: pull-based, polled by the host on each variable's own interval. -----

    public IReadOnlyList<ProvidedVariable> ProvidedVariables => [.. BeefWebVaribles.Declare(PlayerID)];

    public Task<object?> GetValueAsync(string name, CancellationToken cancellationToken)
    {
        string prefix = BeefWebVaribles.prefix;
        string withoutPrefix = name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? name[prefix.Length..] : name;
        string variableName = withoutPrefix;

        var keyPrefix = $"{PlayerID}_";
        var remainder = withoutPrefix[keyPrefix.Length..];
        variableName = remainder;

        if (variableName.Contains("percentage"))
        {
            _logger.Debug("Stop Here");
        }

        return Task.FromResult(BeefWebVaribles.Get(variableName, Player.LastValidState));
    
    }

    // ----- IEventProvider: declares what this plugin can raise; publishing goes through IEventPublisher. -----

    public string ProviderName => "Sample";

    public IReadOnlyList<EventDefinition> EventDefinitions { get; } =
    [

    ];
    internal IReadOnlyList<ActionParameterOption> InstanceOptions()
        => [.. GetInstances().Select(instance => new ActionParameterOption { Value = instance.Id, Label = instance.DisplayName })];

    // ----- IConfigFlowProvider -----

    public IConfigFlow CreateConfigFlow() => new BeefWebConfigFlow();

    public bool AllowsMultipleConfigurations => false;

    // ----- IIntegrationIconProvider -----

    public string IconMimeType => "image/svg+xml";

    public byte[] GetIcon() => _icon;

    /// <summary>
    /// Reads the very file the manifest's "icon" declares, resolved the same way the SDK resolves it
    /// when it validates the manifest at startup: verbatim relative path, forward slashes, against the
    /// content root. Keeping the manifest as the single source means the icon exists once in the build
    /// output rather than once on disk and once more embedded in the assembly.
    /// </summary>
    private static byte[] LoadIcon(PluginMetadata metadata, IHostEnvironment environment)
    {
        if (string.IsNullOrEmpty(metadata.IconPath))
        {
            throw new InvalidOperationException(
                "The manifest declares no \"icon\", so IIntegrationIconProvider has nothing to serve.");
        }

        var path = Path.Combine(environment.ContentRootPath,
            metadata.IconPath.Replace('/', Path.DirectorySeparatorChar));

        return File.ReadAllBytes(path);
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

}
