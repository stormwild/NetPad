using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetPad.Application;
using NetPad.Configuration;
using NetPad.CQs;
using NetPad.Filters;
using NetPad.UiInterop;
using NetPad.Utilities;

namespace NetPad.Controllers;

[ApiController]
[Route("app")]
public class AppController : Controller
{
    private readonly ILogger<AppController> _logger;

    public AppController(ILogger<AppController> logger)
    {
        _logger = logger;
    }

    [HttpGet("identifier")]
    public AppIdentifier GetIdentifier([FromServices] AppIdentifier appIdentifier)
    {
        return appIdentifier;
    }

    [HttpGet("latest-version")]
    public async Task<string?> GetLatestVersion([FromServices] HttpClient httpClient)
    {
        try
        {
            const string url = "https://api.github.com/repos/tareqimbasher/netpad/releases/latest";

            var httpMessage = new HttpRequestMessage(HttpMethod.Get, url);
            httpMessage.Headers.Add("User-Agent", "NetPad");

            var json = await Retry.ExecuteAsync(2, TimeSpan.FromSeconds(2), async () =>
            {
                var result = await httpClient.SendAsync(httpMessage);
                return await result.Content.ReadAsStringAsync();
            });

            if (json == null)
            {
                return null;
            }

            var jsonDocument = JsonDocument.Parse(json);
            var latestVersion = jsonDocument.RootElement.GetProperty("tag_name").GetString();

            if (latestVersion == null || !Version.TryParse(latestVersion.TrimStart('v'), out var version))
            {
                return null;
            }

            return version.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest version");
        }

        return null;
    }

    [HttpPost("client/ready")]
    public async Task NotifyClientAppIsReady([FromServices] IUiDialogService uiDialogService, [FromServices] IMediator mediator)
    {
        var result = await CheckDependencies(mediator);

        if (result.SupportedDotNetSdkVersionsInstalled.Any()) return;

        await uiDialogService.AlertUserAboutMissingDependencies(result);
    }

    [HttpPatch("check-dependencies")]
    public async Task<AppDependencyCheckResult> CheckDependencies([FromServices] IMediator mediator) =>
        await mediator.Send(new CheckAppDependenciesQuery());

    [HttpPatch("open-folder-containing-script")]
    public void OpenFolderContainingScript([FromQuery] string? scriptPath, [FromServices] Settings settings)
    {
        if (scriptPath == null)
            throw new Exception("Script has no path");

        var file = new FileInfo(scriptPath);

        if (!file.Exists || file.Directory?.Exists != true)
            throw new Exception("Not allowed");

        if (!file.Directory.FullName.StartsWith(settings.ScriptsDirectoryPath))
            throw new Exception("Not allowed");

        Process.Start(new ProcessStartInfo
        {
            FileName = file.Directory.FullName,
            UseShellExecute = true
        });
    }

    [HttpPatch("open-scripts-folder")]
    public void OpenScriptsFolder([FromQuery] string? path, [FromServices] Settings settings)
    {
        string sanitized;

        if (string.IsNullOrWhiteSpace(path))
            sanitized = settings.ScriptsDirectoryPath;
        else
            sanitized = Path.Combine(settings.ScriptsDirectoryPath, path.Trim('.', '/', '\\'));

        if (!Directory.Exists(sanitized))
            throw new Exception($"Directory does not exist at: {path}");

        Process.Start(new ProcessStartInfo
        {
            FileName = sanitized,
            UseShellExecute = true
        });
    }

    [HttpPatch("open-package-cache-folder")]
    public void OpenPackageCacheFolder([FromServices] Settings settings)
    {
        var path = settings.PackageCacheDirectoryPath;
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            throw new Exception($"Package cache folder does not exist at: '{settings.PackageCacheDirectoryPath}'");

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    [HttpPost("log/{source}")]
    [SilentResponse]
    public void SendRemoteLog(
        LogSource source,
        [FromBody] RemoteLogMessage[] logs,
        [FromServices] ILoggerFactory loggerFactory)
    {
        var loggers = new Dictionary<string, ILogger>();

        foreach (var log in logs.OrderBy(l => l.Date))
        {
            var message = log.Message ?? string.Empty;

            foreach (var logOptionalParam in log.OptionalParams ??= Array.Empty<string>())
            {
                message += $" {logOptionalParam}";
            }

            var loggerName = $"RemoteLog.{source}.{log.Logger ?? "unknown-logger"}";

            if (!loggers.TryGetValue(loggerName, out var logger))
            {
                logger = loggerFactory.CreateLogger(loggerName);
                loggers.Add(loggerName, logger);
            }

            logger.Log(log.LogLevel, message);
        }
    }

    public enum LogSource
    {
        WebApp = 10,
        ElectronApp = 11
    }

    public class RemoteLogMessage
    {
        public string? Logger { get; set; }
        public LogLevel LogLevel { get; set; }
        public string? Message { get; set; }
        public string?[]? OptionalParams { get; set; }
        public DateTime Date { get; set; }
    }
}
