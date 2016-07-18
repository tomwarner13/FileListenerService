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
    private readonly Uri _endpoint;
    private FileSystemWatcher _watcher;
    private string _rootDir;
    private List<string> _dirsToIgnore;
    private readonly ILog log;

    //may need this
    //[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public Listener(string rootDir, string endpoint, List<string> dirsToIgnore)
    {
      log4net.Config.XmlConfigurator.Configure();
      log = LogManager.GetLogger(typeof(Listener));

      _rootDir = rootDir;
      _endpoint = new Uri(endpoint);
      _dirsToIgnore = dirsToIgnore;

      _watcher = new FileSystemWatcher(_rootDir);
      _watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.FileName;
      _watcher.IncludeSubdirectories = true;

      _watcher.Changed += new FileSystemEventHandler(OnChanged);
      _watcher.Created += new FileSystemEventHandler(OnChanged);

      _watcher.EnableRaisingEvents = true;
    }

    private void NotifyEndpoint(Dictionary<string, string> values)
    {
      log.Debug($"Notifying endpoint at {_endpoint.AbsoluteUri} with path '{values["Path"]}'");
      try
      {
        using (var client = new HttpClient())
        {
          var response = client.PostAsync(_endpoint.AbsoluteUri, new FormUrlEncodedContent(values)).Result;
          log.Debug($"Endpoint responded {response.StatusCode} with content {response.Content.ReadAsStringAsync().Result}");
        }
      }
      catch(Exception e)
      {
        log.Error($"Exception contacting API endpoint: {e.Message}");
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
        NotifyEndpoint(values);
      }
    }
  }
}
