namespace GaspApp.Models
{
    public class Survey
    {
        public Guid Id { get; set; }
        public IList<SurveyItem> Items { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? ActivateDate { get; set; }
        public DateTimeOffset? DeactivateDate { get; set; }
    }
}
