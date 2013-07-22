using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using frqtlib.Core;
using Mono.Options;

namespace CleanVariables
{
    class Program
    {
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Main(string[] args)
        {
            OptionSet p = null;

            bool show_help = false;

            string app = "";

            p = new OptionSet() {
                { "a=|app=", "the qlikview {APPLICATION} fullpath",
                    v => app = v },
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
            }

            QlikView.Application a = new QlikView.Application();
            QlikView.Doc doc = a.OpenDoc(app);

            int vCount = doc.GetVariableDescriptions().Count;
            List<string> vNames = new List<string>();

            for (int i = 0; i < vCount; i++)
                if (!doc.GetVariableDescriptions()[i].IsReserved && !doc.GetVariableDescriptions()[i].IsConfig)
                    vNames.Add(doc.GetVariableDescriptions()[i].Name);

            foreach(string vName in vNames)
                doc.RemoveVariable(vName);

            doc.SaveAs(app);
            doc.CloseDoc();
            


        }
    }
}
