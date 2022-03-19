namespace GaspApp.Models
{
    public class SurveyItem
    {
        public Guid Id { get; set; }

        public int Position { get; set; }
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public SurveyItemType ItemType { get; set; }
        public string ItemContents { get; set; } = ""; // list delimited by ;;

        public IEnumerable<string> ParseContents()
            => ItemContents.Split(";;");
    }
}
