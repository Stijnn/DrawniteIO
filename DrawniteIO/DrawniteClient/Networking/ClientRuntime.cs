using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrawniteClient.Networking
{
    class ClientRuntime
    {
        private TcpClient client;

        public ClientRuntime()
        {
            client = new TcpClient();
            client.Connect("127.0.0.1", 20000);

            new Thread(async () =>
            {
                while (true)
                {
                    byte[] sending = Encoding.UTF8.GetBytes("PING");
                    client.GetStream().Write(sending, 0, sending.Length);
                    int bytesAvailable = client.Available;
                    while (bytesAvailable == 0)
                    {

                    }
                    byte[] buffer = new byte[bytesAvailable];
                    client.GetStream().Read(buffer, 0, buffer.Length);
                    string content = Encoding.UTF8.GetString(buffer);
                }
            }).Start();
        }
    }
}
