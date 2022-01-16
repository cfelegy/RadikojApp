namespace GaspApp.Models
{
    public class Survey
    {
        public Guid Id { get; set; }
        public IList<SurveyItem> Items { get; set; }
    }
}
