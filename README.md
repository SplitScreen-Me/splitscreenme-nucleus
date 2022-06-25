# Nucleus Co-op

![screenshot of the app UI](https://github.com/SplitScreen-Me/splitscreenme-www/blob/master/static/img/hk211.png?raw=true)

Nucleus Co-op is a free and open source tool for Windows that allows split-screen play on many games that do not initially support it, the app purpose is to make it as easy as possible for the average user to play games locally using only one PC and one game copy.

This repo is a new and improved official version of the Nucleus Co-op application and is part of the [SplitScreen.Me](https://www.splitscreen.me/docs/what-is-splitscreen-me) github organization. This new version is based off of the [Nucleus Co-op Alpha 8 Mod](https://github.com/ZeroFox5866/nucleuscoop) build and features a ton of enhancements, such as:

- New overhauled and customizable user interface with support for themes, game covers and screenshots.
- Full support for different monitor scales, UI scaling issues at more than 100% desktop scale are finally fixed (and all other issues/bugs related to it).
- New player and input order processing (left to right).
- New player nickname assignation.
- New optional splitscreen divisions setting (visually  similar to native splitscreen games).
- Massive increase to the amount of compatible games, ~500 as of now.
- Much more game handlers customization.
- Many quality of life improvements and tons of bug fixes.
- And so much more!

View the full list of features/changes and [changelog](CHANGELOG.md), as well as the [known issues](KNOWN_ISSUES.md). Download latest [Nucleus Co-op here](https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases).

## Disclaimer

Nucleus Co-op was originally created by Lucas Assis.

- Github link to the original project: <https://github.com/lucasassislar/nucleuscoop>
- Official website: <https://www.splitscreen.me/docs/what-is-splitscreen-me/>
- Nucleus Co-op FAQ: <https://www.splitscreen.me/docs/faq>
- Subscribe the official Nucleus Co-op subreddit: <https://www.reddit.com/r/nucleuscoop/>
- Join the official Nucleus Co-op Discord server: <https://discord.gg/QDUt8HpCvr>

## How does Nucleus Co-op work?

Nucleus Co-op symlinks and opens multiple instances of the same game files (sometimes mutex killing is required for that, among other methods) that will only answer to one specific gamepad (we do this via Nucleus Co-op custom xinput libraries or xinput plus dlls) and connects those instances via LAN or online multiplayer emulation (Goldberg, Nemirtingas emulators etc.), all while making sure the game windows have focus so they can be playable at the same time with multiple controllers or that the instances are playable even in the background. Nucleus then resizes, removes borders and repositions the game windows so you can have synthetic split-screen to play locally with your friends!

Note that Nucleus does not add multiplayer or co-op to single player games, the game needs to already have some form of online or LAN multiplayer, or another way to connect the instances, like via mods for example.

## Installation

1. Download latest [release](https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases).
2. Extract .zip archive to a non-restrictive folder, that all users have access to (i.e. do NOT extract to Program Files, Desktop, or your Documents folder, to name a few). The root folder that contains majority of your games is a good choice (e.g. C:\).

## Prerequisites

- .NET Framework 4.7.2 or higher  
- Microsoft Visual C++ 2015-2019 Redistributable (both x86 and x64)

## How can you contribute?

### Give Feedback

Please report any bugs you may find and provide any feedback you have regarding the app. We are always open to suggestions and we want to make split-screen available for even more games, for everyone! Don't forget to create game handlers and share!

You can find us in the Nucleus subreddit/discord as well as the Splitscreen Dreams discord, a special community created specifically to split-screen games by any means.

### Donate

In addition, we accept donations should you wish to support our endevor. It is greatly appreciated but completely voluntary, we will continue our best to help the community and enhance this project.

Thank you ^_^

### Make Handlers

If you want to contribute in a meaningful way, a good way to do so is to create a [handler](https://www.splitscreen.me/docs/handlers) for a game that you want to play splitscreen. You can see the full [handler documentation](HANDLER_DOCUMENTATION.md) in this repo, and/or watch the guide on how to [create your handler](https://www.splitscreen.me/docs/create-handlers) in the docs on our [website](https://splitscreen.me).

### Contribute with code

The app is built in [C#](https://docs.microsoft.com/en-us/dotnet/csharp/) and it uses [WinForms](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/?view=netdesktop-6.0) for the UI.
> More info will come here shortly.

## Credits

Big thanks to all the amazing [contributors](CONTRIBUTORS.md)!
