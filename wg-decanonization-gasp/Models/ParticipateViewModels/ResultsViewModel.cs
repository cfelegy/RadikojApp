namespace GaspApp.Models.ParticipateViewModels
{
    public class ResultsViewModel
    {
        public Survey Survey { get; set; }
        public int TotalResponses { get; set; }
        public int UniqueCountries { get; set; }
        public List<QuestionResult> Questions { get; set; }
    }

    public class QuestionResult
    {
        public int Position { get; set; }
        public string Label { get; set; } = "";
        public bool IsFreeResponse { get; set; }
        public List<FreeResponseAnswer>? FreeResponses { get; set; }
        public Dictionary<string, int> FixedResponses { get; set; }
    }

    public class FreeResponseAnswer
    {
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
    }
}
