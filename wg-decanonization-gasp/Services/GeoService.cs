using GaspApp.Models;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace GaspApp.Services
{
    public class GeoService
    {
        private readonly FeatureCollection _rawFeatureCollection;
        private Dictionary<string, Feature> _featuresByIsoName;

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
    }
}
