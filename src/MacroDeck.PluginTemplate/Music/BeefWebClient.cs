using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MacroDeck.BeefWeb.Music.API;
using MacroDeck.BeefWeb.Music.API.Responses;
using MacroDeck.Sdk.MusicPlayer;
using Microsoft.Extensions.DependencyInjection;


namespace MacroDeck.BeefWeb.Music
{

    internal class BeefWebClient : IBeefWebAPI
    {
        public PlayerType PlayerType { get; }
        public string ServerAddress { get; }
        public int ServerPort { get; }
        public string? Username { get; }
        public string? Password { get; }

        private readonly HttpClient sharedClient;
        public BeefWebClient(
            string serverAddress,
            int serverPort,
            PlayerType playerType,
            string? username,
            string? password)
        {
            this.ServerAddress = serverAddress;
            this.ServerPort = serverPort;
            this.PlayerType = playerType;
            this.Username = username;
            this.Password = password;
            this.sharedClient = new HttpClient{ BaseAddress = new Uri($"http://{ServerAddress}:{ServerPort}/api/") };

        }
        public async Task<MusicPlayerArtwork?> GetArtwork()
        {
            HttpResponseMessage response = await sharedClient.GetAsync("artwork/current", HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            string mimeType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            byte[] data = await response.Content.ReadAsByteArrayAsync();

            return new(data, mimeType);

        }
        public async Task<Player?> GetPlayer() => await PlayerRoot.Fetch(sharedClient);
    }
}
