﻿using System;
using System.Collections.Generic;

using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using System.Text;

using System.Reflection;

using myQv.Core;

namespace myQv.Schedule
{
    [AttributeUsage(AttributeTargets.Class |
    AttributeTargets.Constructor |
    AttributeTargets.Field |
    AttributeTargets.Method |
    AttributeTargets.Property)]
    public class FunctionalKeyColumnAttribute : System.Attribute
    {
        public FunctionalKeyColumnAttribute() { }
    }

    public class SchKey
    {
        private SchTable _t;

        public SchKey(SchTable t)
        {
            this._t = t;
        }

        public static bool operator ==(SchKey a, SchKey b)
        {
            if (((object)a == null) && ((object)b == null))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(SchKey a, SchKey b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            Logging.log("Equals called on " + obj.GetType().Name + " ...", LogType.Information, 10);

            if (obj.GetType() != this.GetType())
            {
                Logging.log("Difference on type", LogType.Information, 10);
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                Logging.log("Same reference !", LogType.Information, 10);
                return true;
            }

            if (((SchKey)obj)._t.GetType() != this._t.GetType())
            {
                Logging.log("Difference on embedded type", LogType.Information, 10);
                return false;
            }

            foreach (PropertyInfo pi in this._t.GetType().GetProperties())
            {
                FunctionalKeyColumnAttribute[] cat = (FunctionalKeyColumnAttribute[])pi.GetCustomAttributes(typeof(FunctionalKeyColumnAttribute), true);
                if (cat.Length > 0)
                {
                    object myVal = pi.GetGetMethod().Invoke(this._t, null);
                    object otVal = pi.GetGetMethod().Invoke(((SchKey)obj)._t, null);

                    if ((myVal == null && otVal != null) ||
                        (myVal != null && otVal == null) ||
                        (
                            (myVal != null && otVal != null) &&
                            !myVal.Equals(otVal)
                        ))
                    {
                        Logging.log("Difference on field : " + pi.Name + " : { " + myVal.ToString() + " } != { " + otVal.ToString() + " }", LogType.Information, 10);
                        return false;
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            string s = null;
            foreach (PropertyInfo pi in this._t.GetType().GetProperties())
            {
                FunctionalKeyColumnAttribute[] cat = (FunctionalKeyColumnAttribute[])pi.GetCustomAttributes(typeof(FunctionalKeyColumnAttribute), true);
                if (cat.Length > 0)
                {
                    s = (( s == null) ? "" : s + ", ") + pi.GetGetMethod().Invoke(this._t, null).ToString();

                }
            }

            return "{ " + s + " }";
        }
    }

    public abstract class SchTable
    {
        public abstract long Id { get; set; }

        public SchKey Key
        {
            get
            {
                return new SchKey(this);
            }
        }

        public void copyFields(SchTable dst)
        {
            Logging.log("Copying field values ...", LogType.Information, 10);
            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                Logging.log("Checking field : " + pi.Name, LogType.Information, 10);
                ColumnAttribute[] cat = (ColumnAttribute[])pi.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (cat.Length > 0 && cat[0].IsDbGenerated == false)
                {
                    Logging.log("Ok ! Copying its value (" + (pi.GetGetMethod().Invoke(this, null) ?? "null").ToString() + ")", LogType.Information, 10);
                    dst.GetType().GetProperty(pi.Name).GetSetMethod().Invoke(dst, new[] { pi.GetGetMethod().Invoke(this, null) });
                }
            }
        }

        public static bool operator ==(SchTable a, SchTable b)
        {
            if (((object)a == null) && ((object)b == null))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(SchTable a, SchTable b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            Logging.log("Equals called on " + obj.GetType().Name + " ...", LogType.Information, 10);

            if (obj.GetType() != this.GetType())
            {
                Logging.log("Difference on type", LogType.Information, 10);
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                Logging.log("Same reference !", LogType.Information, 10);
                return true;
            }

            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                ColumnAttribute[] cat = (ColumnAttribute[])pi.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (cat.Length > 0 && pi.Name != "Date_Modif" && !cat[0].IsDbGenerated)
                {
                    object myVal = pi.GetGetMethod().Invoke(this, null);
                    object otVal = pi.GetGetMethod().Invoke(obj, null);

                    if ((myVal == null && otVal != null) ||
                        (myVal != null && otVal == null) ||
                        (
                            (myVal != null && otVal != null) &&
                            !myVal.Equals(otVal)
                        ))
                    {
                        Logging.log("Difference on field : " + pi.Name + " : { " + myVal.ToString() + " } != { " + otVal.ToString() + " }", LogType.Information, 10);
                        return false;
                    }
                }
            }

            return true;
        }
    }

    [Table(Name = "[QvCap].[sched].[T_REF_ACTION]")]
    public class SchAction : SchTable
    {
        [Column(Name = "[ID_REF_ACTION]", DbType = "bigint IDENTITY(100,1) NOT NULL", CanBeNull = false, IsPrimaryKey = true, IsDbGenerated = true)]
        public override long Id { get; set; }

        [Column(Name = "[NAME]", DbType = "varchar(64) NOT NULL", CanBeNull = false)]
        [FunctionalKeyColumnAttribute()]
        public string Name { get; set; }

        [Column(Name = "[PLUGIN_NAME]", DbType = "varchar(64) NOT NULL", CanBeNull = false)]
        [FunctionalKeyColumnAttribute()]
        public string Plugin_Name { get; set; }

        [Column(Name = "[PLUGIN_FUNCTION_NAME]", DbType = "varchar(64) NOT NULL", CanBeNull = false)]
        public string Plugin_Function_Name { get; set; }

        [Column(Name = "[PLUGIN_FUNCTION_RETURN_TYPE]", DbType = "varchar(64) NOT NULL", CanBeNull = false)]
        public string Plugin_Function_Return_Type { get; set; }

        [Column(Name = "[PARAM_0_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_0_Name { get; set; }

        [Column(Name = "[PARAM_0_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_0_Type { get; set; }

        [Column(Name = "[PARAM_1_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_1_Name { get; set; }

        [Column(Name = "[PARAM_1_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_1_Type { get; set; }

        [Column(Name = "[PARAM_2_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_2_Name { get; set; }

        [Column(Name = "[PARAM_2_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_2_Type { get; set; }

        [Column(Name = "[PARAM_3_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_3_Name { get; set; }

        [Column(Name = "[PARAM_3_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_3_Type { get; set; }

        [Column(Name = "[PARAM_4_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_4_Name { get; set; }

        [Column(Name = "[PARAM_4_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_4_Type { get; set; }

        [Column(Name = "[PARAM_5_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_5_Name { get; set; }

        [Column(Name = "[PARAM_5_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_5_Type { get; set; }

        [Column(Name = "[PARAM_6_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_6_Name { get; set; }

        [Column(Name = "[PARAM_6_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_6_Type { get; set; }

        [Column(Name = "[PARAM_7_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_7_Name { get; set; }

        [Column(Name = "[PARAM_7_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_7_Type { get; set; }

        [Column(Name = "[PARAM_8_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_8_Name { get; set; }

        [Column(Name = "[PARAM_8_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_8_Type { get; set; }

        [Column(Name = "[PARAM_9_NAME]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_9_Name { get; set; }

        [Column(Name = "[PARAM_9_TYPE]", DbType = "varchar(64) NULL", CanBeNull = true)]
        public string Param_9_Type { get; set; }

        [Column(Name = "[D_MOD]", DbType = "datetime NOT NULL", CanBeNull = false)]
        public DateTime Date_Modif { get; set; }
    }

    [Table(Name = "[QvCap].[sched].[T_REF_SCHEDULE_LOG]")]
    public class SchScheduleLog : SchTable
    {
        [Column(Name="[ID_REF_SCHEDULE_LOG]", DbType="bigint IDENTITY(100,1) NOT NULL", CanBeNull=false,IsPrimaryKey = true, IsDbGenerated=true)]
        [FunctionalKeyColumnAttribute()]
        public override long Id { get; set; }

        [Column(Name="[FK_REF_SCHEDULE_STATUS]", DbType="bigint NOT NULL", CanBeNull=false)]
        public long Ref_Id_Schedule_Status { get; set; }

        [Column(Name="[FK_REF_SEVERITY]", DbType="bigint NOT NULL", CanBeNull=false)]
        public long Ref_Id_Severity { get; set; }

        [Column(Name="[ERROR_CODE]", DbType="int NOT NULL", CanBeNull=false)]
        public int Error_Code { get; set; }

        [Column(Name="[MESSAGE]", DbType="varchar(max) NOT NULL", CanBeNull=false)]
        public string Message { get; set; }

        [Column(Name="[TECH_MESSAGE]", DbType="varchar(max) NOT NULL", CanBeNull=false)]
        public string Tech_Message { get; set; }

        [Column(Name="[D_MOD]", DbType="datetime NULL", CanBeNull=true)]
        public DateTime Date_Modif { get; set; }


    }

    [Table(Name = "[QvCap].[sched].[T_REF_SCHEDULE_STATUS]")]
    public class SchScheduleStatus : SchTable
    {
        [Column(Name="[ID_REF_SCHEDULE_STATUS]", DbType="bigint IDENTITY(100,1) NOT NULL", CanBeNull=false,IsPrimaryKey = true, IsDbGenerated=true)]
        [FunctionalKeyColumnAttribute()]
        public override long Id { get; set; }

        [Column(Name="[FK_REF_SCHEDULE]", DbType="bigint NOT NULL", CanBeNull=false)]
        public long Ref_Id_Schedule { get; set; }

        [Column(Name="[SCHEDULE_STATUS]", DbType="varchar(3) NOT NULL", CanBeNull=false)]
        public string Status { get; set; }

        [Column(Name="[D_MOD]", DbType="datetime NULL", CanBeNull=true)]
        public DateTime Date_Modif { get; set; }
    }

    [Table(Name = "[QvCap].[sched].[T_REF_STEP]")]
    public class SchStep : SchTable
    {
        [Column(Name="[ID_REF_STEP]", DbType="bigint IDENTITY(100,1) NOT NULL", CanBeNull=false,IsPrimaryKey = true, IsDbGenerated=true)]
        [FunctionalKeyColumnAttribute()]
        public override long Id { get; set; }

        [Column(Name="[INDEX]", DbType="int NOT NULL", CanBeNull=false)]
        public long Index { get; set; }

        [Column(Name="[FK_REF_TASK]", DbType="bigint NOT NULL", CanBeNull=false)]
        public long Ref_Id_Task { get; set; }

        [Column(Name="[FK_REF_ACTION]", DbType="bigint NOT NULL", CanBeNull=false)]
        public long Ref_Id_Action { get; set; }

        [Column(Name="[PARAM_0_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_0_Value { get; set; }

        [Column(Name="[PARAM_1_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_1_Value { get; set; }

        [Column(Name="[PARAM_2_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_2_Value { get; set; }

        [Column(Name="[PARAM_3_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_3_Value { get; set; }

        [Column(Name="[PARAM_4_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_4_Value { get; set; }

        [Column(Name="[PARAM_5_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_5_Value { get; set; }

        [Column(Name="[PARAM_6_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_6_Value { get; set; }

        [Column(Name="[PARAM_7_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_7_Value { get; set; }

        [Column(Name="[PARAM_8_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_8_Value { get; set; }

        [Column(Name="[PARAM_9_VALUE]", DbType="varchar(max) NULL", CanBeNull=true)]
        public string Param_9_Value { get; set; }

        [Column(Name="[D_MOD]", DbType="datetime NULL", CanBeNull=true)]
        public DateTime Date_Modif { get; set; }

    }

    [Table(Name = "[QvCap].[sched].[T_REF_TASK]")]
    public class SchTask : SchTable
    {
        [Column(Name = "[ID_REF_TASK]", DbType = "bigint IDENTITY(100,1) NOT NULL", CanBeNull = false, IsPrimaryKey = true, IsDbGenerated = true)]
        public override long Id { get; set; }

        [Column(Name = "[NAME]", DbType = "varchar(64) NOT NULL", CanBeNull = false)]
        [FunctionalKeyColumnAttribute()]
        public string Name { get; set; }

        [Column(Name = "[COND_TYPE]", DbType = "varchar(3) NOT NULL", CanBeNull = false)]
        [FunctionalKeyColumnAttribute()]
        public string Condition_Type { get; set; }

        [Column(Name = "[D_MOD]", DbType = "datetime NOT NULL", CanBeNull = false)]
        public DateTime Date_Modif { get; set; }
    }

    [Table(Name = "[QvCap].[sched].[T_REF_TRIGGER]")]
    public class SchTrigger : SchTable
    {
        [Column(Name="[ID_REF_TRIGGER]", DbType="bigint IDENTITY(100,1) NOT NULL", CanBeNull=false,IsPrimaryKey = true, IsDbGenerated=true)]
        [FunctionalKeyColumnAttribute()]
        public override long Id { get; set; }

        [Column(Name="[FK_REF_TASK]", DbType="bigint NOT NULL", CanBeNull=false)]
        public long Ref_Id_Task { get; set; }

        [Column(Name="[TRIGGER_TYPE]", DbType="varchar(16) NULL", CanBeNull=true)]
        public long Trigger_Type { get; set; }

        [Column(Name="[DATE_TIME]", DbType="datetime NULL", CanBeNull=true)]
        public DateTime Date_Time { get; set; }

        [Column(Name="[DATE_TYPE]", DbType="varchar(3) NULL", CanBeNull=true)]
        public string Date_Type { get; set; }

        [Column(Name="[DATE_STEP]", DbType="int NULL", CanBeNull=true)]
        public int Date_Step { get; set; }

        [Column(Name="[TASK_ID]", DbType="bigint NULL", CanBeNull=true)]
        public long Parent_Task_Id { get; set; }

        [Column(Name="[TASK_STATUS]", DbType="varchar(3) NULL", CanBeNull=true)]
        public string Parent_Task_Status { get; set; }

        [Column(Name="[D_MOD]", DbType="datetime NULL", CanBeNull=true)]
        public DateTime Date_Modif { get; set; }
    }

    

}


