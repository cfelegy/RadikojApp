namespace GaspApp.Models.ParticipateViewModels
{
	public class MapViewModel
	{
		public int TotalCountries { get; set; }
		public int TotalResponses { get; set; }
		public List<string> LocationCodes { get; set; }
		public List<string> LocationNames { get; set; }
		public List<int> Values { get; set; }
	}
}
