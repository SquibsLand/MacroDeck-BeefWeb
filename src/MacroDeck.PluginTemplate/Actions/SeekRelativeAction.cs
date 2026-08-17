using MacroDeck.BeefWeb.Music;
using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MacroDeck.BeefWeb.Actions
{
    internal class SeekRelativeAction(BeefWebIntergration integration) : IActionDefinition, IDynamicOptionsActionDefinition
    {
        public string Id => "seek-relative";

        public string Name => "Seek Relativly";

        public string Description => "Seek the playback relative to its current position";

        public IReadOnlyList<ActionParameter> Parameters { get; } =
        [
            ActionParameter.DynamicChoice("player", label: "Player", required: true), 
            ActionParameter.Number("seek-value", "Seek (Seconds)", required: true, description: "Seek forward or backward by the set amount of seconds"),
        ];

        public IActionExecutor CreateExecutor() => new Executor(integration);

        public Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken) => Task.FromResult(new DynamicOptionsResult { Options = integration.InstanceOptions() });
        private sealed class Executor(BeefWebIntergration integration) : IActionExecutor
        {
            private BeefWebIntergration intergration = integration;
            public async Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
            {
                var value = context.Parameters.GetValueOrDefault("seek-value");

                if(value is not double seconds)
                {
                    return ActionResult.Failed(ActionErrorCodes.InvalidParameter, "Seek value must be a number");
                }
                await intergration.Player.SeekRelativeAsync(TimeSpan.FromMilliseconds(seconds * 1000));

                return ActionResult.Success();
               
            }
        }
    }
    
}
