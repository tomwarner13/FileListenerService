using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using EventTracking;
using log4net;

namespace FileListenerService
{
  public class Listener : IDisposable
  {
    private readonly IEnumerable<Uri> _endpoints;
    private readonly string _rootDir;
    private readonly IEnumerable<string> _dirsToIgnore;
    private readonly ILog _log;
    private readonly FileSystemWatcher _watcher;
    private readonly EventTracker _tracker;
    private readonly TimeSpan _ignoreTimeSpan = TimeSpan.FromSeconds(5);

    //may need this
    //[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public Listener(string rootDir, IEnumerable<string> endpoints, IEnumerable<string> dirsToIgnore)
    {
      log4net.Config.XmlConfigurator.Configure();
      _log = LogManager.GetLogger(typeof(Listener));

      _rootDir = rootDir;
      _endpoints = endpoints.Select(e => new Uri(e));
      _dirsToIgnore = dirsToIgnore;
      _tracker = new EventTracker(_ignoreTimeSpan);

      _watcher = new FileSystemWatcher(_rootDir);
      _watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.LastAccess |
                              NotifyFilters.FileName;
      _watcher.IncludeSubdirectories = true;

      _watcher.Changed += new FileSystemEventHandler(OnChanged);
      _watcher.Created += new FileSystemEventHandler(OnChanged);

      _watcher.EnableRaisingEvents = true;
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
      Task.Run(() => Respond(e));
    }

    private void Respond(FileSystemEventArgs e)
    {
      _log.Debug($"File change observed: path {e.FullPath} with change {e.ChangeType}");
      var relativePath = e.FullPath.Replace(_rootDir, "");

      var topDirectory =
        relativePath.Contains('\\') ? relativePath.Split('\\')[0] : "";

      //FileSystemWatcher can only take a filter of which directories to watch; no whitelist/blacklist
      //thanks obama
      if (_dirsToIgnore.Contains(topDirectory))
      {
        _log.Debug($"File change ignored: {topDirectory} is blacklisted");
      }
      else if (!FileIsUnlocked(e.FullPath))
      {
        _log.Debug($"File change ignored: file {relativePath} is locked by another process");
      }
      else if (_tracker.CheckEvent(e.FullPath))
      {
        _log.Debug($"File change ignored: change event on {relativePath} has been observed in the last {_ignoreTimeSpan}");
      }
      else
      {
        var values = new Dictionary<string, string>
        {
          {"Path", relativePath},
          {"Event", e.ChangeType.ToString()}
        };
        foreach (var endpoint in _endpoints)
        {
          Task.Run(() => NotifyEndpoint(endpoint, values));
        }
      }
    }

    private async Task NotifyEndpoint(Uri endpoint, Dictionary<string, string> values)
    {
      _log.Debug($"Notifying endpoint at {endpoint.AbsoluteUri} with path '{values["Path"]}'");
      try
      {
        using (var client = new HttpClient())
        {
          var response = await client.PostAsync(endpoint.AbsoluteUri, new FormUrlEncodedContent(values));
          _log.Debug(
            $"Endpoint {endpoint.AbsoluteUri} responded {response.StatusCode} with content {response.Content.ReadAsStringAsync().Result}");
        }
      }
      catch (Exception e)
      {
        _log.Error(
          $"Exception contacting API endpoint {endpoint.AbsoluteUri}: {e.Message}. Inner Exception: {e.InnerException}. Stack trace: {e.StackTrace}");
      }
    }

    private static bool FileIsUnlocked(string path)
    {
      try
      {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
          return true;
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    public void Dispose()
    {
      _watcher.Dispose();
    }
  }
}
