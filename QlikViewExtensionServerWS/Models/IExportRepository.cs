using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QlikViewExtensionServerWS.Models
{
    public interface IExportRepository
    {
        string saveAsPDF(string txt, string nme, int wid, double zoom);
        string saveAsImg(string txt, string nme, int wid);
    }
}