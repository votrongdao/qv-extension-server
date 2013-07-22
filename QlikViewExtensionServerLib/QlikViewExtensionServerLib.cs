using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

using System.Reflection;

using myCore = frqtlib.Core;
using myWeb = frqtlib.Web;
using myQv = frqtlib.Qv;

using frqtlib.QMSAPI;
using System.IO;


namespace frqtlib
{
    public class QlikViewExtensionServerLib : myCore.IWindowsService
    {

        public myWeb.HostWorker WSHostWorkerObject = null;
        public Thread WSHostWorkerThread = null;

        public System.Collections.Specialized.NameValueCollection conf = ConfigurationManager.AppSettings;

        public void deployExtension(myQv.QMSClientEnhanced qvClient, Guid qvsId, string fileName)
        {
            ExtensionUploadHandle handle = qvClient.InitiateUploadExtensionObject(qvsId);

            if (handle != null)
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);

                int maxBufferSize = 1024 * 16;

                byte[] buffer = new byte[maxBufferSize];
                int bytesRead;

                while ((bytesRead = fs.Read(buffer, 0, maxBufferSize)) > 0)
                {
                    //Copy the read bytes into a new buffer with the same size as the number of received bytes
                    byte[] byteWireBuffer = new byte[bytesRead];
                    Buffer.BlockCopy(buffer, 0, byteWireBuffer, 0, bytesRead);

                    //Write the buffer to the QVS
                    handle = qvClient.WriteExtensionObject(handle, byteWireBuffer);
                }

                //Finalize the upload. This will install the extension object if it is valid.
                List<QVSMessage> msg = qvClient.CloseAndInstallExtensionObject(handle);

                msg.ForEach(
                    m => myCore.Logging.log(m.Text, 2, myCore.LogType.Information)
                );
            }
        }

        public void init()
        {
            myCore.Logging.log("Extension Server Lib loading ...", 0, myCore.LogType.Information);

            myCore.Logging.log("Starting log server ...", 1, myCore.LogType.Information);

            myCore.PipeLoggingServer pl = new myCore.PipeLoggingServer();

            myCore.Logging.log("Deploying QARs ...", 1, myCore.LogType.Information);

            myCore.Logging.log("Connecting to QVS ...", 2, myCore.LogType.Information);

            try
            {
                myQv.QMSClientEnhanced qvClient = myQv.QMSClientFactory.getClient(new Uri("http://localhost"));
                Guid qvsId = qvClient.GetServices(frqtlib.QMSAPI.ServiceTypes.QlikViewServer)[0].ID;

                string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                foreach (string fileName in Directory.EnumerateFiles(Path.Combine(path, "Extensions"), "*.QAR"))
                {
                    myCore.Logging.log("Initiating upload for {0} ...", 2, myCore.LogType.Information, fileName);

                    try
                    {
                        this.deployExtension(qvClient, qvsId, fileName);
                    }
                    catch (System.Exception)
                    {
                        myCore.Logging.log("Unable to deploy {0} ... Retrying ...", 2, myCore.LogType.Error, fileName);

                        try
                        {
                            this.deployExtension(qvClient, qvsId, fileName);
                        }
                        catch (System.Exception e)
                        {
                            myCore.Logging.log("Unable to deploy {0} ... ", 2, myCore.LogType.Error, fileName);
                            throw e;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                myCore.Logging.log("Unable to connect to local QMS API ... Manually deploy extensions or add current user to QlikView Management Service API group ...", 2, myCore.LogType.Error);
            }

        }

        public void start()
        {
            myCore.Logging.log("Webserver starting ...", 0, myCore.LogType.Information);

            int port = Convert.ToInt32(conf["port"]);
            List<string> nsNames = new List<string> () { conf["nsName"] };

            myWeb.HttpListenerWrapper lw = myWeb.HttpListenerWrapper.GetWrapper(port, nsNames);
            lw.Start();

            myCore.Logging.log("Listening for requests on port {0} on localhost, 127.0.0.1{1} ...", 1, myCore.LogType.Information, port, ((nsNames != null && nsNames.Count > 0) ? ", " + String.Join(", ", nsNames.ToArray()) : ""));

            this.WSHostWorkerObject = new myWeb.HostWorker();
            this.WSHostWorkerThread = new Thread(this.WSHostWorkerObject.DoWork);

            this.WSHostWorkerThread.Start(lw);
        }

        public void stop()
        {
            WSHostWorkerObject.RequestStop();
            WSHostWorkerThread.Join();
        }
    }
}
