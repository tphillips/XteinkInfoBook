using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

namespace xteinkInfo.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController : ControllerBase
{

    public static List<Info> Infos { get; set; } = new List<Info>();
    
    public InfoController(ILogger<InfoController> logger)
    {
       
    }

    [HttpPost(Name = "Info")]
    public async Task<Info> SendInfo([FromBody] Info infoRequest)
    {
        var currentInfo = infoRequest;
        Infos.Add(currentInfo);
        await SendInfoToDevice(Infos);
        return currentInfo;
    }

    private static async Task RunProcessAndShowOutput(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start process: {fileName} {arguments}");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        Console.WriteLine($"=> {fileName} {arguments}");
        if (!string.IsNullOrWhiteSpace(stdout))
        {
            Console.WriteLine(stdout.TrimEnd());
        }
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            Console.WriteLine(stderr.TrimEnd());
        }
        Console.WriteLine($"Exit code: {process.ExitCode}");
    }

    private async Task SendInfoToDevice(List<Info> infos)
    {
        var host = Environment.GetEnvironmentVariable("XTEINK_HOST") ?? "192.168.4.22";
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
    }
}

