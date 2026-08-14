using MacroDeck.BeefWeb;
using MacroDeck.Plugin.Hosting;
using MacroDeck.Plugin.Hosting.DependencyInjection;

var plugin = MacroDeckPlugin.CreatePlugin(args)
	.UseMacroDeckLogging()
	.ConfigureServices((_, services) => services.AddMacroDeckIntegration<BeefWebIntergration>())
	.Build();

await plugin.RunAsync();
