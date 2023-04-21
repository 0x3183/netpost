using Microsoft.VisualStudio.Threading;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace netpost
{
    public static class Log
    {
        private static readonly FileStream? FS = null;
        private static readonly object Lock = new();
        static Log()
        {
            try
            {
                FS = File.Open("log.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                WaitForExitAsync().Forget();
            }
            catch(Exception ex)
            {
                Warning("Error while initializing logging, logs will not be saved!");
                Warning(ex.ToString());
            }
            Info("Initialized logging.");
        }
        private static async Task WaitForExitAsync()
        {
            try
            {
                await Task.Delay(-1);
            }
            finally
            {
                FS!.Close();
            }
        }
        private static void Any(string text, ConsoleColor color)
        {
            lock (Lock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                FS?.Write(Encoding.UTF8.GetBytes($"{text}\n"));
            }
        }
        public static void Info(string text)
        {
            Any($"[INF] {text}", ConsoleColor.Gray);
        }
        public static void Warning(string text)
        {
            Any($"[WRN] {text}", ConsoleColor.Gray);
        }
        public static void Error(string text)
        {
            Any($"[ERR] {text}", ConsoleColor.Gray);
        }
    }
}
