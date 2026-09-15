using BeefWeb.Events;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeefWeb
{
    internal class BeefWebEvents(Func<IIntegrationContext> getContext )
    {
        public readonly TrackChangedEvent TrackChanged = new(getContext);
        public IReadOnlyList<EventDefinition> GetEvents()
        {
            return [
                TrackChanged.EventDefinition
            ];
        }
    }
}
