using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using frqtlib.Core;

namespace frqtlib.Web
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ILoggingServiceCallback))]
    public interface ILoggingService
    {
        [OperationContract(Name = "log")]
        void log(string msg, LogType l, params Object[] p);

        [OperationContract(Name = "logLvl")]
        void log(string msg, int lvl, LogType l, params Object[] p);

        [OperationContract(Name = "logLvlE")]
        void log(string msg, int lvl, LoggingException e, LogType l, params Object[] p);
    }
}
