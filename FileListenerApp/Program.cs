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
        ConfigurationManager.AppSettings["UrlEndpoint"],
        new List<string>(ConfigurationManager.AppSettings["DirsToIgnore"].Split(new char[] { ';' })));

      Console.WriteLine("Please press the ANY key to exit");
      Console.ReadLine();
    }
  }
}
