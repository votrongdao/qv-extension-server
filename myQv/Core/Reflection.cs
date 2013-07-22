using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace myQv.Core
{
    class CapReflection
    {
        public static void copyFields(object src, object dst)
        {
            Logging.log("Copying field values ...", LogType.Information, 10);
            foreach (PropertyInfo pi in dst.GetType().GetProperties())
            {
                Logging.log("Checking field : " + pi.Name, LogType.Information, 10);
                ColumnAttribute[] cat = (ColumnAttribute[]) pi.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (cat.Length > 0 && cat[0].IsDbGenerated == false)
                {
                    Logging.log("Ok ! Copying its value (" + (pi.GetGetMethod().Invoke(src, null) ?? "null").ToString() + ")", LogType.Information, 10);
                    dst.GetType().GetProperty(pi.Name).GetSetMethod().Invoke(dst, new [] { pi.GetGetMethod().Invoke(src, null) });
                }
            }
        }
    }
}
