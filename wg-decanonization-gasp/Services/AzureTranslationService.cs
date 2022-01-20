using Newtonsoft.Json;
using System.Text;

namespace GaspApp.Services
{
    public class AzureTranslationService
    {
        private const string Endpoint = "api.cognitive.microsofttranslator.com";
        private static readonly string To = string.Join("&", LocaleConstants.SUPPORTED_LOCALES_TWOLETTERS.Select(x => $"to={x}"));

        private readonly string _subscriptionKey;
        private readonly string _location;

        public AzureTranslationService(IConfiguration configuration)
        {
            _subscriptionKey = configuration["Azure:TranslationKey"];
            _location = configuration["Azure:TranslationLocation"];
        }

        public async Task<AzureTranslationResult> TranslateStringAsync(string input)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = Endpoint;
            uriBuilder.Path = "/translate";
            uriBuilder.Query = "api-version=3.0&from=en&" + To;

            var requestBody = JsonConvert.SerializeObject(new object[] { new { Text = input } });

            using var client = new HttpClient();
            using var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = uriBuilder.Uri;
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _location);

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<AzureTranslationResult[]>();

            return result![0];
        }
    }

    public class AzureTranslationResult
    {
        public List<AzureTranslationLocalizedItem> Translations { get; set; }
    }

    public class AzureTranslationLocalizedItem
    {
        public string To { get; set; }
        public string Text { get; set; }
    }
}
