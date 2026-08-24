using MacroDeck.BeefWeb.Music.API;
using MacroDeck.BeefWeb.Music.API.Posts.Player;
using MacroDeck.BeefWeb.Music.API.Responses.Player;
using MacroDeck.BeefWeb.Music.API.Responses.Playlists;
using MacroDeck.Sdk.MusicPlayer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;


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
        public async Task<PlaylistItems?> GetPlaylistItems(string pid, string? range = null) {
            if(range is null)
            {
                SinglePlaylist? item = await GetPlaylist(pid);
                if(item is null) return null;
                range = $"0:{item.itemCount}";
            }
            PlaylistItems? items = await PlaylistItemsRoot.Fetch(sharedClient, new PlaylistItemsArgs { PlaylistId = pid, Range = range });
            if(items is PlaylistItems playlistItems)
            {
                return items.WithPID(pid);
            }
            return null;
        }
        public async Task<PlaylistRoot?> GetAllPlaylists() => await PlaylistRoot.Fetch(sharedClient);
        public async Task<SinglePlaylist?> GetPlaylist(string pid) => await SinglePlaylist.Fetch(sharedClient, new SinglePlaylistArgs { PlaylistId=pid });

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
            public async Task SeekRelative(float seconds)
            {
                await SeekRelativePlayback.Post(_client, seconds);
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
            private async Task<(double, double)?> GetVolumeData()
            {
                Player? player = await _ctx.GetPlayer();
                if (player is null) return null;
                return GetVolumeData(player);
                
            }
            private (double,double) GetVolumeData(Player player)
            {
                double max = player.volume.max;
                double min = player.volume.min;
                return (max, min);
            }
            public async Task SetVolume(float percent) 
            {
                var pair = await GetVolumeData();
                if (pair is null) return;
                var (max, min) = pair.Value;

                double value = new DecibleHandler(min, max).FromPercent(percent);

                await SetCurrentVolume.Post(_client, (float)value);
            }
            public async Task SetRelativeVolume(float percent)
            {
                // TODO: Add logic for other volume types
                Player? player = await _ctx.GetPlayer();
                if (player is null) return;

                var (max, min) = GetVolumeData(player);

                double? oldPercent = player.volume.toPercent();
                if(oldPercent is null) return;
                double newPercent = (double) oldPercent + percent;


                double newDb = new DecibleHandler(min, max).FromPercent(newPercent);

                await SetCurrentVolume.Post(_client, (float) newDb);
                                                   
                
            }
            public async Task TogglePlayPause()
            {
                await PausePlay.Post(_client);
            }

            public async Task SetCurrnetPlaylist(string pid, int index = 0)
            {
                await PlayPlaylist.Post(_client, args: new PlayPlaylistParams { PlaylistId = pid, Index = index.ToString() });
            }
        }
    }
    
}
