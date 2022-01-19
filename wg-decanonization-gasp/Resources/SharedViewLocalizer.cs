using GaspApp.Services;
using Microsoft.Extensions.Localization;

namespace GaspApp
{
    public sealed class SharedViewLocalizer
    {
        private readonly IStringLocalizer _localizer;

        /*public SharedViewLocalizer(IStringLocalizerFactory factory)
        {
            _localizer = factory.Create(typeof(LocalizationData));
        }*/

        public SharedViewLocalizer(DbStringLocalizer localizer)
            => _localizer = localizer;

        public LocalizedString this[string key] => _localizer[key];
    }
}
