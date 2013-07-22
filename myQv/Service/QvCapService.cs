using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceProcess;
using System.Threading;
using Timers = System.Timers;

using System.IO;
using System.IO.Pipes;

using CapCore = myQv.Core;
using CapWeb = myQv.Web;
using CapThread = myQv.Threading;


namespace myQv.Service
{
    public delegate void WSNotificationEventHandler (object sender, WSNotificationEventArgs e);

    public class WSNotificationEventArgs : EventArgs
    {
        public CapCore.IReturnObject msg;
        public Guid g;

        public WSNotificationEventArgs() : base() { }
        public WSNotificationEventArgs(CapCore.IReturnObject msg)
            : base()
        {
            this.msg = msg;
        }

        public WSNotificationEventArgs(CapCore.IReturnObject msg, Guid g)
            : base()
        {
            this.msg = msg;
            this.g = g;
        }

    }

    public abstract class AService
    {
        private bool initialized = false;
        private bool started = false;
        private bool hasWS = false;

        public CapWeb.HostWorker WSHostWorkerObject = null;
        public Thread WSHostWorkerThread = null;
        public Thread clientPipeWorkerThread = null;
        public CapThread.PipeServer WSPipe = null;

        private string serviceName = null;

        public event WSNotificationEventHandler WSNotification;

        protected virtual void OnWSNotification(WSNotificationEventArgs e)
        {
            if (WSNotification != null)
                WSNotification(this, e);
        }

        public void init(string serviceName, params object[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            this.serviceName = serviceName;
            this.initialized = true;

            this.toInit(args);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CapCore.Logging.log(((Exception)e.ExceptionObject).Message, (Exception)e.ExceptionObject, CapCore.LogType.Error, 1);
        }

        public void start(bool hasWS, int port, List<string> nsNames, int pipeNum)
        {
            if (!this.initialized) throw new Exception("Service must be initialized before being started ...");

            this.started = true;
            this.hasWS = hasWS;

            CapCore.Logging.log("Service starting ...", CapCore.LogType.Information, 0);

            WSPipe = new CapThread.PipeServer(this.serviceName, pipeNum);
            WSPipe.newMessage += new CapThread.NewMessageEventHandler(WSPipe_NewMessage);

            if (this.hasWS)
            {
                CapCore.Logging.log("Webserver starting ...", CapCore.LogType.Information, 0);

                CapWeb.HttpListenerWrapper lw = CapWeb.HttpListenerWrapper.GetWrapper(port, nsNames);
                lw.Start();

                CapCore.Logging.log("Listening for requests on port " + port.ToString() + " on localhost, 127.0.0.1" + ((nsNames != null && nsNames.Count > 0) ? ", " + String.Join(", ", nsNames.ToArray()) : "") + " ...", CapCore.LogType.Information, 1);

                this.WSHostWorkerObject = new CapWeb.HostWorker();
                this.WSHostWorkerThread = new Thread(this.WSHostWorkerObject.DoWork);

                this.WSHostWorkerThread.Start(lw);
            }

            this.toStart();
        }

        public List<CapCore.Tuple<T, Guid>> getQueueItems<T>()
        {
            CapCore.Logging.log("Get item from pipe queue requested ...", myQv.Core.LogType.Information, 11);
            return this.WSPipe.readObjects<T>();
        }

        public T getQueueItem<T>(Guid g)
        {
            CapCore.Logging.log("Get item from pipe queue requested ...", myQv.Core.LogType.Information, 11);
            return this.WSPipe.readObject<T>(g, null);
        }

        public void putQueueItem<T>(T o, Guid g)
        {
            CapCore.Logging.log("Put item into pipe queue requested ...", myQv.Core.LogType.Information, 11);
            this.WSPipe.WriteObject<T>(o, g);
        }

        void WSPipe_NewMessage(object sender, CapThread.PipeEventArgs e)
        {
            
            WSNotificationEventArgs args = new WSNotificationEventArgs(e.o, e.g);
            this.OnWSNotification(args);
        }

        

        public void stop()
        {
            if (!this.started) throw new Exception("Service must be started before being stoped ...");

            CapCore.Logging.log("Service stopping ...", CapCore.LogType.Information, 0);

            if (this.hasWS)
            {
                WSHostWorkerObject.RequestStop();
                WSHostWorkerThread.Join();
            }

            this.toStop();
        }

        protected abstract void toInit(params object[] args);
        protected abstract void toStart();
        protected abstract void toStop();

    }
}
