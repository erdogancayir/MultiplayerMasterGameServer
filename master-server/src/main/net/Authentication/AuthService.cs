using System.Net.Sockets;
using Amazon.Runtime.Internal.Util;
using MessagePack;

public interface IAuthService
{
    void HandleLoginRequest(NetworkStream clientStream, byte[] data, string connectionId);
    void HandleLogoutRequest(NetworkStream clientStream,byte[] data, string connectionId);
    void HandleSignUpRequest(NetworkStream clientStream, byte[] data, string connectionId);

}

public class AuthService : IAuthService
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
    public async void HandleSignUpRequest(NetworkStream clientStream, byte[] data, string connectionId)
    {
        try
        {
            Console.WriteLine("Sign up request received.");
            // Deserialize the received data into a SignUpRequest object
            var signUpRequest = MessagePackSerializer.Deserialize<SignUpRequest>(data);
            var response = new SignUpResponse();
            response.OperationTypeId = (int)OperationType.SignUpResponse;
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

            // Serialize the response object and send it back to the client
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
    public async void HandleLoginRequest(NetworkStream clientStream, byte[] data, string connectionId)
    {
        try
        {
            Console.WriteLine("Login request received.");
            // Deserialize the received data into an AuthenticationRequest object
            var loginRequest = MessagePackSerializer.Deserialize<AuthenticationRequest>(data);
            var response = new AuthenticationResponse();
            response.OperationTypeId = (int)OperationType.LoginResponse;
            // Validate the username and password
            var player = await _playerManager.GetPlayerByUsernameAsync(loginRequest.Username);
            if (player != null && _playerManager.ValidatePassword(player.PasswordHash, loginRequest.Password))
            {
                response.Success = true;
                response.Token = _playerManager.GenerateToken(player);
                response.Message = "Login successful.";
                Console.WriteLine(response.Message);
                await _logManager.CreateLogAsync("Info", $"Player logged in: {loginRequest.Username}", player.PlayerID);
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = "Invalid username or password.";
                Console.WriteLine(response.ErrorMessage);
            }

            // Serialize the response object and send it back to the client
            var responseData = MessagePackSerializer.Serialize(response);
            await clientStream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing login request: {ex.Message}");
        }
    }

    public async void HandleLogoutRequest(NetworkStream clientStream, byte[] data, string connectionId)
    {
        try
        {
            Console.WriteLine("Logout request received.");
            // Deserialize the received data into a LogoutRequest object
            var logoutRequest = MessagePackSerializer.Deserialize<LogoutRequest>(data);
            var response = new LogoutResponse();
            response.OperationTypeId = (int)OperationType.LogoutResponse;
            // Assuming you have a method in PlayerManager to get the player's ID by username
            var player = await _playerManager.GetPlayerByUsernameAsync(logoutRequest.Username);
            if (player != null)
            {
                // Invalidate the token
                _playerManager.InvalidateToken(player.PlayerID ?? string.Empty);
                response.Success = true;
                response.Message = "Logout successful.";
                Console.WriteLine($"Token invalidated for user: {logoutRequest.Username}");
                // You can also send a response back to the client if needed
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = "Invalid username.";
                Console.WriteLine("Invalid username.");
                // Handle the case where the username is not found
            }

            // Serialize the response object and send it back to the client
            var responseData = MessagePackSerializer.Serialize(response);
            await clientStream.WriteAsync(responseData, 0, responseData.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing logout request: {ex.Message}");
            // Handle exceptions and possibly send an error response to the client
        }
    }

}
