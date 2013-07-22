using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;

using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;

using System.Data;
using System.Data.SqlClient;

using System.Diagnostics;

using myWeb = frqtlib.Web;

namespace frqtlib.Core
{
    public enum LogType { Warning, Error, Information }

    [Serializable()]
    public class LoggingException : Exception
    {
        public LoggingException(string message) : base(message) { }
        public LoggingException(string message, Exception innerException) : base(message, innerException) { }
    }



    public interface ILogging
    {
        bool ignore();

        void log(string msg, LogType l, params Object[] p);
        void log(string msg, int lvl, LogType l, params Object[] p);
        void log(string msg, int lvl, LoggingException e, LogType l, params Object[] p);
    }

    public static class Logging
    {
        private static List<ILogging> _l = new List<ILogging>();

        public static void addLogging(ILogging l)
        {
            if (!_l.Contains(l)) Logging._l.Add(l);
        }

        public static void log(string msg, LogType l, params Object[] p)
        {
            foreach (ILogging il in Logging._l)
            {
                if(!il.ignore())
                    il.log(msg, l, p);
            }
        }

        public static void log(string msg, int lvl, LogType l, params Object[] p)
        {
            foreach (ILogging il in Logging._l)
            {
                if (!il.ignore())
                    il.log(msg, lvl, l, p);
            }
        }

        public static void log(string msg, int lvl, LoggingException e, LogType l, params Object[] p)
        {
            foreach (ILogging il in Logging._l)
            {
                if (!il.ignore())
                    il.log(msg, lvl, e, l, p);
            }
        }

        public static void logStream(TextWriter _s, string msg, int lvl, LoggingException e, LogType l, params Object[] p)
        {
            msg = string.Format(msg, p) + ((e != null) ? Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace : "");
            string dispMsg = "";

            int i = 0;
            string pf = "";

            foreach (string s in msg.Split('\n'))
            {
                if (msg.Trim().Length == 0)
                {
                    dispMsg += pf;
                    i = 0;
                }
                else
                {
                    dispMsg += pf + ((i++ == 0) ? "" : "\t") + s.Trim();
                    pf = Environment.NewLine;
                }


            }

            dispMsg.Split('\n').ToList().ForEach(
                m => _s.WriteLine(
                    DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK") + ": " +
                    ((l != LogType.Information) ? l.ToString() + " : " : "") +
                    m
                    )
            );

        }
    }

    public class WindowsEventLogging : ILogging
    {
        private string _source;
        private string _log;
        private int _maxLvl;
        private int _dfltLvl;

        private static Dictionary<LogType, EventLogEntryType> d =
            new Dictionary<LogType, EventLogEntryType>() {
                { LogType.Warning, EventLogEntryType.Warning },
                { LogType.Error, EventLogEntryType.Error },
                { LogType.Information, EventLogEntryType.Information }
            };

        public WindowsEventLogging(string source, string log, int maxLvl, int dfltLvl)
        {
            this._source = source;
            this._log = log;
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;

            if (!EventLog.SourceExists(source))
            {

                EventSourceCreationData data = new EventSourceCreationData(source, log);
                EventLog.CreateEventSource(data);

            }

           

        }

        public bool ignore()
        {
            return false;
        }

        public void log(string msg, LogType l, params Object[] p)
        {
            this.log(msg, this._dfltLvl, l, p);
        }

        public void log(string msg, int lvl, LogType l, params Object[] p)
        {
            this.log(msg, lvl, null, l, p);
        }

        public void log(string msg, int lvl, LoggingException e, LogType l, params Object[] p)
        {
            if (lvl <= this._maxLvl)
                EventLog.WriteEntry(this._source, string.Format(msg, p) + ((e != null) ? Environment.NewLine + e.StackTrace : ""), d[l]);
        }

    }

    public class FileSystemLogging : ILogging
    {
        private string _fileName;
        private int _maxLvl;
        private int _dfltLvl;

        public string generateLogFile(string path, string file, string tStamp)
        {
            return Path.Combine(path, file + tStamp + ".log");
        }

        public FileSystemLogging(string path, string file, bool tStamp, int maxLvl, int dfltLvl)
        {
            this._fileName = generateLogFile(path, file, (tStamp) ? "_" + DateTime.Now.ToString("yyyyMMddHHmmss") : "");
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;
        }

        public bool ignore()
        {
            return false;
        }

        public void log(string msg, LogType l, params Object[] p)
        {
            this.log(msg, this._dfltLvl, l, p);
        }

        public void log(string msg, int lvl, LogType l, params Object[] p)
        {
            this.log(msg, lvl, null, l, p);
        }

        public void log(string msg, int lvl, LoggingException e, LogType l, params Object[] p)
        {
            if (lvl <= this._maxLvl)
            {
                using (StreamWriter sw = File.AppendText(this._fileName))// Creates or opens and appends
                {
                    Logging.logStream(sw, msg, lvl, e, l, p);
                } 
            }
        }
    }

    public class ConsoleLogging : ILogging
    {
        private int _maxLvl;
        private int _dfltLvl;

        public ConsoleLogging(int maxLvl, int dfltLvl)
        {
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;
        }

        public bool ignore()
        {
            return false;
        }

        public void log(string msg, LogType l, params Object[] p)
        {
            this.log(msg, this._dfltLvl, l, p);
        }

        public void log(string msg, int lvl, LogType l, params Object[] p)
        {
            this.log(msg, lvl, null, l, p);
        }

        public void log(string msg, int lvl, LoggingException e, LogType l, params Object[] p)
        {
            if (lvl <= this._maxLvl)
            {
                Logging.logStream(Console.Out, msg, lvl, e, l, p);
            }

        }
    }
    
    public class StringLogging : ILogging
    {
        private StringBuilder _s = null;

        private int _maxLvl;
        private int _dfltLvl;

        public StringLogging(StringBuilder s, int maxLvl, int dfltLvl)
        {
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;

            this._s = s;
        }

        public bool ignore()
        {
            return false;
        }

        public void log(string msg, LogType l, params Object[] p)
        {
            this.log(msg, this._dfltLvl, l, p);
        }

        public void log(string msg, int lvl, LogType l, params Object[] p)
        {
            this.log(msg, lvl, null, l, p);
        }

        public void log(string msg, int lvl, LoggingException e, LogType l, params Object[] p)
        {
            lock (this._s)
            {
                if (lvl <= this._maxLvl)
                    this._s.AppendLine(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK") + ": " + ((l != LogType.Information) ? l.ToString() + " : " : "") + string.Format(msg, p) + ((e != null) ? Environment.NewLine + e.StackTrace : ""));
            }

        }
    }

    public class PipeLoggingServer
    {
        public PipeLoggingServer()
        {
            myWeb.ILoggingService lService = new myWeb.LoggingService();
            ServiceHost host = new ServiceHost(lService, new[] { new Uri("net.pipe://localhost") });

            NetNamedPipeBinding b = new NetNamedPipeBinding();

            b.MaxReceivedMessageSize = 2147483647;
            b.MaxBufferPoolSize = 2147483647;
            b.MaxBufferSize = 2147483647;
            b.MaxConnections = 16;

            host.AddServiceEndpoint(typeof(myWeb.ILoggingService), b, "/LoggingService");
            try
            {
                host.Open();
            }
            catch (CommunicationObjectFaultedException cofe)
            {
                throw cofe;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
        }
          
    }


    public class PipeLogging : ILogging, myWeb.ILoggingServiceCallback
    {
        
        DuplexChannelFactory<myWeb.ILoggingService> pipeFactory = null;

        public PipeLogging()
        {
            pipeFactory =
                new DuplexChannelFactory<myWeb.ILoggingService>(
                    this,
                    new NetNamedPipeBinding(),
                    new EndpointAddress(new Uri("net.pipe://localhost/LoggingService"))
                );
        }


        public bool ignore()
        {
            return false;
        }

        public void log(string msg, LogType l, params object[] p)
        {
            myWeb.ILoggingService pipeProxy = pipeFactory.CreateChannel();
            ((IClientChannel)pipeProxy).Open();

            try
            {
                pipeProxy.log(msg, l, p);
            }
            catch
            {
                ((IClientChannel)pipeProxy).Abort();
            }
            finally
            {
                ((IClientChannel)pipeProxy).Close();
            }

            
        }

        public void log(string msg, int lvl, LogType l, params object[] p)
        {
            myWeb.ILoggingService pipeProxy = pipeFactory.CreateChannel();
            ((IClientChannel)pipeProxy).Open();

            try
            {
                pipeProxy.log(msg, lvl, l, p);
            }
            catch
            {
                ((IClientChannel)pipeProxy).Abort();
            }
            finally
            {
                ((IClientChannel)pipeProxy).Close();
            }
            
        }

        public void log(string msg, int lvl, LoggingException e, LogType l, params object[] p)
        {
            myWeb.ILoggingService pipeProxy = pipeFactory.CreateChannel();
            ((IClientChannel)pipeProxy).Open();

            try
            {
                pipeProxy.log(msg, lvl, e, l, p);
            }
            catch
            {
                ((IClientChannel)pipeProxy).Abort();
            }
            finally
            {
                ((IClientChannel)pipeProxy).Close();
            }
            
        }

        public void Logged()
        {
            
        }
    }
}
