namespace GaspApp.Models.DashboardViewModels
{
    public class TranslationsViewModel
    {
        public string ActiveGroup { get; set; }
        public List<string> Groups { get; set; }
        public List<TranslationsLocalizedItem> Items { get; set; }
    }
}
