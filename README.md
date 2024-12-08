# Civitai Firehose

This is a fairly simple Blazor app I made primarily for my own use, but decided to make public.

Civitai is the main place where people are sharing AI images they've generated nowadays.
I have a habit of trawling the feed for newly-posted images.
I find something fascinating in looking at the endless stream of ephemeral Content produced by random people online when they're given access to heinously powerful tools.

It was a pain manually saving all the cool pictures I saw, so I made this app to serve as a dashboard/infinite stream of content for me. 

I typically organise my media in an app called the Hydrus Network, so I additionally added some functionality to quickly
send interesting images to it for download via its built-in REST API.

## TODO
- Let users enter own search term/query for more specialised live hose
- Let users change sort mode etc. in UI, rather than editing config
- Provide feedback if push-to-Hydrus fails
- Add better logging
- Add tests and general robustness