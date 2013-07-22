using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QlikView.Qvx.QvxLibrary;

namespace QvConnectorInterface
{
    public interface IQvConnectorHost
    {
        bool Register(IQvConnector ipi);
    }

    public interface IQvConnector
    {
        string Name { get; }
        IQvConnectorHost Host { get; set; }

        string[] getAvailableAuthMethods();

        bool Test(string server, string auth, string username, string password, string param);
        List<QvxTable> Init(string server, string auth, string username, string password, string param, List<QvxTable> MTables, Func<string, IEnumerable<QvxTable>, QvxTable> FindTable);
    }
}
