using BeefWeb.Music.API.Responses.Playlists;
using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeefWeb.Actions
{
    using static BeefWebCommonParams;
    internal class PlayItemExecutor(BeefWebIntergration integration) : BeefWebExecutor(integration)
    {

        public override async Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            
            if (IsValid(context, PlaylistId.Id, out string? playlistId))
            {
                int trackIndex = 0;
                string[] parts;
                
                if (IsValid(context, TrackItem.Id, out string? trackId))
                {
                    if (!string.IsNullOrEmpty(trackId))
                    {
                        parts = trackId.Split(".");
                        if (int.TryParse(parts[1].Trim(), out var parsed))
                            trackIndex = parsed;
                    }
                }


                await Intergration.Player.SetCurrentPlaylist(playlistId, trackIndex);
                return ActionResult.Success();
            }
            else
            {
                return ActionResult.Failed(ActionErrorCodes.InvalidParameter, "Playlist value must be a string");
            }

        }
    }
    internal class PlayItemAction(BeefWebIntergration integration) : BaseBeefWebAction<PlayItemExecutor>(integration)
    {
        public override string Id => "play-item";

        public override string Name => "Play Item";

        public override string Description => "Play a selected song from a specific playlist";

        public override IReadOnlyList<ActionParameter> Parameters => [
                Player.Create(),
                PlaylistId.Create(),
                TrackItem.Create()
            ];

        public override async Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken)
        {
            string param = context.ParameterName;
            if (param.Contains("playlist-id"))
            {
                return await GetPlaylistOptions();
            }
            else if (param.Contains("player"))
            {
                return await GetPlayerOptions();
            }
            else if (param.Contains("track-item"))
            {
                if (context.CurrentParameters.TryGetValue("playlist-id", out object? value2))
                {
                    if (value2 is not null && value2 is string pid)
                    {
                        return await GetPlaylistItemsOptions(pid);
                    }
                }

            }
            throw UnhandledParam(param);
        }

        protected override PlayItemExecutor CreateNewExecutor(BeefWebIntergration intergration) => new(intergration);
    }
}
