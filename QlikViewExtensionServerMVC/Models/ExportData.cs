using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QlikViewExtensionServerWS.Models
{
    public enum ExportType { PDF, IMG }

    public class ExportData
    {
        public string UserName { get; set; }
        public ExportType type { get; set; }
        public int width { get; set; }
        public double zoom { get; set; }
        public int refresh { get; set; }
        public string q { get; set; }


        public override string ToString()
        {
            return "{" +
                    ((this.UserName == null) ? "" : this.UserName.ToString() + ", ") +
                    this.type.ToString() + ", " +
                    this.width.ToString() + ", " +
                    this.zoom.ToString() + ", " +
                    this.refresh.ToString() + ", " +
                    ((this.q == null) ? "" : this.q.ToString() + ", ") +
                "}";
        }


    }
}