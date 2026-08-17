using MacroDeck.BeefWeb.Music.API;
using MacroDeck.BeefWeb.Music.API.Responses;
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

namespace MacroDeck.BeefWeb.Music.API.Responses
{
    enum VolumeType { db, linear, upDown }
    enum LocalPlaybackState { stopped, playing, paused }
    internal class PlayerRoot : BeefWebAPICall<PlayerRoot, Player>, IApiResource<Player>
    {
        public static string ApiPath => "player";

        public static Dictionary<string, string?> Query => new() { ["columns"] = Columns.GetColumnQuery() };

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
                VolumePercent = volume.toPercent(),
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
            if (activeItem.columns is null) return "unknown";
            string full = $"{activeItem.columns.album}.{activeItem.columns.artist}.{activeItem.columns.title}";

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(full);
            return System.Convert.ToBase64String(bytes);
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
        public Columns? columns { get; private set; }

        public void OnDeserialized()
        {
            columns = new Columns(rawColumns);

            position = TimeSpan.FromSeconds(rawPosition);
            duration = TimeSpan.FromSeconds(rawDuration);
        }

      

    }

    internal sealed class Columns
    {
        private static readonly string[] columnsQuery =
        {
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
        };

        public bool isPlaying { get; init; }
        public bool isPaused { get; init; }
        public string albumArtist { get; init; } = default!;
        public string album { get; init; } = default!;
        public string artist { get; init; } = default!;
        public string title { get; init; } = default!;
        public int trackNumber { get; init; }
        public int length { get; init; }
        public int elapsed { get; init; }
        public int rating { get; init; }


        public Columns(List<JsonElement> columns)
        {
            if (columns.Count < columnsQuery.Length) return;
            isPlaying = columns[0].GetString() == "1";
            isPaused = columns[1].GetString() == "1";
            albumArtist = columns[2].GetString() ?? "?";
            album = columns[3].GetString() ?? "?";
            artist = columns[4].GetString() ?? "?";
            title = columns[5].GetString() ?? "?";
            trackNumber = int.Parse(columns[6].GetString());
            length = int.Parse(columns[7].GetString());
            elapsed = int.Parse(columns[8].GetString());
            rating = int.Parse(columns[9].GetString());
        }
        public static string GetColumnQuery()
        {
            return string.Join(",", columnsQuery);
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

        public int? toPercent()
        {
            float? result = type switch
            {
                VolumeType.db => DbToPercent(value, min, max),
                VolumeType.linear => null,
                VolumeType.upDown => null,
                _ => throw new NotImplementedException(),
            };

            if (result is null) return null;
            else
            {
                return (int) MathF.Round((float)result, MidpointRounding.AwayFromZero);
            }

        }

        private static float? DbToPercent(float value, float min, float max)
        {
            float denom = max - min;
            if (denom == 0) return null;

            float numer = value - min;
            float diff = numer / denom;

            return diff * 100f;
        }

        public void OnDeserialized()
        {
            Enum.TryParse(rawType, true, out VolumeType result);
            type = result;
        }
    }
    internal class Permissions
    {
        public bool changePLaylists { get; init; }
        public bool changeOutput { get; init; }
        public bool changeClientConfig { get; init; }
    }

}