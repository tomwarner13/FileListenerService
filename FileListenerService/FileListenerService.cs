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
    public FileListenerService()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      var l = new Listener(
        ConfigurationManager.AppSettings["RootDirectory"], 
        ConfigurationManager.AppSettings["UrlEndpoints"],
        new List<string>(ConfigurationManager.AppSettings["DirsToIgnore"].Split(new char[] { ';' } )));
    }

    protected override void OnStop()
    {
    }
  }
}
