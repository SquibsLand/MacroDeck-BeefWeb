using BeefWeb.Music.API.Responses.Playlists;
using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeefWeb.Actions
{
    internal class SetActivePlaylistExecutor(BeefWebIntergration integration) : BeefWebExecutor
    {
        private static readonly Random _rng = new Random();
        protected override BeefWebIntergration Intergration => integration;

        public async override Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            if (IsValid(context, "playlist-id", out string? playlistId))
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

                } else if(IsValid(context, "track-item", out string? trackId)){
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
    internal class SetActivePlaylistAction(BeefWebIntergration _integration) : BaseBeefWebAction<SetActivePlaylistExecutor>(_integration)
    {
        public override string Id => "set-active-playlist";

        public override string Name => "Set Active Playlist";

        public override string Description => "Plays the selected playlist, optionally with a specific song";

        public override IReadOnlyList<ActionParameter> Parameters => [
                ActionParameter.DynamicChoice("player", label: "Player", required: true),
                ActionParameter.DynamicChoice("playlist-id", label: "Playlist", required: true),
                ActionParameter.Toggle("random-track", label: "Play a random track", defaultValue: true),
                ActionParameter.DynamicChoice("track-item",
                                              label: "Song",
                                              required: false,
                                              description:"If no track is selected it will play the first, if Random is enabled it will play a random track.")
                
            ];

        public override async Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken) {
            string param = context.ParameterName;
            if (param.Contains("playlist-id"))
            {
                return new DynamicOptionsResult { Options = await Integration.GetPlaylistOptions() ?? [] };
            }
            else if(param.Contains("player"))
            {
                return new DynamicOptionsResult { Options = Integration.InstanceOptions() };
            }
            else if (param.Contains("track-item")){
                if(context.CurrentParameters.TryGetValue("random-track", out object? value1))
                {
                    if(value1 is not null && value1 is bool random && random == true)
                    {
                        return new DynamicOptionsResult { Options = [] };
                    }
                }
                if(context.CurrentParameters.TryGetValue("playlist-id", out object? value2))
                {
                    if (value2 is not null && value2 is string pid)
                    {
                        return new DynamicOptionsResult { Options = await Integration.GetPlaylistItemsOptions(pid) ?? [] };
                    }
                }
                
            }
            throw new InvalidDataException($"Dynamic paramater {param} does not have logic for options");
        }

        protected override SetActivePlaylistExecutor CreateNewExecutor() => new(_integration);
    }
}
