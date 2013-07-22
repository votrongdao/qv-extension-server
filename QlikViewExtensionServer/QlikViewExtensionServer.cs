using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.IO;
using System.Reflection;

using frqtlib;
using myCore = frqtlib.Core;


namespace frqtlib
{
    public partial class QlikViewExtensionServer : ServiceBase
    {
        QlikViewExtensionServerLib lib = new QlikViewExtensionServerLib();

        public QlikViewExtensionServer()
        {
            InitializeComponent();

            int maxLogLvl = Convert.ToInt32(ConfigurationManager.AppSettings["maxLogLvl"]);
            int dfltLogLvl = Convert.ToInt32(ConfigurationManager.AppSettings["dfltLogLvl"]);

            myCore.WindowsEventLogging lHostConsole = new myCore.WindowsEventLogging("QlikView Extension Server", "QlikView Extension Server", maxLogLvl, dfltLogLvl);
            myCore.Logging.addLogging(lHostConsole);

            myCore.FileSystemLogging lHostFile = new myCore.FileSystemLogging(
                Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "log"),
                "log",
                true,
                10,
                6
            );

            myCore.Logging.addLogging(lHostFile);

            lib.init();
        }

        protected override void OnStart(string[] args)
        {
            lib.start();
        }

        protected override void OnStop()
        {
            lib.stop();
        }
    }
}
