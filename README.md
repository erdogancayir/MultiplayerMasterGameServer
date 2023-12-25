![resim](https://github.com/erdogancayir/gameserver/assets/94300378/f57d046b-1c3f-43a9-902a-4cafa98e74e8)
![Screenshot from 2023-12-24 15-04-15](https://github.com/erdogancayir/gameserver/assets/94300378/91281de0-0f51-4e95-becd-1c37779d256b)
![Screenshot from 2023-12-25 15-56-01](https://github.com/erdogancayir/gameserver/assets/94300378/d413754d-4cf2-40f1-9a95-02c5441de2bb)


MASTER SERVER

```
src
├── main
│   ├── config
│   │   ├── DatabaseConfig.cs
│   │   └── ServerConfig.cs
│   │   └── gameServerConfig.json
|   |   └── LoadServerConfiguration.cs
│   ├── tests
│   │   ├── AuthTests.cs
│   │   ├── MatchmakingTests.cs
│   │   └── SocketTests.cs
|   ├── Main.cs
│   ├── net
│   │   ├── Authentication
│   │   │   ├── AuthService.cs
│   │   │   └── TokenManager.cs
│   │   ├── Database
│   │   │   ├── DbInterface.cs
│   │   │   ├── LobbyManager.cs
│   │   │   ├── LogManager.cs
│   │   │   ├── GameManager.cs
│   │   │   ├── LeaderboardManager.cs
│   │   │   └── PlayerManager.cs
|   |   ├── Domain
│   │   │   ├── AuthenticationPack.cs
│   │   │   ├── BasePack.cs
│   │   │   ├── GamePack.cs
│   │   │   ├── CreateLobby.cs
│   │   │   ├── LeaderboardEntryPack.cs
│   │   │   ├── GameStatePack.cs
│   │   │   ├── GenericMessagePack.cs
│   │   │   ├── HeartbeatPack.cs
│   │   │   ├── MatchmakingPack.cs
│   │   │   ├── PlayerLeavingLobby.cs
│   │   │   └── SignUpPack.cs
│   │   ├── GameServerManagement
│   │   │   ├── GameServerManager.cs
│   │   │   └── HeartbeatMonitor.cs
│   │   ├── Matchmaking
│   │   │   └── Matchmaker.cs
│   │   ├── Models
│   │   │   ├── Lobby.cs
|   |   |   ├── GameServer.cs
|   |   |   ├── LogEntry.cs
|   |   |   ├── Matchmaking.cs
|   |   |   ├── LeaderboardEntry.cs
|   |   |   ├── Game.cs
│   │   │   └── Player.cs
│   │   ├── SocketListener
│   │   │   ├── ConnectionHandler.cs
│   │   │   ├── ConnectionManager.cs
│   │   │   └── SocketListener.cs
│   │   ├── Utilities
│   │   │   └── OperationTypes.cs
|   |   └── Documents
|   |       ├── AuthenticationDirectory.md
|   |       ├── ConfigDirectoryDocument.md
|   |       ├── DatabaseDirectoryDocument.md
|   |       ├── MatchmakingDirectory.md
|   |       └── GameServerManagementDic.md
```

![Screenshot from 2023-12-24 15-15-04](https://github.com/erdogancayir/gameserver/assets/94300378/59f07d9e-a181-4522-916c-744f665747b8)

GAME SERVER

```
src
├── main
│   ├── config
│   │   ├── DatabaseConfig.cs
│   │   └── ServerConfig.cs
|   |   └── LoadServerConfiguration.cs
|   ├── Main.cs
│   ├── net
│   │   ├── Database
│   │   │   └── DbInterface.cs
|   |   ├── Domain
│   │   │   ├── BasePack.cs
│   │   │   ├── EndpointPack.cs
│   │   │   ├── GameSavePack.cs
│   │   │   ├── GameStartPack.cs
│   │   │   ├── HeartbeatDataPack.cs
│   │   │   ├── HeartbeatPack.cs
│   │   │   ├── PlayerPositionPack.cs
│   │   │   └── PlayerLobbyInfo.cs
│   │   ├── GameLogic
│   │   │   ├── ConnectionMasterServer.cs
│   │   │   └── PositionManager.cs
│   │   ├── Models
|   |   |   ├── GameStatistic.cs
|   |   |   ├── LeaderboardEntry.cs
|   |   |   ├── Game.cs
│   │   │   └── PlayerData.cs
│   │   ├── SocketListener
│   │   │   ├── TcpConnectionHandler.cs
│   │   │   ├── UdpConnectionHandler.cs
│   │   │   ├── UdpConnectionManager.cs
│   │   │   └── SocketListener.cs
│   │   ├── Utilities
│   │   │   └── OperationTypes.cs
```
