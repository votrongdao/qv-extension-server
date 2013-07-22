using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace frqtlib.Web
{
    public interface ILoggingServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void Logged();
    }
}
