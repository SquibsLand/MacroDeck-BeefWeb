using MacroDeck.BeefWeb.Music.API;
using MacroDeck.BeefWeb.Music.API.Responses;
using MacroDeck.Sdk.Logging;
using MacroDeck.Sdk.MusicPlayer;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace MacroDeck.BeefWeb.Music
{
    internal sealed class BeefWebPlayer : IMusicPlayer
    {
        private static readonly ILogger _logger =
        IntegrationLog.For<BeefWebPlayer>(BeefWebIntergration.IntegrationId);
        public MusicPlayerState LastState { get; private set; } = MusicPlayerState.Disconnected;

        public BeefWebClient? client { get; private set; }

        public void init(string serverAddress,
            int serverPort,
            PlayerType playerType,
            string username = default!,
            string password = default!)
        {
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
                MusicPlayerArtwork? artwork = await client.GetArtwork();
                _logger.Debug(artwork?.Data.ToString());
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
            _logger.Warning($"Client is {client != null}");
            if (client == null) return IsUnavailable;
            else {
                Player? player = await client.GetPlayer();
                _logger.Warning($"Player is {player!= null}");
                if (player == null) return IsUnavailable;
                else return player.ToMusicPlayerState();
            }
        }

        public Task NextAsync(CancellationToken cancellationToken = default)
        {
            _logger.Debug("Next Command");
            throw new NotImplementedException();
        }

        public Task PauseAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task PlayAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task PlayItemAsync(MusicPlayerCatalogItem item, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task PreviousAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetRepeatModeAsync(RepeatMode mode, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetShuffleAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetVolumeAsync(int volumePercent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task TogglePlayPauseAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
