using System.Net.Sockets;
using Amazon.Runtime.Internal.Util;
using MessagePack;

public class AuthService
{
    private readonly PlayerManager _playerManager;
    private readonly LogManager _logManager;
    private readonly ConnectionManager _connectionManager;

    /// <summary>
    /// Initializes a new instance of the AuthService class.
    /// </summary>
    /// <param name="playerManager">The PlayerManager instance for handling player-related operations.</param>
    /// <param name="logManager">The LogManager for logging activities.</param>
    public AuthService(PlayerManager playerManager, LogManager logManager, ConnectionManager connectionManager)
    {
        _playerManager = playerManager;
        _logManager = logManager;
        _connectionManager = connectionManager;
    }


    /// <summary>
    /// Handles the sign-up request by deserializing the data and creating a new player.
    /// </summary>
    /// <param name="data">The byte array containing the serialized sign-up request data.</param>
    /// <param name="bytesRead">The number of bytes read from the stream.</param>
    public async void HandleSignUpRequest(NetworkStream clientStream, byte[] data, int connectionId)
    {
        try
        {
            Console.WriteLine("Sign up request received.");
            var signUpRequest = MessagePackSerializer.Deserialize<SignUpRequest>(data);
            var response = new SignUpResponse
            {
                OperationTypeId = (int)OperationType.SignUpResponse
            };

            if (!await _playerManager.IsUsernameAvailable(signUpRequest.Username))
            {
                response.Success = false;
                response.Message = "Username already taken";
                Console.WriteLine(response.Message);
            }
            else
            {
                var playerCreated = await _playerManager.CreatePlayerAsync(signUpRequest.Username, signUpRequest.Password);
                if (playerCreated)
                {
                    response.Success = true;
                    response.Message = "Player successfully created.";
                    var playerId = await _playerManager.GetPlayerIdByUsername(signUpRequest.Username);
                    // playerId artık int türünde olduğu için doğrudan kullanılıyor
                    _connectionManager.UpdateConnectionId(connectionId, playerId);
                    Console.WriteLine(response.Message);
                }
                else
                {
                    response.Success = false;
                    response.Message = "Failed to create player.";
                    Console.WriteLine(response.Message);
                }
            }
            var responseData = MessagePackSerializer.Serialize(response);
            await clientStream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing sign up request: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the login request by deserializing the data and authenticating the player.
    /// </summary>
    /// <param name="clientStream">The network stream to communicate with the client.</param>
    /// <param name="data">The byte array containing the serialized login request data.</param>
    /// <param name="bytesRead">The number of bytes read from the stream.</param>
    public async void HandleLoginRequest(NetworkStream clientStream, byte[] data, int connectionId)
    {
        try
        {
            Console.WriteLine("Login request received.");
            var loginRequest = MessagePackSerializer.Deserialize<AuthenticationRequest>(data);
            var response = new AuthenticationResponse
            {
                OperationTypeId = (int)OperationType.LoginResponse
            };

            var player = await _playerManager.GetPlayerByUsernameAsync(loginRequest.Username);
            if (player != null && _playerManager.ValidatePassword(player.PasswordHash, loginRequest.Password))
            {
                response.Success = true;
                response.Token = _playerManager.GenerateToken(player);
                response.Message = "Login successful.";
                // Burada doğrudan player.PlayerID kullanılıyor
                _connectionManager.UpdateConnectionId(connectionId, player.PlayerID);
                Console.WriteLine(response.Message);
                await _logManager.CreateLogAsync("Info", $"Player logged in: {loginRequest.Username}", player.PlayerID);
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = "Invalid username or password.";
                Console.WriteLine(response.ErrorMessage);
            }

            var responseData = MessagePackSerializer.Serialize(response);
            await clientStream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing login request: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the logout request by deserializing the data and invalidating the player's token.
    /// </summary>
    /// <param name="clientStream"></param>
    /// <param name="data"></param>
    /// <param name="connectionId"></param>
    public async void HandleLogoutRequest(NetworkStream clientStream, byte[] data, int connectionId)
    {
        try
        {
            Console.WriteLine("Logout request received.");
            var logoutRequest = MessagePackSerializer.Deserialize<LogoutRequest>(data);
            var response = new LogoutResponse
            {
                OperationTypeId = (int)OperationType.LogoutResponse
            };

            var playerId = _playerManager.PlayerValidateToken(logoutRequest.Token ?? "");

            if (playerId.HasValue && playerId != 0)
            {
                response.Success = true;
                response.Message = "Logout successful.";
                _connectionManager.UpdateConnectionId(connectionId, playerId.Value);
                _connectionManager.RemoveConnectionById(playerId.Value);
                await _logManager.CreateLogAsync("Info", $"Player logged out: {playerId}", playerId.Value);
                Console.WriteLine($"Token invalidated for user ID: {playerId}");
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = "Invalid token.";
                Console.WriteLine("Invalid token.");
            }

            Console.WriteLine($"token : {logoutRequest.Token}");
            var responseData = MessagePackSerializer.Serialize(response);
            await clientStream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing logout request: {ex.Message}");
            // Error handling logic
        }
    }


}
