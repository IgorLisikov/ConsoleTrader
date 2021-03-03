using System;
using System.IO;
using System.Text;


namespace ConsoleTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(140, 45);
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Game game = new Game();
            game.ShowStartDialog();
        }
    }
}
