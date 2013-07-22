using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using myCore = frqtlib.Core;

namespace frqtlibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            myCore.ConsoleLogging cl = new myCore.ConsoleLogging(10, 6);
            myCore.Logging.addLogging(cl);

            myCore.PipeLoggingServer pls = new myCore.PipeLoggingServer();

            myCore.PipeLogging plc = new myCore.PipeLogging();
            plc.log("test", myCore.LogType.Information);

            Console.ReadLine();
        }
    }
}
