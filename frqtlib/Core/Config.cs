using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using System.IO;

namespace frqtlib.Core
{
    public static class Config
    {
        private static List<ConfigElement> _l = new List<ConfigElement>();

        public static void addConfig(ConfigElement l)
        {
            Config._l.Add(l);
        }

        public static string getElement(string e)
        {
            foreach (ConfigElement il in Config._l)
                if (il.getElement(e) != null)
                    return il.getElement(e);

            throw new ArgumentOutOfRangeException("Element not found in registered configurations ...");
        }

        public static List<string> getElements(string e)
        {
            List<string> l = new List<string>();

            foreach (ConfigElement il in Config._l)
                if (il.getElement(e) != null)
                    l.AddRange(il.getElements(e));

            return l;
        }
    }

    public class ConfigElement : XmlDocument
    {
        private ConfigElement() : base() { }
        public ConfigElement(string fileName) : base()
        {
            string folder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            this.Load(Path.Combine(folder, fileName));
        }

        public ConfigElement(string folder, string fileName)
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
