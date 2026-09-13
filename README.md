# XteinkInfo

`XteinkInfo` is a _proof of concept_ API for building a compact daily information brief and pushing it to an Xteink e-reader.  The API does not do any data collection, instead facilitates pushing the information to your reader.

## Purpose

This project exposes an API that allows external automation tools such as n8n (this is what I do), cron jobs, or custom scripts to collect and aggregate content from multiple sources and turn it into a single ebook-style document named `info`.

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

Obviously the device should be coonected to your network at the point info is sent. I will have the api periodically retry at some point so you can turn on the network for 10 mins before you go jump on the train, and enjoy your daily brief during your commute.

The resulting file is effectively a generated book called `info` that can be viewed on the Xteink reader.

Run with docker compose. Edit the compose file to set your xteink host IP (which should be reserved on your router).

For those of you who are only here for the how:

```csharp
var html = "<html><head><title>Info</title></head><body>";
foreach (var info in infos)
{
    html += $"<h1>{info.Section}</h1><p>{info.Content}</p>";
}
html += "</body></html>";
try { System.IO.File.Delete("/content/info.html"); } catch {}
await System.IO.File.WriteAllTextAsync("/content/info.html", html);
try { System.IO.File.Delete("/content/info.epub"); } catch {}
await RunProcessAndShowOutput("pandoc", "/content/info.html -o /content/info.epub --toc --no-check-certificate");
await RunProcessAndShowOutput("curl", $"-F \"path=info.epub\" http://{host}/delete");
await RunProcessAndShowOutput("curl", $"-F \"file=@/content/info.epub\" http://{host}/upload");
```
