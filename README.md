# XteinkInfo

`XteinkInfo` is an API for building a compact daily information brief and pushing it to an Xteink e-reader.  The API does not do any data collection, instead facilitates pushing the information to your reader.

## Purpose

This project exposes an API that allows external automation tools such as n8n, cron jobs, or custom scripts to collect and aggregate content from multiple sources and turn it into a single ebook-style document named `info`.

The idea is simple: create an automated “daily brief” or similar reading digest, then send it to an e-reader without manual copy/paste or formatting work.

Examples of what can be fed into the system:

- news summaries
- todo
- calendar info
- email summaries
- product updates
- project status notes
- curated links or highlights
- any aggregated content you want on your ereader

## How it works

The API exposes a single POST endpoint:

- `POST /Info`

When a request is received, it:

1. Adds the submitted item to an in-memory list.
2. Builds a single HTML page containing all collected sections.
3. Converts that HTML to an EPUB using `pandoc`.
4. Deletes the previous EPUB from the target reader.
5. Uploads the new EPUB to the configured device endpoint via `curl`.

Obviously the device should be coonected to your network at the point info is sent. I will have the api periodically retry at some point...

The resulting file is effectively a generated book called `info` that can be viewed on the Xteink reader.

Run with docker compose. Edit the compose file to set your xteink host IP (which should be reserved on your router).
