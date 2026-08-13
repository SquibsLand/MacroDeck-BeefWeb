using MacroDeck.Plugin.Hosting;
using MacroDeck.Plugin.Hosting.DependencyInjection;
using MacroDeck.PluginTemplate;

var plugin = MacroDeckPlugin.CreatePlugin(args)
	.UseMacroDeckLogging()
	.ConfigureServices((_, services) => services.AddMacroDeckIntegration<TemplateIntegration>())
	.Build();

await plugin.RunAsync();
