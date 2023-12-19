public class GameSession
{
    public string SessionId { get; private set; }
    public List<Player> Players { get; private set; }
    public string State { get; set; } // e.g., Active, Waiting, Completed
    public Game GameData { get; private set; }

    public GameSession(string sessionId, List<Player> players)
    {
        SessionId = sessionId;
        Players = players;
        State = "Waiting";
        GameData = new Game();
    }

    public void StartGame()
    {
        State = "Active";
        // Additional logic to start the game
    }

    public void EndGame()
    {
        State = "Completed";
        // Additional logic to end the game
    }

    // Additional methods as needed for game session management
}
