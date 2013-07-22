using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using frqtlib.Core;
using frqtlib.Qv;
using frqtlib.QMSAPI;

using Mono.Options;

namespace PurgeCALs
{
    class Program
    {
        public static ConsoleLogging lHost = new ConsoleLogging();

        static Program() {
            lHost.init(10, 6);
        }

        static void ShowHelp(OptionSet p)
        {
            lHost.log("Options:", LogType.Information);
            p.WriteOptionDescriptions(Console.Out);
        }

        static int Main(string[] args)
        {
            OptionSet p = null;

            bool show_help = false;

            string qms = "http://localhost";
            int lastUsedMonthLimit = 3;

            p = new OptionSet() {
                { "q=|qms=", "the {QMS} IP address or DNS name",
                    v => qms = v },
                { "l=|limit=", "the {NUMBER} of unused months after which licence should be deleted",
                    v => lastUsedMonthLimit = Convert.ToInt32(v) },
                { "h|help",  "show this message and exit", 
                    v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                throw new System.Exception("Error while parsing command line ...", e);
            }

            if (show_help)
            {
                ShowHelp(p);
                return 0;
            }

            lHost.log("Connecting to QVS ...", LogType.Information);

            QMSClientEnhanced qvClient = QMSClientFactory.getClient(new Uri(qms));

            lHost.log("Getting CAL conf ...", LogType.Information);

            Guid qvsId = qvClient.GetServices(frqtlib.QMSAPI.ServiceTypes.QlikViewServer)[0].ID;
            frqtlib.QMSAPI.CALConfiguration cc = qvClient.GetCALConfiguration(qvsId, frqtlib.QMSAPI.CALConfigurationScope.NamedCALs);

            List<AssignedNamedCAL> currentCALs = cc.NamedCALs.AssignedCALs.ToList();
            List<AssignedNamedCAL> removeCALs = new List<AssignedNamedCAL>();

            foreach (AssignedNamedCAL c in currentCALs)
                if (c.LastUsed < DateTime.Now.AddMonths(-lastUsedMonthLimit))
                    removeCALs.Add(c);

            foreach (AssignedNamedCAL c in removeCALs)
            {
                lHost.log("Removing " + c.UserName + " (" + c.LastUsed.ToShortDateString() + ")", LogType.Information);
                currentCALs.Remove(c);
            }

            //cc.NamedCALs.AssignedCALs = currentCALs.ToArray();

            qvClient.SaveCALConfiguration(cc);

            return 0;
        }
    }
}
