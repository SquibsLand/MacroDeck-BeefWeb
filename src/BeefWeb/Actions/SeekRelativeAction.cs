using BeefWeb;
using BeefWeb.Actions;
using BeefWeb.Music;
using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;



namespace BeefWeb.Actions
{
    internal sealed class SeekExecutor(BeefWebIntergration integration) : BeefWebExecutor
    {
        protected override BeefWebIntergration Intergration => integration;

        public override async Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            if (IsValid(context, "seek-value", out double seconds))
            {
                await Intergration.Player.SeekRelativeAsync(TimeSpan.FromMilliseconds(seconds * 1000));
            }
            else
            {
                return ActionResult.Failed(ActionErrorCodes.InvalidParameter, "Seek value must be a number");
            }

            return ActionResult.Success();

        }
    }

    internal class SeekRelativeAction(BeefWebIntergration _integration) : BaseBeefWebAction<SeekExecutor>(_integration)
    {

        public override string Id => "seek-relative";

        public override string Name => "Seek Relativly";

        public override string Description => "Seek the playback relative to its current position";

        public override IReadOnlyList<ActionParameter> Parameters { get; } =
        [
            ActionParameter.DynamicChoice("player", label: "Player", required: true), 
            ActionParameter.Number("seek-value", "Seek (Seconds)", required: true, description: "Seek forward or backward by the set amount of seconds"),
        ];

        protected override SeekExecutor CreateNewExecutor() => new(Integration);

        public override Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken) => Task.FromResult(new DynamicOptionsResult { Options = Integration.InstanceOptions() });
        
    }
    
}
