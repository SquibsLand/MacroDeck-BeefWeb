using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MacroDeck.BeefWeb.Music.API;
using MacroDeck.BeefWeb.Music.API.Posts.Player;
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

        public BeefWebCommands Commands { get; }

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
            this.Commands = new BeefWebCommands(this, this.sharedClient);
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

        internal class BeefWebCommands(BeefWebClient ctx, HttpClient client)
        {
            private readonly BeefWebClient _ctx = ctx;
            private readonly HttpClient _client = client;

            public async Task Next()
            {
                await NextSong.Post(_client);
            }
            public async Task Pause()
            {
                await PauseSong.Post(_client);
            }
            public async Task Play()
            {
                await PlaySong.Post(_client);
            }
            public async Task PlayItem()
            {
                throw new NotImplementedException();
            }
            public async Task Previous()
            {
                await PreviousSong.Post(_client);
            }
            public async Task Seek(float seconds)
            {
                await SeekPlayback.Post(_client, seconds);
            }
            public async Task SetRepeatMode(RepeatMode mode)
            {
                throw new NotImplementedException();
                //Player? player = await _ctx.GetPlayer();
                //if (player is null) return;
                //await SetCurrentRepeatMode.Post(player, _client, mode);
            }
            public async Task SetShuffle()
            {
                throw new NotImplementedException();
            }
            public async Task SetVolume(float percent) 
            {
                Player? player = await _ctx.GetPlayer();
                if (player is null) return;
                double max = player.volume.max;
                double min = player.volume.min;
                double value = new DecibleHandler(min, max).FromPercent(percent / 100);

                await SetCurrentVolume.Post(_client, (float)value);
            }
            public async Task TogglePlayPause()
            {
                await PausePlay.Post(_client);
            }

        }
    }
    
}
