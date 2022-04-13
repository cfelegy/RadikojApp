using Microsoft.AspNetCore.Html;
using Markdig;

namespace Radikoj.Services
{
	public static class MarkdownService
	{
		public static HtmlString CreateHtmlString(string markdown)
			=> new HtmlString(Markdown.ToHtml(markdown));
	}
}
