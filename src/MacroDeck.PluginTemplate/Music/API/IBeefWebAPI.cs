using MacroDeck.BeefWeb.Music.API.Responses;
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
    public interface IApiResource<TChild>
    {
        static abstract string ApiPath { get; }

        static abstract Dictionary<string, string?> Query { get; }
    }
    public interface IApiResource : IApiResource<EmptyClass> { }
    internal abstract class BeefWebAIPBase<TSelf, TChild> : JSONPareser<TSelf>
        where TSelf : BeefWebAIPBase<TSelf, TChild>, IApiResource<TChild>
        where TChild : class
    {
        protected static readonly ILogger _logger =
           IntegrationLog.For<BeefWebPlayer>(BeefWebIntergration.IntegrationId);

        public abstract TChild Get();
        protected static async Task<TChild?> ParseResponse(HttpResponseMessage response)
        {

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            _logger.Information(content);

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
    }

    internal abstract class BeefWebAPICall<TSelf, TChild> : BeefWebAIPBase<TSelf, TChild>
        where TSelf : BeefWebAPICall<TSelf, TChild>, IApiResource<TChild>
        where TChild : class
    {
        public static async Task<TChild?> Fetch(HttpClient client)
        {
            var relativeWithQuery = QueryHelpers.AddQueryString(
                TSelf.ApiPath,
                TSelf.Query);
            using HttpResponseMessage response = await client.GetAsync(relativeWithQuery);

            return await ParseResponse(response);

        }
        
    }
    internal abstract class BeefWebAPISend<TSelf, TParams, TResponse> : BeefWebAIPBase<TSelf, TResponse>
        where TSelf : BeefWebAPISend<TSelf, TParams, TResponse>, IApiResource<TResponse>
        where TResponse : class
        where TParams : HttpContent 
    {

        public static Dictionary<string, string?> Query => [];
        public static async Task<TResponse?> Post(HttpClient client, TParams? postContent = null)
        {
            using HttpResponseMessage reponse = await client.PostAsync(TSelf.ApiPath, postContent);
            
            return await ParseResponse(reponse);

        }
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

