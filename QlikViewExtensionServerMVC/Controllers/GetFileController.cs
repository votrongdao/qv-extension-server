using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Reflection;
using System.IO;

using myCore = frqtlib.Core;

namespace QlikViewExtensionServerWS.Controllers
{
    public class GetFileController : Controller
    {
        static GetFileController()
        {
            myCore.Logging.log("Starting GetFileController ...", 0, myCore.LogType.Information);
        }

        public ActionResult Index(string fName)
        {
            myCore.Logging.log("GetFileController Index({0}) ...", 4, myCore.LogType.Information, fName);

            try
            {
                string path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
                byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(path, "cpcb", fName));

                myCore.Logging.log("Transmitting file : {0}", 3, myCore.LogType.Information, Path.Combine(path, "cpcb", fName));

                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fName);
            }
            catch (System.Exception e)
            {
                myCore.Logging.log("Uncaught error in GetFileController ...", 0, e, myCore.LogType.Error);
                throw e;
            }
        }

    }
}
