namespace Radikoj.Models.ParticipateViewModels
{
    public class ListViewModel
    {
        public string? Message { get; set; }
        public List<WrappedSurvey> Surveys { get; set; }
    }

    public class WrappedSurvey
    {
        public Survey Survey { get; set; }
        public bool HasResponded { get; set; }
    }
}
