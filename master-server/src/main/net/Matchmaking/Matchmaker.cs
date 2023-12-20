using System.Net.Sockets;
using MessagePack;
using MongoDB.Driver;

public class Matchmaker
{
    private LobbyManager lobbyManager;
    private readonly TokenManager _tokenManager;
    private PlayerManager playerManager;
    private readonly ConnectionManager _connectionManager;

    /// <summary>
    /// Initializes a new instance of the Matchmaker class.
    /// </summary>
    /// <param name="lobbyManager">The lobby manager to handle lobby operations.</param>
    /// <param name="tokenManager">The token manager for authentication purposes.</param>
    /// <param name="playerManager">The player manager to manage player data.</param>
    /// <param name="connectionManager">The connection manager to handle network connections.</param>
    public Matchmaker(LobbyManager lobbyManager, TokenManager tokenManager, PlayerManager playerManager, ConnectionManager connectionManager)
    {
        this.lobbyManager = lobbyManager;
        _tokenManager = tokenManager;
        this.playerManager = playerManager;
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// Handles a request to join a lobby.
    /// </summary>
    /// <param name="stream">The network stream for communication.</param>
    /// <param name="data">The byte array data received in the request.</param>
    /// <param name="connectionId">The connection ID of the requesting client.</param>
    public async void HandleJoinLobbyRequest(NetworkStream stream, byte[] data, string connectionId)
    {
        try
        {
            Console.WriteLine("Join lobby request received.");
            var joinLobbyRequest = MessagePackSerializer.Deserialize<MatchmakingRequest>(data);
            var playerId = _tokenManager.ValidateToken(joinLobbyRequest.Token);
            if (playerId == null)
            {
                Console.WriteLine("Invalid token.");
                await SendErrorResponse(stream, "Invalid token.");
                return;
            }

            var allLobbies = await lobbyManager.GetLobbies();
            var lobby = FindOrCreateLobby(allLobbies, playerId);
            await lobbyManager.UpdateLobbyPlayers(lobby.LobbyID ?? string.Empty, lobby.Players ?? new List<string>());
            await UpdateLobbyStatus(lobby);
            await SendJoinLobbyResponse(stream, lobby);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling join lobby request: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles a request to create a new lobby.
    /// </summary>
    /// <param name="stream">The network stream for communication.</param>
    /// <param name="data">The byte array data received in the request.</param>
    /// <param name="connectionId">The connection ID of the requesting client.</param>
    public async void CreateLobby(NetworkStream stream, byte[] data, string connectionId)
    {
        try
        {
            Console.WriteLine("Create lobby request received.");
            var createLobbyRequest = MessagePackSerializer.Deserialize<CreateLobbyRequest>(data);
            var playerId = _tokenManager.ValidateToken(createLobbyRequest.Token);

            if (playerId == null)
            {
                Console.WriteLine("Invalid token.");
                await SendErrorResponse(stream, "Invalid token.");
                return;
            }

            var newLobby = new Lobby
            {
                Players = new List<string> { playerId },
                Status = Lobby.LobbyStatus.Waiting,
                CreationTime = DateTime.UtcNow,
                MaxPlayers = createLobbyRequest.MaxPlayers
            };
            await lobbyManager.CreateLobby(newLobby);
            await SendCreateLobbyResponse(stream, newLobby);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling create lobby request: {ex.Message}");
        }
    }

    /// <summary>
    /// Finds an existing lobby that is waiting for players or creates a new one.
    /// </summary>
    /// <param name="lobbies">The list of current lobbies.</param>
    /// <param name="playerId">The ID of the player looking for a lobby.</param>
    /// <returns>The found or newly created lobby.</returns>
    private Lobby FindOrCreateLobby(List<Lobby> lobbies, string playerId)
    {
        var waitingLobby = lobbies.FirstOrDefault(l => l.Status == Lobby.LobbyStatus.Waiting);
        if (waitingLobby != null)
        {
            waitingLobby.Players?.Add(playerId);
            return waitingLobby;
        }

        var newLobby = new Lobby
        {
            Players = new List<string> { playerId },
            Status = Lobby.LobbyStatus.Waiting,
            CreationTime = DateTime.UtcNow
        };
        lobbyManager.CreateLobby(newLobby).Wait();
        return newLobby;
    }

    /// <summary>
    /// Updates the status of a given lobby and notifies players if the lobby is full.
    /// </summary>
    /// <param name="lobby">The lobby to update.</param>
    private async Task UpdateLobbyStatus(Lobby lobby)
    {
        bool wasFull = lobby.Status == Lobby.LobbyStatus.Full;
        if (lobby.Players?.Count == lobby.MaxPlayers)
        {
            lobby.Status = Lobby.LobbyStatus.Full;
        }
        else
        {
            lobby.Status = Lobby.LobbyStatus.Waiting;
        }

        // Default status is not persisted
        await lobbyManager.UpdateLobby(lobby.LobbyID ?? string.Empty, lobby.Status ?? Lobby.LobbyStatus.DefaultStatus);

        // Notify players if the lobby has just become full
        if (!wasFull && lobby.Status == Lobby.LobbyStatus.Full)
        {
            var playersInLobby = await RetrievePlayersInLobby(lobby.Players);
            await NotifyPlayersAboutLobbyAssignment(playersInLobby, lobby);
        }
    }

    /// <summary>
    /// Notifies players about their assignment to a lobby, indicating that the game is starting.
    /// </summary>
    /// <param name="players">The list of players to notify.</param>
    /// <param name="lobby">The lobby to which the players have been assigned.</param>
    /// <remarks>
    /// This method iterates through all players in the provided list, 
    /// sending a game start response to each player's associated network stream.
    /// </remarks>
    private async Task NotifyPlayersAboutLobbyAssignment(List<Player> players, Lobby lobby)
    {
        if (players == null || lobby == null)
        {
            Console.WriteLine("Players list or lobby is null.");
            return;
        }

        foreach (var player in players)
        {
            try
            {
                var playerStream = GetPlayerStream(player.PlayerID ?? string.Empty);
                if (playerStream == null)
                {
                    Console.WriteLine($"No stream found for player {player.PlayerID}");
                    continue;
                }

                var gameStartResponese = new GameStartResponese
                {
                    OperationTypeId = (int)OperationType.NotifyGameStart,
                    PlayerCount = lobby.Players?.Count ?? 0,
                };

                var responseData = MessagePackSerializer.Serialize(gameStartResponese);
                await playerStream.WriteAsync(responseData, 0, responseData.Length);
                Console.WriteLine($"Lobby assignment notification sent to player {player.PlayerID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending lobby assignment to player {player.PlayerID}: {ex.Message}");
            }
        }
    }

    private async Task<List<Player>> RetrievePlayersInLobby(List<string>? playerIDs)
    {
        if (playerIDs == null)
        {
            return new List<Player>();
        }

        var players = new List<Player>();
        foreach (var playerID in playerIDs)
        {
            var player = await playerManager.GetPlayer(playerID);
            if (player != null)
            {
                players.Add(player);
            }
        }
        return players;
    }

    private async Task SendJoinLobbyResponse(NetworkStream stream, Lobby lobby)
    {
        var response = new MatchmakingResponse
        {
            OperationTypeId = (int)OperationType.JoinLobbyResponse,
            Success = true,
            LobbyID = lobby.LobbyID,
            PlayerIDs = lobby.Players,
            Status = lobby.Status?.ToString()
        };
        var responseData = MessagePackSerializer.Serialize(response);
        await stream.WriteAsync(responseData, 0, responseData.Length);
        //await SendMessage(stream, response);
    }

    private async Task SendCreateLobbyResponse(NetworkStream stream, Lobby lobby)
    {
        var response = new CreateLobbyResponse
        {
            OperationTypeId = (int)OperationType.CreateLobbyResponse,
            Success = true,
            LobbyID = lobby.LobbyID,
            PlayerIDs = lobby.Players,
            Status = lobby.Status?.ToString()
        };
        var responseData = MessagePackSerializer.Serialize(response);
        await stream.WriteAsync(responseData, 0, responseData.Length);
        //await SendMessage(stream, response);
    }

    private async Task SendErrorResponse(NetworkStream stream, string message)
    {
        var response = new MatchmakingResponse
        {
            OperationTypeId = (int)OperationType.JoinLobbyResponse,
            Success = false,
            ErrorMessage = message
        };
        var responseData = MessagePackSerializer.Serialize(response);
        await stream.WriteAsync(responseData, 0, responseData.Length);
        //await SendMessage(stream, response);
    }

    /// <summary>
    /// Retrieves the network stream associated with a given player ID.
    /// </summary>
    /// <param name="playerId">The ID of the player whose network stream is to be retrieved.</param>
    /// <returns>The network stream for the specified player, or null if no such stream exists.</returns>
    /// <remarks>
    /// This method attempts to find the TcpClient associated with the given player ID 
    /// and return its network stream. If the client or stream cannot be found, null is returned.
    /// </remarks>
    private NetworkStream? GetPlayerStream(string playerId)
    {
        try
        {
            TcpClient? client = _connectionManager.GetConnection(playerId);
            return client?.GetStream();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving stream for player {playerId}: {ex.Message}");
            return null;
        }
    }
}
