using MacroDeck.BeefWeb.Music.API;
using MacroDeck.BeefWeb.Music.API.Responses;
using MacroDeck.BeefWeb.Music.API.Responses.Player;
using MacroDeck.BeefWeb.Music.API.Responses.Playlists;
using MacroDeck.Sdk.Logging;
using MacroDeck.Sdk.MusicPlayer;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MacroDeck.BeefWeb.Music
{
    internal sealed class BeefWebPlayer : IMusicPlayer, IMusicPlayerCatalogProvider
    {
        private static readonly ILogger _logger =
        IntegrationLog.For<BeefWebPlayer>(BeefWebIntergration.IntegrationId);
        public PlayerType? PlayerType { get; set; }
        public MusicPlayerState LastState { get; private set; } = MusicPlayerState.Disconnected;
        public MusicPlayerState LastValidState { get; private set; } = MusicPlayerState.Disconnected;
        public PlayQueueItem[] LastPlayQueue { get; private set; } = [];
        public Player? LastPlayer { get; private set;  } 

        public BeefWebClient? client { get; private set; }

        public void init(string serverAddress,
            int serverPort,
            PlayerType playerType,
            string username = default!,
            string password = default!)
        {
            PlayerType = playerType;
            client = new BeefWebClient(serverAddress, serverPort, playerType, username, password);  
        }

        private MusicPlayerState IsUnavailable = new()
        {
            IsUnavailable = true
        };
        public async Task<MusicPlayerArtwork?> GetArtworkAsync(string artworkId, CancellationToken cancellationToken = default)
        {
            _logger.Error("Getting Artwork");
            if (client is null)
            {
                _logger.Warning("Client is not ready, skipping getting arwork");
                return null;
            }
            try
            {
                if(LastPlayer is null)
                {
                    _logger.Warning("No previous player found to get artwork for");
                    return default;
                }
                MusicPlayerArtwork? artwork = await client.GetDynamicArtwork();
                if (artwork is null)
                {
                    _logger.Debug("Artwork not found, yet the client is working. Artwork is likely missing");
                    return null;
                } else return artwork;
            } catch (Exception e)
            {
                _logger.Error("Artwork failed to get. Check connection to player");
                _logger.Debug(e.Message);
                return null;
            }
        }
        public async Task<MusicPlayerState> GetStateAsync(CancellationToken cancellationToken = default)
        {
            MusicPlayerState state;
            _logger.Warning($"Client is {client != null}");
            if (client == null) state = IsUnavailable;
            else {
                Player? player = await GetPlayer();
                _logger.Warning($"Player is {player!= null}");
                if (player == null) state = IsUnavailable;
                else {
                    state = player.ToMusicPlayerState();
                    this.LastState = state;
                    if(state.IsConnected) this.LastValidState = state;
                }
            }
            return state;

        }

        private async Task<Player?> GetPlayer(bool getQueue = true)
        {
            if(client is null) return null;
            Player? player = await client.GetPlayer();
            if(player is null) return null;
            LastPlayer = player;

            if (getQueue)
            {
                LastPlayQueue = await client.GetPlayQueue();
            }
            return LastPlayer;
        }
        public async Task NextAsync(CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.Next();
        }

        public async Task PauseAsync(CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.Pause();
        }

        public async Task PlayAsync(CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.Play();
        }

        public async Task PlayItemAsync(string pid, int index)
        {
            if (client is null) return;
            await client.Commands.PlayItem(pid, index);
        }

        public async Task PreviousAsync(CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.Previous();
        }

        public async Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.Seek(position.Seconds);
        }

        public async Task SeekRelativeAsync(TimeSpan position, CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.SeekRelative(position.Seconds);
        }
        public Task SetRepeatModeAsync(RepeatMode mode, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetShuffleAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task SetVolumeAsync(int volumePercent, CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.SetVolume(volumePercent);
        }
        public async Task SetRelativeVolumeAsync(int volumePercent, CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.SetRelativeVolume(volumePercent);
        }
        public async Task SetCurrentPlaylist(string pid, int index = 0)
        {
            if (client is null) return;
            await client.Commands.SetCurrnetPlaylist(pid, index);
        }
        public async Task TogglePlayPauseAsync(CancellationToken cancellationToken = default)
        {
            if (client is null) return;
            await client.Commands.TogglePlayPause();
        }
        public async Task<IReadOnlyList<MusicPlayerCatalogItem>> GetPlaylists(CancellationToken cancellationToken = default)
        {
            if (client is null) throw new Exception("Client is null");
            IReadOnlyList<MusicPlayerCatalogItem> catalogItems = [];
            if(await client.GetAllPlaylists() is PlaylistRoot playlists)
            {
                catalogItems = playlists.ToCatalogItems();
            }
            return catalogItems;
        }
        public async Task<IReadOnlyList<MusicPlayerCatalogItem>> GetPlaylistItems(string pid,  CancellationToken cancellationToken = default)
        {
            if (client is null) throw new Exception("Client is null");
            IReadOnlyList<MusicPlayerCatalogItem> catalogItems = [];
            if (await client.GetPlaylistItems(pid) is PlaylistItems playlistItems)
            {
                catalogItems = playlistItems.ToCatalogItems();
            }
            return catalogItems;
        }
        public async Task<SinglePlaylist?> GetSinglePlaylist(string pid, CancellationToken cancellationToken = default)
        {
            if (client is null) return null;
            if(await client.GetPlaylist(pid) is SinglePlaylist playlist)
            {
                return playlist;
            }
            return null;
        }
        public async Task<IReadOnlyList<MusicPlayerCatalogItem>> GetCatalogAsync(string instanceId, MusicPlayerCatalogItemKind kind, string? filter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
