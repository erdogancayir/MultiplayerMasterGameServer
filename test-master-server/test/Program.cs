using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

public class TcpClientExample
{
    public static async Task Main(string[] args)
    {
        string server = "127.0.0.1"; // Master Server IP
        int port = 8080; // Master Server Port

        var client = new TcpClient();

        try
        {
            await client.ConnectAsync(server, port);

            var signUpRequest = new SignUpRequest
            {
                OperationTypeId = (int)OperationType.SignUpRequest,
                Username = "efwefewfewef324 rgreg",
                Password = "strongPassword"
            };

            byte[] dataToSend = MessagePackSerializer.Serialize(signUpRequest);
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            Console.WriteLine("SignUpRequest sent to server.");

            // Sunucudan yanıtı okuma
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                // Yanıtı deserialize etme
                var response = MessagePackSerializer.Deserialize<SignUpResponse>(buffer);
                Console.WriteLine($"Response: Success = {response.Success}, Message = {response.Message}");
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
}
