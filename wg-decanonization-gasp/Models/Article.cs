using Microsoft.EntityFrameworkCore;

namespace GaspApp.Models
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Article
    {
        public Guid Id {  get; set; }
        public Account Author { get; set; }
        public DateTimeOffset PublishedDate {  get; set; }
        // Title for editor
        public string Slug { get; set; }
        public List<ArticleContent> Contents { get; set; }
    }
}
