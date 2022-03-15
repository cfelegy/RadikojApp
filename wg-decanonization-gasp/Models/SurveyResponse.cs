namespace GaspApp.Models
{
    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public Survey Survey { get; set; }
        public string Country { get; set; }
        public string ResponseJson { get; set; }
    }
}
