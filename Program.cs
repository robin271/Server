using System;
using System.Threading;

namespace ServerForRunningGame
{
    class Program
    {
        static void Main()
        {
            new Thread(new ThreadStart(() => {
                Thread.Sleep(500);
                 bool dot = false;
                while (true)
                {
                    if (dot) Console.Write(".");
                    else Console.Write('\b');
                    dot = !dot;
                    Thread.Sleep(500);
                }
            })).Start();
            Console.Title = "Game Server";
            Matchmaker.Start();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Welcome to my Server!");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();

        }
    }
}
