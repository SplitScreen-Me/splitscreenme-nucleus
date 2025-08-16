<div align="center">
    <img align="left" width="40" height="44" src="https://github.com/SplitScreen-Me/splitscreenme-www/blob/master/static/img/NucleusCoop1.png">
     <h1 align="left">Nucleus Co-op</h1>  
  </br>
</div>

![v2 3 0](https://github.com/user-attachments/assets/db89456a-714a-4bf6-ac78-53c3fa1d023c)

<div align="center">
    <a href="https://discord.gg/QDUt8HpCvr"><img src="https://img.shields.io/discord/142649962839277568.svg?style=for-the-badge" alt="Discord server"/></a>
    <a href="https://patreon.com/nucleus_coop"><img src="https://img.shields.io/badge/sponsor-Patreon-blue?style=for-the-badge" alt="patreon"/></a>
      </br>
</div>

# What is Nucleus Co-op?

Nucleus Co-op is a free and open source tool for Windows that allows split-screen play on many games that do not initially support it, the app purpose is to make it as easy as possible for the average user to play games locally using only one PC and one game copy. 

This repo is a new and improved official version of the Nucleus Co-op application and is part of the [SplitScreen.Me](https://www.splitscreen.me/docs/what-is-splitscreen-me) github organization. This new version is based off of the [Nucleus Co-op Alpha 8 Mod](https://github.com/ZeroFox5866/nucleuscoop) build and features a ton of enhancements, such as:

- New overhauled and customizable user interface with support for themes, game covers and screenshots.
- Full support for different monitor scales, UI scaling issues at more than 100% desktop scale are finally fixed (and all other issues/bugs related to it).
- New player and input order processing.
- New player nickname assignation.
- New optional splitscreen divisions setting (visually similar to native splitscreen games).
- Massive increase to the amount of compatible games, 750+ as of now.
- A lot more options for game handlers customization.
- Many quality of life improvements and ton of bug fixes.
- And so much more!

View the full list of features/changes and changelog in the Readme.txt inside Nucleus Co-op or in the releases page. Download latest [Nucleus Co-op here](https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases). 

# How does Nucleus Co-op work?
Nucleus Co-op symlinks and opens multiple instances of the same game files (sometimes mutex killing is required for that, among other methods) that will only answer to one specific gamepad (we do this via Nucleus Co-op custom xinput libraries or xinput plus dlls) and connects those instances via LAN or online multiplayer emulation (Goldberg, Nemirtingas emulators etc.), all while making sure the game windows have focus so they can be playable at the same time with multiple controllers or that the instances are playable even in the background. Nucleus Co-op then resizes, removes borders and repositions the game windows so you can have synthetic split-screen to play locally with your friends!

Note that Nucleus does not add multiplayer or co-op to single player games, the game needs to already have some form of online or LAN multiplayer, or another way to connect the instances, like via mods for example.

# 📚 Prerequisites:

- .NET Framework 4.7.2 or higher: [Microsoft direct download link](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net472-web-installer).
  
- Microsoft Visual C++ 2015-2019 Redistributable (both x86 and x64): [Microsoft direct download x86](https://aka.ms/vs/17/release/vc_redist.x86.exe), [Microsoft direct download x64](https://aka.ms/vs/17/release/vc_redist.x64.exe).

- Microsoft Edge WebView2 (Only if the downloader gets stuck on loading screen): [Microsoft Edge WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/consumer/?form=MA13LH ).

# ⚒ Installation:
1. Download latest [release](https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases). 
2. [Installation Guide](https://www.splitscreen.me/docs/installation).

# 🤝 How can you contribute?
If you would like to learn all about contributing to this project, whether it's bug reporting or working on the codebase, check out the [Nucleus Co-op Contributing Guide](CONTRIBUTING.md)!

# 🔎 Website & FAQ

- [Official website](https://www.splitscreen.me/docs/what-is-splitscreen-me/)
- [Nucleus Co-op FAQ](https://www.splitscreen.me/docs/faq)
  
#  👥 Social

- [Subscribe to the official Nucleus Co-op subreddit](https://www.reddit.com/r/nucleuscoop/)
- [Join the official Nucleus Co-op Discord server](https://discord.gg/QDUt8HpCvr)

# 🧡 Patreon supporters
- [All time Patreon supporters list](https://github.com/SplitScreen-Me/splitscreenme-patreon/tree/main).
  
# 📄 Credits
- Original Nucleus Co-op Project: [Lucas Assis (lucasassislar)](https://github.com/lucasassislar)  
- Nucleus Co-op Alpha 8 Mod : [ZeroFox](https://github.com/ZeroFox5866)  
- Proto Input, USS, multiple keyboards/mice & hooks: [Ilyaki](https://github.com/Ilyaki)  
- Official Nucleus Co-op 2.0 and Up: [Mikou27](https://github.com/Mikou27) 
- Website & handler API: [r-mach](https://github.com/r-mach)  
- Handlers development, Nucleus Co-op general testing, feedback and improvement: [Talos91](https://github.com/Talos910), [PoundlandBacon](https://github.com/PoundlandBacon), [Pizzo](https://github.com/Bizzo499), [maxine64](https://github.com/Maxine202) and many more.

Additional credits to all original developers of the third party utilities Nucleus Co-op uses:
- Mr_Goldberg ([Goldberg Emulator](https://gitlab.com/Mr_Goldberg/goldberg_emulator))
- syahmixp ([SmartSteamEmu](https://github.com/MAXBURAOT/SmartSteamEmu))
- atom0s ([Steamless](https://github.com/atom0s/Steamless))
- EJocys ([x360ce](https://github.com/x360ce/x360ce))
- 0dd14 Lab ([Xinput Plus](https://sites.google.com/site/0dd14lab/xinput-plus))
- r1ch ([ForceBindIP](https://r1ch.net/projects/forcebindip))
- HaYDeN ([Flawless Widescreen](https://www.flawlesswidescreen.org/))
- briankendall ([devreorder](https://github.com/briankendall/devreorder))
- VerGreeneyes ([DirectXWrapper](https://community.pcgamingwiki.com/files/file/87-the-bards-tale-2005-windowed-mode/))
- wizark952 (dinput8 blocker)
- Nemirtingas ([Epic\Galaxy Emulator](https://gitlab.com/Nemirtingas) & [OpenXinput](https://github.com/Nemirtingas/OpenXinput))
- Josivan88 (SplitCalculator)
- darkreader ([Dark Reader extension](https://github.com/darkreader/darkreader?tab=readme-ov-file)) 
  
Special thanks to the SplitScreenDreams discord community, this wouldn't have been possible without all their contributions.

