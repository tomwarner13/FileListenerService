using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FileListenerService
{
  public partial class FileListenerService : ServiceBase
  {
    private Listener _listener;

    public FileListenerService()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      _listener = new Listener(
        ConfigurationManager.AppSettings["RootDirectory"], 
        ConfigurationManager.AppSettings["UrlEndpoints"].Split(';'),
        ConfigurationManager.AppSettings["DirsToIgnore"].Split(';'));
    }

    protected override void OnStop()
    {
      _listener.Dispose();
    }
  }
}
