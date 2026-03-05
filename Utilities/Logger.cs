using System;
using System.Collections.Generic;
using System.Text;

namespace CoelhoBot.Utilities
{
    public static class Logger
    {
        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Erro] {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        public static void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[Bot-Info] {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
