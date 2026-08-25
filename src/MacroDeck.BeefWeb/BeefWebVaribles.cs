using MacroDeck.BeefWeb.Music;
using MacroDeck.BeefWeb.Music.API.Responses.Player;
using MacroDeck.Sdk.MusicPlayer;
using MacroDeck.Sdk.Variables;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static MacroDeck.BeefWeb.BeefWebVaribles.Variables;

namespace MacroDeck.BeefWeb
{
    
    internal static class BeefWebVaribles
    {
        public static readonly string prefix = "beefweb_";
        internal static class Variables
        {
            public static readonly Variable<MusicPlayerState, string?> Track = PlayerStateHelper.Text("current_track_name", s => s.TrackName);
            public static readonly Variable<MusicPlayerState, string?> Artist = PlayerStateHelper.Text("current_artist", s => s.Artists.Count > 0 ? string.Join(", ", s.Artists) : null);
            public static readonly Variable<MusicPlayerState, string?> Album = PlayerStateHelper.Text("current_album", s => s.AlbumName);
            public static readonly Variable<MusicPlayerState, string?> State = PlayerStateHelper.Text("playback_state", s => s.PlaybackState.ToString().ToLowerInvariant());
            public static readonly Variable<MusicPlayerState, bool?> Playing = PlayerStateHelper.Boolean("is_playing", s => s.PlaybackState == PlaybackState.Playing);
            public static readonly Variable<MusicPlayerState, int?> Volume = PlayerStateHelper.Numeric("volume", s => s.VolumePercent);
            public static readonly Variable<MusicPlayerState, float?> Duration = PlayerStateHelper.Numeric("track_duration", s => s.Duration is { } d ? (float)d.TotalMilliseconds / 1000 : null);
            public static readonly Variable<MusicPlayerState, float?> Position = PlayerStateHelper.Numeric("current_position", s => s.Position is { } p ? (float)p.TotalMilliseconds / 1000 : null);
            // TODO: Add Percentage Logic
            public static readonly Variable<MusicPlayerState, int?> ProgressPercentage = PlayerStateHelper.Numeric("progress_percentage", s => s.Position is not null && s.Duration is not null ? (int) MathF.Round((float) (s.Position.Value / s.Duration.Value) * 100) : null);
            // TODO: Add Album Art Logic
            public static readonly Variable<Player, string?> AlbumArt = PlayerDataHelper.Text("album_art_url", s => null);
            public static readonly Variable<MusicPlayerState, string?> DeviceName = PlayerStateHelper.Text("device_name", s => s.DeviceName);
            public static readonly Variable<MusicPlayerState, string?> DeviceType = PlayerStateHelper.Text("device_type", s => s.DeviceType);

            public static readonly Variable<MusicPlayerState, bool?> Shuffled = PlayerStateHelper.Boolean("shuffle_enabled", s => s.ShuffleEnabled);
            public static readonly Variable<MusicPlayerState, string?> RepeatMode = PlayerStateHelper.Text("repeat_mode", s => s.RepeatMode.ToString().ToLowerInvariant());
            public static readonly Variable<MusicPlayerState, bool?> Connected = PlayerStateHelper.Boolean("is_connected", s => s.IsConnected);
            public static readonly Variable<Player, int?> Rating = PlayerDataHelper.Numeric("rating", s => s.activeItem.columns?.rating);
            public static readonly Variable<Player, string?> Playlist = PlayerDataHelper.Text("playlist", s => s.activeItem.playlistId);


            private static readonly Dictionary<string, IVariable> ByKey =
                typeof(Variables)
                    .GetFields(
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic)
                    .Where(field => typeof(IVariable).IsAssignableFrom(field.FieldType))
                    .Select(field => field.GetValue(null))
                    .OfType<IVariable>()
                    .ToDictionary(
                        variable => variable.Key,
                        variable => variable);

            internal interface IVariable
            {
                string Key { get; }
                VariableType VariableType { get; }
                Type DataType { get; }
                Type ValueType { get; }
                double? RefreshInterval { get; init; }
                int? DecimalPlaces { get; init; }
                object? GetValue(object data);
            }
            internal sealed record Variable<TData, TValue>(
                string Key,
                Func<TData, TValue> GetValue,
                VariableType VariableType,
                double? RefreshInterval = 3,
                int? DecimalPlaces = null)
                : IVariable
            {
                public Type DataType => typeof(TData);
                public Type ValueType => typeof(TValue);
                object? IVariable.GetValue(object data)
                {
                    if (data is not TData typedData)
                    {
                        throw new ArgumentException(
                            $"Expected {typeof(TData).Name}, got {data.GetType().Name}.");
                    }

                    return GetValue(typedData);
                }
            }

            internal record VariableHelper<TData>
            {
                public static Variable<TData, TValue> Create<TValue>(
                    string key,
                    Func<TData, TValue> getter,
                    VariableType variableType) =>
                    new(key, getter, variableType);

                public static Variable<TData, string?> Text(
                    string key,
                    Func<TData, string?> getter) =>
                    Create(key, getter, VariableType.Text);

                public static Variable<TData, float?> Numeric(
                    string key,
                    Func<TData, float?> getter) =>
                    Create(key, getter, VariableType.Numeric);

                public static Variable<TData, int?> Numeric(
                    string key,
                    Func<TData, int?> getter) =>
                    Create(key, getter, VariableType.Numeric);

                public static Variable<TData, bool?> Boolean(
                    string key,
                    Func<TData, bool?> getter) =>
                    Create(key, getter, VariableType.Boolean);
            }
            internal sealed record PlayerStateHelper : VariableHelper<MusicPlayerState>;
            internal sealed record PlayerDataHelper : VariableHelper<Player>;

            internal static TValue? GetValueByKey<TData, TValue>(
                string key,
                TData data)
            {
                if (!ByKey.TryGetValue(key, out var variable))
                    return default;

                if (variable is not Variable<TData, TValue> typedVariable)
                {
                    throw new InvalidOperationException(
                        $"Variable '{key}' expects " +
                        $"{variable.DataType.Name} and returns " +
                        $"{variable.ValueType.Name}.");
                }

                return typedVariable.GetValue(data);
            }
            internal static object? GetValueByKey<TData>(
                string key,
                TData data)
            {
                if (!ByKey.TryGetValue(key, out var variable))
                    return null;

                if (variable.DataType != typeof(TData))
                {
                    throw new InvalidOperationException(
                        $"Variable '{key}' expects " +
                        $"{variable.DataType.Name}, but received " +
                        $"{typeof(TData).Name}.");
                }

                return variable.GetValue(data!);
            }
            internal static IVariable? GetVariableByKey(string key)
            {
                if (!ByKey.TryGetValue(key, out var variable))
                    return default;
                return variable;
            }
            internal static IReadOnlyList<ProvidedVariable> Declare(string key) => [.. ByKey.Select(item => CreateVariable(key, item.Value))];
        }

        public static IReadOnlyList<ProvidedVariable> Declare(string instanceKey) => Variables.Declare(prefix + instanceKey);

        private static TValue? TryGetBase<TData, TValue>(string key, TData data) => Variables.GetValueByKey<TData, TValue>(key, data);
        private static object? TryGetBase<TData>(string key, TData data) => Variables.GetValueByKey<TData>(key, data);
        public static TValue? TryGet<TValue>(string key, MusicPlayerState state) => TryGetBase<MusicPlayerState, TValue>(key, state);
        public static object? TryGet(string key, MusicPlayerState state) => TryGetBase(key, state);
        public static TValue? TryGet<TValue>(string key, Player player) => TryGetBase<Player, TValue>(key, player);
        public static object? TryGet(string key, Player player) => TryGetBase(key, player);
        public static IVariable? TryGetVariable(string key) => GetVariableByKey(key);
        private static ProvidedVariable CreateVariable(string prefix, IVariable variable) => 
            new($"{prefix}_{variable.Key}",
                variable.VariableType,
                variable.DecimalPlaces,
                variable.RefreshInterval is not null ? TimeSpan.FromSeconds(variable.RefreshInterval.Value) : null);

            

    }
}
