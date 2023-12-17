public enum OperationType
{
    // Authentication Operations
    LoginRequest = 100,
    LoginResponse = 101,
    LogoutRequest = 101,
    AuthenticationResponse = 103,
    SignUpRequest = 104,
    SignUpResponse = 105,
    // Matchmaking Operations
    JoinLobbyRequest = 200,
    LeaveLobbyRequest = 201,
    MatchmakingResponse = 202,

    // Lobby Management
    UpdateLobbyStatus = 300,
    StartGameCountdown = 301,

    // Server Management
    ServerStatusUpdate = 400,
    AllocateGameServer = 401,

    // Heartbeat and Monitoring
    HeartbeatPing = 500,
    HeartbeatResponse = 501,

    // Multi-Server Operations
    ServerTransferRequest = 600,
    ServerTransferResponse = 601,

    // Other Operations
    CustomOperation = 700 // For future expansion or custom operations
}