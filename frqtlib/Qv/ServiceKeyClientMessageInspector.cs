using System;
using System.IO;
using System.Net;
using System.Text;

using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

using System.Xml;

using myCore = frqtlib.Core;

namespace frqtlib.Qv
{
    public class ServiceKeyClientMessageInspector : IClientMessageInspector
    {
        private const string SERVICE_KEY_HTTP_HEADER = "X-Service-Key";

        public string ServiceKey { get; set; }

        private object _queriesLock = new object();
        public Dictionary<Guid, Tuple<Message, IClientChannel>> queries = new Dictionary<Guid, Tuple<Message, IClientChannel>>();

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            object httpRequestMessageObject;
            string request2 = request.ToString();

            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                HttpRequestMessageProperty httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (httpRequestMessage != null)
                {
                    httpRequestMessage.Headers[SERVICE_KEY_HTTP_HEADER] = (this.ServiceKey ?? string.Empty);
                }
                else
                {
                    httpRequestMessage = new HttpRequestMessageProperty();
                    httpRequestMessage.Headers.Add(SERVICE_KEY_HTTP_HEADER, (this.ServiceKey ?? string.Empty));
                    request.Properties[HttpRequestMessageProperty.Name] = httpRequestMessage;
                }
            }
            else
            {
                HttpRequestMessageProperty httpRequestMessage = new HttpRequestMessageProperty();
                httpRequestMessage.Headers.Add(SERVICE_KEY_HTTP_HEADER, (this.ServiceKey ?? string.Empty));
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }

            string request3 = request.ToString();

            Guid g = Guid.Empty; ;

            lock (this._queriesLock)
            {
                g = Guid.NewGuid();
                this.queries.Add(g, Tuple.Create(request, channel));
            }

            return g;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) {
            Message request = null;
            IClientChannel channel = null;

            lock (this._queriesLock)
            {
                request = this.queries[(Guid)correlationState].Item1;
                channel = this.queries[(Guid)correlationState].Item2;
            }

            XmlDocument replydoc = new XmlDocument();
            replydoc.LoadXml(reply.ToString());

            XmlNamespaceManager replyNsmgr = new XmlNamespaceManager(replydoc.NameTable);
            replyNsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");

            XmlElement replyRoot = replydoc.DocumentElement;
            XmlNode replyFault = replyRoot.SelectSingleNode("//s:Fault", replyNsmgr);

            if (replyFault != null)
            {
                if (replyFault.SelectSingleNode("//faultcode", replyNsmgr).InnerXml == "s:Client" && replyFault.SelectSingleNode("//faultstring", replyNsmgr).InnerXml.Contains("Service key"))
                {
                    QMSAPI.QMSClient client = frqtlib.Qv.QMSClientFactory.getClient(channel.RemoteAddress.Uri);
                    this.ServiceKey = client.GetTimeLimitedServiceKey();

                    WebRequest retryRequest = WebRequest.Create(channel.RemoteAddress.Uri);
                    retryRequest.Credentials = CredentialCache.DefaultCredentials;


                    XmlDocument retryRequestDoc = new XmlDocument();
                    retryRequestDoc.LoadXml(request.ToString());

                    XmlNamespaceManager retryRequestNsmgr = new XmlNamespaceManager(retryRequestDoc.NameTable);
                    retryRequestNsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");

                    XmlElement retryRequestRoot = retryRequestDoc.DocumentElement;
                    XmlNode retryRequestBody = retryRequestRoot.SelectSingleNode("//s:Body", retryRequestNsmgr);

                    retryRequest.Headers.Add("SOAPAction", "\"http://ws.qliktech.com/QMS/11/IQMS/" + retryRequestBody.ChildNodes[0].Name + "\"");
                    retryRequest.Headers.Add("Accept-Encoding", "gzip, deflate,gzip, deflate,gzip, deflate");
                    retryRequest.Headers.Add(SERVICE_KEY_HTTP_HEADER, this.ServiceKey);

                    retryRequest.Method = "POST";
                    string postData = request.ToString();
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    retryRequest.ContentType = "text/xml; charset=utf-8";
                    retryRequest.ContentLength = byteArray.Length;
                    Stream dataStream = retryRequest.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();

                    WebResponse retryReply = retryRequest.GetResponse();
                    string retryReplyStr = new StreamReader(retryReply.GetResponseStream()).ReadToEnd();

                    XmlDocument retryReplyDoc = new XmlDocument();
                    retryReplyDoc.LoadXml(retryReplyStr);

                    XmlNamespaceManager retryReplyNsmgr = new XmlNamespaceManager(retryReplyDoc.NameTable);
                    retryReplyNsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");

                    XmlElement retryReplyRoot = retryReplyDoc.DocumentElement;
                    XmlNode retryReplyBody = retryReplyRoot.SelectSingleNode("//s:Body", retryReplyNsmgr);

                    XmlTextReader reader = new XmlTextReader(new StringReader(retryReplyBody.InnerXml));
                    Message replacedMessage = Message.CreateMessage(reply.Version, "http://ws.qliktech.com/QMS/11/IQMS/" + retryReplyBody.ChildNodes[0].Name, reader);

                    replacedMessage.Headers.CopyHeadersFrom(reply.Headers);
                    replacedMessage.Properties.CopyProperties(reply.Properties);
                    reply = replacedMessage;
                }
            }

            lock (this._queriesLock)
            {
                this.queries.Remove((Guid)correlationState);
            }
            
        }
    }
}