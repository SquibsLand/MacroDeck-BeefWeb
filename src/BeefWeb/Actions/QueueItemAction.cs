using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeefWeb.Actions
{
    using static BeefWebCommonParams;
    internal class QueueItemActionExecutor(BeefWebIntergration Intergration) : BeefWebExecutor(Intergration)
    {
        public async override Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            if (!IsValid(context, PlaylistId.Id, out string? playlistId))
            {
                return ActionResult.Failed(
                    ActionErrorCodes.InvalidParameter,
                    "Parameter playlist is invalid");
            }

            if (!IsValid(context, TrackItem.Id, out string? trackId))
            {
                return ActionResult.Failed(
                    ActionErrorCodes.InvalidParameter,
                    "Parameter track item is invalid");
            }

            if (!IsValid(context, "position", out double? position))
            {
                return ActionResult.Failed(
                    ActionErrorCodes.InvalidParameter,
                    "Parameter track position is invalid");
            }


            if (!string.IsNullOrEmpty(trackId))
            {
                string[] parts = trackId.Split(".");
                if (int.TryParse(parts[1].Trim(), out var parsed))
                {
                    await Intergration.Player.QueueItemAsync(playlistId, parsed, double.ConvertToInteger<int>(position??0));
                    return ActionResult.Success();
                }
 
            }
            return ActionResult.Failed(ActionErrorCodes.InvalidParameter, "Track index could not be parsed");

        }
    }
    internal class QueueItemAction(BeefWebIntergration Intergration) : BaseBeefWebAction<QueueItemActionExecutor>(Intergration)
    {
        public override string Id => "queue-item";

        public override string Name => "Add to Playback Queue";

        public override string Description => "Add the selected song to the playback queue";

        public override IReadOnlyList<ActionParameter> Parameters => [
                Player.Create(),
                PlaylistId.Create(),
                TrackItem.Create(),
                ActionParameter.Number("position", "Queue Position", "Position in the playqueue to add the song to.", 0, step: 1, defaultValue: 0, required:true)
            ];

        public override async Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken)
        {
            string param = context.ParameterName;

            if(param.Contains(Player.Id))
            {
                return await GetPlayerOptions();
            }
            else if (param.Contains(PlaylistId.Id))
            {
                return await GetPlaylistOptions();
            }
            else if (param.Contains(TrackItem.Id))
            {
                if (context.CurrentParameters.TryGetValue(PlaylistId.Id, out object? value2))
                {
                    if (value2 is not null && value2 is string pid)
                    {
                        return await GetPlaylistItemsOptions(pid);
                    }
                }
            }
   
            throw UnhandledParam(param);
        }

        protected override QueueItemActionExecutor CreateNewExecutor(BeefWebIntergration intergration) => new(intergration);
    }
}
