using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Threading;
using System.Diagnostics;
using System.IO;

using myCore = frqtlib.Core;

namespace frqtlib.Web
{
    public class HttpListenerWrapper : MarshalByRefObject
    {
        private HttpListener _listener;
        private string _virtualDir;
        private string _physicalDir;

        private IAsyncResult _result;

        public static HttpListenerWrapper GetWrapper(int port, List<string> nsNames)
        {
            string webFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Web";

            myCore.Logging.log("Trying to start wrapper on folder : " + webFolder, 1, myCore.LogType.Information);

            HttpListenerWrapper lw = (HttpListenerWrapper)ApplicationHost.CreateApplicationHost(typeof(HttpListenerWrapper), "/", webFolder);

            List<string> prefixes = new List<string>();

            if (nsNames != null)
                foreach (string nsName in nsNames)
                {
                    prefixes.Add("http://" + nsName + ":" + port.ToString() + "/QvExtApi/");
                }

            HttpListenerWrapper.addPrefix(prefixes, "http://localhost:" + port.ToString() + "/QvExtApi/");
            HttpListenerWrapper.addPrefix(prefixes, "http://127.0.0.1:" + port.ToString() + "/QvExtApi/");

            lw.Configure(prefixes.ToArray(), "/", webFolder);

            return lw;
        }

        private static void addPrefix(List<string> prefixes, string prefix)
        {
            if (!prefixes.Contains(prefix)) prefixes.Add(prefix);
        }

        public void Configure(string[] prefixes, string vdir, string pdir)
        {
            _virtualDir = vdir;
            _physicalDir = pdir;
            _listener = new HttpListener();

            foreach (string prefix in prefixes)
                _listener.Prefixes.Add(prefix);
        }

        public void Start()
        {
            myCore.ILogging pl = new myCore.PipeLogging(); // new myCore.ConsoleLogging(10, 6); // 
            myCore.Logging.addLogging(pl);

            myCore.Logging.log("Start()", 5, myCore.LogType.Information);
            
            _listener.Start();
        }

        public void Stop()
        {
            myCore.Logging.log("Stop()", 5, myCore.LogType.Information);
            try
            {
                _listener.Abort();
                _listener.Stop();
            }
            catch { }
        }

        public void BeginProcessRequest()
        {
            try
            {
                myCore.Logging.log("BeginProcessRequest()", 6, myCore.LogType.Information);
                _result = _listener.BeginGetContext(this.EndProcessRequest, null);
            }
            catch (Exception e)
            {
                

                throw e;
            }
        }

        public bool WaitHandle(int timeOut)
        {
            try
            {
                // myCore.Logging.log("WaitHandle()", 11, myCore.LogType.Information);
                if (!(_result == null) && !(_result.IsCompleted)) return _result.AsyncWaitHandle.WaitOne(timeOut);
                return true;
            }
            catch (Exception e)
            {
                myCore.Logging.log("Error", 0, e, myCore.LogType.Error);

                throw e;
            }
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            try
            {

                myCore.Logging.log("EndProcessRequest()", 6, myCore.LogType.Information);
                HttpListenerContext ctx = _listener.EndGetContext(result);
                HttpListenerWorkerRequest workerRequest = new HttpListenerWorkerRequest(ctx, _virtualDir, _physicalDir);
                HttpRuntime.ProcessRequest(workerRequest);

                _result = null;
            }
            catch (Exception e)
            {
                myCore.Logging.log("Error", 0, e, myCore.LogType.Error);

                throw e;
            }
        }
    }

    public class HttpListenerWorkerRequest : HttpWorkerRequest 
    {
        private HttpListenerContext _context;
        private string _virtualDir;
        private string _physicalDir;

        public HttpListenerWorkerRequest(
            HttpListenerContext context, string vdir, string pdir)
        {
            myCore.Logging.log("HttpListenerWorkerRequest()", 7, myCore.LogType.Information);

            if (null == context)
                throw new ArgumentNullException("context");
            if (null == vdir || vdir.Equals(""))
                throw new ArgumentException("vdir");
            if (null == pdir || pdir.Equals(""))
                throw new ArgumentException("pdir");

            _context = context;
            _virtualDir = vdir;
            _physicalDir = pdir;
        }

        // required overrides (abstract)
        public override void EndOfRequest()
        {
            myCore.Logging.log("EndOfRequest()", 8, myCore.LogType.Information);

            try
            {
                _context.Response.OutputStream.Close();
                _context.Response.Close();
            }
            catch (Exception e)
            {
                myCore.Logging.log("Error", 0, e, myCore.LogType.Error);

                throw e;
            }
        }
        public override void FlushResponse(bool finalFlush)
        {
            myCore.Logging.log("FlushResponse()", 8, myCore.LogType.Information);
            _context.Response.OutputStream.Flush();
        }
        public override string GetHttpVerbName()
        {
            myCore.Logging.log("GetHttpVerbName()", 8, myCore.LogType.Information);
            return _context.Request.HttpMethod;
        }
        public override string GetHttpVersion()
        {
            myCore.Logging.log("GetHttpVersion()", 8, myCore.LogType.Information);
            return string.Format("HTTP/{0}.{1}",
                _context.Request.ProtocolVersion.Major,
                _context.Request.ProtocolVersion.Minor);
        }
        public override string GetLocalAddress()
        {
            myCore.Logging.log("GetLocalAddress()", 8, myCore.LogType.Information);
            return _context.Request.LocalEndPoint.Address.ToString();
        }
        public override int GetLocalPort()
        {
            myCore.Logging.log("GetLocalPort()", 8, myCore.LogType.Information);
            return _context.Request.LocalEndPoint.Port;
        }
        public override string GetQueryString()
        {
            myCore.Logging.log("GetQueryString()", 8, myCore.LogType.Information);

            string queryString = "";
            string rawUrl = _context.Request.RawUrl;

            int index = rawUrl.IndexOf('?');
            if (index != -1)
                queryString = rawUrl.Substring(index + 1);

            return queryString;
        }
        public override string GetRawUrl()
        {
            myCore.Logging.log("GetRawUrl()", 8, myCore.LogType.Information);
            return _context.Request.RawUrl;
        }
        public override string GetRemoteAddress()
        {
            myCore.Logging.log("GetRemoteAddress()", 8, myCore.LogType.Information);
            return _context.Request.RemoteEndPoint.Address.ToString();
        }
        public override int GetRemotePort()
        {
            myCore.Logging.log("GetRemotePort()", 8, myCore.LogType.Information);
            return _context.Request.RemoteEndPoint.Port;
        }
        public override string GetUriPath()
        {
            myCore.Logging.log("GetUriPath()", 8, myCore.LogType.Information);
            return _context.Request.Url.LocalPath;
        }
        public override void SendKnownResponseHeader(int index, string value)
        {
            myCore.Logging.log("SendKnownResponseHeader()", 8, myCore.LogType.Information);
            _context.Response.Headers[
                HttpWorkerRequest.GetKnownResponseHeaderName(index)] = value;
        }
        public override void SendResponseFromMemory(byte[] data, int length)
        {
            myCore.Logging.log("SendResponseFromMemory()", 8, myCore.LogType.Information);
            _context.Response.OutputStream.Write(data, 0, length);
        }
        public override void SendStatus(int statusCode, string statusDescription)
        {
            myCore.Logging.log("SendStatus()", 8, myCore.LogType.Information);
            _context.Response.StatusCode = statusCode;
            _context.Response.StatusDescription = statusDescription;
        }
        public override void SendUnknownResponseHeader(string name, string value)
        {
            myCore.Logging.log("SendUnknownResponseHeader()", 8, myCore.LogType.Information);
            _context.Response.Headers[name] = value;
        }
        public override void SendResponseFromFile(
            IntPtr handle, long offset, long length) {
                myCore.Logging.log("SendResponseFromFile()", 8, myCore.LogType.Information);
        }
        public override void SendResponseFromFile(
            string filename, long offset, long length) {
                myCore.Logging.log("SendResponseFromFile()", 8, myCore.LogType.Information);
        }

        // additional overrides
        public override void CloseConnection()
        {
            myCore.Logging.log("CloseConnection()", 8, myCore.LogType.Information);
        }
        public override string GetAppPath()
        {
            myCore.Logging.log("GetAppPath()", 8, myCore.LogType.Information);
            return _virtualDir;
        }
        public override string GetAppPathTranslated()
        {
            myCore.Logging.log("GetAppPathTranslated()", 8, myCore.LogType.Information);
            return _physicalDir;
        }
        public override int ReadEntityBody(byte[] buffer, int size)
        {
            myCore.Logging.log("ReadEntityBody()", 8, myCore.LogType.Information);
            return _context.Request.InputStream.Read(buffer, 0, size);
        }
        public override string GetUnknownRequestHeader(string name)
        {
            myCore.Logging.log("GetUnknownRequestHeader()", 8, myCore.LogType.Information);
            return _context.Request.Headers[name];
        }
        public override string[][] GetUnknownRequestHeaders()
        {
            myCore.Logging.log("GetUnknownRequestHeaders()", 8, myCore.LogType.Information);
            string[][] unknownRequestHeaders;
            System.Collections.Specialized.NameValueCollection headers = _context.Request.Headers;
            int count = headers.Count;
            List<string[]> headerPairs = new List<string[]>(count);
            for (int i = 0; i < count; i++)
            {
                string headerName = headers.GetKey(i);
                if (GetKnownRequestHeaderIndex(headerName) == -1)
                {
                    string headerValue = headers.Get(i);
                    headerPairs.Add(new string[] { headerName, headerValue });
                }
            }
            unknownRequestHeaders = headerPairs.ToArray();
            return unknownRequestHeaders;
        }
        public override string GetKnownRequestHeader(int index)
        {
            myCore.Logging.log("GetKnownRequestHeader()", 8, myCore.LogType.Information);
            switch (index)
            {
                case HeaderUserAgent:
                    return _context.Request.UserAgent;
                default:
                    return _context.Request.Headers[GetKnownRequestHeaderName(index)];
            }
        }
         public override string GetServerVariable(string name)
        {
            myCore.Logging.log("GetServerVariable()", 8, myCore.LogType.Information);
            // TODO: vet this list
            switch (name)
            {
                case "HTTPS":
                    return _context.Request.IsSecureConnection ? "on" : "off";
                case "HTTP_USER_AGENT":
                    return _context.Request.Headers["UserAgent"];
                default:
                    return null;
            }
        }
        public override string GetFilePath()
        {
            myCore.Logging.log("GetFilePath()", 8, myCore.LogType.Information);

            // TODO: this is a hack
            string s = _context.Request.Url.LocalPath;

            if (s.IndexOf(".aspx") != -1)
                s = s.Substring(0, s.IndexOf(".aspx") + 5);
            else if (s.IndexOf(".asmx") != -1)
                s = s.Substring(0, s.IndexOf(".asmx") + 5);

            myCore.Logging.log(s, myCore.LogType.Information, 9);

            return s;
        }
        public override string GetFilePathTranslated()
        {
            myCore.Logging.log("GetFilePathTranslated()", 8, myCore.LogType.Information);
            string s = GetFilePath();
            s = s.Substring(_virtualDir.Length);
            s = s.Replace('/', '\\');
            return _physicalDir + s;
        }

        public override string GetPathInfo()
        {
            myCore.Logging.log("GetPathInfo()", 8, myCore.LogType.Information);
            string s1 = GetFilePath();
            string s2 = _context.Request.Url.LocalPath;
            if (s1.Length == s2.Length)
                return "";
            else
                return s2.Substring(s1.Length);
        }
    }

    public class HostWorker
    {
        private volatile bool _shouldStop = false;

        public void DoWork(object p)
        {

            while (!_shouldStop)
            {
                myCore.Logging.log("Waiting for a request to come ...", 4, myCore.LogType.Information);
                ((HttpListenerWrapper)p).BeginProcessRequest();

                while (!_shouldStop && !((HttpListenerWrapper)p).WaitHandle(1000))
                {
                    myCore.Logging.log("Waiting ...", 11, myCore.LogType.Information);
                }

                if (_shouldStop)
                {
                    ((HttpListenerWrapper)p).Stop();
                    myCore.Logging.log("Exiting ...", 1, myCore.LogType.Information);
                }
                else
                {
                    myCore.Logging.log("Request received !", 4, myCore.LogType.Information);
                }
            }
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
    }
}