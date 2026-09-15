using MacroDeck.Sdk.MusicPlayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace BeefWeb.Music.API.Responses.Playlists
{
    internal class PlaylistRoot : BeefWebAPICall<PlaylistRoot>, IApiResource<PlaylistRoot>
    {
        public static string ApiPath => "playlists";

        public required List<Playlist> playlists {  get; init; }
        

        public override PlaylistRoot Get() => this;

        public IReadOnlyList<MusicPlayerCatalogItem> ToCatalogItems()
        {
            List<MusicPlayerCatalogItem> result = [];
            foreach (Playlist item in playlists)
            {
                result.Add(item.ToCatalogItem());
            }
            return result;

        }
    }
    internal class SinglePlaylistArgs
    {
        public required string PlaylistId { get; init; }
    }
 
    internal class SinglePlaylist : BeefWebAPICall<SinglePlaylist, SinglePlaylist, SinglePlaylistArgs>, IApiResource<SinglePlaylist, SinglePlaylistArgs>
    {
        public static string ApiPath => "playlists/{0}";
        public string id { get; init; }
        public int index { get; init; }
        public bool isCurrent { get; init; }
        public int itemCount { get; init; }
        public string title { get; init; }



        public static string BuildApiPath(SinglePlaylistArgs args) => string.Format(CultureInfo.InvariantCulture, ApiPath, args.PlaylistId);

        public override SinglePlaylist Get() => this;
    }
    internal class Playlist
    {

        public string id { get; init; }
        public int index { get; init; }
        public bool isCurrent { get; init; }
        public int itemCount { get; init; }
        public string title { get; init; }

        public static string BuildApiPath(EmptyClass args)
        {
            throw new NotImplementedException();
        }

        //public int totalTime { get; init; }
        public MusicPlayerCatalogItem ToCatalogItem()
        {
            return new(id, title, MusicPlayerCatalogItemKind.Playlist);
        }
    }
   
}
