using MacroDeck.BeefWeb.Music;
using MacroDeck.Sdk.MusicPlayer;
using MacroDeck.Sdk.Variables;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MacroDeck.BeefWeb
{
    
    internal static class BeefWebVaribles
    {
        public static readonly string prefix = "beefweb_";

        internal static class Variables
        {
            public static readonly Variable<string?> Track = Variable.Text("current_track_name", s => s.TrackName);
            public static readonly Variable<string?> Artist = Variable.Text("current_artist", s => s.Artists.Count > 0 ? string.Join(", ", s.Artists) : null);
            public static readonly Variable<string?> Album = Variable.Text("current_album", s => s.AlbumName);
            public static readonly Variable<string?> State = Variable.Text("playback_state", s => s.PlaybackState.ToString().ToLowerInvariant());
            public static readonly Variable<bool?> Playing = Variable.Boolean("is_playing", s => s.PlaybackState == PlaybackState.Playing);
            public static readonly Variable<int?> Volume = Variable.Numeric("volume", s => s.VolumePercent);
            public static readonly Variable<float?> Duration = Variable.Numeric("track_duration", s => s.Duration is { } d ? (float)d.TotalMilliseconds / 1000 : null);
            public static readonly Variable<float?> Position = Variable.Numeric("current_position", s => s.Position is { } p ? (float)p.TotalMilliseconds / 1000 : null);
            // TODO: Add Percentage Logic
            public static readonly Variable<int?> ProgressPercentage = Variable.Numeric("progress_percentage", s => s.Position is not null && s.Duration is not null ? (int) MathF.Round((float) (s.Position.Value / s.Duration.Value) * 100) : null);
            // TODO: Add Album Art Logic
            public static readonly Variable<string?> AlbumArt = Variable.Text("album_art_url", s => null);
            public static readonly Variable<string?> DeviceName = Variable.Text("device_name", s => s.DeviceName);
            public static readonly Variable<string?> DeviceType = Variable.Text("device_type", s => s.DeviceType);

            public static readonly Variable<bool?> Shuffled = Variable.Boolean("shuffle_enabled", s => s.ShuffleEnabled);
            public static readonly Variable<string?> RepeatMode = Variable.Text("repeat_mode", s => s.RepeatMode.ToString().ToLowerInvariant());
            public static readonly Variable<bool?> Connected = Variable.Boolean("is_connected", s => s.IsConnected);
            private static readonly Dictionary<string, VariableDefinition> dictionary = new()
            {
                [Track.Key] = Track,
                [Artist.Key] = Artist,
                [Album.Key] = Album,
                [State.Key] = State,
                [Playing.Key] = Playing,
                [Volume.Key] = Volume,
                [Duration.Key] = Duration,
                [Position.Key] = Position,
                [ProgressPercentage.Key] = ProgressPercentage,
                [DeviceName.Key] = DeviceName,
                [DeviceType.Key] = DeviceType,
                [Shuffled.Key] = Shuffled,
                [RepeatMode.Key] = RepeatMode,
                [Connected.Key] = Connected,
            };
            private static readonly IReadOnlyDictionary<string, VariableDefinition> ByKey =
                dictionary;
            internal abstract record VariableDefinition(
                string Key,
                VariableType Type);
            internal sealed record Variable<T>(string Key, Func<MusicPlayerState, T> GetValue, VariableType VariableType) : VariableDefinition(Key, VariableType);

            internal static class Variable
            {
                public static Variable<string?> Text(
                    string key,
                    Func<MusicPlayerState, string?> getter) =>
                    new(key, getter, VariableType.Text);

                public static Variable<float?> Numeric(
                    string key,
                    Func<MusicPlayerState, float?> getter) =>
                    new(key, getter, VariableType.Numeric);
                public static Variable<int?> Numeric(string key,
                    Func<MusicPlayerState, int?> getter) =>
                    new(key, getter, VariableType.Numeric);

                public static Variable<bool?> Boolean(
                    string key,
                    Func<MusicPlayerState, bool?> getter) =>
                    new(key, getter, VariableType.Boolean);
            }
            public static T Get<T>(
                Variable<T> variable,
                MusicPlayerState state) =>
                    variable.GetValue(state);
            internal static object? GetByKey(
                string key,
                MusicPlayerState state)
            {
                if (!ByKey.TryGetValue(key, out var variable))
                    return null;

                return variable switch
                {
                    Variable<string?> v => v.GetValue(state),
                    Variable<float?> v => v.GetValue(state),
                    Variable<int?> v => v.GetValue(state),
                    Variable<bool?> v => v.GetValue(state),

                    _ => throw new NotSupportedException(
                        $"Unsupported variable type: {variable.GetType()}")
                };
            }
        }
        public static IReadOnlyList<ProvidedVariable> Declare(string instanceKey) {
            string full_prefix = prefix + instanceKey; 
            return [
                CreateVariable(full_prefix, Variables.Track, RefreshInterval: TimeSpan.FromSeconds(2)),
                CreateVariable(full_prefix, Variables.Artist, RefreshInterval: TimeSpan.FromSeconds(2)),
                CreateVariable(full_prefix, Variables.Album, RefreshInterval: TimeSpan.FromSeconds(2)),
                CreateVariable(full_prefix, Variables.State, RefreshInterval: TimeSpan.FromSeconds(2)),
                CreateVariable(full_prefix, Variables.Playing, RefreshInterval: TimeSpan.FromSeconds(2)),
                CreateVariable(full_prefix, Variables.Volume,
                    DecimalPlaces: 0,
                    RefreshInterval: TimeSpan.FromSeconds(2)),
                CreateVariable(full_prefix, Variables.Duration,
                    DecimalPlaces: 0,
                    RefreshInterval: TimeSpan.FromSeconds(2)),
                CreateVariable(full_prefix, Variables.Position,
                    DecimalPlaces: 0,
                    RefreshInterval: TimeSpan.FromSeconds(1)),
                CreateVariable(full_prefix, Variables.ProgressPercentage,
                    DecimalPlaces: 0,
                    RefreshInterval: TimeSpan.FromSeconds(1)),
                CreateVariable(full_prefix, Variables.AlbumArt, RefreshInterval: TimeSpan.FromSeconds(2)),
                CreateVariable(full_prefix, Variables.DeviceName, RefreshInterval: TimeSpan.FromSeconds(5)),
                CreateVariable(full_prefix, Variables.DeviceType, RefreshInterval: TimeSpan.FromSeconds(5)),
                CreateVariable(full_prefix, Variables.Shuffled, RefreshInterval: TimeSpan.FromSeconds(5)),
                CreateVariable(full_prefix, Variables.RepeatMode, RefreshInterval: TimeSpan.FromSeconds(5)),
                CreateVariable(full_prefix, Variables.Connected, RefreshInterval: TimeSpan.FromSeconds(5))
            ];
        }
        public static object? Get(string key, MusicPlayerState state) => Variables.GetByKey(key, state);
        private static ProvidedVariable CreateVariable<T>(string prefix,
                                                       Variables.Variable<T> variable,
                                                       int? DecimalPlaces = null,
                                                       TimeSpan? RefreshInterval = null) => new($"{prefix}_{variable.Key}", variable.Type, DecimalPlaces, RefreshInterval);

    }
}
