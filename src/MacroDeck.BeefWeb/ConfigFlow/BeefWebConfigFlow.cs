using MacroDeck.BeefWeb.Music.API;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.ConfigFlow;

namespace MacroDeck.BeefWeb.ConfigFlow;

sealed class BeefWebConfigFlow : IConfigFlow
{
	internal const string ServerFieldName = "server";
	internal const string PortFieldName = "port";
	internal const string PlayerTypeFieldName = "type";

	internal const string UsernameFieldName = "username";
	internal const string PasswordFieldName = "password";

	private const string StepId = "server";

	public Task<ConfigFlowResult> StartAsync(IConfigFlowContext context, CancellationToken cancellationToken)
		=> Task.FromResult(ConfigFlowResult.Step(BuildStep()));

	public Task<ConfigFlowResult> SubmitAsync(
		string stepId,
		IReadOnlyDictionary<string, object?> input,
		IConfigFlowContext context,
		CancellationToken cancellationToken)
	{
		if (!string.Equals(stepId, StepId, StringComparison.Ordinal))
		{
			return Task.FromResult(ConfigFlowResult.Error(BuildStep(), "Unknown step."));
		}

		if (input.GetValueOrDefault(ServerFieldName) is not string { Length: > 0 } server)
		{
			return Task.FromResult(ConfigFlowResult.Error(BuildStep(),
				"Enter a server address.",
				new Dictionary<string, Localization.LocalizedText> { [ServerFieldName] = "Required." }));
		}
        if (input.GetValueOrDefault(PlayerTypeFieldName) is not string { Length: > 0 } type)
        {
            return Task.FromResult(ConfigFlowResult.Error(BuildStep(),
                "Enter a player name.",
                new Dictionary<string, Localization.LocalizedText> { [PlayerTypeFieldName] = "Required." }));
        }

        return Task.FromResult(ConfigFlowResult.Complete($"{type} ({server})"));
	}

	private static ConfigFlowStep BuildStep() => new()
	{
		StepId = StepId,
		Title = "Server setup",
		Description = "Input the server configuration",
		Fields =
		[
			ActionParameter.Text(ServerFieldName,
				label: "Server Address",
				defaultValue: "127.0.0.1",
				required: true),
			ActionParameter.Number(PortFieldName,
				label: "Server Port",
				defaultValue: 8880,
				required: true),
			ActionParameter.Choice(PlayerTypeFieldName, 
				options: [
					new ActionParameterOption{
						Value = PlayerType.FOOBAR.ToString(),
						Label = "Foobar2000"
					},
					new ActionParameterOption{
					 Value = PlayerType.DEADBEEF.ToString(),
					 Label = "DeaDBeeF"
					}
				],
				label: "Player Type",
				required: true
				),
			ActionParameter.Text(UsernameFieldName,
				label: "Username (Unused)"),
			ActionParameter.Password(PasswordFieldName, 
				label:"Password (Unused)")
		]
	};
}
