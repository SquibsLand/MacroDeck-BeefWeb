using BeefWeb;
using BeefWeb.Actions;
using BeefWeb.Music;
using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;



namespace BeefWeb.Actions
{
    using static BeefWebCommonParams;
    internal sealed class SeekExecutor(BeefWebIntergration Intergration) : BeefWebExecutor(Intergration)
    {
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

    internal class SeekRelativeAction(BeefWebIntergration Intergration) : BaseBeefWebAction<SeekExecutor>(Intergration)
    {

        public override string Id => "seek-relative";

        public override string Name => "Seek Relativly";

        public override string Description => "Seek the playback relative to its current position";

        public override IReadOnlyList<ActionParameter> Parameters { get; } =
        [
            Player.Create(), 
            ActionParameter.Number("seek-value", "Seek (Seconds)", required: true, description: "Seek forward or backward by the set amount of seconds"),
        ];

        protected override SeekExecutor CreateNewExecutor(BeefWebIntergration intergration) => new(intergration);

        public override Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken) {
            string param = context.ParameterName;
            if (param.Contains(Player.Id))
            {
                return GetPlayerOptions();
            }
            throw UnhandledParam(param);
        }
        
    }
    
}
