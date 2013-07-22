using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using myQv.QMSAPI;

using System.Xml;
using System.Xml.Linq;

namespace myQv.Qv
{
    public class QvClient : QMSBackendClient
    {
        class ServiceKeyBehaviorExtensionElement : BehaviorExtensionElement
        {
            public override Type BehaviorType
            {
                get { return typeof(ServiceKeyEndpointBehavior); }
            }

            protected override object CreateBehavior()
            {
                return new ServiceKeyEndpointBehavior();
            }
        }

        class ServiceKeyEndpointBehavior : IEndpointBehavior
        {
            public void Validate(ServiceEndpoint endpoint) { }

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
                clientRuntime.MessageInspectors.Add(new ServiceKeyClientMessageInspector());
            }
        }

        class ServiceKeyClientMessageInspector : IClientMessageInspector
        {
            private const string SERVICE_KEY_HTTP_HEADER = "X-Service-Key";

            public object BeforeSendRequest(ref Message request, IClientChannel channel)
            {
                object httpRequestMessageObject;
                string ServiceKey = null;

                XmlDocument rx = new XmlDocument();
                rx.LoadXml(request.ToString());

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(rx.NameTable);
                nsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");

                string body = rx.SelectSingleNode("/s:Envelope/s:Body", nsmgr).FirstChild.Name;
                if (body != "GetTimeLimitedServiceKey")
                {
                    ServiceKey = ((IQMSBackend)channel).GetTimeLimitedServiceKey();
                }


                if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
                {
                    HttpRequestMessageProperty httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                    if (httpRequestMessage != null)
                    {
                        httpRequestMessage.Headers[SERVICE_KEY_HTTP_HEADER] = (ServiceKey ?? string.Empty);
                    }
                    else
                    {
                        httpRequestMessage = new HttpRequestMessageProperty();
                        httpRequestMessage.Headers.Add(SERVICE_KEY_HTTP_HEADER, (ServiceKey ?? string.Empty));
                        request.Properties[HttpRequestMessageProperty.Name] = httpRequestMessage;
                    }
                }
                else
                {
                    HttpRequestMessageProperty httpRequestMessage = new HttpRequestMessageProperty();
                    httpRequestMessage.Headers.Add(SERVICE_KEY_HTTP_HEADER, (ServiceKey ?? string.Empty));
                    request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
                }
                return null;
            }

            public void AfterReceiveReply(ref Message reply, object correlationState)
            {
            }
        }

        private QvClient() : base() { }
        private QvClient(BasicHttpBinding binding, EndpointAddress address) : base(binding, address) { }

        public static QvClient getClient(string srvURI)
        {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;

            binding.MaxReceivedMessageSize = 10000000;

            EndpointAddress address = new EndpointAddress(srvURI);
            
            QvClient client = new QvClient(binding, address);
            client.Endpoint.Behaviors.Add(new ServiceKeyEndpointBehavior());

            return client;
        }

    }

    public class QvClientFactory
    {
        private static Dictionary<string, QvClient> cList = new Dictionary<string, QvClient>();

        private QvClientFactory() { }

        public static QvClient newQvClient(string srvURI)
        {
            QvClient rValue = null;

            if (cList.ContainsKey(srvURI))
            {
                rValue = cList[srvURI];
                try
                {
                    rValue.Ping();
                    return rValue;
                }
                catch { }
            }

            rValue = QvClient.getClient(srvURI);
            cList.Add(srvURI, rValue);
            return rValue;
        }

    }
}
