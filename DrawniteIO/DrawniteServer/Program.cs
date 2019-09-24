using DrawniteCore.Networking;
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
        static async Task Main(string[] args)
        {
            ListeningService listeningService = new ListeningService(new System.Net.IPEndPoint(IPAddress.Parse("145.49.23.205"), 20000));
            await listeningService.StartAsync();
            string command = string.Empty;
            while (!(command == "shutdown"))
            {
                command = Console.ReadLine();
            }
            await listeningService.ShutdownAsync();
        }
    }
}
