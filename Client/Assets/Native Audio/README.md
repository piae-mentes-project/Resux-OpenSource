# Native Audio

**Thank you for purchasing a license of Native Audio!**

Lower audio latency via direct unmixed audio stream at native side.

So your Unity game outputs WAY slower audio than other apps even on the same device? Turns out, Unity adds as much as 79% of the total latency you hear. Your devices can do better.

Unity won't let you use only a subset of its hard-wired audio pipeline. But with Native Audio we can go native and make as much compromises as we want. Simplify, take dirty shortcuts, and aim for the lowest latency. Sacrificing functions and convenience that Unity was designed for as a friendly game engine.

## Links and contacts

- Website : https://exceed7.com/native-audio
- E-mail : 5argon@exceed7.com
- Discord : https://discord.gg/8gthuWA

## Requirements

Requires Unity 2019.4 LTS or newer. Only iOS and Android build target. (It compiles in other build targets as well, but the methods will logs error if called.)

Only works at runtime. You have to provide your own wrapper such as #if switches if you want to hear things non-natively. In Unity editor it counts as either Windows or macOS platform, and Native Audio has no Windows or macOS support. All methods throw [`NotSupportedException`](https://docs.microsoft.com/en-us/dotnet/api/system.notsupportedexception) if executed in non-supported platforms.

## Documentation 

Online documentation is available at : https://exceed7.com/native-audio.

Without internet access, there are also **offline documentation and samples** included in this package.

1. Unzip **Documentation~.zip** and **Samples~.zip** to get **unimporting folders** named `Documentation~` and `Samples~`.
2. Inside `Documentation~`, you can read an entire documentation website completely offline with any Markdown reader by starting at `index.md`. All links and images works like on the website.