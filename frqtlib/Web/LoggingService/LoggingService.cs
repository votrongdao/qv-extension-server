using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using frqtlib.Core;

namespace frqtlib.Web
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class LoggingService : ILoggingService
    {
        void ILoggingService.log(string msg, LogType l, params Object[] p)
        {
            Logging.log(msg, l, p);

            ILoggingServiceCallback callback = OperationContext.Current.GetCallbackChannel<ILoggingServiceCallback>();

            if (callback != null)
                callback.Logged();
        }

        void ILoggingService.log(string msg, int lvl, LogType l, params Object[] p)
        {
            Logging.log(msg, lvl, l, p);

            ILoggingServiceCallback callback = OperationContext.Current.GetCallbackChannel<ILoggingServiceCallback>();

            if (callback != null)
                callback.Logged();
        }

        void ILoggingService.log(string msg, int lvl, LoggingException e, LogType l, params Object[] p)
        {
            Logging.log(msg, lvl, e, l, p);

            ILoggingServiceCallback callback = OperationContext.Current.GetCallbackChannel<ILoggingServiceCallback>();

            if (callback != null)
                callback.Logged();
        }

    }
}
