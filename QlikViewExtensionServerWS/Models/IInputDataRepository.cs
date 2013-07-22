using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QlikViewExtensionServerWS.Models
{
    public interface IInputDataRepository
    {
        IEnumerable<InputData> Get(int? id = null, string userName = null, string bucket = null, string bucketCategory = null);
        InputData Add(InputData item);
        void Remove(int id);
        bool Update(InputData item);
    }
}
