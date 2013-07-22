using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using myQv.Core;

namespace myQv.Threading
{
    public class WorkParameters : Dictionary<string, object>
    {
        public string _f = null;

        public WorkParameters(string function)
            : base()
        {
            this._f = function;
        }

        public new void Add(string param, object paramValue)
        {
            if (!base.ContainsKey(param))
                base.Add(param, paramValue);
            else
                base[param] = paramValue;
        }
    }


    public class WorkMethodParameterDesc : Dictionary<string, System.Type>
    {
    }

    public class WorkParametersDesc : Dictionary<string, WorkMethodParameterDesc>
    {

    }
}


