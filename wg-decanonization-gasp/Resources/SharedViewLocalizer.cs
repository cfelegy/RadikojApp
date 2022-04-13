using Radikoj.Services;
using Microsoft.Extensions.Localization;

namespace Radikoj
{
    public sealed class SharedViewLocalizer
    {
        private readonly DbStringLocalizer _localizer;
        private readonly string? _group = default;

        public SharedViewLocalizer(DbStringLocalizer localizer)
            => _localizer = localizer;
        private SharedViewLocalizer(DbStringLocalizer localizer, string group)
        {
            _localizer = localizer;
            _group = group;
        }

        public LocalizedString this[string key] => _localizer.GetLocalizedString(key, _group);

        public SharedViewLocalizer ScopeForGroup(string group)
            => new SharedViewLocalizer(_localizer, group);

        public LocalizedString Get(string key, string group) => (_localizer as DbStringLocalizer)!.GetLocalizedString(key, group);
    }
}
