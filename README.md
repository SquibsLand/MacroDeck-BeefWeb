# MacroDeck BeefWeb
![Dynamic XML Badge](https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2FSquibsLand%2FMacroDeck-BeefWeb%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FMacroDeckSdkVersion%5B1%5D&label=MacroDeck%20SDK)
![GitHub License](https://img.shields.io/github/license/squibsland/MacroDeck-BeefWeb)
![GitHub Release](https://img.shields.io/github/v/release/squibsland/MacroDeck-BeefWeb?include_prereleases)
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/squibsland/MacroDeck-BeefWeb)

This plugin for [MacroDeck](https://macro-deck.app) allows for you to control your [Foobar2000](https://www.foobar2000.org) or [DeaDBeeF](https://deadbeef.sourceforge.io) music player! This plugin is possible using the [Beefweb API](https://github.com/hyperblast/beefweb) by [hyperblast](https://github.com/hyperblast). 

## Links

- [Latest Release](https://github.com/SquibsLand/MacroDeck-BeefWeb/releases/latest)
- [Wiki](https://github.com/SquibsLand/MacroDeck-BeefWeb/wiki)
- [Bug Report](https://github.com/SquibsLand/MacroDeck-BeefWeb/issues/new/choose)

## Guide

> [!NOTE]
> This guide has been verified for [v0.0.4](https://github.com/SquibsLand/MacroDeck-BeefWeb/releases/tag/v0.0.4)

This guide assumes that you already have a Foobar2000 or DeaDBeeF instillation. 

### Beefweb Setup

1. Download the latest version of the Beefweb fb2k-component by hyperblast 
2. Follow the instructions on Beefweb regarding installing for your player: https://github.com/hyperblast/beefweb#how-to-install
3. Restart the application after instillation
4. Go to Preferences, then Beefweb Remote Control (Position may vary depending on player & version. Check under tools as well). Use the following settings:
    - Port: `8880` (You can change this to another port if needed)
    - Allow remote connections: ✓
    - Music Directories: Add all needed directories
    - Require Authentication: - This is optional (Only supported in v0.0.4 and up)
5. Navigate to the sub menu within Beefweb Remote Control for Permissions and use the following settings:
    - Changing Playlists: ✓
    - Changing Output Device: - (Not supported in v0.0.1, but enabling will cause no issues)
    - Changing Default Web Interface: - (Only use if you have a need for it)

> [!NOTE]
> If your music player is on a different device than you MacroDeck Host, you may need to enable Access-Control-Allow-Origin, please read the [advanced config](https://github.com/hyperblast/beefweb/blob/master/docs/advanced-config.md) for more info

### Integration Setup

1. Navigate to the Macro Deck Store and download the latest version for `com.squibsland.beefweb`. Or download the latest release from https://github.com/SquibsLand/MacroDeck-BeefWeb/releases, then install manually
2. Go to Integrations, find Beefweb, then select it and add/edit a configuration. Use the following settings:
<img height="500" alt="image" src="https://github.com/user-attachments/assets/dc60aba8-d421-4195-a38f-d1aef955d5e8" />

- Server Address: This is the address of your machine that you want to connect to. This can be your own machine or a different machine.
    - `127.0.0.1` OR `localhost`: This is your own machine, it is recommended to use these.
    - `192.168.x.x`: This is a LAN address, local to your network.
    - `my-pc.local` mDNS for local devices, this maps to a LAN address
    - IPv6 is also allowed, although untested
- Server Port: This is the port for the server, you should not need to change this unless you adjusted the server settings in your player.
    - Recommended Value: `8880`
- Player Type: Simply choose what your player is.
- Username: This is the username set within your beefweb config. This field is optional, and will not prevent usage if set in macro deck and when authentication is disabled on the server.
- Password: Please note that although the password is saved an encrypted, because the [BeefWeb API](https://github.com/hyperblast/beefweb) uses HTTP and not HTTPS it will not be encrypted over your network. **Use a unique password!**

## License

MIT - see [LICENSE](LICENSE). Macro Deck itself is licensed under Apache 2.0.


## Developer Documentation

- [Plugin development docs](https://github.com/Macro-Deck-App/Macro-Deck-3/tree/main/docs/plugin-development)
- [`plugin-hosting.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/plugin-hosting.md) - the builder API, registration modes, the artifact format and every `MACRO_DECK_PLUGIN_*` variable
- [`sdk-reference.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/sdk-reference.md) - every interface and record you build against
- [`cli.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/cli.md) - every CLI command and option
- [`conformance.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/conformance.md) - the conformance suite and its check ids
- [`analyzers.md`](https://github.com/Macro-Deck-App/Macro-Deck-3/blob/main/docs/plugin-development/analyzers.md) - the compile-time diagnostics
