using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QvJSONConnector;
using System.IO;

using Moor.XmlConversionLibrary.XmlToCsvStrategy;

namespace QvJSONConnectorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            QvJSONConnector.QvJSONConnector json = new QvJSONConnector.QvJSONConnector();

            Console.WriteLine(json.Test("api.jcdecaux.com", "None", null, null, "webProto§https|webPage§/vls/v1/contracts/|webGetParams§apiKey=e8ae8d6e9822751ce906bb5aeff0d8488eaeac40"));
            //Console.WriteLine(json.Test("http://www.pouc.fr:8083/api/2290c5826b7d4c8eb393b678a19b1748/movie.list/", "None", null, null, "qualifySep§_|webGetParams§limit_offset=3"));
            //List<QlikView.Qvx.QvxLibrary.QvxTable> t = json.Init("http://www.pouc.fr:8083/api/2290c5826b7d4c8eb393b678a19b1748/movie.list/", "None", null, null, "None", null, null);

            Console.ReadLine();
        }
    }
}
