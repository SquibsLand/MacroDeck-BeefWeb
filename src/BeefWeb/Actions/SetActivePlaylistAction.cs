using BeefWeb.Music.API.Responses.Playlists;
using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeefWeb.Actions
{
    using static BeefWebCommonParams;
    internal class SetActivePlaylistExecutor(BeefWebIntergration Intergration) : BeefWebExecutor(Intergration)
    {
        private static readonly Random _rng = new();

        public async override Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            if (IsValid(context, PlaylistId.Id, out string? playlistId))
            {
                int trackIndex = 0;
                string[] parts;
               
                if (IsValid(context, "random-track", out bool randomTrack))
                {
                    if (randomTrack)
                    {
                        SinglePlaylist? value = await Intergration.Player.GetSinglePlaylist(playlistId);
                        if (value is SinglePlaylist singlePlaylist) trackIndex = _rng.Next(0, singlePlaylist.itemCount);
                    }

                } else if(IsValid(context, TrackItem.Id, out string? trackId)){
                    if (!string.IsNullOrEmpty(trackId)){
                        parts = trackId.Split(".");
                        if (int.TryParse(parts[1].Trim(), out var parsed))
                            trackIndex = parsed;
                    }
                }
                

                await Intergration.Player.SetCurrentPlaylist(playlistId, trackIndex);
                return ActionResult.Success();
            }else
            {
                return ActionResult.Failed(ActionErrorCodes.InvalidParameter, "Playlist value must be a string");
            }
            
        }
    }
    internal class SetActivePlaylistAction(BeefWebIntergration integration) : BaseBeefWebAction<SetActivePlaylistExecutor>(integration)
    {
        public override string Id => "set-active-playlist";

        public override string Name => "Set Active Playlist";

        public override string Description => "Plays the selected playlist, optionally with a specific song";

        public override IReadOnlyList<ActionParameter> Parameters => [
                Player.Create(),
                PlaylistId.Create(),
                ActionParameter.Toggle("random-track", label: "Play a random track", defaultValue: true),
                TrackItem.Create("If no track is selected it will play the first, if Random is enabled it will play a random track.", false)
            ];

        public override async Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken) {
            string param = context.ParameterName;
            if (param.Contains(PlaylistId.Id))
            {
                return await GetPlaylistOptions();
            }
            else if(param.Contains(Player.Id))
            {
                return await GetPlayerOptions();
            }
            else if (param.Contains(TrackItem.Id)){
                if(context.CurrentParameters.TryGetValue("random-track", out object? value1))
                {
                    if(value1 is not null && value1 is bool random && random == true)
                    {
                        return new DynamicOptionsResult { Options = [] };
                    }
                }
                if(context.CurrentParameters.TryGetValue(PlaylistId.Id, out object? value2))
                {
                    if (value2 is not null && value2 is string pid)
                    {
                        return await GetPlaylistItemsOptions(pid);
                    }
                }
                
            }
            throw UnhandledParam(param);
        }

        protected override SetActivePlaylistExecutor CreateNewExecutor(BeefWebIntergration intergration) => new(intergration);
    }
}
