using Microsoft.EntityFrameworkCore;

namespace Radikoj.Models
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

        public virtual bool IsPublished(DateTimeOffset? at = null)
        {
            if (at == null)
                at = DateTimeOffset.UtcNow;

            return at >= PublishedDate;
        }
        public virtual bool IsSpecialPage()
		{
            return Slug == "[home]";
		}
    }
}
