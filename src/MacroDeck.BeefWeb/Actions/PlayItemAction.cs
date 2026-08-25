using MacroDeck.BeefWeb.Music.API.Responses.Playlists;
using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MacroDeck.BeefWeb.Actions
{
    internal class PlayItemExecutor(BeefWebIntergration integration) : BeefWebExecutor
    {
        protected override BeefWebIntergration Intergration => integration;

        public override async Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            if (IsValid(context, "playlist-id", out string? playlistId))
            {
                int trackIndex = 0;
                string[] parts;
                
                if (IsValid(context, "track-item", out string? trackId))
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
    internal class PlayItemAction(BeefWebIntergration _integration) : BaseBeefWebAction<PlayItemExecutor>(_integration)
    {
        public override string Id => "play-item";

        public override string Name => "Play Item";

        public override string Description => "Play a selected song from a specific playlist";

        public override IReadOnlyList<ActionParameter> Parameters => [ActionParameter.DynamicChoice("player", label: "Player", required: true),
                ActionParameter.DynamicChoice("playlist-id", label: "Playlist", required: true),
                ActionParameter.DynamicChoice("track-item",
                                              label: "Song",
                                              required: true)];

        public override async Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken)
        {
            string param = context.ParameterName;
            if (param.Contains("playlist-id"))
            {
                return new DynamicOptionsResult { Options = await Integration.GetPlaylistOptions() ?? [] };
            }
            else if (param.Contains("player"))
            {
                return new DynamicOptionsResult { Options = Integration.InstanceOptions() };
            }
            else if (param.Contains("track-item"))
            {
                if (context.CurrentParameters.TryGetValue("playlist-id", out object? value2))
                {
                    if (value2 is not null && value2 is string pid)
                    {
                        return new DynamicOptionsResult { Options = await Integration.GetPlaylistItemsOptions(pid) ?? [] };
                    }
                }

            }
            throw new InvalidDataException($"Dynamic paramater {param} does not have logic for options");
        }

        protected override PlayItemExecutor CreateNewExecutor() => new(_integration);
    }
}
