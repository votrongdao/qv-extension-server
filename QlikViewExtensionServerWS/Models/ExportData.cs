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
        public DateTime ModificationDate { get; set; }
        public ExportType type { get; set; }
        public int width { get; set; }
        public double zoom { get; set; }
        public int refresh { get; set; }
        public string q { get; set; }


        public override string ToString()
        {
            return "{" +
                    ((this.UserName == null) ? "" : this.UserName.ToString() + ", ") +
                    ((this.ModificationDate == null) ? "" : this.ModificationDate.ToString() + ", ") +
                    ((this.type == null) ? "" : this.type.ToString() + ", ") +
                    ((this.width == null) ? "" : this.width.ToString() + ", ") +
                    ((this.zoom == null) ? "" : this.zoom.ToString() + ", ") +
                    ((this.refresh == null) ? "" : this.refresh.ToString() + ", ") +
//                    ((this.q == null) ? "" : this.q.ToString() + ", ") +
                "}";
        }


    }
}