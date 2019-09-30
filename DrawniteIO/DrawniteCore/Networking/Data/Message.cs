using System;
using System.Collections.Generic;
using System.Text;

namespace DrawniteCore.Networking.Data
{
    public class Message
    {
        public string Command { get; set; }
        public dynamic Data { get; set; }

        public Message(string command, dynamic data)
        {
            this.Command = command;
            this.Data = data;
        }
    }
}
