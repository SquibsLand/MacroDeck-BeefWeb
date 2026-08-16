using MacroDeck.BeefWeb.Music.API.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MacroDeck.BeefWeb.Music.API.Posts.Player
{
    internal class PlayerRequest
    {
        public float? volume { get; init; }
        public float? relativeVolume { get; init; }
        public bool? isMuted { get; init; }
        public int? position { get; init; } //NOTE: Possible that this should be a float
        public int? relativePosition { get; init; } //NOTE: Possible that this should be a float
        public IOption[]? options { get; init; }
        internal interface IOption
        {
            public string id { get; init; }
            public JsonValue value { get; init; } //NOTE: Datatype is unknown
        }
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

    // TODO: Set Repeat Mode

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

   
    internal abstract class SetPlayer<TSelf, TResponse> : BeefWebAPISend<TSelf, JsonContent, TResponse>, IApiResource<TResponse>
        where TSelf : SetPlayer<TSelf, TResponse>, IApiResource<TResponse>
        where TResponse : class
    {
        public static string ApiPath => "player";

        protected static async Task<TResponse?> Post(HttpClient client, PlayerRequest request) => await Post(client, JsonContent.Create(request));

    }
    internal abstract class SetPlayer<TSelf> : SetPlayer<TSelf,EmptyClass>
       where TSelf : SetPlayer<TSelf>, IApiResource
    {
        public override EmptyClass Get() => new();
    }
    internal class SetCurrentVolume : SetPlayer<SetCurrentVolume>, IApiResource
    {
        public static async Task Post(HttpClient client, float volume) => await Post(client, new PlayerRequest { volume = volume });
    }
    internal class PausePlay : BeefWebAPISend<PausePlay>, IApiResource
    {
        public static string ApiPath => "player/play-pause";
    }
}
