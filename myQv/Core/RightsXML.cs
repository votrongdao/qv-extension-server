﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using System.IO;

namespace myQv.Core
{
    public class UserDocumentAction
    {
        public enum TheAction { Add, Remove, New, Delete };

        public string srv;
        public string doc;
        public string usr;
        public TheAction act;

        public UserDocumentAction(string srv, string doc, string usr, TheAction act)
        {
            this.srv = srv;
            this.doc = doc;
            this.usr = usr;
            this.act = act;
        }

        public override String ToString()
        {
            return srv + ":" + doc + ":" + usr + ":" + ((act == TheAction.Add) ? "Add" : ((act == TheAction.Remove) ? "Remove" : ((act == TheAction.New) ? "New" : ((act == TheAction.Delete) ? "Delete" : "?"))));
        }
    }


    public class DocumentRights : List<string>
    {
        string docName = null;

        private DocumentRights() { }
        public DocumentRights(string docName)
        {
            this.docName = docName;
        }

        public void addUserRight(string userName)
        {
            if (!this.Contains(userName))
            {
                this.Add(userName);
            }
        }
    }

    public class ServerRights : Dictionary<string, DocumentRights>
    {
        string srvName = null;

        private ServerRights() { }
        public ServerRights(string srvName)
        {
            this.srvName = srvName;
        }

        public DocumentRights getDocumentRights(string docName)
        {
            if (!this.ContainsKey(docName))
                this.Add(docName, new DocumentRights(docName));

            return this[docName];
        }

        public void addUserRight(string docName, string userName)
        {
            this.getDocumentRights(docName).addUserRight(userName);
        }
    }

    public class PlatformRights : Dictionary<string, ServerRights>
    {
        public ServerRights getServerRights(string srvName)
        {
            if (!this.ContainsKey(srvName))
                this.Add(srvName, new ServerRights(srvName));

            return this[srvName];
        }

        public DocumentRights getDocumentRights(string srvName, string docName)
        {
            return this.getServerRights(srvName).getDocumentRights(docName);
        }

        public void addUserRight(string srvName, string docName, string userName)
        {
            this.getServerRights(srvName).getDocumentRights(docName).addUserRight(userName);
        }
    }



    public class RightsXML
    {

        public static PlatformRights loadRights(Config config)
        {
            string lf = config.getElement("/config/Bkp/Folder");

            XmlDocument mxd = null;

            foreach (FileInfo f in new DirectoryInfo(lf).GetFiles())
            {
                if (f.Extension == ".xml")
                {
                    XmlDocument xd = new XmlDocument();
                    xd.Load(f.FullName);

                    DateTime t = DateTime.FromBinary(Convert.ToInt64(xd.SelectSingleNode("bkp/timestamp").Attributes["value"].Value));
                    if (mxd == null || DateTime.FromBinary(Convert.ToInt64(mxd.SelectSingleNode("bkp/timestamp").Attributes["value"].Value)) < t)
                        mxd = xd;
                }
            }

            PlatformRights dic = new PlatformRights();

            if (mxd != null)
                foreach (XmlNode n1 in mxd.SelectNodes("bkp/server"))
                    foreach (XmlNode n2 in n1.ChildNodes)
                        foreach (XmlNode n3 in n2.ChildNodes)
                            dic.addUserRight(n1.Attributes["name"].Value, n2.Attributes["name"].Value, n3.Attributes["name"].Value);

            return dic;
        }

        public static string changeRightXML(Config config, string env, string doc, string user, AccessRightAction action)
        {
            string lf = config.getElement("/config/Bkp/Folder");

            XmlDocument mxd = null;
            string fileName = null;

            foreach (FileInfo f in new DirectoryInfo(lf).GetFiles())
            {
                if (f.Extension == ".xml")
                {
                    XmlDocument xd = new XmlDocument();
                    xd.Load(f.FullName);

                    DateTime t = DateTime.FromBinary(Convert.ToInt64(xd.SelectSingleNode("bkp/timestamp").Attributes["value"].Value));
                    if (mxd == null || DateTime.FromBinary(Convert.ToInt64(mxd.SelectSingleNode("bkp/timestamp").Attributes["value"].Value)) < t)
                    {
                        mxd = xd;
                        fileName = f.FullName;
                    }
                }
            }

            if (mxd != null)
            {
                if (action == AccessRightAction.DEL)
                {
                    XmlNode NodeToRemove = mxd.SelectSingleNode("//bkp/server[@name='" + env + "']/document[@name='" + doc + "']/user[@name='" + user+ "']");
                    if (NodeToRemove != null)
                    {
                        NodeToRemove.ParentNode.RemoveChild(NodeToRemove);
                        mxd.Save(fileName);
                        return env + " " + doc + " " + user + " succesfully removed from XML";
                    }
                    else
                        return "fail to remove " + env + " " + doc + " " + user + " from XML";
                }
                else if (action == AccessRightAction.ADD)
                {
                    XmlNode NodeToAdd = mxd.SelectSingleNode("//bkp/server[@name='" + env + "']/document[@name='" + doc + "']");
                    XmlNode AddedNode = NodeToAdd.AppendChild(mxd.CreateElement("user"));
                    AddedNode.Attributes.Append(mxd.CreateAttribute("name"));
                    AddedNode.Attributes["name"].Value = user;
                    mxd.Save(fileName);
                    return env + " " + doc + " " + user + " succesfully added in XML";
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
                return "Problem accessing XML";

        }

        public static List<UserDocumentAction> compareRights(PlatformRights dic1, PlatformRights dic2)
        {
            List<UserDocumentAction> ret = new List<UserDocumentAction>();
            foreach (KeyValuePair<string, ServerRights> k1 in dic1)
                if (dic2.ContainsKey(k1.Key))
                {
                    foreach (KeyValuePair<string, DocumentRights> k2 in k1.Value)
                        if (dic2[k1.Key].ContainsKey(k2.Key))
                        {
                            foreach (string k3 in k2.Value)
                                if (!dic2[k1.Key][k2.Key].Contains(k3))
                                    ret.Add(new UserDocumentAction(k1.Key, k2.Key, k3, UserDocumentAction.TheAction.Add));

                            foreach (string k4 in dic2[k1.Key][k2.Key])
                                if (!k2.Value.Contains(k4))
                                    ret.Add(new UserDocumentAction(k1.Key, k2.Key, k4, UserDocumentAction.TheAction.Remove));

                        }
                        else
                        {
                            foreach (string k3 in k2.Value)
                                ret.Add(new UserDocumentAction(k1.Key, k2.Key, k3, UserDocumentAction.TheAction.New));
                        }

                    foreach (KeyValuePair<string, DocumentRights> k2 in dic2[k1.Key])
                        if (!k1.Value.ContainsKey(k2.Key))
                        {
                            foreach (string k4 in dic2[k1.Key][k2.Key])
                                ret.Add(new UserDocumentAction(k1.Key, k2.Key, k4, UserDocumentAction.TheAction.Delete));
                        }

                }
                else
                {
                    foreach (KeyValuePair<string, DocumentRights> k5 in k1.Value)
                        foreach (string k6 in k5.Value)
                            ret.Add(new UserDocumentAction(k1.Key, k5.Key, k6, UserDocumentAction.TheAction.New));
                }

            return ret;
        }

        public static void saveRights(Config config, PlatformRights dic)
        {
            RightsXML.saveRights(config, dic, null);
        }

        public static void saveRights(Config config, PlatformRights dic, String suf)
        {
            String lf = config.getElement("/config/Bkp/Folder");
            String fn = DateTime.Now.ToString(config.getElement("/config/Bkp/Mask")) + "Rights" + ((suf != null) ? "(" + suf + ")" : "") + ".xml";

            using (StreamWriter sx = new StreamWriter(new FileInfo(Path.Combine(lf, fn)).Create()))
            {

                XmlDocument xd = new XmlDocument();

                XmlNode n1 = xd.AppendChild(xd.CreateElement("bkp"));

                n1.AppendChild(xd.CreateElement("timestamp")).Attributes.Append(xd.CreateAttribute("value"));
                n1.ChildNodes[0].Attributes["value"].Value = DateTime.Now.ToBinary().ToString();


                foreach (KeyValuePair<String, ServerRights> k1 in dic)
                {
                    XmlNode n2 = n1.AppendChild(xd.CreateElement("server"));
                    n2.Attributes.Append(xd.CreateAttribute("name"));
                    n2.Attributes["name"].Value = k1.Key;

                    foreach (KeyValuePair<String, DocumentRights> k2 in k1.Value)
                    {
                        XmlNode n3 = n2.AppendChild(xd.CreateElement("document"));
                        n3.Attributes.Append(xd.CreateAttribute("name"));
                        n3.Attributes["name"].Value = k2.Key;

                        foreach (string k3 in k2.Value)
                        {
                            XmlNode n4 = n3.AppendChild(xd.CreateElement("user"));
                            n4.Attributes.Append(xd.CreateAttribute("name"));
                            n4.Attributes["name"].Value = k3;
                        }
                    }
                }

                sx.Write(xd.InnerXml);
                sx.Flush();

            }
        }

        public static String RemoveDocumentXML(Config config, String Server_URI, String Rel_Path)
        {
            try
            {
                String lf = config.getElement("/config/Bkp/Folder");

                XmlDocument mxd = null;
                String fileName = null;

                foreach (FileInfo f in new DirectoryInfo(lf).GetFiles())
                {
                    if (f.Extension == ".xml")
                    {
                        XmlDocument xd = new XmlDocument();
                        xd.Load(f.FullName);

                        DateTime t = DateTime.FromBinary(Convert.ToInt64(xd.SelectSingleNode("bkp/timestamp").Attributes["value"].Value));
                        if (mxd == null || DateTime.FromBinary(Convert.ToInt64(mxd.SelectSingleNode("bkp/timestamp").Attributes["value"].Value)) < t)
                        {
                            mxd = xd;
                            fileName = f.FullName;
                        }
                    }
                }

                if (mxd != null)
                {
                    XmlNode NodeToRemove = mxd.SelectSingleNode("//bkp/server[@name='" + Server_URI + "']/document[@name='" + Rel_Path + "']");
                    if (NodeToRemove != null)
                    {
                        NodeToRemove.ParentNode.RemoveChild(NodeToRemove);
                        mxd.Save(fileName);
                        return Server_URI + " " + Rel_Path + " succesfully removed from XML";
                    }
                    else
                        return "fail to remove document " + Server_URI + " " + Rel_Path + " from XML";
                }
                else
                    return "Problem accessing XML";
            }
            catch (Exception exp)
            {
                return "Problem while removing document in XML. Error message: " + exp.Message + " Technical Message : " + exp.StackTrace;
            }
        }

        public static String AddDocumentXML(Config config, String Server_URI, String Rel_Path)
        {
            try
            {
                String lf = config.getElement("/config/Bkp/Folder");

                XmlDocument mxd = null;
                String fileName = null;

                foreach (FileInfo f in new DirectoryInfo(lf).GetFiles())
                {
                    if (f.Extension == ".xml")
                    {
                        XmlDocument xd = new XmlDocument();
                        xd.Load(f.FullName);

                        DateTime t = DateTime.FromBinary(Convert.ToInt64(xd.SelectSingleNode("bkp/timestamp").Attributes["value"].Value));
                        if (mxd == null || DateTime.FromBinary(Convert.ToInt64(mxd.SelectSingleNode("bkp/timestamp").Attributes["value"].Value)) < t)
                        {
                            mxd = xd;
                            fileName = f.FullName;
                        }
                    }
                }

                if (mxd != null)
                {

                    XmlNode NodeToAdd = mxd.SelectSingleNode("//bkp/server[@name='" + Server_URI + "']");//document[@name='" + doc + "']");
                    XmlNode AddedNode = NodeToAdd.AppendChild(mxd.CreateElement("document"));
                    AddedNode.Attributes.Append(mxd.CreateAttribute("name"));
                    AddedNode.Attributes["name"].Value = Rel_Path;
                    mxd.Save(fileName);
                    return Server_URI + " " + Rel_Path + " succesfully added in XML";
                }
                else
                    return "Problem accessing XML";
            }
            catch (Exception exp)
            {
                return "Problem while adding document in XML. Error message: " + exp.Message + " Technical Message : " + exp.StackTrace;
            }
        }

    }
}