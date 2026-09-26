using MacroDeck.Localization;
using MacroDeck.Sdk.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BeefWeb.Actions
{
    // TODO: Currently this only does logic for all keys having the same value, this should be changed.
    public abstract class BeefWebExecutor(BeefWebIntergration integration) : IActionExecutor
    {
        protected BeefWebIntergration Intergration => integration;

        public abstract Task<ActionResult> ExecuteAsync(ActionExecutionContext context);
        protected static bool IsValid<T>(ActionExecutionContext context, string key, [NotNullWhen(true)] out T? result)
        {
            context.Parameters.TryGetValue(key, out object? value);
            
            if (value is not null && value is T typedValue)
            {
                result = typedValue;
                return true;
            }

            result = default;
            return false;
        }
    }

    internal static class BeefWebCommonParams
    {
        internal record Paramater(string Id, Func<string?, bool?, ActionParameter> Function) {
        
            public ActionParameter Create(string? description = null, bool? required = null)
            {
                return Function(description, required);
            }
        }
        internal static Paramater Player = new("player", (description, required) => ActionParameter.DynamicChoice("player", label: "Player", required: required ?? true, description: description));
        internal static Paramater PlaylistId = new("playlist-id", (description, required) => ActionParameter.DynamicChoice("playlist-id", label: "Playlist", required: required ?? true, description: description));
        internal static Paramater TrackItem = new("track-item",(description, required) => ActionParameter.DynamicChoice("track-item", label: "Song", required: required ?? true, description: description));
    }

    internal abstract class BaseBeefWebAction<TExecutor>(BeefWebIntergration integration) : IActionDefinition, IDynamicOptionsActionDefinition
        where TExecutor : BeefWebExecutor
    {
        protected BeefWebIntergration Integration => integration;
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract IReadOnlyList<ActionParameter> Parameters { get; }

        LocalizedText IActionDefinition.Name => Name;

        LocalizedText IActionDefinition.Description => Description;


        protected abstract TExecutor CreateNewExecutor(BeefWebIntergration intergration);
        IActionExecutor IActionDefinition.CreateExecutor() => CreateNewExecutor(Integration);


        public abstract Task<DynamicOptionsResult> GetDynamicOptionsAsync(DynamicOptionsContext context, CancellationToken cancellationToken);

        protected virtual InvalidDataException UnhandledParam(string param) => new($"Dynamic paramater {param} does not have logic for options");

        protected virtual async Task<DynamicOptionsResult> GetPlayerOptions() => new(){ Options = Integration.InstanceOptions() };
        protected virtual async Task<DynamicOptionsResult> GetPlaylistOptions() => new(){ Options = await Integration.GetPlaylistOptions() ?? [] };

        protected virtual async Task<DynamicOptionsResult> GetPlaylistItemsOptions(string pid) => new() { Options = await Integration.GetPlaylistItemsOptions(pid) ?? []};
}


}
