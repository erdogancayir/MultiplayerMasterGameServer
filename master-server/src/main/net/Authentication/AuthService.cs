using System.Net.Sockets;
using MessagePack;

public interface IAuthService
{
    void HandleLoginRequest(NetworkStream clientStream, byte[] data, int bytesRead);
    void HandleLogoutRequest(NetworkStream clientStream,byte[] data, int bytesRead);
    void HandleSignUpRequest(NetworkStream clientStream, byte[] data, int bytesRead);
    // Diğer metodlar...
}

public class AuthService : IAuthService
{
    private readonly PlayerManager _playerManager;

    /// <summary>
    /// Initializes a new instance of the AuthService class.
    /// </summary>
    /// <param name="playerManager">The PlayerManager instance for handling player-related operations.</param>
    public AuthService(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }


    /// <summary>
    /// Handles the sign-up request by deserializing the data and creating a new player.
    /// </summary>
    /// <param name="data">The byte array containing the serialized sign-up request data.</param>
    /// <param name="bytesRead">The number of bytes read from the stream.</param>
    public async void HandleSignUpRequest(NetworkStream clientStream, byte[] data, int bytesRead)
    {
        try
        {
            // Deserialize the received data into a SignUpRequest object
            var signUpRequest = MessagePackSerializer.Deserialize<SignUpRequest>(data);
            var response = new SignUpResponse();
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
    public void HandleLoginRequest(NetworkStream clientStream, byte[] data, int bytesRead)
    {
        // Process login request
        try
        {
            var authenticationRequest = MessagePackSerializer.Deserialize<AuthenticationRequest>(data);
            var response = new AuthenticationResponse();

            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing login request: {ex.Message}");
        }

    }

    public void HandleLogoutRequest(NetworkStream clientStream, byte[] data, int bytesRead)
    {
        // Process logout request
    }
}
