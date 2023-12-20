using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

public class TcpClientExample
{
    private static async Task SendRequest(OperationType operationType, string token = "")
    {
        string server = "127.0.0.1"; // Master Server IP
        int port = 8080; // Master Server Port

        var client = new TcpClient();

        try
        {
            await client.ConnectAsync(server, port);

            byte[] dataToSend;
            switch (operationType)
            {
                case OperationType.SignUpRequest:
                    var signUpRequest = new SignUpRequest
                    {
                        OperationTypeId = (int)operationType,
                        Username = "1",
                        Password = "22"
                    };
                    dataToSend = MessagePackSerializer.Serialize(signUpRequest);
                    break;

                case OperationType.LoginRequest:
                    var authenticationRequest = new AuthenticationRequest
                    {
                        OperationTypeId = (int)operationType,
                        Username = "1",
                        Password = "22"
                    };
                    dataToSend = MessagePackSerializer.Serialize(authenticationRequest);
                    break;
                case OperationType.LogoutRequest:
                    var logoutRequest = new LogoutRequest
                    {
                        OperationTypeId = (int)operationType,
                        Token = token
                    };
                    dataToSend = MessagePackSerializer.Serialize(logoutRequest);
                    break;
                case OperationType.JoinLobbyRequest:
                    var joinLobbyRequest = new MatchmakingRequest
                    {
                        OperationTypeId = (int)operationType,
                        Token = token
                    };
                    dataToSend = MessagePackSerializer.Serialize(joinLobbyRequest);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported operation type.");
            }

            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            Console.WriteLine($"{operationType} sent to server.");

            // Sunucudan yanıtı okuma ve işleme
            byte[] buffer = new byte[1024];
            string? _token = null;
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                try
                {
                    // Deserialize the data into a BasePack object to extract the OperationType.
                    var basePack = MessagePackSerializer.Deserialize<BasePack>(buffer);
                    OperationType operationTypeResponse = (OperationType)basePack.OperationTypeId;
                    Console.WriteLine($"OperationType: {operationTypeResponse}");
        
                    switch (operationTypeResponse)
                    {
                        case OperationType.SignUpResponse: // Corrected enum access
                            var signUpResponse = MessagePackSerializer.Deserialize<SignUpResponse>(buffer);
                            Console.WriteLine($"SignUp Response: Success = {signUpResponse.Success}, Message = {signUpResponse.Message}");
                            break;
                        case OperationType.LoginResponse:
                            var authenticationResponse = MessagePackSerializer.Deserialize<AuthenticationResponse>(buffer);
                            _token = authenticationResponse.Token;
                            Console.WriteLine($"Login Response: Token = {authenticationResponse.Token}, ErrorMessage = {authenticationResponse.ErrorMessage}");
                            break;
                        case OperationType.LogoutResponse:
                            var logoutResponse = MessagePackSerializer.Deserialize<LogoutResponse>(buffer);
                            Console.WriteLine($"Logout Response: Success = {logoutResponse.Success}, ErrorMessage = {logoutResponse.ErrorMessage}");
                            break;
                        case OperationType.JoinLobbyResponse:
                            var res1 = MessagePackSerializer.Deserialize<MatchmakingResponse>(buffer);
                            Console.WriteLine($"MatchmakingResponse: Success = {res1.Success}, ErrorMessage = {res1.ErrorMessage}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing operation type: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    public static async Task Main(string[] args)
    {
        //await SendRequest(OperationType.SignUpRequest);
        //await SendRequest(OperationType.LoginRequest);
        await SendRequest(OperationType.JoinLobbyRequest, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI2NTgzMzEzMGFmNjZlNjM1NmVjMmEyM2EiLCJuYmYiOjE3MDMxMDMwMTQsImV4cCI6MTcwMzcwNzgxNCwiaWF0IjoxNzAzMTAzMDE0fQ.7meebvw5dVuqz9WwHpLLsXzJQJydpRrdHbQDs2zh2GQ");
        //await SendRequest(OperationType.LogoutRequest, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI2NTgzMzEzMGFmNjZlNjM1NmVjMmEyM2EiLCJuYmYiOjE3MDMwOTY5ODksImV4cCI6MTcwMzcwMTc4OSwiaWF0IjoxNzAzMDk2OTg5fQ.c2pPzrw0ID2c7LRA5o-HYSqKMdglLC_M4W0bXPLFK-k");
    }
}
