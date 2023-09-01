#define DEBUG

using log4net;
using log4net.Config;

[assembly: XmlConfigurator]


namespace ProstirTgBot
{
    internal class Program
    {
        static Data.ProstirTgBotContext _context = new();
        static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.InputEncoding = System.Text.Encoding.Unicode;
            BasicConfigurator.Configure();

            Console.WriteLine("started. If no messages after, logger haven't initialized");
            Log.Info("logger initialized");
            Console.WriteLine($"is debug enabled: {Log.IsDebugEnabled}");
            Console.WriteLine($"is info enabled: {Log.IsInfoEnabled}");
            Console.WriteLine($"is warn enabled: {Log.IsWarnEnabled}");
            Console.WriteLine($"is error enabled: {Log.IsErrorEnabled}");
            Console.WriteLine($"is fatal enabled: {Log.IsFatalEnabled}");

            string dotEnv = File.ReadAllText(".env");
            DotNetEnv.Env.Load(".env");

            var tg = new TelegramHandler();
            await tg.Init(_context);

            while (true)
            {
                Thread.Sleep(5);
            }
        }
    }
}