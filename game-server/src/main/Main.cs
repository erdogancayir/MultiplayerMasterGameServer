using dotenv.net;

namespace GameServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                DotEnv.Load();
                Console.WriteLine("Starting Game Server...");
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
    }
}