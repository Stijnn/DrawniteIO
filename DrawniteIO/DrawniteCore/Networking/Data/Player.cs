using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DrawniteCore.Networking.Data
{
    public class Player
    {
        public Guid PlayerId { get; set; }
        public string Username { get; set; }
        public bool IsLeader { get; set; }

        [JsonIgnore]
        public IConnection ReplicatedConnection { get; set; }

        [JsonConstructor]
        public Player(string PlayerId, string Username, bool IsLeader)
        {
            this.PlayerId = Guid.Parse(PlayerId);
            this.Username = Username;
            this.IsLeader = IsLeader;
        }

        public Player(Guid PlayerId, string Username, bool IsLeader)
        {
            this.PlayerId = PlayerId;
            this.Username = Username;
            this.IsLeader = IsLeader;
        }
    }
}
