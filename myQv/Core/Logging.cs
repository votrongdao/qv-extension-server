﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using System.IO;

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using System.Data;
using System.Data.SqlClient;

using System.Diagnostics;

using CapSQL = myQv.SQL;

namespace myQv.Core
{
    public enum LogType { Warning, Error, Information }

    public class LoggingException : Exception
    {
        public LoggingException(string message) : base(message) { }
        public LoggingException(string message, Exception innerException) : base(message, innerException) { }
    }

    public interface ILogging
    {
        void log(string msg, LogType l);
        void log(string msg, LogType l, int lvl);
        void log(string msg, Exception e, LogType l, int lvl);
    }

    public static class Logging
    {
        private static List<ILogging> _l = new List<ILogging>();

        public static void addLogging(ILogging l)
        {
            Logging._l.Add(l);
        }

        public static void log(string msg, LogType l)
        {
            foreach (ILogging il in Logging._l)
            {
                il.log(msg, l);
            }
        }

        public static void log(string msg, LogType l, int lvl)
        {
            foreach (ILogging il in Logging._l)
            {
                il.log(msg, l, lvl);
            }
        }

        public static void log(string msg, Exception e, LogType l, int lvl)
        {
            foreach (ILogging il in Logging._l)
            {
                il.log(msg, e, l, lvl);
            }
        }
    }

    public class WindowsEventLogging : ILogging
    {
        private string _source;
        private string _log;
        private bool _init = false;
        private int _maxLvl;
        private int _dfltLvl;

        private static Dictionary<LogType, EventLogEntryType> d =
            new Dictionary<LogType, EventLogEntryType>() {
                { LogType.Warning, EventLogEntryType.Warning },
                { LogType.Error, EventLogEntryType.Error },
                { LogType.Information, EventLogEntryType.Information }
            };

        public void init(string source, string log, int maxLvl, int dfltLvl)
        {
            this._source = source;
            this._log = log;
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;

            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, log);

            this._init = true;
        }

        public void log(string msg, LogType l)
        {
            this.log(msg, l, this._dfltLvl);
        }

        public void log(string msg, LogType l, int lvl)
        {
            this.log(msg, null, l, lvl);
        }

        public void log(string msg, Exception e, LogType l, int lvl)
        {
            if (!this._init)
                throw new LoggingException("Please init logging module before using it ...");

            if (lvl <= this._maxLvl)
                EventLog.WriteEntry(this._source, msg + ((e != null) ? Environment.NewLine + e.StackTrace : ""), d[l]);
        }

    }

    public class FileSystemLogging : ILogging
    {
        private StreamWriter _sw;
        private bool _init = false;
        private int _maxLvl;
        private int _dfltLvl;

        public StreamWriter generateLogFile(string path, string file)
        {
            return new StreamWriter(new FileInfo(Path.Combine(path, file)).Create());
        }

        public void init(string path, string file, int maxLvl, int dfltLvl)
        {
            this._sw = generateLogFile(path, file);
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;

            this._init = true;
        }

        public void log(string msg, LogType l)
        {
            this.log(msg, l, this._dfltLvl);
        }

        public void log(string msg, LogType l, int lvl)
        {
            this.log(msg, null, l, lvl);
        }

        public void log(string msg, Exception e, LogType l, int lvl)
        {
            if (!this._init)
                throw new LoggingException("Please init logging module before using it ...");

            if (lvl <= this._maxLvl)
                this._sw.WriteLine(((l != LogType.Information) ? l.ToString() + " : " : "") + msg + ((e != null) ? Environment.NewLine + e.StackTrace : ""));

        }
    }

    public class ConsoleLogging : ILogging
    {
        private bool _init = false;
        private int _maxLvl;
        private int _dfltLvl;

        public void init(int maxLvl, int dfltLvl)
        {
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;

            this._init = true;
        }

        public void log(string msg, LogType l)
        {
            this.log(msg, l, this._dfltLvl);
        }

        public void log(string msg, LogType l, int lvl)
        {
            this.log(msg, null, l, lvl);
        }

        public void log(string msg, Exception e, LogType l, int lvl)
        {
            if (!this._init)
                throw new LoggingException("Please init logging module before using it ...");

            if (lvl <= this._maxLvl)
                Console.WriteLine(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK") + ": " + ((l != LogType.Information) ? l.ToString() + " : " : "") + msg + ((e != null) ? Environment.NewLine + e.StackTrace : ""));

        }



    }
    
    public class StringLogging : ILogging
    {
        private bool _init = false;

        private StringBuilder _s = null;

        private int _maxLvl;
        private int _dfltLvl;

        public void init(StringBuilder s, int maxLvl, int dfltLvl)
        {
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;

            this._s = s;

            this._init = true;
        }

        public void log(string msg, LogType l)
        {
            this.log(msg, l, this._dfltLvl);
        }

        public void log(string msg, LogType l, int lvl)
        {
            this.log(msg, null, l, lvl);
        }

        public void log(string msg, Exception e, LogType l, int lvl)
        {
            if (!this._init)
                throw new LoggingException("Please init logging module before using it ...");

            lock (this._s)
            {
                if (lvl <= this._maxLvl)
                    this._s.AppendLine(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK") + ": " + ((l != LogType.Information) ? l.ToString() + " : " : "") + msg + ((e != null) ? Environment.NewLine + e.StackTrace : ""));
            }

        }
    }

    public class SQLLogging : ILogging
    {
        private static Dictionary<LogType, int> d =
            new Dictionary<LogType, int>() {
                { LogType.Warning, -1 },
                { LogType.Error, 1 },
                { LogType.Information, 0 }
            };

        private bool _init = false;
        private int _maxLvl;
        private int _dfltLvl;

        private long? _procRunId;

        private CapSQL.SQL _s;

        public void init(CapSQL.SQL s, long? procRunId, int maxLvl, int dfltLvl)
        {
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;
            this._procRunId = procRunId;

            this._s = s;
               
            this._init = true;
        }
  
        public void log(string msg, LogType l)
        {
            this.log(msg, l, this._dfltLvl);
        }

        public void log(string msg, LogType l, int lvl)
        {
            this.log(msg, null, l, lvl);
        }

        public Int64 getProcRunId(long? parentProcRunId, string procName, string commandLine)
        {
            if (!this._init)
                throw new LoggingException("Please init logging module before using it ...");

            SqlDataReader dr = null;

            try
            {
                dr = this._s.Execute(
                        "DECLARE @Proc_Run_ID bigint" + "\n" +
                        "EXEC [<db />].[<schema />].[SP_ADD_L_PROC]" +
                        "    @Command_Line = @Command_Line" +
                        "    ,@Process_Name = @Process_Name" +
                        "    ,@Parent_Process_run_ID = @Parent_Process_run_ID" +
                        "    ,@Process_Run_ID = @Proc_Run_ID OUT" + "\n" +
                        "SELECT @Proc_Run_ID"
                    , new NameValueCollection() {
                        {"@Command_Line", commandLine}
                        ,{"@Process_Name", procName}
                        ,{"@Parent_Process_run_ID", (parentProcRunId.HasValue) ? parentProcRunId.Value.ToString() : null }
                    }
                );

                List<Int64> l = new List<Int64>();

                while (dr.Read())
                    l.Add(dr.GetInt64(0));

                return l[0];
            }
            finally
            {
                if (dr != null && !dr.IsClosed) dr.Close();
            }

        }

        public void log(string msg, Exception e, LogType l, int lvl)
        {
            if (!this._init)
                throw new LoggingException("Please init logging module before using it ...");

            if (lvl <= this._maxLvl)
            {
                SqlDataReader dr = null;

                try
                {
                    dr = this._s.Execute(
                         "EXEC [<db />].[<schema />].[SP_ADD_L_PROC_LOG]" +
                         "    @Process_run_ID = @Process_run_ID" +
                         "    ,@Tec_Error_Code = @Tec_Error_Code" +
                         "    ,@Error_Crit = @Error_Crit" +
                         "    ,@Func_Err_msg = @Func_Err_msg" +
                         "    ,@Tech_Err_Msg = @Tech_Err_Msg"
                         , new NameValueCollection() {
                            {"@Process_run_ID", this._procRunId.ToString()}
                            ,{"@Tec_Error_Code", ((l == LogType.Error) ? -1 : 0).ToString()}
                            ,{"@Error_Crit", d[l].ToString()}
                            ,{"@Func_Err_msg", msg}
                            ,{"@Tech_Err_Msg", ((e != null) ? e.Message + ((e.StackTrace != null) ? Environment.NewLine + e.StackTrace : null) : null) }
                        }
                    );
                }
                finally
                {
                    if (dr != null && !dr.IsClosed) dr.Close();
                }

            }

          
        }
    }

    public class WSLogging : ILogging
    {
        public class WSLoggingTransport
        {
            public enum WSLoggingActionEnum { LOG, GET_PROC_RUN_ID };

            public WSLoggingActionEnum action;
            public long? procRunId;
            public int maxLvl;
            public int dfltLvl;
            public string msg;
            public string tech_msg;
            public LogType l;
            public int lvl;

            public WSLoggingTransport() { }
            public WSLoggingTransport(WSLoggingActionEnum action, long? procRunId, int maxLvl, int dfltLvl, string msg, string tech_msg, LogType l, int lvl)
            {
                this.action = action;
                this.procRunId = procRunId;
                this.maxLvl = maxLvl;
                this.dfltLvl = dfltLvl;
                this.msg = msg;
                this.tech_msg = tech_msg;
                this.l = l;
                this.lvl = lvl;
            }

            public override string ToString()
            {
                return this.msg;
            }
        }

        private int _maxLvl;
        private int _dfltLvl;
        private string _endPoint;

        private long? _procRunId;

        public long init(string endPoint, long? parentProcRunId, string procName, string commandLine, int maxLvl, int dfltLvl)
        {
            this._maxLvl = maxLvl;
            this._dfltLvl = dfltLvl;

            this._endPoint = endPoint;

            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;

            binding.MaxReceivedMessageSize = 10000000;

            EndpointAddress address = new EndpointAddress(this._endPoint);

            com.capgemini.gbi.QvCapLogWS.QvCapLogWSSoapClient client = new myQv.com.capgemini.gbi.QvCapLogWS.QvCapLogWSSoapClient(binding, address);
            myQv.com.capgemini.gbi.QvCapLogWS.returnObjectOfNullableOfInt64 ro = client.getProcRunId(parentProcRunId, procName, commandLine);

            if(ro.Error_Code != 0)
                throw new LoggingException(ro.msg + Environment.NewLine + ro.tech_msg);

            this._procRunId = ro.o;
            client.Close();

            if (!this._procRunId.HasValue)
                throw new LoggingException(procName + " is not a valid process name !");

            return this._procRunId.Value;
        }

        public void log(string msg, LogType l)
        {
            this.log(msg, l, this._dfltLvl);
        }

        public void log(string msg, LogType l, int lvl)
        {
            this.log(msg, null, l, lvl);
        }

        public void log(string msg, Exception e, LogType l, int lvl)
        {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;

            binding.MaxReceivedMessageSize = 10000000;

            EndpointAddress address = new EndpointAddress(this._endPoint);

            com.capgemini.gbi.QvCapLogWS.QvCapLogWSSoapClient client = new myQv.com.capgemini.gbi.QvCapLogWS.QvCapLogWSSoapClient(binding, address);

            myQv.com.capgemini.gbi.QvCapLogWS.returnObjectOfInt32 ro = client.log(this._procRunId, this._maxLvl, this._dfltLvl, msg, ((e != null) ? e.Message + ((e.StackTrace != null) ? Environment.NewLine + e.StackTrace : null) : null), (myQv.com.capgemini.gbi.QvCapLogWS.LogType)Enum.Parse(typeof(myQv.com.capgemini.gbi.QvCapLogWS.LogType), l.ToString()), lvl);

            if (ro.Error_Code != 0)
                throw new LoggingException(ro.msg + Environment.NewLine + ro.tech_msg);

            client.Close();
        }
    }
}
