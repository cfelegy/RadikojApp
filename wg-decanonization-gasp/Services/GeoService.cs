using GaspApp.Models;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace GaspApp.Services
{
    public class GeoService
    {
        private readonly FeatureCollection _rawFeatureCollection;
        private Dictionary<string, Feature> _featuresByIsoName;
        private Dictionary<string, string> _isoToCountryName;

        public GeoService()
        {
            var worldGeoJson = File.ReadAllText("WorldGeoJson.json");
            _rawFeatureCollection = JsonConvert.DeserializeObject<FeatureCollection>(worldGeoJson)!;
            _featuresByIsoName = new Dictionary<string, Feature>();

            foreach (var feature in _rawFeatureCollection.Features)
            {
                var name = (string)feature.Properties["ISO_A3"];
                _featuresByIsoName[name] = feature;
            }

            _isoToCountryName = new Dictionary<string, string>();
            var countryNameJson = File.ReadAllText("wwwroot/js/Countries.json");
            var countryNames = JsonConvert.DeserializeObject<CountryJsonEntry[]>(countryNameJson)!;
            foreach (var countryName in countryNames)
			{
                _isoToCountryName[countryName.Abbreviation] = countryName.Country;
			}
        }

        public string GenerateGeoJson(List<SurveyResponse> responses)
        {
            var countByCountry = responses
                .GroupBy(x => x.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToList();
            var features = new List<Feature>();
            
            foreach (var group in countByCountry)
            {
                if (group.Country == "null")
                    continue;

                var feature = _featuresByIsoName[group.Country];
                feature.Properties["Count"] = group.Count;
                features.Add(feature);
            }

            var featureCollection = new FeatureCollection(features);
            return JsonConvert.SerializeObject(featureCollection);
        }

        public string GetCountryName(string iso)
		{
            if (iso == "null") return "";
            if (!_isoToCountryName.TryGetValue(iso, out var ret)) return "";
            return ret;
		}

        private class CountryJsonEntry
		{
            public string Abbreviation { get; set; }
            public string Country { get; set; }
		}
    }
}
