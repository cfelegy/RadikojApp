namespace GaspApp.Models
{
    public class LocalizedItem
    {
        public Guid Id { get; set; }
        public string CultureCode { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public bool Automatic { get; set; } = false;
    }
}
