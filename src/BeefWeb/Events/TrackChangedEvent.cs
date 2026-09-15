using MacroDeck.BeefWeb.Music.API.Responses.Player;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace MacroDeck.BeefWeb.Events
{
    internal class TrackChangedEvent : BeefWebEvent<ActiveItem>
    {

        public TrackChangedEvent(Func<IIntegrationContext?> getContext) : base(getContext, "track-changed", "Track Changed") { }

        protected override IReadOnlyList<ActionParameter> GetPayloadParameters() => [
            ActionParameter.Text("track", "Track"),
            ActionParameter.Text("artist", "Artist")
        ];
        internal override void Publish(ActiveItem data)
        {
            GetContext()?.Events.Publish(Id, new Dictionary<string, object?>
            {
                ["track"] = data.columns?.title,
                ["artist"] = data.columns?.artist
            });
        }
    }
}
