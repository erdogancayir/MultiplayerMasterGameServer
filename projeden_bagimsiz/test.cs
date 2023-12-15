public class Program
{
    public static event EventHandler<string>? HelloWorldEvent;
     private static void HelloWorldHandler(object? sender, string e)
    {
        Console.WriteLine(e);
    }
    public static void Main(string[] args)
    {
        // Olaya bir işleyici ekleniyor.
        HelloWorldEvent += HelloWorldHandler;

        // Olay tetikleniyor.
        HelloWorldEvent?.Invoke(null, "Hello World!");
    }
}