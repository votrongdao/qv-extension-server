using System;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using System.Linq;
using System.Text;

using System.IO;
using System.IO.Pipes;
using System.Threading;

using CapCore = myQv.Core;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Security.Permissions;

namespace myQv.Threading
{

    public delegate void NewMessageEventHandler(object sender, PipeEventArgs e);

    public delegate void RefreshEndedEventHandler(object sender, PipeConnectionEventArgs e);
    public delegate void CheckDataEndedEventHandler(object sender, PipeConnectionEventArgs e);
    public delegate void SendDataEndedEventHandler(object sender, PipeConnectionEventArgs e);

    public delegate void NewConnectionMessageEventHandler(object sender, PipeConnectionEventArgs e);

    public delegate void MadeOneEventHandler<U>(object sender, MadeOneEventArgs<U> e);

    public class PipeEventArgs : EventArgs
    {
        public CapCore.IReturnObject o;
        public Guid g;

        public PipeEventArgs() : base() { }
        public PipeEventArgs(CapCore.IReturnObject o)
            : base()
        {
            this.o = o;
        }

        public PipeEventArgs(CapCore.IReturnObject o, Guid g)
            : base()
        {
            this.o = o;
            this.g = g;
        }

    }
    public class PipeConnectionEventArgs : EventArgs
    {
        public PipeTransport pt;
        public IPipeConnection pc;

        public PipeConnectionEventArgs() : base() { }
        public PipeConnectionEventArgs(IPipeConnection pc, PipeTransport pt)
            : base()
        {
            this.pt = pt;
            this.pc = pc;
        }

    }
    public class MadeOneEventArgs<U> : EventArgs
    {
        public IPipeConnection ipc;
        public U one;

        public MadeOneEventArgs() : base() { }
        public MadeOneEventArgs(IPipeConnection ipc, U o)
            : base()
        {
            this.ipc = ipc;
            this.one = o;
        }

    }

    public class PipeTransport : IXmlSerializable
    {
        private Type type;
        private CapCore.IReturnObject iro;
        private bool isResponseExpected = false;
        private Guid guid;

        public PipeTransport() { }

        public PipeTransport(Type type, CapCore.IReturnObject iro, bool isResponseExpected)
        {
            this.type = type;
            this.iro = iro;
            this.isResponseExpected = isResponseExpected;
            if(isResponseExpected) this.guid = Guid.NewGuid();
        }

        public PipeTransport(Type type, CapCore.IReturnObject iro, Guid g)
        {
            this.type = type;
            this.iro = iro;
            this.isResponseExpected = true;
            this.guid = g;

        }

        public bool IsResponseExpected()
        {
            return this.isResponseExpected;
        }

        public CapCore.IReturnObject getObject()
        {
            return this.iro;
        }

        public Type getType()
        {
            return this.type;
        }

        public Guid getGuid()
        {
            return this.guid;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            try
            {
                reader.ReadStartElement("PipeTransport");
                reader.ReadStartElement("type");

                this.type = Type.GetType(reader.ReadContentAsString());

                reader.ReadEndElement();
                reader.ReadStartElement("iro");

                
                XmlSerializer xSer = new XmlSerializer(typeof(CapCore.returnObject<>).MakeGenericType(new Type[] { this.getType() }));
                byte[] byteArray = Encoding.ASCII.GetBytes(reader.ReadOuterXml());

                if (byteArray.Length != 0)
                {
                    MemoryStream stream = new MemoryStream(byteArray);
                    this.iro = ((CapCore.IReturnObject)xSer.Deserialize(stream));
                }

                reader.ReadEndElement();
                reader.ReadStartElement("isResponseExpected");

                this.isResponseExpected = Convert.ToBoolean(reader.ReadContentAsString());

                reader.ReadEndElement();
                reader.ReadStartElement("guid");

                string s = reader.ReadContentAsString();

                if (isResponseExpected)
                {
                    this.guid = new Guid(s);
                }

                
                // reader.ReadEndElement();
            }
            catch (Exception e)
            {
                CapCore.Logging.log("Error while deserializing: " + e.Message, CapCore.LogType.Warning, 5);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("type");
            writer.WriteString(this.getType().FullName);
            writer.WriteEndElement();
            writer.WriteStartElement("iro");

            MemoryStream memStream = new MemoryStream();

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = true;

            using (XmlWriter xmlWriter = XmlWriter.Create(memStream, writerSettings))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                XmlSerializer xSer = new XmlSerializer(typeof(CapCore.returnObject<>).MakeGenericType(new Type[] { this.getType() }));
                xSer.Serialize(xmlWriter, this.getObject(), ns);
            }
            
            memStream.Position = 0;
            StreamReader streamReader = new StreamReader(memStream);
            XmlDocument serializedXML = new XmlDocument();
            serializedXML.Load(streamReader);

            writer.WriteRaw(serializedXML.OuterXml);

            writer.WriteEndElement();
            writer.WriteStartElement("isResponseExpected");
            writer.WriteString(this.isResponseExpected.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("guid");
            writer.WriteString(this.guid.ToString());
            writer.WriteEndElement();

        }

    }

    //public class PipeServer


    public interface IPipeConnection : IDisposable
    {
        event NewConnectionMessageEventHandler newMessage;

        event RefreshEndedEventHandler refreshEnded;
        event CheckDataEndedEventHandler checkDataEnded;
        event SendDataEndedEventHandler sendDataEnded;
        
        bool isConnected();
        bool isLazy();

        bool isReading();
        bool isWriting();
        bool isDisposed();

        bool refresh();
        bool checkData();
        bool sendData();

        void WriteObject<T>(T o, Guid g);
    }
    public abstract class APipeFactory<U>
    {
        public event MadeOneEventHandler<U> madeOne;

        public abstract void makeOne(IPipeConnection ipc, string pipeName, PipeDirection pipeDir, int index);

        public void OnMadeOne(MadeOneEventArgs<U> e)
        {
            MadeOneEventHandler<U> handler = madeOne;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public class PipeServerFactory : APipeFactory<NamedPipeServerStream>
    {
        private int _maxConnections;
        private int _pipeIndex = 0;

        private Dictionary<int, CapCore.Tuple<NamedPipeServerStream, IPipeConnection>> _pipeList = new Dictionary<int, CapCore.Tuple<NamedPipeServerStream, IPipeConnection>>();
        
        public PipeServerFactory(int maxConnections)
        {
            this._maxConnections = maxConnections;
        }

        public override void makeOne(IPipeConnection ipc, string pipeName, PipeDirection pipeDir, int index)
        {
            CapCore.Logging.log("Creating " + pipeDir.ToString() + " " + index.ToString() + " pipe (" + pipeName + ") ...", myQv.Core.LogType.Information, 10);

            try
            {
                NamedPipeServerStream ro = new NamedPipeServerStream(pipeName, PipeDirection.InOut, this._maxConnections, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                this._pipeList.Add(this._pipeIndex, new CapCore.Tuple<NamedPipeServerStream, IPipeConnection>(ro, ipc));
                ro.BeginWaitForConnection(new AsyncCallback(this.endmakeOne), this._pipeIndex++);
            }
            catch (Exception e)
            {
                CapCore.Logging.log("Error creating " + pipeDir.ToString() + " " + index.ToString() + " pipe (" + pipeName + "): " + e.Message, myQv.Core.LogType.Error, 5);
                throw e;
            }
        }

        private void endmakeOne(IAsyncResult state)
        {
            int ai = (int) state.AsyncState;

            CapCore.Logging.log("End creating pipe ...", myQv.Core.LogType.Information, 10);

            CapCore.Tuple<NamedPipeServerStream, IPipeConnection> ro = this._pipeList[ai];
            this._pipeList.Remove(ai);

            try
            {
                ro.First.EndWaitForConnection(state);
            }
            catch (Exception e)
            {
                CapCore.Logging.log("Error: " + e.Message, myQv.Core.LogType.Error, 10);
            }
            base.OnMadeOne(new MadeOneEventArgs<NamedPipeServerStream>(ro.Second, ro.First));

        }

        
    }
    public class PipeClientFactory : APipeFactory<NamedPipeClientStream>
    {
        public PipeClientFactory() { }

        public override void makeOne(IPipeConnection ipc, string pipeName, PipeDirection pipeDir, int index)
        {
            CapCore.Logging.log("Creating " + pipeDir.ToString() + " " + index.ToString() + " pipe (" + pipeName + ") ...", myQv.Core.LogType.Information, 10);
            NamedPipeClientStream ro = new NamedPipeClientStream("localhost", pipeName, PipeDirection.InOut);
            ro.Connect(10000);
            ro.ReadMode = PipeTransmissionMode.Message;
            this.OnMadeOne(new MadeOneEventArgs<NamedPipeClientStream>(ipc, ro));
        }

    }

    public class UniPipeConnection<U> : IPipeConnection where U: PipeStream
    {
        public event NewConnectionMessageEventHandler newMessage;

        public event RefreshEndedEventHandler refreshEnded;
        public event CheckDataEndedEventHandler checkDataEnded;
        public event SendDataEndedEventHandler sendDataEnded;

        private APipeFactory<U> _pf = null;
        private int _index;
        private U _iPipe = null;
        private string _pipeName;
        private PipeDirection _pipeDir;

        private volatile object _classLock = new object();

        private volatile bool _isWaiting = false;
        private volatile object _isWaitingLock = new object();

        private volatile bool _isReading = false;
        private volatile object _isReadingLock = new object();

        private volatile bool _isWriting = false;
        private volatile object _isWritingLock = new object();

        private volatile bool _isDisposed = false;
        private volatile object _isDisposedLock = new object();

        public byte[] curBuf = new byte[512];
        public MemoryStream msgStream = new MemoryStream();

        public Queue<CapCore.Tuple<CapCore.IReturnObject, Guid>> toSend = new Queue<CapCore.Tuple<CapCore.IReturnObject, Guid>>();
        public object toSendLock = new object();

        private readonly AutoResetEvent _hasFinishedRefreshingSignal = new AutoResetEvent(false);
        private readonly AutoResetEvent _hasFinishedWritingSignal = new AutoResetEvent(true);
        private readonly AutoResetEvent _hasFinishedReadingSignal = new AutoResetEvent(true);

        public UniPipeConnection(APipeFactory<U> pf, string pipeName, PipeDirection pipeDir, int index)
        {
            this._pf = pf;
            this._pipeName = pipeName;
            this._pipeDir = pipeDir;
            this._index = index;

            this._pf.madeOne += new MadeOneEventHandler<U>(endEndRefresh);
        }

        public bool isConnected()
        {
            lock (this._isWaitingLock)
            {
                return !this._isWaiting && this._iPipe != null && this._iPipe.IsConnected;
            }
        }

        public bool isReading()
        {
            lock (this._isReadingLock)
            {
                return this.isConnected() && this._isReading;
            }
        }

        public bool isWriting()
        {
            lock (this._isWritingLock)
            {
                return this.isConnected() && this._isWriting;
            }
        }

        public bool isDisposed()
        {
            lock (this._isDisposedLock)
            {
                return this._isDisposed;
            }
        }

        public bool isLazy()
        {
            lock (this._isWaitingLock)
            {
                return !this.isConnected() && !this._isWaiting;
            }
        }

        public void clearStream()
        {
            this.msgStream.Flush();
        }

        public bool refresh()
        {
            lock (this._classLock)
            {
                this._hasFinishedRefreshingSignal.Reset();

                CapCore.Logging.log("Named pipe " + this._index + ": _hasFinishedRefreshingSignal => 0", myQv.Core.LogType.Information, 11);
                CapCore.Logging.log("Named pipe " + this._index + ": Checking if refresh is possible (Disposed = " + this.isDisposed() + ", Lazy = " + this.isLazy() + ") ...", CapCore.LogType.Information, 10);

                if (this.isDisposed()) return false;
                // if (!this.isLazy()) return true;

                CapCore.Logging.log("Named pipe " + this._index + ": Refreshing ...", CapCore.LogType.Information, 10);

                lock (this._isWaitingLock)
                {
                    if (this._isWaiting) return false;
                    this._isWaiting = true;
                }

                this.clearStream();

                if (this._iPipe != null)
                {
                    this._iPipe.Dispose();
                    this._iPipe = null;
                }

                try
                {
                    this._pf.makeOne(this, this._pipeName, this._pipeDir, this._index);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            
        }

        private void endEndRefresh(object sender, MadeOneEventArgs<U> e)
        {
            if (this == e.ipc)
            {
                lock (this._classLock)
                {
                    lock (this._isWaitingLock)
                    {
                        this._isWaiting = false;
                        this._iPipe = e.one;
                    }

                    this._hasFinishedRefreshingSignal.Set();

                    CapCore.Logging.log("Named pipe " + this._index + ": this._hasFinishedRefreshingSignal => 1", myQv.Core.LogType.Information, 11);
                }

                CapCore.Logging.log("Named pipe " + this._index + ": connection established!", CapCore.LogType.Information, 5);

                if (this.refreshEnded != null) this.refreshEnded(this, new PipeConnectionEventArgs(this, null));
            }
        }

        public bool checkData()
        {

            lock (this._classLock)
            {
                this._hasFinishedReadingSignal.Reset();

                CapCore.Logging.log("Named pipe " + this._index + ": _hasFinishedReadingSignal => 0", myQv.Core.LogType.Information, 11);
            }

            if (this.isDisposed()) return false;
            if (!this.isConnected()) return false;

            bool goodTogo = false;

            lock (this._isReadingLock)
            {
                if (!this._isReading)
                {
                    this._isReading = true;
                    goodTogo = true;
                }
            }

            if (goodTogo)
            {
                CapCore.Logging.log("Named pipe " + this._index + ": checking for data to recieve ...", CapCore.LogType.Information, 10);

                this._iPipe.BeginRead(this.curBuf, 0, this.curBuf.Length, new AsyncCallback(this.endCheckData), null);
                CapCore.Logging.log("Named pipe " + this._index + ": begin of read operation", CapCore.LogType.Information, 10);
            }

            lock (this._classLock)
            {
                CapCore.Logging.log("Named pipe " + this._index + ": _hasFinishedReadingSignal => 1", myQv.Core.LogType.Information, 11);
                this._hasFinishedReadingSignal.Set();
            }

            return true;
        }

        private void endCheckData(IAsyncResult state)
        {
            lock (this._classLock)
            {
                this._hasFinishedReadingSignal.Reset();

                CapCore.Logging.log("Named pipe " + this._index + ": _hasFinishedReadingSignal => 0", myQv.Core.LogType.Information, 11);
            }

            CapCore.Logging.log("Named pipe " + this._index + " (" + ((this.isConnected()) ? "connected" : "disconnected") + "): end of read operation", CapCore.LogType.Information, 10);

            if (this.isDisposed())
                lock (this._isReadingLock)
                {
                    this._isReading = false;
                    return;
                }

            bool connStateOK = this._iPipe.IsConnected;
            bool isStillReading = false;

            try
            {
                int bytesRead = this._iPipe.EndRead(state);
                this.msgStream.Write(this.curBuf, 0, bytesRead);

                if (connStateOK && !this._iPipe.IsMessageComplete)
                {
                    this._iPipe.BeginRead(this.curBuf, 0, this.curBuf.Length, new AsyncCallback(this.endCheckData), null);
                    isStillReading = true;
                }
                else
                {
                    this.msgStream.Position = 0;

                    if (connStateOK)
                    {
                        PipeTransport pt = null;

                        try
                        {
                            XmlSerializer xSer = new XmlSerializer(typeof(PipeTransport));
                            pt = ((PipeTransport)xSer.Deserialize(this.msgStream));
                        }
                        catch (Exception e)
                        {
                            CapCore.Logging.log("Named pipe " + this._index + ": recieved undeserializable data (" + ASCIIEncoding.ASCII.GetString(this.msgStream.ToArray()).Replace("\n", "").Replace("\r", "") + "). Skipping ...", CapCore.LogType.Information, 5);
                        }

                        if (pt != null && pt.getObject() != null)
                        {
                            if (this.newMessage != null) this.newMessage(this, new PipeConnectionEventArgs(this, pt));

                            CapCore.Logging.log("Named pipe " + this._index + ": recieved: " + pt.getObject().getObject().ToString(), CapCore.LogType.Information, 5);
                        }
                        else
                        {
                            CapCore.Logging.log("Named pipe " + this._index + ": recieved: null", CapCore.LogType.Information, 5);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CapCore.Logging.log("Named pipe " + this._index + ": Error: " + e.Message + Environment.NewLine + e.StackTrace, CapCore.LogType.Error, 5);
            }
            finally {

                if (!isStillReading)
                {
                    this.msgStream = new MemoryStream();

                    lock (this._isReadingLock)
                    {
                        this._isReading = false;
                    }

                    lock (this._classLock)
                    {
                        this._hasFinishedReadingSignal.Set();

                        CapCore.Logging.log("Named pipe " + this._index + ": _hasFinishedReadingSignal => 1", myQv.Core.LogType.Information, 11);
                    }

                    if (this.checkDataEnded != null) this.checkDataEnded(this, new PipeConnectionEventArgs(this, null));
                }
            }
        }

        public bool sendData()
        {
            lock (this._classLock)
            {
                this._hasFinishedWritingSignal.Reset();

                CapCore.Logging.log("Named pipe " + this._index + ": _hasFinishedWritingSignal => 0", myQv.Core.LogType.Information, 11);
            }

            if (!this.isConnected()) return false;

            bool goodTogo = false;
            bool isStillWriting = false;

            lock (this._isWritingLock)
            {
                if (!this._isWriting)
                {
                    this._isWriting = true;
                    goodTogo = true;
                }
            }

            if (goodTogo)
            {
                CapCore.Tuple<CapCore.IReturnObject, Guid> tirog = null;
                lock (this.toSendLock)
                {
                    if (this.toSend.Count > 0) tirog = this.toSend.Dequeue();
                }

                if (tirog != null)
                {
                    CapCore.Logging.log("Named pipe " + this._index + ": checking for data to send ...", CapCore.LogType.Information, 10);

                    MemoryStream ms = new MemoryStream();
                    XmlSerializer xSer = new XmlSerializer(typeof(PipeTransport));
                    PipeTransport pt = new PipeTransport(tirog.First.getObjectType(), tirog.First, tirog.Second);
                    xSer.Serialize(ms, pt);

                    try
                    {
                        this._iPipe.BeginWrite(ms.GetBuffer(), 0, ms.GetBuffer().Length, new AsyncCallback(this.endSendData), null);
                        CapCore.Logging.log("Named pipe " + this._index + ": begin of write operation", CapCore.LogType.Information, 10);

                        isStillWriting = true;
                    }
                    catch (Exception e)
                    {
                        CapCore.Logging.log("Error: Named pipe " + this._index + ": Impossible to send data ...", CapCore.LogType.Error, 5);
                    }
                }
            }

            if (!isStillWriting)
            {
                lock (this._isWritingLock)
                {
                    this._isWriting = false;
                }

                lock (this.toSendLock)
                {
                    if (this.toSend.Count == 0)
                    {
                        lock (this._classLock)
                        {
                            this._hasFinishedWritingSignal.Set();

                            CapCore.Logging.log("Named pipe " + this._index + ": _hasFinishedWritingSignal => 1", myQv.Core.LogType.Information, 11);
                        }

                        CapCore.Logging.log("Named pipe " + this._index + ": finished writing signal sent ...", CapCore.LogType.Information, 10);
                    }
                }
            }

            return true;
        }

        private void endSendData(IAsyncResult state)
        {
            CapCore.Logging.log("Named pipe " + this._index + " (" + ((this.isConnected()) ? "connected" : "disconnected") + "): end of write operation", CapCore.LogType.Information, 10);

            try
            {
                this._iPipe.EndWrite(state);
            }
            catch (Exception e)
            {
                CapCore.Logging.log("Named pipe " + this._index + ": Error: " + e.Message, CapCore.LogType.Error, 5);
            }
            finally
            {
                CapCore.Logging.log("Named pipe " + this._index + ": locking _isWritingLock", myQv.Core.LogType.Information, 12);
                
                lock (this._isWritingLock)
                {
                    this._isWriting = false;
                }

                CapCore.Logging.log("Named pipe " + this._index + ": locking toSendLock", myQv.Core.LogType.Information, 12);
                lock (this.toSendLock)
                {
                    CapCore.Logging.log("Named pipe " + this._index + ": " + this.isDisposed() + ", " + this.toSend.Count, myQv.Core.LogType.Information, 12);
                    if (this.toSend.Count == 0)
                    {
                        this._hasFinishedWritingSignal.Set();

                        CapCore.Logging.log("Named pipe " + this._index + ": _hasFinishedWritingSignal => 1", myQv.Core.LogType.Information, 11);
                        CapCore.Logging.log("Named pipe " + this._index + ": finished writing sent ...", CapCore.LogType.Information, 10);
                    }
                }

                if (this.sendDataEnded != null) this.sendDataEnded(this, new PipeConnectionEventArgs(this, null));
            }
        }

        public void WriteObject<T>(T o, Guid g)
        {
            CapCore.Logging.log("Write object required: " + o.ToString() + " ...", myQv.Core.LogType.Information, 10);

            if (this.isDisposed()) throw new PipeException("Pipe is disposed!");

            lock (this.toSendLock)
            {
                this.toSend.Enqueue(
                    new CapCore.Tuple<CapCore.IReturnObject, Guid>(
                        new CapCore.returnObject<T>(o, 0, "ok!"),
                        g
                    )
                );
            }
        }

        public void Dispose()
        {
            lock (this._classLock)
            {
                lock (this._isDisposedLock)
                {
                    this._isDisposed = true;
                }
            }

            CapCore.Logging.log("Named pipe " + this._index + ": Dispose required ...", CapCore.LogType.Information, 10);
            bool gtg = false;

            while (!gtg)
            {
                gtg = WaitHandle.WaitAll(new WaitHandle[] { this._hasFinishedWritingSignal, this._hasFinishedReadingSignal, this._hasFinishedRefreshingSignal }, 1000);
                CapCore.Logging.log("Named pipe " + this._index + ": waiting all handles ...", CapCore.LogType.Information, 10);
            }


            CapCore.Logging.log("Named pipe " + this._index + ": ok for dispose ...", CapCore.LogType.Information, 10);
            

            if (this._iPipe != null)
            {
                this._iPipe.Close();
                this._iPipe.Dispose();
            }
        }

    }
    public class PipeConnection<U> : IPipeConnection where U : PipeStream
    {
        public event NewConnectionMessageEventHandler newMessage;

        public event RefreshEndedEventHandler refreshEnded;
        public event CheckDataEndedEventHandler checkDataEnded;
        public event SendDataEndedEventHandler sendDataEnded;

        private IPipeConnection _i = null;
        private IPipeConnection _o = null;

        bool _isDisposed = false;

        private int _refreshedCount = 0;
        private object _refreshedCountLock = new object();

        APipeFactory<U> _pf = null;
        string _pipeName = null;
        PipeDirection _pipeDir = PipeDirection.InOut;
        int _index = -1;

        public PipeConnection(APipeFactory<U> pf, string pipeName, PipeDirection pipeDir, int index)
        {
            this._pf = pf;
            this._pipeName = pipeName;
            this._pipeDir = pipeDir;
            this._index = index;

            this.refresh();
        }


        void _i_newMessage(object sender, PipeConnectionEventArgs e)
        {
            if (this.newMessage != null) this.newMessage(this, new PipeConnectionEventArgs(this, e.pt));
        }
        void _io_refreshEnded(object sender, PipeConnectionEventArgs e)
        {
            lock (this._refreshedCountLock)
            {
                this._refreshedCount++;
                if (this.refreshEnded != null && this._refreshedCount == 2) this.refreshEnded(this, e);
            }
        }
        void _i_checkDataEnded(object sender, PipeConnectionEventArgs e)
        {
            if (this.checkDataEnded != null) this.checkDataEnded(this, e);
        }
        void _o_sendDataEnded(object sender, PipeConnectionEventArgs e)
        {
            if (this.sendDataEnded != null) this.sendDataEnded(this, e);
        }


        public bool isConnected()
        {
            return this._i.isConnected() && this._o.isConnected();
        }

        public bool isLazy()
        {
            return this._i.isLazy() || this._o.isLazy();
        }

        public bool isReading()
        {
            return this._i.isReading();
        }

        public bool isWriting()
        {
            return this._o.isWriting();
        }

        public bool isDisposed()
        {
            return this._i.isDisposed() || this._o.isDisposed();
        }

        public bool refresh()
        {
            lock (this._refreshedCountLock)
            {
                this._refreshedCount = 0;
            }

            bool i = true;
            bool o = true;

            if (this._isDisposed) return false;

            if (this._i != null && !this._i.isDisposed()) i = this._i.refresh();
            else
            {
                this._i = new UniPipeConnection<U>(this._pf, this._pipeName + ((this._pipeDir == PipeDirection.In) ? "/In" : "/Out"), PipeDirection.In, this._index);

                this._i.newMessage += new NewConnectionMessageEventHandler(_i_newMessage);
                this._i.refreshEnded += new RefreshEndedEventHandler(_io_refreshEnded);
                this._i.checkDataEnded += new CheckDataEndedEventHandler(_i_checkDataEnded);

                i = this._i.refresh();
            }

            if (this._o != null && !this._o.isDisposed()) o = this._o.refresh();
            else
            {
                this._o = new UniPipeConnection<U>(this._pf, this._pipeName + ((this._pipeDir == PipeDirection.In) ? "/Out" : "/In"), PipeDirection.Out, this._index);

                this._o.refreshEnded += new RefreshEndedEventHandler(_io_refreshEnded);
                this._o.sendDataEnded += new SendDataEndedEventHandler(_o_sendDataEnded);

                o = this._o.refresh();
            }

            return i && o;
        }

        public bool checkData()
        {
            if (this._i.isDisposed()) return false;
            return this._i.checkData();
        }

        public bool sendData()
        {
            return this._o.sendData();
        }

        public void WriteObject<T>(T o, Guid g)
        {
            this._o.WriteObject<T>(o, g);
        }

        public void Dispose()
        {
            this._isDisposed = true;

            this._i.Dispose();
            this._o.Dispose();
        }

    }

    public class Pipe<U> : IDisposable where U : PipeStream
    {
        public event NewMessageEventHandler newMessage;

        protected PipeConnection<U>[] _iPipes;

        private Dictionary<Guid, IPipeConnection> _responsePipe = new Dictionary<Guid, IPipeConnection>();
        private object _responsePipeLock = new object();

        private Dictionary<Type, List<CapCore.Tuple<CapCore.IReturnObject, Guid>>> _received = new Dictionary<Type, List<CapCore.Tuple<CapCore.IReturnObject, Guid>>>();
        private object _receivedLock = new object();

        private readonly AutoResetEvent _receivedSignal = new AutoResetEvent(false);
        private readonly AutoResetEvent _disposedSignal = new AutoResetEvent(false); 

        public Pipe(APipeFactory<U> pf, string pipeName, PipeDirection pipeDir, int pipeNum)
        {
            this._iPipes = new PipeConnection<U>[pipeNum];

            for (int i = 0; i < this._iPipes.Length; i++)
            {
                this._iPipes[i] = new PipeConnection<U>(pf, pipeName, pipeDir, i);
                this._iPipes[i].newMessage += new NewConnectionMessageEventHandler(Pipe_newMessage);
            }
        }

        private void Pipe_newMessage(object sender, PipeConnectionEventArgs e)
        {
            CapCore.IReturnObject ro = null;

            lock (this._responsePipeLock)
            {
                if (!this._responsePipe.ContainsKey(e.pt.getGuid())) this._responsePipe.Add(e.pt.getGuid(), e.pc);
            }

            lock (this._receivedLock)
            {
                if (!this._received.ContainsKey(e.pt.getType())) this._received.Add(e.pt.getType(), new List<CapCore.Tuple<CapCore.IReturnObject, Guid>>());
                this._received[e.pt.getType()].Add(new CapCore.Tuple<CapCore.IReturnObject, Guid>(e.pt.getObject(), e.pt.getGuid()));
                this._receivedSignal.Set();
                ro = e.pt.getObject();
            }

            if (this.newMessage != null) this.newMessage(this, new PipeEventArgs(ro, e.pt.getGuid()));
        }

        public List<CapCore.Tuple<T, Guid>> readObjects<T>()
        {
            List<CapCore.Tuple<T, Guid>> ro = new List<CapCore.Tuple<T, Guid>>();
            lock (this._receivedLock)
            {
                if (this._received.ContainsKey(typeof(T)))
                {
                    this._received[typeof(T)].ForEach(i => ro.Add(new CapCore.Tuple<T, Guid>(i.First.getObject<T>(), i.Second))); ;
                    this._received[typeof(T)] = new List<CapCore.Tuple<CapCore.IReturnObject, Guid>>();
                }
            }
            return ro;
        }

        public T readObject<T>(Guid g, int? milisecond)
        {
            DateTime startTime = DateTime.Now;

            CapCore.Tuple<CapCore.IReturnObject, Guid> ro = null;

            do
            {
                bool gtg = false;
                lock (_receivedLock)
                {
                    if (this._received.ContainsKey(typeof(T)) && this._received[typeof(T)].Count > 0)
                    {
                        gtg = true;
                    }
                }

                double? timeLeft = null;
                if (milisecond.HasValue)
                {
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    timeLeft = ((double)milisecond.Value) - duration.TotalMilliseconds;

                    if (timeLeft < 0) break;
                }

                if (!gtg)
                {

                    if (timeLeft.HasValue) gtg = (WaitHandle.WaitAny(new WaitHandle[] { this._receivedSignal, this._disposedSignal }, (int)timeLeft.Value) == 0);
                    else gtg = gtg = (WaitHandle.WaitAny(new WaitHandle[] { this._receivedSignal, this._disposedSignal }) == 0);
                }

                if (!gtg) break;

                lock (_receivedLock)
                {
                    if (!this._received.ContainsKey(typeof(T))) this._received.Add(typeof(T), new List<myQv.Core.Tuple<myQv.Core.IReturnObject, Guid>>());

                    ro =
                        this._received[typeof(T)].SingleOrDefault<CapCore.Tuple<CapCore.IReturnObject, Guid>>(
                            tuple => tuple.Second == g
                        );

                    if (ro != null) this._received[typeof(T)].Remove(ro);
                }
            } while (ro == null);

            if (ro == null) throw new PipeException("Something wrong with pipe. No response received (" + g.ToString() + ") ...");
            return ro.First.getObject<T>();
        }

        public void WriteObject<T>(T o, Guid? g)
        {
            this.WriteObject<T, object>(o, g, false, null);
        }

        public V WriteObject<T, V>(T o, Guid? g, bool rvExp, int? milisecond)
        {
            Guid tg = (g.HasValue) ? g.Value : Guid.NewGuid();
            lock (this._responsePipeLock)
            {
                if (!this._responsePipe.ContainsKey(tg)) this._responsePipe.Add(tg, this._iPipes[0]);
                this._responsePipe[tg].WriteObject<T>(o, tg);
            }

            if (!rvExp) return default(V);
            else return this.readObject<V>(tg, milisecond);
        }

        public virtual void Dispose()
        {
            foreach(PipeConnection<U> pipe in this._iPipes) pipe.Dispose();
            this._disposedSignal.Set();
        }
    }

    public class PipeClient : Pipe<NamedPipeClientStream>, IDisposable
    {
        private System.Timers.Timer Clock;

        public PipeClient(string pipeName)
            : base(new PipeClientFactory(), pipeName, PipeDirection.Out, 1)
        {
            this._iPipes[0].refreshEnded += new RefreshEndedEventHandler(PipeClient_refreshEnded);
            this._iPipes[0].checkDataEnded += new CheckDataEndedEventHandler(PipeClient_checkDataEnded);
            this._iPipes[0].sendDataEnded += new SendDataEndedEventHandler(PipeClient_sendDataEnded);

            this.Clock = new System.Timers.Timer();
            this.Clock.Interval = 100;

            this.Clock.Elapsed += new System.Timers.ElapsedEventHandler(Clock_Elapsed);

            this.Clock.Start();

            // this._iPipes[0].refresh();
        }

        void Clock_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CapCore.Logging.log("Checking work in pipe ...", CapCore.LogType.Information, 11);

            if (this._iPipes[0].isLazy())
            {
                if (!this._iPipes[0].refresh())
                {
                    CapCore.Logging.log("Impossible to refresh pipe. Server must be down ...", CapCore.LogType.Error, 5);
                    this.Dispose();
                    throw new PipeException("Impossible to refresh pipe. Server must be down ...");
                }

            }

            

            if (!this._iPipes[0].isReading()) this._iPipes[0].checkData();
            if (!this._iPipes[0].isWriting()) this._iPipes[0].sendData();
        }

        void PipeClient_refreshEnded(object sender, PipeConnectionEventArgs e)
        {
            // if(!this.Clock.Enabled) this.Clock.Start();
        }

        void PipeClient_checkDataEnded(object sender, PipeConnectionEventArgs e)
        {
            
        }

        void PipeClient_sendDataEnded(object sender, PipeConnectionEventArgs e)
        {
           
        }

        public override void Dispose()
        {
            this.Clock.Stop();
            base.Dispose();
            
        }

    }
    public class PipeServer : Pipe<NamedPipeServerStream>
    {
        private System.Timers.Timer Clock;

        public PipeServer(string pipeName, int pipeNum)
            : base(new PipeServerFactory(pipeNum), pipeName, PipeDirection.In, pipeNum)
        {
            for (int i = 0; i < this._iPipes.Length; i++)
            {
                this._iPipes[i].refreshEnded += new RefreshEndedEventHandler(PipeClient_refreshEnded);
                this._iPipes[i].checkDataEnded += new CheckDataEndedEventHandler(PipeClient_checkDataEnded);
                this._iPipes[i].sendDataEnded += new SendDataEndedEventHandler(PipeClient_sendDataEnded);
            }

            this.Clock = new System.Timers.Timer();
            this.Clock.Interval = 100;

            this.Clock.Elapsed += new System.Timers.ElapsedEventHandler(Clock_Elapsed);
            this.Clock.Start();
        }

        void Clock_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.refresh();
                this.checkData();
                this.sendData();

                CapCore.Logging.log("Server inner loop ...", myQv.Core.LogType.Information, 11);
         