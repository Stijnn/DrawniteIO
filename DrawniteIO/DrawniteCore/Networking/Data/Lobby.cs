using System;
using System.Collections.Generic;
using System.Text;

namespace DrawniteCore.Networking.Data
{
    class Lobby
    {
        private IList<IConnection> connectedClients;
        private readonly int port;
        private Guid lobbyId;
        private IConnection lobbyLeader;
        private LobbyStatus status;

        public Lobby(int port, Guid lobbyId, ref IConnection lobbyLeader)
        {
            this.port = port;
            this.lobbyId = lobbyId;
            this.lobbyLeader = lobbyLeader;
            this.connectedClients = new List<IConnection>();
            this.connectedClients.Add(lobbyLeader);
            status = LobbyStatus.AWAITING_START;
        }
    }
    
    enum LobbyStatus
    {
        AWAITING_START,
        PLAYING,
        AWAITING_RESTART,
    }
}
