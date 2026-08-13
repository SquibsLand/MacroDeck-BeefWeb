# Macro Deck plugin template

A starting point for an out-of-process Macro Deck 3 plugin. It builds against the published SDK
packages (`MacroDeck.Sdk`, `MacroDeck.Plugin.Hosting`, `MacroDeck.Plugin.Serilog`), so you do not need
the Macro Deck source tree to build it - only to run a host against it during development.

The template exercises one integration across every capability kind: three actions (plain, slider,
dynamic options), variables, an event, a config flow, a weather station and an icon.

## Requirements

- .NET SDK 10.0
- For the [full dev loop](#running-against-a-real-host): a running Macro Deck 3 host - either a
  development host from the [Macro Deck 3 repository](https://github.com/Macro-Deck-App/Macro-Deck-3),
  or an installed desktop app

You do **not** need Macro Deck installed to build, run or test a plugin - the
[CLI](#the-developer-cli) runs one against a disposable stub host.

## Quick start

```bash
dotnet build
dotnet tool install --global MacroDeck.Plugin.Cli --prerelease
macrodeck-plugin run --project src/MacroDeck.PluginTemplate
```

That launches the plugin against a disposable stub host, composing the environment exactly the way the
real supervisor does. Ctrl-C runs the documented shutdown sequence.

## Running against a real host

The plugin registers itself with a running host rather than being launched by it, so the host has to
be up first. Self-registration only works against a host on the **same machine**: the plugin endpoints
are local-only by design.

### 1. Start the host and the UI

From your Macro Deck 3 checkout:

```bash
dotnet run --project host/src/MacroDeckHost
```

A development host listens on `7193` (public) and `5191` (trusted loopback). An installed desktop app
uses `8193` instead - use that port below if you are developing against one, and skip to step 3, since
it brings its own UI.

Then start the configuration UI, from `ui/angular`:

```bash
npm install && npm run start
```

It serves on `http://localhost:4200` and proxies to the host's loopback port. Connections over
loopback are implicitly admin, so no login is needed in development.

### 2. Create a Developer token

In the UI, go to **Developer Tools → Plugin tokens** and press **Create token**. Copy the plaintext
value - it is shown once.

### 3. Start the plugin

```bash
dotnet build
cd src/MacroDeck.PluginTemplate/bin/Debug/net10.0

MACRO_DECK_PLUGIN_MODE=SelfRegistering \
MACRO_DECK_PLUGIN_HOST_URL=http://127.0.0.1:7193 \
MACRO_DECK_PLUGIN_ENROLLMENT_TOKEN=<your token> \
./MacroDeck.PluginTemplate
```

On PowerShell:

```powershell
$env:MACRO_DECK_PLUGIN_MODE = "SelfRegistering"
$env:MACRO_DECK_PLUGIN_HOST_URL = "http://127.0.0.1:7193"
$env:MACRO_DECK_PLUGIN_ENROLLMENT_TOKEN = "<your token>"
.\MacroDeck.PluginTemplate.exe
```

Run it from the build output directory: the SDK reads `manifest.json` from the content root, and the
manifest's icon path is resolved against it. `macrodeck-plugin run --host-url ... --mode
self-registering --enrollment-token ...` does the same thing without the manual environment.

### 4. Configure it

The integration appears under **Integrations** as `PluginTemplate`, marked `EXTERNAL`. It starts
disabled because it provides a config flow - open it, enter a location name, and it enables itself.
Its variables (`sample_location`, `sample_temperature_celsius`) then carry values, and its weather
station can be pointed at a Weather widget.

### Later runs

The enrollment token is only needed the first time. The plugin exchanges it for a secret and persists
that, so afterwards this is enough:

```bash
MACRO_DECK_PLUGIN_MODE=SelfRegistering \
MACRO_DECK_PLUGIN_HOST_URL=http://127.0.0.1:7193 \
./MacroDeck.PluginTemplate
```

The secret is stored per plugin id under the platform state directory:

| Platform | Location |
| --- | --- |
| Windows | `%LOCALAPPDATA%\MacroDeck\plugins\<pluginId>\credentials.json` |
| macOS | `~/Library/Application Support/MacroDeck/plugins/<pluginId>/credentials.json` |
| Linux | `$XDG_STATE_HOME/macro-deck/plugins/<pluginId>/credentials.json` |

Delete that file to force a fresh enrollment. `MACRO_DECK_PLUGIN_STATE_DIRECTORY` overrides the
location.

## How a plugin is put together

### Project layout

```
src/MacroDeck.PluginTemplate/
  Program.cs              the host builder - three lines and a RunAsync
  manifest.json           identity, icon and per-platform entrypoints
  TemplateIntegration.cs  the integration: identity, lifecycle, capabilities
  Actions/                one class per action
  ConfigFlow/             the setup wizard
  Weather/                the weather station this template's capability serves
  Assets/icon.svg         the icon, served through IIntegrationIconProvider
```

### The entry point

`MacroDeckPlugin.CreatePlugin(args)` wraps `WebApplication.CreateBuilder`, so everything an ASP.NET
Core application has is available - configuration, options binding, `IHttpClientFactory`, hosted
services, dependency injection:

```csharp
var plugin = MacroDeckPlugin.CreatePlugin(args)
    .UseMacroDeckLogging()
    .ConfigureServices((_, services) => services.AddMacroDeckIntegration<TemplateIntegration>())
    .Build();

await plugin.RunAsync();
```

Integrations are constructed by DI, so yours can take `IHttpClientFactory`, `IOptions<T>`, Serilog's
`ILogger` or `PluginMetadata` in its constructor. `UseMacroDeckLogging()` routes your log output to the
host's log viewer.

### The manifest

`manifest.json` is the plugin's identity, read from the content root at startup. `Build()` validates it
and fails fast on an invalid id, a missing name or version, or an unsafe/missing icon path.

```json
{
  "manifestVersion": 1,
  "id": "app.macro-deck.template",
  "name": "PluginTemplate",
  "version": "1.0.0",
  "description": "A template for creating Macro Deck plugins.",
  "icon": "Assets/icon.svg",
  "entrypoints": {
    "win-x64": { "executable": "MacroDeck.PluginTemplate.exe" },
    "osx-arm64": { "executable": "MacroDeck.PluginTemplate" },
    "linux-x64": { "executable": "MacroDeck.PluginTemplate" }
  }
}
```

A manifest may also declare `permissions`, `dependencies`, `conflicts`, `iconPacks`, `compatibility`
and `files[]` - `macrodeck-plugin inspect` reports all of them, and `pack` recomputes `files[]` for
you.

### Capabilities

An integration implements `IIntegration` for identity, lifecycle and actions, then **opts into**
capability interfaces. The host discovers each one by filtering on the interface, so you only implement
what you need.

| Capability | Interface | In this template |
| --- | --- | --- |
| Actions | `IActionDefinition` (on the base contract) | `Actions/` - three of them |
| Config flow | `IConfigFlowProvider` | `ConfigFlow/TemplateConfigFlow.cs` |
| Variables | `IVariableProvider` | `sample_location`, `sample_temperature_celsius` |
| Events | `IEventProvider` | `weather-refreshed` |
| Weather | `IWeatherProvider` | `Weather/SampleWeatherStation.cs` |
| Icon | `IIntegrationIconProvider` | `Assets/icon.svg` |
| Music players | `IMusicPlayerProvider` | - |
| Virtual profiles | `IProfileProvider` | - |
| Issues | `IIntegrationIssueProvider` | - |

An integration that provides a config flow starts **disabled** until the user configures it; everything
else defaults to enabled.

Capability ids are namespaced by the host as `integrationId::localId`, so you declare provider-local
ids and never the qualified form.

### Actions

Three shapes, one class each:

- **Plain** - `RefreshWeatherAction`: parameters in, work done, nothing returned.
- **Slider** - `SetAlertThresholdAction` implements `ISliderActionDefinition`, so a Slider widget binds
  to it two-way. `CommitOnRelease` suppresses intermediate drag values when they would be disruptive.
- **Dynamic options** - `SetConditionAction` implements `IDynamicOptionsActionDefinition` and supplies
  its dropdown choices at edit time instead of declaring them statically.

Forward `context.CancellationToken` from `ExecuteAsync` into anything you await - the analyzers warn
(MDP3001) when you drop it.

### Configuration

A config flow is a Home Assistant style wizard that produces configuration entries. Secrets are stored
as `ConfigFlowValue.Secret` and kept in the host's encrypted secret store, never in plain text. At
runtime, `IIntegrationContext.Config` reads the entries back - see `InitializeAsync` in
`TemplateIntegration`.

## The developer CLI

`macrodeck-plugin` validates, inspects, packs, runs and conformance-tests a plugin without Macro Deck
installed.

```bash
dotnet tool install --global MacroDeck.Plugin.Cli --prerelease
```

`--prerelease` is required while the 3.0 SDK is in preview: only preview versions are published, and
`dotnet tool install` picks stable ones by default. Drop it once 3.0 ships.

The tool needs the **ASP.NET Core shared framework**, not just the .NET runtime - its stub host is a
real Kestrel server.

| Command | What it does |
| --- | --- |
| `validate` | Checks a manifest, version directory or artifact against the real manifest reader, the JSON Schema, the permission vocabulary and declared file digests. |
| `inspect` | Reports what installing an artifact would find - entrypoints, permissions, dependencies, conflicts, compatibility, signature shape, size. |
| `pack` | Builds a `.macroDeckPlugin` artifact, validating the manifest first and recomputing `files[]` digests. |
| `run` | Launches the plugin the way the supervisor does, against a stub host or a real one. |
| `test` | Runs the conformance suite and writes a text, JSON or Markdown report. |

### Packing a release

```bash
dotnet build -c Release
macrodeck-plugin validate --manifest src/MacroDeck.PluginTemplate/bin/Release/net10.0/manifest.json
macrodeck-plugin pack --source src/MacroDeck.PluginTemplate/bin/Release/net10.0 \
                      --output PluginTemplate-1.0.0.macroDeckPlugin
macrodeck-plugin inspect --artifact PluginTemplate-1.0.0.macroDeckPlugin
```

`pack` validates before it writes, so a bad manifest never becomes an artifact. It discards whatever
`files[]` the source manifest declared and recomputes every digest from disk. It cannot sign anything:
sign *after* packing, against the packed manifest, or the digest will not match.

`--output` defaults to `<id>-<version>.macroDeckPlugin`; `--force` overwrites an existing file.

### Conformance

```bash
macrodeck-plugin test --project src/MacroDeck.PluginTemplate --report markdown --output conformance.md
```

The suite drives a real session against your plugin: capability contracts, invocation and cancellation
semantics, reconnect and resume behaviour, the reserved `/_macrodeck/*` endpoints, and logging limits.
Checks are Required or Recommended, each with a stable id (`MDC0401`, …) you can select with `--check`
or `--category`. A check can report `SKIP` with a reason when your plugin gives it nothing to observe.

Exit codes make it usable as a CI gate - `0` conformant, `1` the plugin is wrong, `2` usage error, `3`
input unreadable, `4` cancelled. `1` and `3` are deliberately distinct: a missing file is an
environment problem, not a verdict about the plugin.

## Testing

```bash
dotnet test
```

The test project references `MacroDeck.Plugin.Testing`, which provides a loopback test host, fakes and
assertions for testing a plugin without a running Macro Deck. What ships here is a placeholder test -
add a project reference to `MacroDeck.PluginTemplate` before writing tests against your integration.

The conformance suite above covers the protocol contract; these tests are for your own behaviour.

## Making it your own

1. Change the identity in `manifest.json` (`id`, `name`, `version`, `description`) and in
   `TemplateIntegration` (`Id`, `Name`, `Version`). The manifest id and the integration id are separate
   values - the host registers the plugin under the manifest id.
2. Replace `Assets/icon.svg`. It is served through `IIntegrationIconProvider`, which reads the file the
   manifest declares, so the manifest path stays the single source of truth.
3. Delete the capabilities you do not need - each is an interface on `TemplateIntegration` plus the
   members implementing it.
4. Rename the variables and the event id to your own namespace.

## License

MIT - see [LICENSE](LICENSE). Macro Deck itself is licensed under Apache 2.0.

## Further reading

- [Plugin development docs](https://github.com/Macro-Deck-App/Macro-Deck-3/tree/main/docs/plugin-development)
- [`plugin-hosting.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/plugin-hosting.md) - the builder API, registration modes, the artifact format and every `MACRO_DECK_PLUGIN_*` variable
- [`sdk-reference.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/sdk-reference.md) - every interface and record you build against
- [`cli.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/cli.md) - every CLI command and option
- [`conformance.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/conformance.md) - the conformance suite and its check ids
- [`analyzers.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/analyzers.md) - the compile-time diagnostics
