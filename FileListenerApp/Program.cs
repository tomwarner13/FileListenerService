using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileListenerService;
using System.Configuration;

namespace FileListenerApp
{
  class Program
  {
    static void Main(string[] args)
    {
      var listener = new Listener(
        ConfigurationManager.AppSettings["RootDirectory"],
        ConfigurationManager.AppSettings["UrlEndpoints"].Split(';'),
        ConfigurationManager.AppSettings["DirsToIgnore"].Split(';'));

      Console.WriteLine("Please press the ANY key to exit");
      Console.ReadLine();
      listener.Dispose();
    }
  }
}
