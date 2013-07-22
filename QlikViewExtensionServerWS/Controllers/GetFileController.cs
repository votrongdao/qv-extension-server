using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.IO;

namespace QlikViewExtensionServerWS.Controllers
{
    public class GetFileController : Controller
    {
        //
        // GET: /GetFile/

        public ActionResult Index(string fName)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes("Web/cpcb/" + fName);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fName);
        }

    }
}
