using System;
using System.Net.Sockets;
using System.Text;

namespace TCPClient
{
    public class TCPClient
    {
        public static void Main(string[] args)
        {
            try
            {
                Int32 port = 13000;
                TcpClient client = new TcpClient("127.0.0.1", port);

                NetworkStream stream = client.GetStream();

                // Sunucuya gönderilecek veri
                string message = "Merhaba Sunucu!";
                byte[] dataToSend = Encoding.ASCII.GetBytes(message);
                stream.Write(dataToSend, 0, dataToSend.Length);
                Console.WriteLine("Gönderilen Veri: {0}", message);

                // Sunucudan gelen veriyi oku
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                Console.WriteLine("Alınan Veri: {0}", response);

                // Bağlantıyı kapat
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Hata: " + e.Message);
                throw;
            }
        }
    }
}