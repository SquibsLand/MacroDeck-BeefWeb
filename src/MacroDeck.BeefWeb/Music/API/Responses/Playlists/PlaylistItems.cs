using MacroDeck.Sdk.MusicPlayer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MacroDeck.BeefWeb.Music.API.Responses.Playlists
{
    internal class PlaylistItemsArgs
    {
        public required string PlaylistId {  get; init; }
        public required string Range { get; init; }
    }
    internal class PlaylistItemsRoot : BeefWebAPICall<PlaylistItemsRoot, PlaylistItems, PlaylistItemsArgs>, IApiResource<PlaylistItems, PlaylistItemsArgs>
    {
        public static string ApiPath => "playlists/{0}/items/{1}";
        public static Dictionary<string, string?> Query => new() { ["columns"] = PlaylistColumns.GetColumnQuery() };

        public required PlaylistItems playlistItems { get; init; }
        public static string BuildApiPath(PlaylistItemsArgs args) => string.Format(CultureInfo.InvariantCulture, ApiPath, args.PlaylistId, args.Range);

        public override PlaylistItems Get() => playlistItems;
    }
    internal class PlaylistItems
    {
        public required PlaylistItem[] items { get; init; }
        public string? PlaylistId { get; private set; }

        public IReadOnlyList<MusicPlayerCatalogItem> ToCatalogItems()
        {
            if(items == null) throw new ArgumentNullException("items");
            if (PlaylistId == null) throw new ArgumentNullException("PlaylistId is null. Please call WithPID first");
            List<MusicPlayerCatalogItem> result = [];
            foreach (PlaylistItem item in items)
            {
                if(item.columns is PlaylistColumns newColumns)
                {
                    result.Add(new($"{PlaylistId}.{newColumns.index - 1}", newColumns.title, MusicPlayerCatalogItemKind.Track));
                }
                
            }
            return result;
        }

        [MemberNotNull(nameof(PlaylistId))]
        public PlaylistItems WithPID(string pid)
        {
            PlaylistId = pid;
            return this;
        }
    }
    internal class PlaylistItem : IJsonOnDeserialized
    {
        [JsonPropertyName("columns")]
        public required List<JsonElement> rawColumns { get; init; }

        [JsonIgnore]
        public PlaylistColumns? columns { get; private set; }

        public void OnDeserialized()
        {
            columns = new PlaylistColumns(rawColumns);
        }
    }
    internal sealed class PlaylistColumns : Columns<PlaylistColumns>, IColumns
    {
        public static string[] ColumnsQuery =>
        [
            "%album artist%",
            "%album%",
            "%artist%",
            "%title%",
            "%track number%",
            "%length_seconds%",
            "%rating%",
            "%list_index%"
        ];

        public string albumArtist { get; init; }
        public string album { get; init; }
        public string artists { get; init; }
        public string title { get; init; }
        public int? trackNumber { get; init; }
        public int? trackSeconds { get; init; }
        public int? rating { get; init; }
        public int? index { get; init; }

        public PlaylistColumns(List<JsonElement> columns) : base(columns)
        {
            albumArtist = NextString();
            album = NextString();
            artists = NextString();
            title = NextString();
            trackNumber = NextInt();
            trackSeconds = NextInt();
            rating = NextInt();
            index = NextInt();

        }
    }
}
