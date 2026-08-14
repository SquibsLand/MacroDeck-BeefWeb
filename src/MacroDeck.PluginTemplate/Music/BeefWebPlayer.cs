using MacroDeck.Sdk.Logging;
using MacroDeck.Sdk.MusicPlayer;
using System;
using System.Collections.Generic;
using System.Text;
using Serilog;

namespace MacroDeck.BeefWeb.Music
{
    internal sealed class BeefWebPlayer : IMusicPlayer
    {
        private static readonly ILogger _logger =
        IntegrationLog.For<BeefWebPlayer>(BeefWebIntergration.IntegrationId);
        public MusicPlayerState LastState { get; private set; } = MusicPlayerState.Disconnected;

        public Task<MusicPlayerArtwork?> GetArtworkAsync(string artworkId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<MusicPlayerState> GetStateAsync(CancellationToken cancellationToken = default)
        {
       

            return new MusicPlayerState
            {
                IsConnected = true,
                TrackName = "Example",
            };
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
