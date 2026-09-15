using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeefWeb.Actions
{
    sealed internal class VolumeExecutor(BeefWebIntergration intergration) : BeefWebExecutor
    {
        protected override BeefWebIntergration Intergration => intergration;

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

    internal class SetVolumeRelativeAction(BeefWebIntergration _intergration) : BaseBeefWebAction<VolumeExecutor>(_intergration)
    {
        public override string Id => "relative-volume";

        public override string Name => "Set Volume Relativly";

        public override string Description => "Increase or Decrease the volume based on its current value";

        public override IReadOnlyList<ActionParameter> Parameters => [
            ActionParameter.DynamicChoice("player", label: "Player", required: true),
            ActionParameter.Number("volume-value", "Volume", required: true, description: "Change the volume higher or lower by the set percent", min: 0, max: 100),
         ];

        public override Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken) => Task.FromResult(new DynamicOptionsResult { Options = Integration.InstanceOptions() });

        protected override VolumeExecutor CreateNewExecutor() => new(Integration);
    }

}
