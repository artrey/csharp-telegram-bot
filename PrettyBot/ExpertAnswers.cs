using System;
using System.Collections.Generic;
using System.Globalization;

using LangData = System.Collections.Generic.Dictionary<string, string>;

namespace PrettyBot
{
    public class SimpleAnswer
    {
        private readonly LangData _data;
        
        public SimpleAnswer(LangData data = null)
        {
            _data = data ?? new LangData
            {
                {"ru", string.Empty},
                {"en", string.Empty}
            };
        }

        public virtual bool TryGetValue(string lang, out string answer)
        {
            return _data.TryGetValue(lang, out answer);
        }
    }

    public abstract class InteractiveAnswer : SimpleAnswer
    {
        protected InteractiveAnswer(LangData data) : base(data) {}

        protected abstract string PostProcess(string answer, string lang);
        
        public override bool TryGetValue(string lang, out string answer)
        {
            if (!base.TryGetValue(lang, out answer)) return false;
            answer = PostProcess(answer, lang);
            return true;
        }
    }

    public class TimeAnswer : InteractiveAnswer
    {
        public TimeAnswer() : base(new LangData
        {
            {"ru", "Текущее время: {0}"},
            {"en", "Current time: {0}"}
        })
        {}

        protected override string PostProcess(string answer, string lang) 
            => string.Format(answer, DateTime.Now.ToString("HH:mm:ss"));
    }

    public class DateAnswer : InteractiveAnswer
    {
        public DateAnswer() : base(new LangData
        {
            {"ru", "Сегодня у нас на дворе {0}"},
            {"en", "Today is {0}"}
        })
        {}

        protected override string PostProcess(string answer, string lang) 
            => string.Format(answer, DateTime.Now.ToString("dd.MM.yyyy"));
    }

    public class DayOfWeekAnswer : InteractiveAnswer
    {
        public DayOfWeekAnswer() : base(new LangData
        {
            {"ru", "Сегодня {0}"},
            {"en", "Today is {0}"}
        })
        {}

        protected override string PostProcess(string answer, string lang)
        {
            var culture = new CultureInfo($"{lang}-{lang.ToUpper()}");
            var day = culture.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek);
            return string.Format(answer, day);
        }
    }

    public class TimeOfDayAnswer : InteractiveAnswer
    {
        public TimeOfDayAnswer() : base(new LangData
        {
            {"ru", "Ночь,Утро,День,Вечер"},
            {"en", "Night,Morning,Day,Evening"}
        })
        {}

        protected override string PostProcess(string answer, string lang)
        {
            // 23-4: night
            // 5-10: morning
            // 11-16: day
            // 17-22: evening
            var quoter = (DateTime.Now.TimeOfDay.Hours + 1) / 6 % 4;
            return answer.Split(',')[quoter];
        }
    }

    public abstract class LanguageAnswer : InteractiveAnswer
    {
        protected readonly ExpertSystem ExpertSystem;

        protected LanguageAnswer(LangData data, ExpertSystem expertSystem) : base(data)
        {
            ExpertSystem = expertSystem;
        }
    }
    
    public class EnglishAnswer : LanguageAnswer
    {
        public EnglishAnswer(ExpertSystem expertSystem) : base(null, expertSystem) {}

        protected override string PostProcess(string answer, string lang)
        {
            ExpertSystem.ChangeLanguage("en");
            return "Ok, let’s do it";
        }
    }
    
    public class RussianAnswer : LanguageAnswer
    {
        public RussianAnswer(ExpertSystem expertSystem) : base(null, expertSystem) {}

        protected override string PostProcess(string answer, string lang)
        {
            ExpertSystem.ChangeLanguage("ru");
            return "Без проблем";
        }
    }

    public class AbilityAnswer : InteractiveAnswer
    {
        private readonly IEnumerable<string> _questions;
        
        public AbilityAnswer(IEnumerable<string> questions) : base(null)
        {
            _questions = questions;
        }

        protected override string PostProcess(string answer, string lang)
        {
            return "Я могу ответить на такие вопросы и прокомментировать утверждения:" +
                   Environment.NewLine + Environment.NewLine +
                   string.Join(Environment.NewLine, _questions);
        }
    }
}
