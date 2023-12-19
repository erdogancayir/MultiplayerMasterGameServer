public enum OperationType
{
    // Authentication Operations
    PlayerPositionUpdate = 90, // UDP TThis operation is typically handled via UDP for real-time updates with minimal latency.
    StartGameSession = 92, // TCP This operation initializes a new game session, typically sent by the Master Server or initiated by a lobby leader.
    HeartbeatPing = 94, // UDP or TCP This operation is a simple heartbeat mechanism to check if the player or server is still connected.
}
