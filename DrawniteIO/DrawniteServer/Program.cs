using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrawniteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerRuntime sr = new ServerRuntime(new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            sr.Start();

            while (true)
            {

            }
        }
    }
}
