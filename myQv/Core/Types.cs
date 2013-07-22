﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;

namespace myQv.Core
{
    public enum AccessRight { SAD, USR, ADM, DEV, UNK };

    public enum AccessRightAction { ADD, DEL, UPD, UNK };

    public class QvUserIn<T>
    {
        public long Global_Id;
        public String Login;
        public T t;
        public bool inherited;
        public AccessRight User_Right;

        public QvUserIn() { }
        public QvUserIn(long Global_Id, String Login, T t, bool inherited, AccessRight User_Right)
        {
            this.Global_Id = Global_Id;
            this.Login = Login;
            this.t = t;
            this.inherited = inherited;
            this.User_Right = User_Right;
        }
    }

    public class QvUserGroupIn<T>
    {
        public long User_Group_Id;
        public String User_Group_Name;
        public T t;
        public bool inherited;
        public AccessRight User_Group_Right;

        public QvUserGroupIn() { }
        public QvUserGroupIn(long User_Group_Id, String User_Group_Name, T t, bool inherited, AccessRight User_Group_Right)
        {
            this.User_Group_Id = User_Group_Id;
            this.User_Group_Name = User_Group_Name;
            this.t = t;
            this.inherited = inherited;
            this.User_Group_Right = User_Group_Right;
        }
    }

    public class QvUserGroup
    {
        public long Id;
        public long Parent_Id;
        public long Orig_Id;
        public String Name;
        public String Display_Name;
        public AccessRight Right;
        public long Pid;
        public long Depth;

        public QvUserGroup() { }
        public QvUserGroup(long Id, long Parent_Id, long Orig_Id, String Name, String Display_Name, AccessRight Right, long Pid, long Depth)
        {
            this.Id = Id;
            this.Parent_Id = Parent_Id;
            this.Orig_Id = Orig_Id;
            this.Name = Name;
            this.Display_Name = Display_Name;
            this.Right = Right;
            this.Pid = Pid;
            this.Depth = Depth;
        }
    }

    public class QvDocGroup
    {
        public long Id;
        public long Parent_Id;
        public long Orig_Id;
        public String Name;
        public String Display_Name;
        public String Root_Path;
        public String Path;
        public AccessRight Right;

        public QvDocGroup() { }
        public QvDocGroup(long Id, long Parent_Id, long Orig_Id, String Name, String Display_Name, String Root_Path, String Path, AccessRight Right)
        {
            this.Id = Id;
            this.Parent_Id = Parent_Id;
            this.Orig_Id = Orig_Id;
            this.Name = Name;
            this.Display_Name = Display_Name;
            this.Root_Path = Root_Path;
            this.Path = Path;
            this.Right = Right;
        }
    }

    public class QvEnvDocGroup
    {
        public long Id_Dg;
        public long Id_Env;
        public long Id_Dg_Orig;
        public String Env_Root_Path;
        public String Folder_Path;
        public String Rel_Path;
        public String Abs_Path;
        public String Cur_Path;

        public QvEnvDocGroup() { }
        public QvEnvDocGroup(long Id_Dg, long Id_Env, long Id_Dg_Orig, String Env_Root_Path, String Folder_Path, String Rel_Path, String Abs_Path, String Cur_Path)
        {
            this.Id_Dg = Id_Dg;
            this.Id_Env = Id_Env;
            this.Id_Dg_Orig = Id_Dg_Orig;
            this.Env_Root_Path = Env_Root_Path;
            this.Folder_Path = Folder_Path;
            this.Rel_Path = Rel_Path;
            this.Abs_Path = Abs_Path;
            this.Cur_Path = Cur_Path;
        }
    }

    public class QvEnvDoc
    {
        public long Id_Doc;
        public String Folder_Path;
        public String File_Path;
        public String Server_Uri;
        public String Rel_Path;

        public QvEnvDoc() { }
        public QvEnvDoc(long Id_Doc, String Folder_Path, String File_Path, String Server_Uri, String Rel_Path)
        {
            this.Id_Doc = Id_Doc;
            this.Folder_Path = Folder_Path;
            this.File_Path = File_Path;
            this.Server_Uri = Server_Uri;
            this.Rel_Path = Rel_Path;
        }
    }

    public class QvDoc
    {
        public long Id;
        public long Orig_Document_Group_Id;
        public long Document_Group_Id;
        public String Name;
        public String Display_Name;
        public String Document_Version;
        public String Qlikview_Version;
        public String Reload_Period;
        public String Reload_Value;
        public bool Accept_Document_Cal;
        public Int32 Document_Cal_Quota;
        public AccessRight Right;
        public String Path;
        public String Server;


        public QvDoc() { }
        public QvDoc(long Id, long Orig_Document_Group_Id, long Document_Group_Id, String Name, String Display_Name, String Document_Version, String Qlikview_Version, String Reload_Period, String Reload_Value, bool Accept_Document_Cal, Int32 Document_Cal_Quota, AccessRight Right, String Path, String Server)
        {
            this.Id = Id;
            this.Orig_Document_Group_Id = Orig_Document_Group_Id;
            this.Document_Group_Id = Document_Group_Id;
            this.Name = Name;
            this.Display_Name = Display_Name;
            this.Document_Version = Document_Version;
            this.Qlikview_Version = Qlikview_Version;
            this.Reload_Period = Reload_Period;
            this.Reload_Value = Reload_Value;
            this.Accept_Document_Cal = Accept_Document_Cal;
            this.Document_Cal_Quota = Document_Cal_Quota;
            this.Right = Right;
            this.Path = Path;
            this.Server = Server;

        }

        public string ToString(bool Upd_Group_Id, bool Upd_Name, bool Upd_Version, bool Upd_QlikView_Version, bool Upd_Reload_Period, bool Upd_Reload_Value, bool Upd_Document_Cal_Ok, bool Upd_Document_Cal_Quota)
        {
            return
                this.Id.ToString() +
                ((Upd_Group_Id) ? "Document_Group_Id = " + this.Document_Group_Id : "");
        }
    }

    public class QvRight
    {
        public long User_Group_Id;
        public long Document_Group_Id;
        public long Document_Id;

        public AccessRight Right;

        public QvRight() { }
        public QvRight(long User_Group_Id, long Document_Group_Id, long Document_Id, AccessRight Right)

        {
            this.User_Group_Id = User_Group_Id;
            this.Document_Group_Id = Document_Group_Id;
            this.Document_Id = Document_Id;
            this.Right = Right;
        }
    }

    public class QvCredentials
    {
        public String Server;
        public String Login;
        public String Document;

        public QvCredentials() { }
        public QvCredentials(String Server, String Login, String Document)
        {
            this.Server = Server;
            this.Login = Login;
            this.Document = Document;
        }
    }

    public class QvTypeSync
    {
        public String IdLastExec;
        public String TypeSync;

        public QvTypeSync() { }
        public QvTypeSync(String IdLastExec, String TypeSync)
        {
            this.IdLastExec = IdLastExec;
            this.TypeSync = TypeSync;
        }
    }

    public class QvInfoSync
    {
        public String StartDate;
        public String EndDate;
        public String Duration;
        public String Status;

        public QvInfoSync() { }
        public QvInfoSync(String StartDate, String EndDate, String Duration, String Status)
        {
            this.StartDate = StartDate;
            this.EndDate = EndDate;
            this.Duration = Duration;
            this.Status = Status;
        }
    }

    public class QvEnv
    {
        public long Env_Id;
        public string Name;
        public string Path;
        public string Server_URI;
        public string Root;
        public string Server_Name;

        public QvEnv() { }
        public QvEnv(long Env_Id, String Name, String Path) : this(Env_Id, Name, Path, null, null, null) { }
        public QvEnv(long Env_Id, string Name, string Path, string Server_URI, string Root, string Server_Name)
        {
            this.Env_Id = Env_Id;
            this.Name = Name;
            this.Path = Path;
            this.Server_URI = Server_URI;
            this.Root = Root;
            this.Server_Name = Server_Name;
        }            
    }

    public interface IReturnObject
    {
        int Error_Code { get; set; }
        string msg { get; set; }
        string tech_msg { get; set; }

        System.Type getObjectType();
        object getObject();
        T getObject<T>();
    }

    [Serializable()]
    public class returnObject<T> : IReturnObject, ISerializable
        {
            public T o;

            public int _Error_Code;
            public string _msg;
            public string _tech_msg;

            public int Error_Code
            {
                get { return this._Error_Code; }
                set { this._Error_Code = value; }
            }

            public string msg
            {
                get { return this._msg; }
                set { this._msg = value; }
            }

            public string tech_msg
            {
                get { return this._tech_msg; }
                set { this._tech_msg = value; }
            }

            public returnObject() { }

            public returnObject(T o, Int32 Error_Code, String msg)
                : base()
            {
                this.o = o;
                this.Error_Code = Error_Code;

                this.msg = msg;
            }

            public returnObject(T o, Int32 Error_Code, String msg, String tech_msg)
                : this(o, Error_Code, msg)
            {
                this.tech_msg = tech_msg;
            }

            public returnObject(SerializationInfo info, StreamingContext ctxt)
            {
                this._Error_Code = (int)info.GetValue("_Error_Code", typeof(int));
                this._msg = (string)info.GetValue("_msg", typeof(string));
                this._tech_msg = (string)info.GetValue("_tech_msg", typeof(string));
                this.o = (T)info.GetValue("o", typeof(T));
            }

           public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
           {
               info.AddValue("_Error_Code", this._Error_Code);
               info.AddValue("_msg", this._msg);
               info.AddValue("_tech_msg", this._tech_msg);
               info.AddValue("o", this.o);
           }

            public object getObject()
            {
                return this.o;
            }

            public U getObject<U>()
            {
                return ((U) ((object) this.o));
            }

            public Type getObjectType()
            {
                return typeof(T);
            }

        }

    public class PluginException : Exception
    {
        public PluginException(string msg) : base(msg) { }
    }
    public class ReloadData
    {
        public String srv;
        public String tsk;
        public String psw;
        public List<String> log;
        public int cod;

        public ReloadData() { }
        public ReloadData(String srv, String tsk, String psw, List<String> log, int cod)
        {
            this.srv = srv;
            this.tsk = tsk;
            this.psw = psw;
            this.log = log;
            this.cod = cod;
        }

        public ReloadData(String srv, String tsk, String psw, String[] log, int cod)
        {
            this.srv = srv;
            this.tsk = tsk;
            this.psw = psw;
            this.log = new List<string>();
            foreach (string value in log)
                this.log.Add(value);
            this.cod = cod;
        }
    }

    public class QMSServerInfo
    {
        public String server_name;
        public int Limit_Doc_Cal;
        public int Assigned_Doc_Cal;
        public int Limit_Named_Cal;
        public int Assigned_Named_Cal;

        public QMSServerInfo() { }

        public QMSServerInfo(String server_name, int Limit_Doc_Cal, int Assigned_Doc_Cal, int Limit_Named_Cal, int Assigned_Named_Cal)
        {
            this.server_name = server_name;
            this.Limit_Doc_Cal = Limit_Doc_Cal;
            this.Assigned_Doc_Cal = Assigned_Doc_Cal;
            this.Limit_Named_Cal = Limit_Named_Cal;
            this.Assigned_Named_Cal = Assigned_Named_Cal;
        }
    }

    public class QvUser
    {
        public Int64 Global_Id;
        public String Login;
        public String First_Name;
        public String Last_Name;

        public QvUser() { }

        public QvUser(Int64 Global_Id, String Login, String First_Name, String Last_Name)
        {
            this.Global_Id = Global_Id;
            this.Login = Login;
            this.First_Name = First_Name;
            this.Last_Name = Last_Name;
        }
    }

    public class QvDocLight
    {
        public Int64 Id;
        public Int64 Document_Group_Id;
        public String Name;
        public String Display_Name;
        public AccessRight Right;

        public QvDocLight() { }

        public QvDocLight(Int64 Id, Int64 Document_Group_Id, String Name, String Display_Name, AccessRight Right)
        {
            this.Id = Id;
            this.Document_Group_Id = Document_Group_Id;
            this.Name = Name;
            this.Display_Name = Display_Name;
            this.Right = Right;

        }
    }

    public class QvDocGroupLight
    {
        public Int64 Id;
        public Int64 Parent_Id;
        public String Name;
        public String Display_Name;
        public String Root_Path;
        public AccessRight Right;

        public QvDocGroupLight() { }

        public QvDocGroupLight(Int64 Id, Int64 Parent_Id, String Name, String Display_Name, String Root_Path, AccessRight Right)
        {
            this.Id = Id;
            this.Parent_Id = Parent_Id;
            this.Name = Name;
            this.Display_Name = Display_Name;
            this.Root_Path = Root_Path;
            this.Right = Right;
        }
    }

    public class QvUserInDoc
    {
        public Int64 Global_Id;
        public String Login;
        public bool inherited;
        public AccessRight User_Right;

        public QvUserInDoc() { }

        public QvUserInDoc(Int64 Global_Id, String Login, bool inherited, AccessRight User_Right)
        {
            this.Global_Id = Global_Id;
            this.Login = Login;
            this.inherited = inherited;
            this.User_Right = User_Right;
        }

    }

    public class QvUserGroupInDoc
    {
        public Int64 User_Group_Id;
        public String User_Group_Name;
        public bool inherited;
        public AccessRight User_Group_Right;

        public QvUserGroupInDoc() { }

        public QvUserGroupInDoc(Int64 User_Group_Id, String User_Group_Name, bool inherited, AccessRight User_Group_Right)
        {
            this.User_Group_Id = User_Group_Id;
            this.User_Group_Name = User_Group_Name;
            this.inherited = inherited;
            this.User_Group_Right = User_Group_Right;
        }
    }
      
    public class QvInfoServer
    {
        public String Message;
        public String SrvName;

        public QvInfoServer() { }
        public QvInfoServer(String SrvName, String Message)
        {
            this.SrvName = SrvName;
            this.Message = Message;
        }
    }

}
