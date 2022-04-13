using Markdig;

namespace Radikoj.Models
{
    public class ArticleContent
    {
        public Guid Id { get; set; }
        public string Culture { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public string Synopsis(int chars)
		{
            TextWriter writer = new StringWriter();
            Markdown.ToPlainText(Body, writer);
            var plainBody = writer.ToString();
            if (plainBody!.Length < chars)
                return plainBody;
            else
                return plainBody.Substring(0, chars) + "...";
		}
    }
}
