namespace Ex
{
    class Program
    {
        private static readonly object _lock = new object();
       //static void Main()
       //{
       //    Thread thread1 = new Thread(new ParameterizedThreadStart(func));
       //    Thread thread2 = new Thread(new ParameterizedThreadStart(func));
       //    thread1.Start("fkdnt");
       //    thread2.Start("new");
       //}

        static void func(object? obj)
        {
            lock (_lock)
            {
                Console.WriteLine(obj ?? "null");
            }
        }
    }
}

