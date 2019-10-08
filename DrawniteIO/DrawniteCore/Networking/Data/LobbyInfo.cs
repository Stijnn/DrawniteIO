using System;
using System.Collections.Generic;
using System.Text;

namespace DrawniteCore.Networking.Data
{
    public class LobbyInfo
    {
        public Guid LobbyId { get; set; }
        public int LobbyPort { get; set; }
        public Guid LobbyLeader { get; set; }
        public LobbyStatus LobbyStatus { get; set; }

        public LobbyInfo(Guid LobbyId, int LobbyPort, Guid LobbyLeader)
        {
            this.LobbyId = LobbyId;
            this.LobbyPort = LobbyPort;
            this.LobbyLeader = LobbyLeader;
            this.LobbyStatus = LobbyStatus.AWAITING_START;
        }
    }

    public enum LobbyStatus
    {
        AWAITING_START,
        STARTING,
        PLAYING,
        AWAITING_RESTART,
        RESTARTING,
    }
}
