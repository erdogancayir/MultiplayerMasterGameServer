using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPServer
{
    public class TCPServer
    {
        public static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(localAddr, port);
                server.Start();
                while (true)
                {
                    Console.WriteLine("Bağlantı Bekleniyor...");
                    if (server != null)
                    {
                        TcpClient client = server.AcceptTcpClient();
                    
                    Console.WriteLine("Bağlantı Sağlandı!");
                    NetworkStream stream = client.GetStream();
                    Byte[] bytes = new Byte[256];
                    String data = null;
                    Int32 i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Alınan Veri: {0}", data);
                        data = data.ToUpper();
                        Byte[] msg = Encoding.ASCII.GetBytes(data);
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Gönderilen Veri: {0}", data);
                    }
                    client.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                server?.Stop();
            }
        }
    }
}