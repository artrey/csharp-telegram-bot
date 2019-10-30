using Newtonsoft.Json;

namespace PrettyBot
{
    public class ApiResult
    {
        [JsonProperty(PropertyName="result")]
        public ApiUpdate[] Result { get; set; }
    }
    public class ApiUpdate
    {
        [JsonProperty(PropertyName="update_id")]
        public int UpdateId { get; set; }
        
        [JsonProperty(PropertyName="message")]
        public ApiMessage Message { get; set; }
    }

    public class ApiMessage
    {
        [JsonProperty(PropertyName="chat")]
        public ApiChat Chat { get; set; }
        
        [JsonProperty(PropertyName="text")]
        public string Text { get; set; }
    }

    public class ApiChat
    {
        [JsonProperty(PropertyName="id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName="first_name")]
        public string FirstName { get; set; }
    }
}
