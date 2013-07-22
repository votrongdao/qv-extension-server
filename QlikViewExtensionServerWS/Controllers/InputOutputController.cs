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
            myCore.ConsoleLogging lHost = new myCore.ConsoleLogging();
            lHost.init(10, 10);

            myCore.Logging.addLogging(lHost);
        }

        public IEnumerable<InputData> GetInputOutput(int? id = null, string userName = null, string bucket = null, string bucketCategory = null)
        {
            myCore.Logging.log("InputOutputController GetInputOutput(string userName, string bucket, string bucketCategory) ...", myCore.LogType.Information, 0);

            IEnumerable<InputData> item = repository.Get(id: id, userName: userName, bucket: bucket, bucketCategory: bucketCategory);
            if (item == null || item.Count() == 0)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return item;
        }

        public HttpResponseMessage PostInputOutput(InputData item)
        {
            myCore.Logging.log("InputOutputController PostInputOutput() ... " + item.ToString(), myCore.LogType.Information, 0);

            item = repository.Add(item);
            var response = Request.CreateResponse<InputData>(HttpStatusCode.Created, item);

            string uri = Url.Link("DefaultApi", new { id = item.Id });
            response.Headers.Location = new Uri(uri);
            return response;
        }

        public void PutInputOutput(int id, InputData item)
        {
            myCore.Logging.log("InputOutputController PutInputOutput() ... " + id + " " + item.ToString(), myCore.LogType.Information, 0);

            item.Id = id;
            if (!repository.Update(item))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        public void DeleteInputOutput(int id)
        {
            myCore.Logging.log("InputOutputController DeleteInputOutput() ... " + id, myCore.LogType.Information, 0);

            IEnumerable<InputData> item = repository.Get(id: id);
            if (item == null || item.Count() != 1)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            repository.Remove(id);
        }
    }
}
