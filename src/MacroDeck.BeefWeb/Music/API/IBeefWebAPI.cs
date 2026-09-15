using MacroDeck.BeefWeb.Music.API.Responses.Player;
using MacroDeck.Sdk.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Serilog.Formatting.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Web;
using ILogger = Serilog.ILogger;

namespace MacroDeck.BeefWeb.Music.API
{
    
    public sealed class EmptyClass { }
    enum PlayerType
    {
        FOOBAR,
        DEADBEEF
    }
    internal interface IBeefWebAPI
    {
        public PlayerType PlayerType { get; }
        string ServerAddress { get; }
        int ServerPort { get; }
        string? Username { get; }
        string? Password { get; }
        Task<Player?> GetPlayer();
    }
    public interface IApiResource<TChild, TArgs>
    {
        static abstract string ApiPath { get; }

        static virtual Dictionary<string, string?>? Query => null;
        static abstract string BuildApiPath(TArgs args);
    }
    public interface IApiResource : IApiResource<EmptyClass> { }
    public interface IApiResource<TChild> : IApiResource<TChild, EmptyClass> { }
    internal abstract class BeefWebAPIBase<TSelf, TChild, TArgs> : JSONPareser<TSelf>
        where TSelf : BeefWebAPIBase<TSelf, TChild, TArgs>, IApiResource<TChild, TArgs>
        where TChild : class
        where TArgs : class
    {
        protected static readonly ILogger _logger =
           IntegrationLog.For<BeefWebPlayer>(BeefWebIntergration.IntegrationId);

        public abstract TChild Get();
        protected static async Task<TChild?> ParseResponse(HttpResponseMessage response)
        {

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();


            if (string.IsNullOrEmpty(content))
            {
                _logger.Warning("No Response Recieved");
                return null;
            }
            try
            {
                return fromJSON(content)?.Get();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to parse response from {response.RequestMessage?.RequestUri}");
                _logger.Information(ex.Message);
                return null;
            }
        }
        protected static string GetRelativeQuery(TArgs? args)
        {

            var apiPath = args is EmptyClass || args is null
                ? TSelf.ApiPath
                : TSelf.BuildApiPath(args);

            return TSelf.Query is Dictionary<string, string?> query
                ? QueryHelpers.AddQueryString(apiPath, query)
                : apiPath;
        }
    }

    internal abstract class BeefWebAPICall<TSelf, TChild, TArgs> : BeefWebAPIBase<TSelf, TChild, TArgs>
        where TSelf : BeefWebAPICall<TSelf, TChild, TArgs>, IApiResource<TChild, TArgs>
        where TChild : class
        where TArgs : class
    {
        public static async Task<TChild?> Fetch(HttpClient client, TArgs args )
        {
            string relativeWithQuery = GetRelativeQuery(args);
            using HttpResponseMessage response = await client.GetAsync(relativeWithQuery);

            return await ParseResponse(response);

        }
    }
    internal abstract class BeefWebAPICall<TSelf> : BeefWebAPICall<TSelf, TSelf>
        where TSelf : BeefWebAPICall<TSelf>, IApiResource<TSelf>
    { }
    internal abstract class BeefWebAPICall<TSelf, TChild> : BeefWebAPICall<TSelf, TChild, EmptyClass>
        where TSelf : BeefWebAPICall<TSelf,TChild>, IApiResource<TChild>
        where TChild : class
    {
        public static string BuildApiPath(EmptyClass args) => TSelf.ApiPath;
        public static Task<TChild?> Fetch(HttpClient client) => Fetch(client, new EmptyClass());
    }
    internal abstract class BeefWebAPISend<TSelf, TParams, TResponse, TArgs> : BeefWebAPIBase<TSelf, TResponse, TArgs>
        where TSelf : BeefWebAPISend<TSelf, TParams, TResponse, TArgs>, IApiResource<TResponse, TArgs>
        where TResponse : class
        where TParams : HttpContent
        where TArgs : class
    {

        public static Dictionary<string, string?> Query => [];
        public static async Task<TResponse?> Post(HttpClient client, TParams? postContent = null, TArgs? args = null)
        {
            using HttpResponseMessage reponse = await client.PostAsync(GetRelativeQuery(args), postContent);
            
            return await ParseResponse(reponse);

        }
        public static string BuildApiPath(EmptyClass args) => TSelf.ApiPath;
    }
    internal abstract class BeefWebAPISend<TSelf>
        : BeefWebAPISend<TSelf, NoBodyContent, EmptyClass>
        where TSelf : BeefWebAPISend<TSelf>, IApiResource
    {
        public override EmptyClass Get() => new();
    }
    internal abstract class BeefWebAPISend<TSelf, TResponse>
        : BeefWebAPISend<TSelf, NoBodyContent, TResponse>
        where TSelf : BeefWebAPISend<TSelf, TResponse>, IApiResource<TResponse>
        where TResponse : class
    { }
    internal abstract class BeefWebAPISend<TSelf, TParams, TResponse> : BeefWebAPISend<TSelf, TParams, TResponse, EmptyClass>
        where TSelf : BeefWebAPISend<TSelf, TParams, TResponse>, IApiResource<TResponse>
        where TResponse : class
        where TParams : HttpContent
    { }
    internal sealed class NoBodyContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => Task.CompletedTask;

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return true;
        }
    }
    internal abstract class JSONPareser<TSelf> where TSelf : JSONPareser<TSelf>
    {
        public TSelf Self => (TSelf)this;
        public string toJSON()
        {
            return JsonSerializer.Serialize(this);
        }

        public static TSelf? fromJSON(string json) => JsonSerializer.Deserialize<TSelf>(json);
    }
}

