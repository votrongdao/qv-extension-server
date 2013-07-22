﻿using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using myQv.QMSAPI;

using System.Xml;
using System.Xml.Linq;

namespace myQv.Qv
{
    public class QvSecu
    {
        public QvSecu() { }

        public static string[] getFolders(string srvURI)
        {
            QvClient apiClient = QvClientFactory.newQvClient(srvURI);
            List<string> rValue = new List<string>();

            foreach (ServiceInfo qvsService in apiClient.GetServices(ServiceTypes.QlikViewServer))
            {
                foreach (DocumentFolder f in apiClient.GetUserDocumentFolders(qvsService.ID, DocumentFolderScope.All))
                {
                    rValue.Add(f.General.Path);
                }
            }

            return rValue.ToArray();
        }

       
    }
}
