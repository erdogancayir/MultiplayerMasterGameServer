public enum OperationType
{
    // Authentication Operations
    PlayerPositionUpdate = 90, // UDP TThis operation is typically handled via UDP for real-time updates with minimal latency.
    StartGameSession = 92, // TCP This operation initializes a new game session, typically sent by the Master Server or initiated by a lobby leader.
    PlayerLobbyInfo = 96, // TCP This operation is sent by the client to the server to update the player's lobby info.
    PlayerLobbyInfoResponse = 98,

    PlayerJoinedLobbyRequest = 1001,

    // Heartbeat and Monitoring
    HeartbeatPing = 500,
    HeartbeatResponse = 501,
    GameEndData = 1000, // TCP This operation is sent by the server to the client to notify the client that the game has ended.
    Test = 666,
    EndPoint = 602,
    GameOverRequest = 603,
    GameOverResponse = 604,

    LeaveLobbyRequest = 202,

}
