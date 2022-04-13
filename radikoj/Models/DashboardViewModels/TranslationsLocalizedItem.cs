namespace Radikoj.Models.DashboardViewModels
{
    public class TranslationsLocalizedItem
    {
        public string Key { get; set; }
        // <two letter culture code, value>
        public Dictionary<string, string> Values { get; set; }
    }
}

