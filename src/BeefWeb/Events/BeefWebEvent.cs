using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeefWeb.Events
{
    internal abstract class BeefWebEvent<T>
    {
        public readonly string Id;
        public readonly string Name;
        public string? Description;
        public EventDefinition EventDefinition;

        protected readonly Func<IIntegrationContext?> GetContext;

        public BeefWebEvent(
            Func<IIntegrationContext?> getContext,
            string id, string name) {
            GetContext = getContext;
            Id = id;
            Name = name;
            EventDefinition = Create();
        }

        internal EventDefinition Create() => new() { Id = Id, Name = Name, ConfigurationParameters = GetConfigurationParameters(), PayloadParameters = GetPayloadParameters() };

        internal abstract void Publish(T data);

        protected virtual IReadOnlyList<ActionParameter> GetConfigurationParameters() => [];
        protected virtual IReadOnlyList<ActionParameter> GetPayloadParameters() => [];

    }
}
