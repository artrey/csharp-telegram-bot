using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PrettyBot
{
    public class DbEntry
    {
        public string Question { get; }
        public HashSet<string> QuestionWords { get; }
        public SimpleAnswer Answer { get; }

        public DbEntry(string question, SimpleAnswer answer)
        {
            Question = question;
            QuestionWords = ExpertSystem.ToWords(question);
            Answer = answer;
        }
    }
    
    public class ExpertSystem
    {
        private const string NotFoundAnswer = @"К сожалению, не могу ничего ответить на это ¯\_(ツ)_/¯";
        private readonly HashSet<string> _supportedLanguages = new HashSet<string> {
            "ru", "en"
        }; 
        
        public const string DefaultLang = "ru";

        private readonly IList<DbEntry> Db;

        private string _lang;

        public ExpertSystem(string dbFile, string lang = DefaultLang)
        {
            var data = File.ReadAllText(dbFile);
            Db = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(data)
                .Select(kv => new DbEntry(kv.Key, new SimpleAnswer(kv.Value)))
                .Concat(new List<DbEntry>
                {
                    new DbEntry("Сколько времени", new TimeAnswer()),
                    new DbEntry("Какой день", new DateAnswer()),
                    new DbEntry("Какой день недели", new DayOfWeekAnswer()),
                    new DbEntry("Какое время суток", new TimeOfDayAnswer()),
                    new DbEntry("Давай поговорим по-английски", new EnglishAnswer(this)),
                    new DbEntry("Давай снова по-русски", new RussianAnswer(this))
                }).ToList();
            Db.Add(new DbEntry("Что можешь", new AbilityAnswer(Db.Select(e => e.Question))));
            _lang = lang;
        }

        public void ChangeLanguage(string lang)
        {
            lang = lang.ToLower();
            if (!_supportedLanguages.Contains(lang))
            {
                throw new ArgumentException($"Language '{lang}' not supported yet");
            }
            _lang = lang;
        }

        public static HashSet<string> ToWords(string sentence)
        {
            return sentence.Split(new char[0], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => string.Join("", s.ToLower().Where(char.IsLetter))).ToHashSet();
        }
        
        public string GetAnswer(string question)
        {
            var answers = new List<string>();

            question = question.ToLower();
            var questionWords = ToWords(question);
            
            foreach (var entry in Db)
            {
                if (!entry.QuestionWords.IsSubsetOf(questionWords)) continue;

                // try to find answer for specified language
                if (!entry.Answer.TryGetValue(_lang, out var answer))
                {
                    // not found, if was not default language - try to find answer for default language
                    if (_lang != DefaultLang)
                    {
                        entry.Answer.TryGetValue(DefaultLang, out answer);
                    }
                }

                // if found
                if (!string.IsNullOrEmpty(answer))
                {
                    answers.Add(answer);
                }
            }

            if (answers.Count == 0)
            {
                answers.Add(NotFoundAnswer);
            }

            return string.Join(". ", answers);
        }
    }
}
