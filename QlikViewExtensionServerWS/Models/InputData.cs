using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QlikViewExtensionServerWS.Models
{
    public class InputData
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime ModificationDate { get; set; }
        public string Bucket { get; set; }
        public string BucketCategory { get; set; }
        public Dictionary<string, List<string>> Context { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return "{" +
                    this.Id.ToString() + ", " +
                    ((this.UserName == null) ? "" : this.UserName.ToString() + ", ") +
                    ((this.ModificationDate == null) ? "" : this.ModificationDate.ToString() + ", ") +
                    ((this.Bucket == null) ? "" : this.Bucket.ToString() + ", ") +
                    ((this.BucketCategory == null) ? "" : this.BucketCategory.ToString() + ", ") +
                    ((this.Context == null) ? "" : "{" + string.Join(", ", this.Context.Select(x => x.Key + "=" + string.Join("|", x.Value))) + "}, ") +
                    ((this.Value == null) ? "" : this.Value.ToString()) +
                "}";
        }


    }
}