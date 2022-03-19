using Newtonsoft.Json.Linq;

namespace GaspApp.Models
{
    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public Guid ResponderId { get; set; }
        public Survey Survey { get; set; }
        public string Country { get; set; }
        public string ResponseJson { get; set; }
        public virtual JObject Response => JObject.Parse(ResponseJson);
    }
}
