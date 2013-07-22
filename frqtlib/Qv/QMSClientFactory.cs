using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace frqtlib.Qv
{
    public static class QMSClientFactory
    {
        private static Uri defaultUri = new Uri("http://localhost");
        private static Dictionary<Uri, QMSClientEnhanced> clientList = new Dictionary<Uri, QMSClientEnhanced>();

        public static QMSClientEnhanced getClient(Uri qms = null)
        {
            if (qms == null) qms = QMSClientFactory.defaultUri;
            
            if (QMSClientFactory.clientList.ContainsKey(qms) && QMSClientFactory.clientList[qms].State == System.ServiceModel.CommunicationState.Opened)
            {
                return QMSClientFactory.clientList[qms];
            }
            else
            {
                QMSClientEnhanced client = ((qms == QMSClientFactory.defaultUri) ? new QMSClientEnhanced() : new QMSClientEnhanced(qms));

                if (QMSClientFactory.clientList.ContainsKey(qms)) QMSClientFactory.clientList.Remove(qms);
                QMSClientFactory.clientList.Add(qms, client);

                return client;
            }

            
        }
    }
}
