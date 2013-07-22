using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using System.IO;

namespace myQv.Core
{
    public class Config : XmlDocument
    {
        private Config() : base() { }
        public Config(string fileName) : base()
        {
            string folder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            this.Load(Path.Combine(folder, fileName));
        }

        public Config(string folder, string fileName)
            : base()
        {
            this.Load(Path.Combine(folder, fileName));
        }

        public string getElement(string e)
        {
            return this.DocumentElement.SelectSingleNode(e).InnerText;
        }

        public List<string> getElements(string e)
        {
            List<string> l = new List<string>();
            foreach (XmlNode n in this.DocumentElement.SelectNodes(e))
            {
                l.Add(n.InnerText);
            }

            return l;
        }

        public void setElement(string e, string v)
        {
            throw new NotImplementedException();
        }

    }
}
