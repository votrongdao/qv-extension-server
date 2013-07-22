using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QvConnectorInterface;
using QlikView.Qvx.QvxLibrary;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;
using System.Data;

using System.Xml;

namespace QvJSONConnector
{
    public class MyWebRequest
    {
        private WebRequest request;
        private Stream dataStream;

        private string status;

        public String Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }
        public HttpStatusCode StatusCode;

        public MyWebRequest(string url, string auth, string user, string password)
        {
            if (auth != "None") throw new Exception("Invalid Auth Type");

            // Create a request using a URL that can receive a post.
            request = WebRequest.Create(url);
        }

        public MyWebRequest(string url, string method, string auth, string user, string password)
            : this(url, auth, user, password)
        {

            if (method.Equals("GET") || method.Equals("POST"))
            {
                // Set the Method property of the request to POST.
                request.Method = method;
            }
            else
            {
                throw new Exception("Invalid Method Type");
            }
        }

        public MyWebRequest(string url, string method, string data, string auth, string user, string password)
            : this(url, method, auth, user, password)
        {

            // Create POST data and convert it to a byte array.
            string postData = data;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";

            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            // Get the request stream.
            dataStream = request.GetRequestStream();

            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);

            // Close the Stream object.
            dataStream.Close();

        }

        public string GetResponse()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "GetResponse()");

            // Get the original response.
            WebResponse response = null;

            try
            {
                this.Status = null;
                this.StatusCode = HttpStatusCode.NotFound;

                response = request.GetResponse();

                this.Status = ((HttpWebResponse)response).StatusDescription;
                this.StatusCode = ((HttpWebResponse)response).StatusCode;
            }
            catch (WebException e)
            {
                this.Status = ((HttpWebResponse)e.Response).StatusDescription;
                this.StatusCode = ((HttpWebResponse)e.Response).StatusCode;
            }

            if (this.StatusCode == HttpStatusCode.OK)
            {

                // Get the stream containing all content returned by the requested server.
                dataStream = response.GetResponseStream();

                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);

                // Read the content fully up to the end.
                string responseFromServer = reader.ReadToEnd();

                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();

                return responseFromServer;

            }

            return null;
        }

    }

    public class QvJSONConnector : IQvConnector
    {
        public string Name
        {
            get { return "JSON"; }
        }

        public bool WildcardSelectionAllowed { get { return true; } }

        private IQvConnectorHost _Host = null;
        public IQvConnectorHost Host
        {
            get
            {
                return this._Host;
            }
            set
            {
                this._Host = value;
            }
        }

        private MyWebRequest prepareRequest(string server, string auth, string username, string password, string param, out Dictionary<string, string> parameters)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "prepareRequest()");

            MyWebRequest q = null;

            Dictionary<string, string> myParameters = new Dictionary<string, string>();
            if (param != "None")
            {
                param
                    .Replace("\\§", "////////// REPLACE MARQUER PARAGRAPH \\\\\\\\\\")
                    .Replace("\\|", "////////// REPLACE MARQUER PIPE \\\\\\\\\\")
                    .Split('|').ToList().ForEach(delegate(string str)
                {
                    myParameters.Add(
                        str.Split('§')[0].Replace("////////// REPLACE MARQUER PARAGRAPH \\\\\\\\\\", "\\§").Replace("////////// REPLACE MARQUER PIPE \\\\\\\\\\", "\\|").Trim(),
                        str.Split('§')[1].Replace("////////// REPLACE MARQUER PARAGRAPH \\\\\\\\\\", "\\§").Replace("////////// REPLACE MARQUER PIPE \\\\\\\\\\", "\\|").Trim()
                    );
                });
            }

            parameters = myParameters;

            string proto = (parameters.ContainsKey("webProto") ? parameters["webProto"] + "://" : "http://");
            string page = (parameters.ContainsKey("webPage") ? parameters["webPage"] : "");

            if (parameters.ContainsKey("webPostParams"))
                q = new MyWebRequest(proto + server + page, "POST", parameters["webPostParams"], auth, username, password);
            else
                q = new MyWebRequest(proto + server + page + ((parameters.ContainsKey("webGetParams")) ? "?" + parameters["webGetParams"] : ""), "GET", auth, username, password);

            return q;
        }

        public bool Test(string server, string auth, string username, string password, string param)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Test()");
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("{0} {1} {2} {3} {4}", server, auth, username, password, param));

            Dictionary<string, string> parameters = null;
            MyWebRequest q = prepareRequest(server, auth, username, password, param, out parameters);

            q.GetResponse();

            return (q.StatusCode == HttpStatusCode.OK);
        }

        public delegate int MyDelegateType(string value);

        public List<QvxTable> Init(string server, string auth, string username, string password, string param, List<QvxTable> MTables, Func<string, IEnumerable<QvxTable>, QvxTable> FindTable)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Init()");

            Dictionary<string, string> parameters = null;
            MyWebRequest q = prepareRequest(server, auth, username, password, param, out parameters);

            string s = q.GetResponse();

            List<QvxTable> lt = new List<QvxTable>();
            if (q.StatusCode == HttpStatusCode.OK) {

                XmlDocument doc = JsonConvert.DeserializeXmlNode("{\"root\":" + s + "}", "root");
                string xml = doc.InnerXml;

                XmlToCsvUsingDataSetFromString converter = new XmlToCsvUsingDataSetFromString(xml, (parameters.ContainsKey("qualifySep")) ? parameters["qualifySep"] : null);
                XmlToCsvContext context = new XmlToCsvContext(converter);

                foreach (DataTable dt in converter.XmlDataSet.Tables)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Found table " + dt.TableName + ": " + dt.Rows.Count.ToString());

                    List<QvxField> l = new List<QvxField>();
                    foreach(DataColumn dc in dt.Columns) {
                        l.Add(new QvxField(dc.ColumnName, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
                    }

                    lt.Add(new QvxTable
                    {
                        TableName = dt.TableName,
                        GetRows = delegate() { return GetJSONRows(dt, MTables, FindTable); },
                        Fields = l.ToArray()
                    });
                }
            }

            return lt;
        }

        private IEnumerable<QvxDataRow> GetJSONRows(DataTable dt, List<QvxTable> MTables, Func<string, IEnumerable<QvxTable>, QvxTable> FindTable)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "GetJSONRows()");

            foreach (DataRow dr in dt.Rows)
            {
                yield return MakeEntry(dr, FindTable(dt.TableName, MTables));
            }
        }

        private QvxDataRow MakeEntry(DataRow dr, QvxTable table)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "MakeEntry()");

            var row = new QvxDataRow();

            foreach (DataColumn dc in dr.Table.Columns)
            {
                row[table[dc.ColumnName]] = dr[dc].ToString();
            }

            return row;
        }

        public string[] getAvailableAuthMethods()
        {
            return new string[] { "Basic", "Digest", "NTLM" };
        }
    }
}
