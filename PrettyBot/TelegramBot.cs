using System.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace PrettyBot
{
    public class TelegramBot
    {
        private readonly RestClient _client = new RestClient();
        private readonly string _baseUrl;
        private int _lastUpdateId = 0;
        
        public TelegramBot(string token)
        {
            _baseUrl = $"https://api.telegram.org/bot{token}/";
        }

        public void SendMessage(int chatId, string text)
        {
            SendApiRequest("sendMessage", $"chat_id={chatId}&text={text}");
        }

        public ApiUpdate[] GetUpdates()
        {
            var json = SendApiRequest("getUpdates", $"offset={_lastUpdateId}");
            var updates = JsonConvert.DeserializeObject<ApiResult>(json).Result;
            var last = updates.LastOrDefault();
            if (last != null)
            {
                _lastUpdateId = last.UpdateId + 1;
            }
            return updates;
        }

        private string SendApiRequest(string method, string parameters)
        {
            var url = $"{_baseUrl}{method}?{parameters}";
            var req = new RestRequest(url);
            return _client.Get(req).Content;
        }
    }
}
