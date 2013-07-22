using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using QlikViewExtensionServerWS.Models;

using myCore = frqtlib.Core;

namespace QlikViewExtensionServerWS.Controllers
{
    public class InputOutputController : ApiController
    {
        static readonly IInputDataRepository repository = new InputDataRepository();

        static InputOutputController()
        {
            myCore.Logging.log("Starting InputOutputController ...", 0, myCore.LogType.Information);
        }

        public IEnumerable<InputData> GetInputOutput(int? id = null, string userName = null, string bucket = null, string bucketCategory = null)
        {
            myCore.Logging.log(@"GET InputOutputController GetInputOutput({0}, {1}, {2}, {3}) ...", 4, myCore.LogType.Information, id, userName, bucket, bucketCategory);

            IEnumerable<InputData> item = repository.Get(id: id, userName: userName, bucket: bucket, bucketCategory: bucketCategory);
            if (item == null || item.Count() == 0)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return item;
        }

        public HttpResponseMessage PostInputOutput(InputData item)
        {
            myCore.Logging.log("POST InputOutputController PostInputOutput({0}) ... ", 4, myCore.LogType.Information, item);

            item = repository.Add(item);
            var response = Request.CreateResponse<InputData>(HttpStatusCode.Created, item);

            string uri = Url.Link("DefaultApi", new { id = item.Id });
            response.Headers.Location = new Uri(uri);
            return response;
        }

        public void PutInputOutput(int id, InputData item)
        {
            myCore.Logging.log("PUT InputOutputController PutInputOutput({0}, {1}) ...", 4, myCore.LogType.Information, id, item);
            item.Id = id;
            if (!repository.Update(item))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        public void DeleteInputOutput(int id)
        {
            myCore.Logging.log("DELETE InputOutputController DeleteInputOutput({0}) ... ", 4, myCore.LogType.Information, id);

            IEnumerable<InputData> item = repository.Get(id: id);
            if (item == null || item.Count() != 1)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            repository.Remove(id);
        }
    }
}
