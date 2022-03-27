using GaspApp.Data;
using GaspApp.Models;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace GaspApp.Services
{
    public class DbStringLocalizer : IStringLocalizer
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DbStringLocalizer(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public LocalizedString this[string name] => GetLocalizedString(name);

        public LocalizedString this[string name, params object[] arguments] => throw new NotImplementedException();

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetService<GaspDbContext>();
            return db!.LocalizedItems
                .Where(r => r.CultureCode == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                .Select(x => new LocalizedString(x.Key, x.Text));
        }

        public LocalizedString GetLocalizedString(string key, string? group = default)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetService<GaspDbContext>();

            group = group ?? "[global]";

            var item = db.LocalizedItems
                .Where(r => r.CultureCode == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                .Where(r => r.Group == group)
                .FirstOrDefault(r => r.Key == key);
            if (item == null)
                CreateTranslationSet(db, key, group);
            return new LocalizedString(key, item?.Text ?? key, item == null);
        }

        public void CreateTranslationSet(GaspDbContext db, string key, string group)
        {
            foreach (var cul in LocaleConstants.SUPPORTED_LOCALES_TWOLETTERS)
            {
                db.LocalizedItems.Add(new LocalizedItem
                {
                    Automatic = false,
                    Key = key,
                    Group = group,
                    Text = cul == "en" ? key : "",
                    CultureCode = cul,
                });
            }
            db.SaveChanges();
        }
    }
}
