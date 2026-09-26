using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeefWeb.Actions
{
    using static BeefWebCommonParams;
    sealed internal class VolumeExecutor(BeefWebIntergration Intergration) : BeefWebExecutor(Intergration)
    {
        public override async Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            if(IsValid(context, "volume-value", out double volume))
            {
                await Intergration.Player.SetRelativeVolumeAsync((int) MathF.Round((float) volume));
            }
            else
            {
                return ActionResult.Failed(ActionErrorCodes.InvalidParameter, "Seek value must be a number");
            }

            return ActionResult.Success();
        }

    }

    internal class SetVolumeRelativeAction(BeefWebIntergration intergration) : BaseBeefWebAction<VolumeExecutor>(intergration)
    {
        public override string Id => "relative-volume";

        public override string Name => "Set Volume Relativly";

        public override string Description => "Increase or Decrease the volume based on its current value";

        public override IReadOnlyList<ActionParameter> Parameters => [
            Player.Create(),
            ActionParameter.Number("volume-value", "Volume", required: true, description: "Change the volume higher or lower by the set percent", min: 0, max: 100),
         ];

        public override Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken) => Task.FromResult(new DynamicOptionsResult { Options = Integration.InstanceOptions() });

        protected override VolumeExecutor CreateNewExecutor(BeefWebIntergration intergration) => new(intergration);
    }

}
