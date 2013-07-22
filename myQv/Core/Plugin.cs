﻿using System;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;

using myQv.Threading;
using myQv.Schedule;

namespace myQv.Core
{

    [AttributeUsage(AttributeTargets.Class |
        AttributeTargets.Constructor |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.Property)]
    public class PluginMethod : System.Attribute
    {
        public PluginMethod() { }
    }


    

    public class PluginWorkItem : WorkItem
    {
        private IPlugin _plugin = null;
        private WorkParameters _params = null;
        private IReturnObject _returnValue = null;

        public IReturnObject ReturnValue
        {
            get
            {
                return _returnValue;
            }
            set
            {
                _returnValue = value;
            }
        }

        public WorkParameters Parameters
        {
            get
            {
                return _params;
            }
            set
            {
                _params = value;
            }
        }

        public PluginWorkItem(IPlugin pl, WorkParameters pa)
        {
            this._plugin = pl;
            this._params = pa;
        }

        public override void Perform()
        {
            this._plugin.Perform(this);
        }
    }

    public abstract class IPlugin
    {
        private static IPluginHost _host;
        public static string _name;
        public static WorkParametersDesc _d = null;

        public string getName()
        {
            return IPlugin._name;
        }

        public bool Init(IPluginHost host)
        {
            if (IPlugin._name == null) throw new NotImplementedException("Name not set for this plugin ...");

            IPlugin._d = new WorkParametersDesc();
            foreach (MethodInfo mi in this.GetType().GetMethods())
            {
                if (typeof(IReturnObject).IsAssignableFrom(mi.ReturnType))
                {
                    WorkMethodParameterDesc pfpd = new WorkMethodParameterDesc();
                    IPlugin._d.Add(mi.Name, pfpd);

                    foreach (ParameterInfo pi in mi.GetParameters())
                    {
                        pfpd.Add(pi.Name, pi.ParameterType);
                    }
                }

            }

            IPlugin._host = host;
            IPlugin._host.Register(IPlugin._name, this);

            return true; ;
        }

        public void Perform(PluginWorkItem pwi)
        {
            try
            {

                object[] op = new object[this.GetType().GetMethod(pwi.Parameters._f).GetParameters().Length];
                foreach (ParameterInfo pi in this.GetType().GetMethod(pwi.Parameters._f).GetParameters())
                    op[pi.Position] = pwi.Parameters[pi.Name];

                pwi.ReturnValue = (IReturnObject)this.GetType().GetMethod(pwi.Parameters._f).Invoke(this, op);
            }
            catch (Exception e)
            {
                pwi.ReturnValue = new returnObject<int>(-1, -1, "Error : Unhandled plugin exception ..." + Environment.NewLine + e.Message, e.StackTrace);
                throw e;
            }
        }
    }

    public interface IPluginHost
    {
        bool Register(string name, IPlugin ip);
        void RegisterPlugins();
        PluginWorkItem getWorkItem(string pName, WorkParameters p);
        List<SchAction> getActions();
    }

    public class PluginHost : IPluginHost
    {
        private Dictionary<string, IPlugin> ipi = new Dictionary<string,IPlugin>();

        public bool Register(string name, IPlugin ip)
        {
            try
            {
                ipi[name] = ip;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void RegisterPlugins()
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\plugins";
            string[] pluginFiles = Directory.GetFiles(path, "*.DLL");

            Logging.log("Starting plugin registration on " + path + " ... ", LogType.Information, 1);

            for (int i = 0; i < pluginFiles.Length; i++)
            {
                try
                {
                    Logging.log("Browsing : " + pluginFiles[i], LogType.Information, 4);
                    Assembly a = Assembly.LoadFrom(pluginFiles[i]);

                    if (a != null)
                    {
                        Logging.log("Assembly loaded !", LogType.Information, 7);
                        foreach (Type t in a.GetTypes())
                        {
                            Logging.log("Checking type : " + t.FullName, LogType.Information, 7);
                            if (t.IsSubclassOf(typeof(IPlugin)))
                            {
                                IPlugin ip = (IPlugin)Activator.CreateInstance(t);
                                ip.Init(this);
                                Logging.log("Plugin loaded : " + ip.getName(), LogType.Information, 4);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.log("Error : " + ex.Message, LogType.Error, 0);
                }
            }
        }

        public PluginWorkItem getWorkItem(string pName, WorkParameters p)
        {
            return new PluginWorkItem(ipi[pName], p);
        }

        public List<SchAction> getActions()
        {
            List<SchAction> l = new List<SchAction>();
            foreach (IPlugin ip in this.ipi.Values)
            {
                Logging.log("Getting actions on " + ip.getName(), LogType.Information, 10);
                foreach (MethodInfo mi in ip.GetType().GetMethods())
                {
                    Logging.log("Scanning method " + mi.Name, LogType.Information, 10);
                    if (mi.GetCustomAttributes(typeof(PluginMethod), true).Length > 0)
                    {
                        Logging.log("Ok ! Generating action ...", LogType.Information, 10);
                        l.Add(new SchAction()
                        {
                            Name = mi.Name,
                            Plugin_Name = ip.getName(),
                            Plugin_Function_Name = mi.Name,
                            Plugin_Function_Return_Type = mi.ReturnType.GetGenericArguments()[0].Name,
                            Param_0_Name = mi.GetParameters().Length > 0 ? mi.GetParameters()[0].Name : null,
                            Param_0_Type = mi.GetParameters().Length > 0 ? mi.GetParameters()[0].ParameterType.Name : null,
                            Param_1_Name = mi.GetParameters().Length > 1 ? mi.GetParameters()[1].Name : null,
                            Param_1_Type = mi.GetParameters().Length > 1 ? mi.GetParameters()[1].ParameterType.Name : null,
                            Param_2_Name = mi.GetParameters().Length > 2 ? mi.GetParameters()[2].Name : null,
                            Param_2_Type = mi.GetParameters().Length > 2 ? mi.GetParameters()[2].ParameterType.Name : null,
                            Param_3_Name = mi.GetParameters().Length > 3 ? mi.GetParameters()[3].Name : null,
                            Param_3_Type = mi.GetParameters().Length > 3 ? mi.GetParameters()[3].ParameterType.Name : null,
                            Param_4_Name = mi.GetParameters().Length > 4 ? mi.GetParameters()[4].Name : null,
                            Param_4_Type = mi.GetParameters().Length > 4 ? mi.GetParameters()[4].ParameterType.Name : null,
                            Param_5_Name = mi.GetParameters().Length > 5 ? mi.GetParameters()[5].Name : null,
                            Param_5_Type = mi.GetParameters().Length > 5 ? mi.GetParameters()[5].ParameterType.Name : null,
                            Param_6_Name = mi.GetParameters().Length > 6 ? mi.GetParameters()[6].Name : null,
                            Param_6_Type = mi.GetParameters().Length > 6 ? mi.GetParameters()[6].ParameterType.Name : null,
                            Param_7_Name = mi.GetParameters().Length > 7 ? mi.GetParameters()[7].Name : null,
                            Param_7_Type = mi.GetParameters().Length > 7 ? mi.GetParameters()[7].ParameterType.Name : null,
                            Param_8_Name = mi.GetParameters().Length > 8 ? mi.GetParameters()[8].Name : null,
                            Param_8_Type = mi.GetParameters().Length > 8 ? mi.GetParameters()[8].ParameterType.Name : null,
                            Param_9_Name = mi.GetParameters().Length > 9 ? mi.GetParameters()[9].Name : null,
                            Param_9_Type = mi.GetParameters().Length > 9 ? mi.GetParameters()[9].ParameterType.Name : null,
                            Date_Modif = DateTime.Now
                        });
                    }
                }
            }
            return l;
        }
    }
}
