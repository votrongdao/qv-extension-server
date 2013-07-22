﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Threading;
using System.Diagnostics;
using System.IO;

using CapCore = myQv.Core;

namespace myQv.Web
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
            CapCore.Logging.log("Trying to start wrapper on folder : " + webFolder, CapCore.LogType.Information, 1);

            HttpListenerWrapper lw = (HttpListenerWrapper)ApplicationHost.CreateApplicationHost(typeof(HttpListenerWrapper), "/", webFolder);

            List<string> prefixes = new List<string>();
            if (nsNames != null)
                foreach (string nsName in nsNames)
                    prefixes.Add("http://" + nsName + ":" + port.ToString() + "/");

            prefixes.Add("http://localhost:" + port.ToString() + "/");
            prefixes.Add("http://127.0.0.1:" + port.ToString() + "/");

            lw.Configure(prefixes.ToArray(), "/", webFolder);

            return lw;
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
            _listener.Start();
        }

        public void Stop()
        {
            try
            {
                _listener.Abort();
                _listener.Stop();
            }
            catch { }
        }

        public void BeginProcessRequest()
        {
           _result = _listener.BeginGetContext(this.EndProcessRequest, null);
        }

        public bool WaitHandle(int timeOut)
        {
            if (!(_result == null) && !(_result.IsCompleted)) return _result.AsyncWaitHandle.WaitOne(timeOut);
            return true;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            HttpListenerContext ctx = _listener.EndGetContext(result);
            HttpListenerWorkerRequest workerRequest = new HttpListenerWorkerRequest(ctx, _virtualDir, _physicalDir);
            HttpRuntime.ProcessRequest(workerRequest);

            _result = null;
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
            try
            {
                _context.Response.OutputStream.Close();
                _context.Response.Close();
            }
            catch (Exception e)
            {
                CapCore.Logging.log("Error: " + e.Message + "\n" + e.StackTrace, CapCore.LogType.Error, 3);
            }
        }
        public override void FlushResponse(bool finalFlush)
        {
            _context.Response.OutputStream.Flush();
        }
        public override string GetHttpVerbName()
        {
            return _context.Request.HttpMethod;
        }
        public override string GetHttpVersion()
        {
            return string.Format("HTTP/{0}.{1}",
                _context.Request.ProtocolVersion.Major,
                _context.Request.ProtocolVersion.Minor);
        }
        public override string GetLocalAddress()
        {
            return _context.Request.LocalEndPoint.Address.ToString();
        }
        public override int GetLocalPort()
        {
            return _context.Request.LocalEndPoint.Port;
        }
        public override string GetQueryString()
        {
            string queryString = "";
            string rawUrl = _context.Request.RawUrl;
            int index = rawUrl.IndexOf('?');
            if (index != -1)
                queryString = rawUrl.Substring(index + 1);
            return queryString;
        }
        public override string GetRawUrl()
        {
            return _context.Request.RawUrl;
        }
        public override string GetRemoteAddress()
        {
            return _context.Request.RemoteEndPoint.Address.ToString();
        }
        public override int GetRemotePort()
        {
            return _context.Request.RemoteEndPoint.Port;
        }
        public override string GetUriPath()
        {
            return _context.Request.Url.LocalPath;
        }
        public override void SendKnownResponseHeader(int index, string value)
        {
            _context.Response.Headers[
                HttpWorkerRequest.GetKnownResponseHeaderName(index)] = value;
        }
        public override void SendResponseFromMemory(byte[] data, int length)
        {
            _context.Response.OutputStream.Write(data, 0, length);
        }
        public override void SendStatus(int statusCode, string statusDescription)
        {
            _context.Response.StatusCode = statusCode;
            _context.Response.StatusDescription = statusDescription;
        }
        public override void SendUnknownResponseHeader(string name, string value)
        {
            _context.Response.Headers[name] = value;
        }
        public override void SendResponseFromFile(
            IntPtr handle, long offset, long length) { }
        public override void SendResponseFromFile(
            string filename, long offset, long length) { }

        // additional overrides
        public override void CloseConnection()
        {
        }
        public override string GetAppPath()
        {
            return _virtualDir;
        }
        public override string GetAppPathTranslated()
        {
            return _physicalDir;
        }
        public override int ReadEntityBody(byte[] buffer, int size)
        {
            return _context.Request.InputStream.Read(buffer, 0, size);
        }
        public override string GetUnknownRequestHeader(string name)
        {
            return _context.Request.Headers[name];
        }
        public override string[][] GetUnknownRequestHeaders()
        {
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
            // TODO: this is a hack
            string s = _context.Request.Url.LocalPath;
            if (s.IndexOf(".aspx") != -1)
                s = s.Substring(0, s.IndexOf(".aspx") + 5);
            else if (s.IndexOf(".asmx") != -1)
                s = s.Substring(0, s.IndexOf(".asmx") + 5);
            return s;
        }
        public override string GetFilePathTranslated()
        {
            string s = GetFilePath();
            s = s.Substring(_virtualDir.Length);
            s = s.Replace('/', '\\');
            return _physicalDir + s;
        }

        public override string GetPathInfo()
        {
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
                CapCore.Logging.log("Waiting for a request to come ...", CapCore.LogType.Information, 4);
                ((HttpListenerWrapper)p).BeginProcessRequest();

                while (!_shouldStop && !((HttpListenerWrapper)p).WaitHandle(1000))
                {
                    CapCore.Logging.log("Waiting ...", CapCore.LogType.Information, 11);
                }

                if (_shouldStop)
                {
                    ((HttpListenerWrapper)p).Stop();
                    CapCore.Logging.log("Exiting ...", CapCore.LogType.Information, 1);
                }
                else
                {
                    CapCore.Logging.log("Request received !", CapCore.LogType.Information, 4);
                }
            }
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
    }
}