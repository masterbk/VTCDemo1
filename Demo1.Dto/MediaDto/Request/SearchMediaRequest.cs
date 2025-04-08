using System.Text.Json.Serialization;

namespace Demo1.Dto.MediaDto.Request
{
    public class SearchMediaRequest
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; }
    }
}
