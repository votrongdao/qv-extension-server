using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace frqtlib.Core
{
    public interface IWindowsService
    {
        void init();
        void start();
        void stop();
    }
}
