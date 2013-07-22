using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO.Pipes;

namespace frqtlib.Core
{
    public class NamedPipe
    {
        public NamedPipe(string pipeName, int maxPipes = 10)
        {
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);

            byte[] buf = new byte[1024];
            pipeClient.BeginRead(buf, 0, 1024, new AsyncCallback(test), null);


            // NamedPipeServerStream pipeServer = new NamedPipeServerStream(".", pipeName, PipeDirection.InOut, numThreads);


        }



        void test(IAsyncResult ar)
        {

        }
    }
}
