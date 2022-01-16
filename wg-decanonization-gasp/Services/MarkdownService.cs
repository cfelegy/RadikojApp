using Markdig;
using Microsoft.AspNetCore.Html;

namespace GaspApp.Services
{
	public static class MarkdownService
	{
		public static HtmlString CreateHtmlString(string markdown)
			=> new HtmlString(Markdown.ToHtml(markdown));
	}
}
