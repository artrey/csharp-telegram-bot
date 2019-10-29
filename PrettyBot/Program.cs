using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

using LangData = System.Collections.Generic.Dictionary<string, string>;

namespace PrettyBot
{
    class Program
    {
        private const string DefaultLang = "ru";
        private const string NotFoundAnswer = @"К сожалению, не могу ничего ответить на это ¯\_(ツ)_/¯";
        
        private static Dictionary<string, LangData> _db;

        private static string _lang = DefaultLang;
        
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Provide bot token!");
                return;
            }

            var token = args[0];
            var bot = new TelegramBot(token);

            var data = File.ReadAllText("database.json");
            _db = JsonConvert.DeserializeObject<Dictionary<string, LangData>>(data)
                .ToDictionary(kv => kv.Key.ToLower(), kv => kv.Value);

            while (true)
            {
                var updates = bot.GetUpdates();
                
                foreach (var update in updates)
                {
                    var answer = GetAnswer(update.Message.Text.ToLower(), _lang);
                    bot.SendMessage(update.Message.Chat.Id, answer);
                }
            }
        }

        static string GetAnswer(string question, string lang = DefaultLang)
        {
            var answers = new List<string>();
            
            foreach (var (q, a) in _db)
            {
                if (!question.Contains(q)) continue;
                
                // try to find answer for specified language
                if (!a.TryGetValue(lang, out var answer))
                {
                    // not found, if was not default language - try to find answer for default language
                    if (lang != DefaultLang)
                    {
                        a.TryGetValue(DefaultLang, out answer);
                    }
                }

                // if found
                if (!string.IsNullOrEmpty(answer))
                {
                    answers.Add(answer);
                }
            }

            if (question.Contains("сколько времени"))
            {
                answers.Add(DateTime.Now.ToString("HH:mm:ss"));
            }

            if (question.Contains("какой день"))
            {
                answers.Add(DateTime.Now.ToString("dd.MM.yyyy"));
            }

            if (answers.Count == 0)
            {
                answers.Add(NotFoundAnswer);
            }

            return string.Join(", ", answers);
        }
    }
}
