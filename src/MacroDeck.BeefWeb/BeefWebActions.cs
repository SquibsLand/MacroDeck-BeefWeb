using MacroDeck.BeefWeb.Actions;
using MacroDeck.BeefWeb.Music;
using MacroDeck.BeefWeb.Music.API.Posts.Player;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.MusicPlayer;
using MacroDeck.Sdk.MusicPlayer.Actions;
using System;
using System.Collections.Generic;
using System.Text;


namespace MacroDeck.BeefWeb
{
    internal class BeefWebActions(BeefWebIntergration intergration, MusicPlayerResolver resolver, Func<IReadOnlyList<MusicPlayerInstance>> getInstances)
 
    {
        private readonly BeefWebIntergration intergration = intergration; 
        private readonly MusicPlayerResolver resolver = resolver;
        private readonly Func<IReadOnlyList<MusicPlayerInstance>> getInstances = getInstances;
        public IReadOnlyList<IActionDefinition> Get()
        {
        return [.. Common(), ..Custom(), ..Debuging()];
        }

        public IReadOnlyList<IActionDefinition> Common() => MusicPlayerActions.Common(resolver, getInstances);
        public IReadOnlyList<IActionDefinition> Custom() => [new SeekRelativeAction(intergration), new SetVolumeRelativeAction(intergration), new SetActivePlaylistAction(intergration)];
        internal IReadOnlyList<IActionDefinition> Debuging() => [new PlayItemAction(intergration)];

    }
}
