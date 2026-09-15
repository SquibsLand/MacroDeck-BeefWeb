using BeefWeb;
using MacroDeck.Plugin.Hosting;
using MacroDeck.Plugin.Hosting.DependencyInjection;
using MacroDeck.Plugin.Serilog;

var plugin = MacroDeckPlugin.CreatePlugin(args)
    .UseMacroDeckLogging()
    .RegisterIntegration<BeefWebIntergration>()
	.Build();

await plugin.RunAsync();
