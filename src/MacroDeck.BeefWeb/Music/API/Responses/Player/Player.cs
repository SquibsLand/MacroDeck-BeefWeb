using MacroDeck.Sdk.MusicPlayer;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MacroDeck.BeefWeb.Music.API.Responses.Player
{
    enum VolumeType { db, linear, upDown }
    enum LocalPlaybackState { stopped, playing, paused }
    internal class PlayerRoot : BeefWebAPICall<PlayerRoot, Player>, IApiResource<Player>
    {
        public static string ApiPath => "player";

        public static Dictionary<string, string?> Query => new() { ["columns"] = PlayerColumns.GetColumnQuery() };

        public required Player player { get; init; }

        public override Player Get()
        {
            return player;
        }
    }

    internal partial class Player : IJsonOnDeserialized
    {
        public required PlayerInfo info { get; init; }
        public required ActiveItem activeItem { get; init; }

        [JsonPropertyName("playbackState")]
        public required string rawPlaybackState { get; init; }
        [JsonIgnore]
        public PlaybackState playbackState { get; set; }

        public required Volume volume { get; init; }

        public int playbackMode { get; init; }
        public string[]? playbackModes { get; init; }

        public bool ShuffleEnabled { get; set; }
        public RepeatMode RepeatMode { get; set; }

        public MusicPlayerState ToMusicPlayerState()
        {
            if (activeItem.columns == null) return new() { IsUnavailable = true };
            return new()
            {
                IsConnected = true,
                TrackName = activeItem.columns.title,
                AlbumName = activeItem.columns.album,
                Artists = [activeItem.columns.artist],
                PlaybackState = playbackState,
                Position = activeItem.position,
                Duration = activeItem.duration,
                VolumePercent = (int) MathF.Round(volume.toPercent()),
                DeviceName = info.name,
                DeviceType = $"{info.version} ({info.pluginVersion})",
                RepeatMode = RepeatMode,
                ShuffleEnabled = ShuffleEnabled,
                ArtworkId = ArtworkId()

            };
        }

        public int? FindPlaybackMode(RepeatMode newMode)
        {
            if(playbackModes is null || playbackModes.Length == 0) return null;
            string[] knownEquivelents;

            switch(newMode)
            {
                case RepeatMode.Off:
                    knownEquivelents = ["default", "none", "off"];
                    break;
                case RepeatMode.Track:
                    knownEquivelents = ["repeat (track)", "track"];
                    break;
                case RepeatMode.Context:
                    knownEquivelents = ["repeat (playlist)", "playlist"];
                    break;
                default:
                    return null;
            }
            for(int i=0; i<playbackModes.Length; i++) 
            {
                string mode = playbackModes[i].ToLowerInvariant();
                if (knownEquivelents.Contains(mode)) return i;
                continue;
            }
            return null;
        }

        private static RepeatMode GetRepeatMode(int index, string[] modes, StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            
            string mode = modes[index];

            if (mode.Contains("repeat", comp))
            {
                return mode.Contains("track", comp) ? RepeatMode.Track : RepeatMode.Context;
            }
            return RepeatMode.Off;
        }

        public string ArtworkId()
        {

            string uri = activeItem.GetArtworkUri();
            byte[] bytes = Encoding.UTF8.GetBytes(uri);
            return Convert.ToBase64String(bytes);
        }

        private static bool GetShuffleEnabled(int index, string[] modes,
                                              StringComparison comp = StringComparison.OrdinalIgnoreCase) 
            => modes[index].Contains("repeat") || modes[index].Contains("shuffle");
        public void OnDeserialized()
        {
            Enum.TryParse(rawPlaybackState, true, out LocalPlaybackState value);

            playbackState = value switch
            {
                LocalPlaybackState.stopped => PlaybackState.Stopped,
                LocalPlaybackState.paused => PlaybackState.Paused,
                LocalPlaybackState.playing => PlaybackState.Playing,
                _ => throw new NotImplementedException(),
            };

            if (playbackModes is null || playbackModes.Length <= 0) { }
            else
            {
                RepeatMode = GetRepeatMode(playbackMode, playbackModes);
                ShuffleEnabled = GetShuffleEnabled(playbackMode, playbackModes);

            }
        }
    }

    internal class PlayerInfo
    {
        public required string name { get; init; }
        public required string title { get; init; }
        public required string version { get; init; }
        public required string pluginVersion { get; init; }
    }
    internal sealed class ActiveItem : IJsonOnDeserialized
    {
        public string playlistId { get; init; }
        public int playlistIndex { get; init; }
        public int index { get; init; }
        [JsonPropertyName("position")]
        public float rawPosition { get; init; }
        [JsonPropertyName("duration")]
        public float rawDuration { get; init; }
        [JsonIgnore]
        public TimeSpan? position { get; set; }
        [JsonIgnore]
        public TimeSpan? duration { get; set; }

        [JsonPropertyName("columns")]
        public required List<JsonElement> rawColumns { get; init; }

        [JsonIgnore]
        public PlayerColumns? columns { get; private set; }

        public void OnDeserialized()
        {
            columns = new PlayerColumns(rawColumns);

            position = TimeSpan.FromSeconds(rawPosition);
            duration = TimeSpan.FromSeconds(rawDuration);
        }

        public string GetArtworkUri() => $"artwork/{playlistId}/{index}";
      

    }

    internal sealed class PlayerColumns : Columns<PlayerColumns>, IColumns
    {
        public static string[] ColumnsQuery => [
            "%isplaying%",
            "%ispaused%",
            "%album artist%",
            "%album%",
            "%artist%",
            "%title%",
            "%track number%",
            "%length_seconds%",
            "%playback_time_seconds%",
            "%rating%",
        ];


        public bool isPlaying { get; init; }
        public bool isPaused { get; init; }
        public string albumArtist { get; init; }
        public string album { get; init; }
        public string artist { get; init; }
        public string title { get; init; }
        public int? trackNumber { get; init; }
        public int? length { get; init; }
        public int? elapsed { get; init; }
        public int? rating { get; init; }

        public PlayerColumns(List<JsonElement> columns) : base(columns)
        {
            isPlaying = NextString() == "1";
            isPaused = NextString() == "1";
            albumArtist = NextString();
            album = NextString();
            artist = NextString();
            title = NextString();
            trackNumber = NextInt();
            length = NextInt();
            elapsed = NextInt();
            rating = NextInt();
        }
    }
    internal class Volume : IJsonOnDeserialized
    {

        public bool isMuted { get; init; }
        public float max { get; init; }
        public float min { get; init; }
        [JsonPropertyName("type")]
        public required string rawType { get; init; }
        [JsonIgnore]
        public VolumeType type { get; private set; }
        public float value { get; init; }

        public float toPercent()
        {
            double result = type switch
            {
                VolumeType.db => new DecibleHandler(min, max).ToPercent(value),
                VolumeType.linear => throw new NotImplementedException(),
                VolumeType.upDown => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

             return MathF.Round((float)result * 100, MidpointRounding.AwayFromZero);
  

        }

        public void OnDeserialized()
        {
            Enum.TryParse(rawType, true, out VolumeType result);
            type = result;
        }
    }
    internal abstract class VolumeHandler
    {
        protected readonly double min;
        protected readonly double max;

        protected VolumeHandler(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        public abstract double ToPercent(double value);
        public abstract double FromPercent(double value);
    }
    internal class DecibleHandler(double min, double max, double intensity = 0.45) : VolumeHandler(min, max)
    {
        private readonly double intensity = intensity;

        public override double FromPercent(double percent)
        {
            double p = NormalizeToPercent(percent);

            double minGain = Math.Pow(10, min / 20.0);
            double maxGain = Math.Pow(10, max / 20.0);

            double normalized = Math.Pow(p, 1.0 / intensity);
            double valueGain = normalized * (maxGain - minGain) + minGain;

            return 20.0 * Math.Log10(valueGain);
        }

        public override double ToPercent(double value)
        {

            double minGain = Math.Pow(10, min / 20);
            double maxGain = Math.Pow(10, max / 20);
            double valueGain = Math.Pow(10, value / 20);

            double normalized = (valueGain - minGain) / (maxGain - minGain);

            double percentage = Math.Pow(normalized, intensity);
            return percentage;
        }

        private static double NormalizeToPercent(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
                throw new ArgumentOutOfRangeException(nameof(x));

            if (x >= 0.0 && x <= 1.0)
                return Math.Clamp(x, 0.0, 1.0);
            double y = x / 100.0;
            return Math.Clamp(y, 0.0, 1.0);
        }
    }
    internal class Permissions
    {
        public bool changePLaylists { get; init; }
        public bool changeOutput { get; init; }
        public bool changeClientConfig { get; init; }
    }

}