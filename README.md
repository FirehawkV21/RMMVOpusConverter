# Conversion to Opus and Disguise Tool for RPG Maker MV

## What's this?
This experimental command line FFMPEG wrapper app, it converts audio files to Opus and disguises them as OGG files so RPG Maker MV loads them.
It's a handy trick since Opus is supported in nwjs and web browsers and it provides better compression versus OGG Vorbis when the bitrate is the same.

## System Requirements
- Any Operating System that can run .NET Core 2.2.
- FFMPEG. Any version that supports Opus.

## Compiling

You can either use Visual Studio 2017 (or newer) to compile it or run the following (make sure that you have the .NET Core 2.2 SDK):

```
dotnet build -c Release
dotnet run --project "RMMVOpusConverter"
```
