using System;

namespace PrettyBot
{
    class Program
    {
        
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Provide bot token!");
                return;
            }

            var token = args[0];
            var bot = new TelegramBot(token);

            var expertSystem = new ExpertSystem("database.json");

            while (true)
            {
                var updates = bot.GetUpdates();
                
                foreach (var update in updates)
                {
                    var answer = expertSystem.GetAnswer(update.Message.Text);
                    bot.SendMessage(update.Message.Chat.Id, answer);
                }
            }
        }
    }
}
