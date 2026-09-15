using MacroDeck.BeefWeb.Music.API.Responses.Player;
using MacroDeck.BeefWeb.Music.API.Responses.Playlists;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MacroDeck.BeefWeb.Music.API.Responses
{
    internal class PlayQueueRoot : BeefWebAPICall<PlayQueueRoot, PlayQueueItem[]>, IApiResource<PlayQueueItem[]>
    {
        public static string ApiPath => "playqueue";

        public required PlayQueueItem[] playQueue { get; init; }

        public static Dictionary<string, string?> Query => new() { ["columns"] = PlaylistColumns.GetColumnQuery() };

        public override PlayQueueItem[] Get() => playQueue;
    }

    internal partial class PlayQueueItem : IJsonOnDeserialized
    {
        [JsonPropertyName("columns")]
        public required List<JsonElement> rawColumns { get; init; }
        public int itemIndex { get; init; }
        public required string playlistId {  get; init; }
        public required int playlistIndex { get; init; }
        [JsonIgnore]
        public PlaylistColumns? columns { get; private set; }
        public void OnDeserialized()
        {
            columns = new PlaylistColumns(rawColumns);
        }
    }
}
