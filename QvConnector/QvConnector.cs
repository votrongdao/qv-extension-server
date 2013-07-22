using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QlikView.Qvx.QvxLibrary;
using System.Windows.Interop;
using System.Security.Authentication;
using System.Windows.Forms;
using System.IO;
using QvConnectorInterface;
using System.Reflection;

namespace QvConnector
{
    public class QvConnectorServer : QvxServer, IQvConnectorHost
    {
        private Dictionary<string, IQvConnector> ConnectorMap = new Dictionary<string, IQvConnector>();

        public QvConnectorServer() : base()
        {
            QvxLog.SetLogLevels(true, true);
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "QvConnectorServer()");
            this.getPlugins();
        }

        private void getPlugins()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getPlugins()");

            string path = Application.StartupPath;
            string[] pluginFiles = new string[] {};

            try
            {
                pluginFiles = Directory.GetFiles(path, "*.DLL");
            }
            catch (Exception) { }

            IQvConnector[] ipi = new IQvConnector[pluginFiles.Length];

            for (int i = 0; i < pluginFiles.Length; i++)
            {

                string args = pluginFiles[i].Substring(pluginFiles[i].LastIndexOf("\\") + 1, pluginFiles[i].IndexOf(".DLL", StringComparison.OrdinalIgnoreCase) - pluginFiles[i].LastIndexOf("\\") - 1);

                Type ObjType = null;

                Assembly ass = Assembly.Load(args);

                if (ass != null)
                {
                    Type[] types = ass.GetTypes();
                    foreach (Type t in types)
                    {
                        if (t.GetInterface(typeof(IQvConnector).FullName) != null)
                        {
                            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "found plugin: " + t.ToString() + " in " + pluginFiles[i]);
                            ObjType = t;
                        }
                    }
                }

                if (ObjType != null)
                {
                    ipi[i] = (IQvConnector)Activator.CreateInstance(ObjType);
                    ipi[i].Host = this;
                    this.Register(ipi[i]);
                }
            }
        }

        public override QvxConnection CreateConnection()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "CreateConnection()");

            return new QvConnectorConnection(this);
        }

        public override string CreateConnectionString()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "CreateConnectionString()");

            Login login = CreateLoginWindowHelper();
            login.ShowDialog();

            string connectionString = null;
            if (login.DialogResult.Equals(true))
            {
                if (login.GetServer() == null || login.GetAuth() == null || login.GetUsername() == null || login.GetPassword() == null || login.GetDriver() == null) throw new Exception("All connection parameters have to be set ...");
                connectionString =
                    (login.GetAuth() == Login.defaultInput) ? String.Format("Server={0};Driver={1};Auth={2};Param={3}", login.GetServer(), login.GetDriver(), login.GetAuth(), login.GetParam())
                    : String.Format("Server={0};Driver={3};Auth={4};UserId={1};Password={2};Param={5}", login.GetServer(), login.GetUsername(), login.GetPassword(), login.GetDriver(), login.GetAuth(), login.GetParam());
            }

            return connectionString;
        }

        public override string CreateSelectStatement()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "CreateSelectStatement()");

            return null;
        }

        private Login CreateLoginWindowHelper()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "CreateLoginWindowHelper()");

            // Since the owner of the loginWindow is a Win32 process we need to
            // use WindowInteropHelper to make it modal to its owner.
            var login = new Login(this.ConnectorMap);
            var wih = new WindowInteropHelper(login);
            wih.Owner = MParentWindow;

            return login;
        }

        public bool Register(IQvConnector ipi)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Register()");

            if (this.ConnectorMap.ContainsKey(ipi.Name)) return false;
            ConnectorMap.Add(ipi.Name, ipi);
            return true;
        }

        public IQvConnector Registered(string plugin)
        {
            if (!this.ConnectorMap.ContainsKey(plugin)) return null;
            return this.ConnectorMap[plugin];
        }

    }

    class QvConnectorConnection : QvxConnection
    {
        private QvConnectorServer parent;

        public QvConnectorConnection(QvConnectorServer qcs) : base()
        {
            this.parent = qcs;
        }

        public override void Init()
        {
            string auth, username = null, password = null, server, driver, param;

            this.MParameters.TryGetValue("Auth", out auth);

            if (auth != Login.defaultInput)
            {
                this.MParameters.TryGetValue("UserId", out username);
                this.MParameters.TryGetValue("Password", out password);
            }

            this.MParameters.TryGetValue("Server", out server);
            this.MParameters.TryGetValue("Driver", out driver);
            this.MParameters.TryGetValue("Param", out param);

            IQvConnector iqc = this.parent.Registered(driver);

            if (iqc == null) throw new Exception("driver:" + driver + " not registered ...");
            if (!iqc.Test(server, auth, username, password, param)) throw new Exception("driver:" + driver + " registered, but unavailable ...");

            List<QvxTable> l = iqc.Init(server, auth, username, password, param, MTables, FindTable);
            this.MTables = l;
        }

        public override QvxDataTable ExtractQuery(string query, List<QvxTable> tables)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "ExtractQuery()");
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, query);

            return base.ExtractQuery(query, tables);
        }


    }

}
