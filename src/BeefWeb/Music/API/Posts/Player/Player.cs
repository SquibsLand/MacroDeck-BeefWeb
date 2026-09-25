using MacroDeck.Sdk.MusicPlayer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static BeefWeb.Music.API.Responses.Player.Player;

namespace BeefWeb.Music.API.Posts.Player
{
    internal class PlayerRequestOption
    {
        public required string id { get; init; }
        public required JsonNode value { get; init; } //NOTE: Datatype is unknown
    }
    internal class PlayerRequest
    {
        public float? volume { get; init; }
        public float? relativeVolume { get; init; }
        public bool? isMuted { get; init; }
        public float? position { get; init; } //NOTE: Possible that this should be a float
        public float? relativePosition { get; init; } //NOTE: Possible that this should be a float
        public PlayerRequestOption[]? options { get; init; }
        
    }

    internal abstract class SetPlayer<TSelf, TResponse> : BeefWebAPISend<TSelf, JsonContent, TResponse>, IApiResource<TResponse>
        where TSelf : SetPlayer<TSelf, TResponse>
        where TResponse : class
    {
        public static string ApiPath => "player";

        protected static async Task<ApiResponse<TResponse?>> Post(HttpClient client, PlayerRequest request) => await Post(client, JsonContent.Create(request));

    }
    internal abstract class SetPlayer<TSelf> : SetPlayer<TSelf, EmptyClass>
       where TSelf : SetPlayer<TSelf>
    {
        public override EmptyClass Get() => new();
    }

    internal class NextSong : BeefWebAPISend<NextSong>, IApiResource
    {
       public static string ApiPath => "player/next";
    }

    internal class PauseSong : BeefWebAPISend<PauseSong>, IApiResource
    {
        public static string ApiPath => "player/pause";
    }
    internal class PlaySong : BeefWebAPISend<PlaySong>, IApiResource
    {
        public static string ApiPath => "player/play";
    }

    // TODO: PlayItem

    internal class PreviousSong : BeefWebAPISend<PreviousSong>, IApiResource
    {
        public static string ApiPath => "player/previous";
    }

    // TODO: Seek

    internal class SeekPlayback : SetPlayer<SeekPlayback>
    {
        public static async Task Post(HttpClient client, float position) => await Post(client, new PlayerRequest { position = position });
    }

    internal class SeekRelativePlayback : SetPlayer<SeekRelativePlayback>
    {
        public static async Task Post(HttpClient client, float position) => await Post(client, new PlayerRequest { relativePosition = position });
    }

    // TODO: Set Repeat Mode

    internal class SetCurrentRepeatMode : SetPlayer<SetCurrentRepeatMode>
    {
        public static async Task Post(Responses.Player.Player player, HttpClient client, RepeatMode mode)
        {
            throw new NotImplementedException();
            //int? index = player.FindPlaybackMode(mode);
            //if(index is null) return;
            //PlayerRequest request = new PlayerRequest
            //{
            //    options = [new PlayerRequestOption { id = "playbackMode", value = (int)index }]
            //};
            //string temp = JsonSerializer.Serialize(request);
            //await Post(client,  request);
        }
    }

    // TODO: Set Shuffled
    //internal class SetShuffled : BeefWebAPISend<SetShuffled, JsonContent, EmptyClass>, IApiResource
    //{
    //    public static string ApiPath => "player/previous";

    //    public static async Task<EmptyClass> Post(HttpClient client, bool shuffled)
    //    {
    //        JsonSerializer.Serialize<PlayerRequest>(value: new()))
    //        await Post(client, new JsonContent()
    //    }
    //}

    internal class SetCurrentVolume : SetPlayer<SetCurrentVolume>
    {
        public static async Task Post(HttpClient client, float volume) => await Post(client, new PlayerRequest { volume = volume });
    }
   
    internal class PausePlay : BeefWebAPISend<PausePlay>, IApiResource
    {
        public static string ApiPath => "player/play-pause";
    }
}
