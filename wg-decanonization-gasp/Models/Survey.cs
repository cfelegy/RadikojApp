using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GaspApp.Models
{
    public class Survey
    {
        public Guid Id { get; set; }
        [ValidateNever]
        public List<SurveyItem> Items { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? ActivateDate { get; set; }
        public DateTimeOffset? DeactivateDate { get; set; }

        public bool IsActive(DateTimeOffset? at = null)
        {
            at = at ?? DateTimeOffset.UtcNow;
            var start = ActivateDate ?? DateTimeOffset.MinValue;
            var end = DeactivateDate ?? DateTimeOffset.MaxValue;
            return at > start && at < end;
        }
    }
}
