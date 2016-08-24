using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using log4net;

namespace FileListenerService
{
  public class Listener
  {
    private readonly List<Uri> _endpoints;
    private FileSystemWatcher _watcher;
    private string _rootDir;
    private List<string> _dirsToIgnore;
    private readonly ILog log;
    private char _endpointSeparator = '|';

    //may need this
    //[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public Listener(string rootDir, string endpoints, List<string> dirsToIgnore)
    {
      log4net.Config.XmlConfigurator.Configure();
      log = LogManager.GetLogger(typeof(Listener));

      _rootDir = rootDir;
      _endpoints = endpoints.Split(_endpointSeparator).Select(e => new Uri(e)).ToList();
      _dirsToIgnore = dirsToIgnore;

      _watcher = new FileSystemWatcher(_rootDir);
      _watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.FileName;
      _watcher.IncludeSubdirectories = true;

      _watcher.Changed += new FileSystemEventHandler(OnChanged);
      _watcher.Created += new FileSystemEventHandler(OnChanged);

      _watcher.EnableRaisingEvents = true;
    }

    private void NotifyEndpoints(Dictionary<string, string> values)
    {
      foreach (var endpoint in _endpoints)
      {
        log.Debug($"Notifying endpoint at {endpoint.AbsoluteUri} with path '{values["Path"]}'");
        try
        {
          using (var client = new HttpClient())
          {
            var response = client.PostAsync(endpoint.AbsoluteUri, new FormUrlEncodedContent(values)).Result;
            log.Debug($"Endpoint responded {response.StatusCode} with content {response.Content.ReadAsStringAsync().Result}");
          }
        }
        catch (Exception e)
        {
          log.Error($"Exception contacting API endpoint: {e.Message}. Inner Exception: {e.InnerException}. Stack trace: {e.StackTrace}");
        }
      }
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
      log.Debug($"File change observed: path {e.FullPath} with change {e.ChangeType}");
      var relativePath = e.FullPath.Replace(_rootDir, "");

      var topDirectory = 
        relativePath.Contains('\\') ? relativePath.Split('\\')[0] : "";

      //FileSystemWatcher can only take a filter of which directories to watch; no whitelist/blacklist
      //thanks obama
      if (_dirsToIgnore.Contains(topDirectory))
      {
        log.Debug($"File change ignored: {topDirectory} is blacklisted");
      }
      else
      {
        var values = new Dictionary<string, string>
        {
          { "Path", relativePath },
          { "Event", e.ChangeType.ToString() }
        };
        NotifyEndpoints(values);
      }
    }
  }
}
