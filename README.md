# Civitai Firehose

This is a fairly simple Blazor app I made primarily for my own use.

[Civitai](https://civitai.com) is the main place where people are sharing AI images nowadays.
I enjoy trawling its feed for newly-posted images -- 
there is something fascinating in the endless stream of ephemeral Content produced by the gestalt consciousness of the internet.

I made this app:
- as a live-updating dashboard/infinite stream of content
- to get hands-on experience with the dirty bits of Blazor
- for fun.

## Hydrus
I use [the Hydrus Network](https://hydrusnetwork.github.io/hydrus/index.html) to organise my media.
It exposes a REST API for importing material, so I've added features for quickly pushing images en masse from the firehose
to your local Hydrus instance.

This functionality is primarily accessed by the icons seen when hovering over an image in the feed.
You can use the 'help' button in the UI for a reference on what each button does.

## Usage

1. `git clone` to your machine
2. `dotnet run ./CivitaiFirehose/CivitaiFirehose.csproj`

Logs are automatically output to `logs` in the binary's working directory.

I make no claim this code will be useful to anyone else, but you're welcome to use it if you would like to.

## Configuration

Configuration is housed in `appsettings.json`.

- `EnableTelemetry`: enables OpenTelemetry data collection. Currently this is only exported to the console.
- CivitaiSettings
  - PollingPeriod: how often the app will scrape Civitai's new-images feed.
  - ExcludedCreators: usernames in this list will be excluded from your new-images feed.
  - QueryDefaults: these properties are automatically applied to your HTTP calls to Civitai's API. Check [this documentation](https://github.com/civitai/civitai/wiki/REST-API-Reference#get-apiv1images) for information on what you can specify here.
- HydrusSettings
  - BaseUrl: where your Hydrus instance is running.
  - ApiKey: the key used to authenticate with Hydrus's REST API.
  - AvailabilityWaitPeriod: how long the app will wait before checking if your Hydrus instance has become available.

## Contributions

Any changes and comments are welcome. Please keep in mind that this is just a personal project, though.