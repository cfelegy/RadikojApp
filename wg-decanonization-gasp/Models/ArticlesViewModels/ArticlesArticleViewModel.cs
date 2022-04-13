namespace Radikoj.Models.ArticlesViewModels
{
	public class ArticlesArticleViewModel
	{
		public Article Article { get; set; }
		public ArticleContent Content { get; set; }
		public bool Fallback { get; set; } = false;
	}
}
