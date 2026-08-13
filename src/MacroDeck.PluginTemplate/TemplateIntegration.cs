using MacroDeck.Plugin.Hosting;
using MacroDeck.Plugin.Hosting.Integrations;
using MacroDeck.Plugin.Protocol.Handshake;
using MacroDeck.PluginTemplate.Actions;
using MacroDeck.PluginTemplate.ConfigFlow;
using MacroDeck.PluginTemplate.Weather;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.ConfigFlow;
using MacroDeck.Sdk.Events;
using MacroDeck.Sdk.Variables;
using MacroDeck.Sdk.Weather;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroDeck.PluginTemplate;

/// <summary>
/// The worked example for issue #413's plugin capability parity: one integration exercising actions
/// (plain, slider, dynamic options), variables, events, a config flow and a provider capability
/// (weather). Every piece is wired to the others rather than standing alone - see the file-level
/// README for the map - so running the sample against a real host shows one coherent plugin, not five
/// disconnected stubs.
/// </summary>
public sealed class TemplateIntegration : IIntegration, IVariableProvider, IEventProvider,
	IConfigFlowProvider, IWeatherProvider, IIntegrationIconProvider
{
	/// <summary>The sample's one weather station instance id - see <see cref="GetInstances"/>.</summary>
	private const string StationId = "primary";

	private const string WeatherRefreshedEventId = "weather-refreshed";

	/// <summary>
	/// The conditions the "force weather condition" action lets a user pick, out of every value
	/// <see cref="WeatherCondition"/> declares - curated down to a handful so a demo deck's picker
	/// stays short rather than listing all thirteen.
	/// </summary>
	internal static readonly IReadOnlyList<WeatherCondition> SelectableConditions =
	[
		WeatherCondition.Clear, WeatherCondition.PartlyCloudy, WeatherCondition.Overcast,
		WeatherCondition.Rain, WeatherCondition.Thunderstorm, WeatherCondition.Snow
	];

	private readonly byte[] _icon;

	private readonly IPluginCatalogNotifier _catalogNotifier;
	private readonly ILogger _logger = Log.ForContext<TemplateIntegration>();

	private IIntegrationContext? _context;

	// Constructor injection, the same as IPluginCatalogNotifier above: MacroDeck.Plugin.Serilog's
	// UseMacroDeckLogging() (see Program.cs) is what routes this ILogger<T> - and MacroDeckSdk.Logging's
	// IntegrationLog, and ordinary Serilog APIs - to the host's log viewer over log.publish.
	public TemplateIntegration(IPluginCatalogNotifier catalogNotifier,
		PluginMetadata metadata,
		IHostEnvironment environment)
	{
		_catalogNotifier = catalogNotifier;
		_icon = LoadIcon(metadata, environment);
		Station = new SampleWeatherStation(this);
		Actions = [new RefreshWeatherAction(this), new SetAlertThresholdAction(this), new SetConditionAction(this)];
	}

	public string Id => "app.macro-deck.sample";

	public string Name => "Sample";

	public string Version => "1.0.0";

	public bool IsInitialized { get; private set; }

	public IReadOnlyList<IActionDefinition> Actions { get; }

	/// <summary>The current location name, shown in the weather snapshot and the location variable.
	/// Defaults so the sample already has something plausible to show before it is ever configured.</summary>
	internal string LocationName { get; private set; } = "Berlin, Germany";

	/// <summary>The threshold <see cref="SampleWeatherStation.Tick"/> compares a fresh reading
	/// against, set by the "set alert threshold" slider action.</summary>
	internal double AlertThresholdCelsius { get; set; } = 30;

	internal SampleWeatherStation Station { get; }

	public async Task InitializeAsync(IIntegrationContext context)
	{
		_context = context;

		// A real integration reads its config entries here, after the user has been through the flow
		// below - the same division of labor as SpotifyIntegration.ConnectFromConfig. The sample keeps
		// to a single entry: the location name typed into the config flow's one field.
		var entries = await context.Config.GetEntriesAsync();
		if (entries.Count > 0)
		{
			var stored = await context.Config.GetStringAsync(entries[0].Id, SampleConfigFlow.LocationFieldName);
			if (!string.IsNullOrWhiteSpace(stored))
			{
				LocationName = stored;
			}
		}

		// Seeds a first reading so the weather widget and the temperature variable already have
		// something to show before anyone presses "refresh weather".
		Station.Tick();
		IsInitialized = true;

		// The bug this sample exists to demonstrate the fix for (issue #413): the host sends weather's
		// describe concurrently with this method running, so a first-ever describe can win the race and
		// capture LocationName's Berlin default before the config read above ever ran - leaving the
		// weather widget's location dropdown (the host's stale snapshot) disagreeing with both the
		// widget itself and the config card (a live read and the just-submitted value). Telling the host
		// both catalogues are stale, now that LocationName is final, is what makes all three agree -
		// weather because GetInstances() below carries the location, variables because
		// "sample_location" does too.
		_catalogNotifier.CatalogChanged(CapabilityKinds.Weather, reason: "location config applied");
		_catalogNotifier.CatalogChanged(CapabilityKinds.Variables, reason: "location config applied");
		_logger.Information("Template Integration Initialized");
	}

	public Task ShutdownAsync()
	{
		IsInitialized = false;
		_context = null;
		return Task.CompletedTask;
	}

	// ----- IVariableProvider: pull-based, polled by the host on each variable's own interval. -----

	public IReadOnlyList<ProvidedVariable> ProvidedVariables { get; } =
	[
		new ProvidedVariable("sample_location", VariableType.Text) { DefinitionId = "location" },
		new ProvidedVariable("sample_temperature_celsius", VariableType.Numeric, DecimalPlaces: 1)
		{
			DefinitionId = "temperature-celsius"
		}
	];

	public Task<object?> GetValueAsync(string name, CancellationToken cancellationToken)
		=> Task.FromResult(name switch
		{
			"sample_location" => (object?)LocationName,
			"sample_temperature_celsius" => Station.LastSnapshot.Temperature,
			_ => null
		});

	// ----- IEventProvider: declares what this plugin can raise; publishing goes through IEventPublisher. -----

	public string ProviderName => "Sample";

	public IReadOnlyList<EventDefinition> EventDefinitions { get; } =
	[
		new EventDefinition
		{
			Id = WeatherRefreshedEventId,
			Name = "Weather refreshed",
			Description = "Raised whenever the sample's synthetic weather reading changes.",
			PayloadParameters =
			[
				ActionParameter.Number("temperatureCelsius", "Temperature (°C)"),
				ActionParameter.Text("condition", "Condition"),
				ActionParameter.Toggle("isAlert", "Above alert threshold")
			]
		}
	];

	/// <summary>
	/// Called by <see cref="SampleWeatherStation.Tick"/> after it computes a fresh reading.
	/// <see cref="IEventPublisher.Publish"/> is fire-and-forget by contract, so this mirrors that and
	/// never throws; <see cref="_context"/> is only null if something calls this before
	/// <see cref="InitializeAsync"/> has run or after <see cref="ShutdownAsync"/> has, neither of which
	/// a normally driven plugin process ever does.
	/// </summary>
	internal void PublishWeatherRefreshed(WeatherSnapshot snapshot, bool isAlert)
		=> _context?.Events.Publish(WeatherRefreshedEventId, new Dictionary<string, object?>
		{
			["temperatureCelsius"] = snapshot.Temperature,
			["condition"] = snapshot.Condition.ToString(),
			["isAlert"] = isAlert
		});

	// ----- IConfigFlowProvider -----

	public IConfigFlow CreateConfigFlow() => new SampleConfigFlow();

	public bool AllowsMultipleConfigurations => false;

	// ----- IWeatherProvider: the provider capability, deliberately the synchronous-snapshot kind. -----

	public IReadOnlyList<WeatherStationInstance> GetInstances() => [new WeatherStationInstance(StationId, LocationName)];

	public IWeatherStation? GetStation(string instanceId)
		=> string.Equals(instanceId, StationId, StringComparison.Ordinal) ? Station : null;

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
}
