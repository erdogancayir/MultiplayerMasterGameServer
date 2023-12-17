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

        // SignUpRequest nesnesi oluştur
        var signUpRequest = new SignUpRequest
        {
            OperationTypeId = 103, // SignUpRequest için OperationTypeId
            Username = "exampleUser",
            Email = "user@example.com",
            Password = "securePassword123"
        };

        // Nesneyi serileştir
        byte[] dataToSend = MessagePackSerializer.Serialize(signUpRequest);

        // Veri akışını al ve veriyi gönder
        NetworkStream stream = client.GetStream();
        await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
        Console.WriteLine("SignUpRequest sunucuya gönderildi.");
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
