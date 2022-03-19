namespace GaspApp.Models.DashboardViewModels
{
    public class DashboardIndexViewModel
    {
        public List<Article> Articles { get; set; }
        public List<WrappedSurvey> Surveys { get; set; }
    }

    public class WrappedSurvey
    {
        public Survey Survey { get; set; }
        public int ResponseCount { get; set; } = 0;
    }
}
