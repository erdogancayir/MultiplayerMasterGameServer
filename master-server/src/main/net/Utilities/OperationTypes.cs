public enum OperationType
{
    // Authentication Operations
    LoginRequest = 100,
    LoginResponse = 101,
    LogoutRequest = 102,
    LogoutResponse = 103,
    AuthenticationResponse = 104,
    SignUpRequest = 105,
    SignUpResponse = 106,
    // Matchmaking Operations
    JoinLobbyRequest = 200,
    JoinLobbyResponse = 201,
    LeaveLobbyRequest = 202,
    LeavingLobbyResponse = 207,
    MatchmakingResponse = 203,
    CreateLobbyRequest = 204,
    CreateLobbyResponse = 205,
    NotifyGameStart = 206,

    // Lobby Management
    UpdateLobbyStatus = 300,
    StartGameCountdown = 301,

    // Server Management
    ServerStatusUpdate = 400,
    AllocateGameServer = 401,

    // Game Operations
    CreateGame = 450,
    UpdateGame = 451,
    GetGame = 452,
    GameEndData = 1000,
    GameSaveResponsePack = 1001,
    // Game Statistics Operations
    CreateGameStatistics = 453,
    GetGameStatistics = 454,

    // Leaderboard Operations
    UpdateLeaderboardEntry = 455,
    GetTopLeaderboardEntries = 456,

    // Heartbeat and Monitoring
    HeartbeatPing = 500,
}
