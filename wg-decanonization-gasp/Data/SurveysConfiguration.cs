using GaspApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace GaspApp.Data
{
    public class SurveysConfiguration : IEntityTypeConfiguration<Survey>
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        public void Configure(EntityTypeBuilder<Survey> builder)
        {
            builder.Property(e => e.Items).HasConversion(
                v => JsonConvert.SerializeObject(v, _serializerSettings),
                v => JsonConvert.DeserializeObject<IList<SurveyItem>>(v, _serializerSettings)!
            );
        }
    }
}
