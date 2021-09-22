using Microsoft.Extensions.Localization;

namespace GaspApp
{
    public sealed class SharedViewLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public SharedViewLocalizer(IStringLocalizerFactory factory)
        {
            _localizer = factory.Create(typeof(LocalizationData));
        }

        public LocalizedString this[string key] => _localizer[key];
    }
}
