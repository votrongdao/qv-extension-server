using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.IO;
using System.Reflection;

using frqtlib;
using myCore = frqtlib.Core;

namespace QlikViewExtensionServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int maxLogLvl = Convert.ToInt32(ConfigurationManager.AppSettings["maxLogLvl"]);
            int dfltLogLvl = Convert.ToInt32(ConfigurationManager.AppSettings["dfltLogLvl"]);

            myCore.ConsoleLogging lHostConsole = new myCore.ConsoleLogging(maxLogLvl, dfltLogLvl);
            myCore.Logging.addLogging(lHostConsole);

            myCore.FileSystemLogging lHostFile = new myCore.FileSystemLogging(
                Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "log"),
                "log",
                true,
                10,
                6
            );

            myCore.Logging.addLogging(lHostFile);

            QlikViewExtensionServerLib lib = new QlikViewExtensionServerLib();

            lib.init();
            lib.start();

        }
    }
}
