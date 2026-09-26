using BeefWeb.Music.API.Posts.Player;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;

namespace BeefWeb.Music.API.Posts.Playqueue
{
    internal class PlayqueueAddParams
    {
        public required string plref { get; init; }
        public required int itemIndex { get; init; }
        public required int queueIndex { get; init; }
    }
    internal class PlayqueueAdd : BeefWebAPISend<PlayqueueAdd, JsonContent, EmptyClass>, IApiResource
    {
        public static string ApiPath => "playqueue/add";
        public static async Task Post(HttpClient client, PlayqueueAddParams args) => await Post(client, JsonContent.Create(args));
        public override EmptyClass Get() => new();
    }
}
