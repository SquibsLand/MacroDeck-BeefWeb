using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MacroDeck.BeefWeb.Music.API.Posts.Player
{
    internal class PlayItemParams
    {
        public required string PlaylistId { get; init; }
        public string? Index { get; init; }
    }
    internal class PlayItem : BeefWebAPISend<PlayItem, NoBodyContent, EmptyClass, PlayItemParams>, IApiResource<EmptyClass, PlayItemParams>
    {
        public static string ApiPath => "player/play/{0}/{1}";

        public static string BuildApiPath(PlayItemParams args) => string.Format(CultureInfo.InvariantCulture, ApiPath, args.PlaylistId, args.Index ?? "0");

        public override EmptyClass Get() => new();
    }
}
