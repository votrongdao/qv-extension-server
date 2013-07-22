using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Mvc;
using System.Web.Http;
using System.IO;

using myCore = frqtlib.Core;

using QlikViewExtensionServerWS.Models;

namespace QlikViewExtensionServerWS.Controllers
{
    public class ExportController : ApiController
    {
        static readonly IExportRepository repository = new ExportRepository();

        static ExportController()
        {
            myCore.Logging.log("Starting ExportController ...", 0, myCore.LogType.Information);
        }


        // GET api/export
        public IEnumerable<string> GetExport()
        {
            throw new NotImplementedException();
        }

        // GET api/export/5
        public HttpResponseMessage GetExport(string fName)
        {
            throw new NotImplementedException();
        }

        // POST api/export
        public string PostExport(ExportData e)
        {
            myCore.Logging.log("ExportController PostExport({0}) ...", 4, myCore.LogType.Information, e.ToString());

            try
            {
                if (e.type == ExportType.PDF)
                    return repository.saveAsPDF(e.q, e.UserName, e.width, e.zoom);
                else if (e.type == ExportType.IMG)
                    return repository.saveAsImg(e.q, e.UserName, e.width);
                else
                    return null;
            }
            catch (System.Exception ex)
            {
                myCore.Logging.log("Uncaught error in ExportController ...", 0, ex, myCore.LogType.Error);
                throw ex;
            }
        }

        // PUT api/export/5
        public void PutExport(int id, [FromBody]string value)
        {
            throw new NotImplementedException();
        }

        // DELETE api/export/5
        public void DeleteExport(int id)
        {
            throw new NotImplementedException();
        }
    }
}
