using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawniteServer
{
    class LobbyManager
    {
        private static LobbyManager instance;
        public static LobbyManager Instance => instance;

        public static void Init()
        {
            if (instance == null)
                instance = new LobbyManager();
        }

        private Stack<Lobby> closingLobbies;
        private List<Lobby> runningLobbies;

        private LobbyManager()
        {
            closingLobbies = new Stack<Lobby>();
            runningLobbies = new List<Lobby>();
        }

        private Random rnd = new Random(DateTime.Now.Millisecond);
        public Lobby NewLobby(Guid lobbyLeader)
        {
            int lobbyPort = 0;
            while (lobbyPort < 20001 || runningLobbies.Where(x => x.LobbyInfo.LobbyPort == lobbyPort).Count() > 0)
            {
                lobbyPort = rnd.Next(20001, 30000);
            }
            Lobby newLobby = new Lobby(lobbyLeader, lobbyPort);
            runningLobbies.Add(newLobby);
            return newLobby;
        }

        public void Update()
        {
            while (closingLobbies.Count > 0)
                runningLobbies.Remove(closingLobbies.Pop());

            runningLobbies.ForEach(x =>
            {
                if (x.PlayerCount <= 0)
                {
                    closingLobbies.Push(x);
                }
            });
        }
    }
}
