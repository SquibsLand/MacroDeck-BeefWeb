using MacroDeck.BeefWeb.Music.API.Responses;
using MacroDeck.Sdk.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebUtilities;
using Serilog.Formatting.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Web;
using ILogger = Serilog.ILogger;

namespace MacroDeck.BeefWeb.Music.API
{
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
        Task<Player> GetPlayer();
    }
    public interface IApiResource<TSelf>
        where TSelf : IApiResource<TSelf>
    {
        static abstract string ApiPath { get; }

        static abstract Dictionary<string, string?> Query { get; }
    }
    internal abstract class BeefWebAPICall<TSelf, TChild> : JSONPareser<TSelf>
        where TSelf : BeefWebAPICall<TSelf, TChild>, IApiResource<TSelf>
        where TChild : class
    {
        private static readonly ILogger _logger =
            IntegrationLog.For<BeefWebPlayer>(BeefWebIntergration.IntegrationId);

        public abstract TChild Get();

        public static async Task<TChild?> Fetch(HttpClient client)
        {
            var relativeWithQuery = QueryHelpers.AddQueryString(
                TSelf.ApiPath,
                TSelf.Query);
            using HttpResponseMessage response = await client.GetAsync(relativeWithQuery);

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
            catch (Exception ex) {
                _logger.Error($"Failed to parse response from {response.RequestMessage?.RequestUri}");
                _logger.Information(ex.Message);
                return null;
            }
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

