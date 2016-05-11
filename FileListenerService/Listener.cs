using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

namespace FileListenerService
{
  public class Listener
  {
    private readonly Uri _endpoint;
    private FileSystemWatcher _watcher;
    private string _rootDir;
    private List<string> _dirsToIgnore;

    //may need this
    //[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public Listener(string rootDir, string endpoint, List<string> dirsToIgnore)
    {
      _rootDir = rootDir;
      _endpoint = new Uri(endpoint);
      _dirsToIgnore = dirsToIgnore;

      _watcher = new FileSystemWatcher(_rootDir);
      _watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
      _watcher.IncludeSubdirectories = true;

      _watcher.Changed += new FileSystemEventHandler(OnChanged);
      _watcher.Created += new FileSystemEventHandler(OnChanged);

      _watcher.EnableRaisingEvents = true;
    }

    private void NotifyEndpoint(Dictionary<string, string> values)
    {
      using (var client = new HttpClient())
      {
        var response = client.PostAsync(_endpoint.AbsoluteUri, new FormUrlEncodedContent(values)).Result;
      }
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
      var relativePath = e.FullPath.Replace(_rootDir, "");

      var topDirectory = 
        relativePath.Contains('\\') ? relativePath.Split('\\')[0] : "";

      //FileSystemWatcher can only take a filter of which directories to watch; no whitelist/blacklist
      //thanks obama
      if(_dirsToIgnore.Contains(topDirectory))
        return;

      var values = new Dictionary<string, string>
      {
        { "Path", relativePath },
        { "Event", e.ChangeType.ToString() }
      };
      NotifyEndpoint(values);
    }
  }
}
