using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using frqtlib.QMSAPI;
using frqtlib.Core;

namespace frqtlib.Qv
{
    // public class DocumentCAL : Tuple<Guid, Guid, string, DateTime> { }

    public class QMSClientEnhanced : QMSClient
    {
        public QMSClientEnhanced()
            : base()
        {
            this.Endpoint.Behaviors.Add(new ServiceKeyEndpointBehavior());
        }

        public QMSClientEnhanced(Uri qms)
            : base("BasicHttpBinding_IQMS", new System.ServiceModel.EndpointAddress(qms))
        {
            this.Endpoint.Behaviors.Add(new ServiceKeyEndpointBehavior());
        }

       /* public List<DocumentCAL> getDocCals()
        {
            List<DocumentCAL> r = new List<DocumentCAL>();
            foreach (QVSMount mountedFolder in this.GetQVSSettings(this.GetServices(QMSAPI.ServiceTypes.QlikViewServer)[0].ID, QMSAPI.QVSSettingsScope.Folders).Folders.UserDocumentMounts)
                foreach (DocumentNode dn in this.GetUserDocuments(this.GetServices(QMSAPI.ServiceTypes.QlikViewServer)[0].ID).FindAll(g => (g.FolderID == mountedFolder.FolderID)))
                    foreach (AssignedNamedCAL anc in this.GetDocumentMetaData(dn, QMSAPI.DocumentMetaDataScope.Licensing).Licensing.AssignedCALs)
                    {
                        r.Add((DocumentCAL) Tuple.New(mountedFolder.FolderID, dn.ID, anc.UserName, anc.LastUsed));
                    }

            return r;
        }*/
    }
}
