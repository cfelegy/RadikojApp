using GaspApp.Data;
using GaspApp.Models;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace GaspApp.Services
{
    public class DbStringLocalizer : IStringLocalizer
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string[] _cultureCodes;

        public DbStringLocalizer(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _cultureCodes = LocaleConstants.SUPPORTED_LOCALES.Select(x => new CultureInfo(x)).Select(x => x.TwoLetterISOLanguageName).ToArray();
            
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

        public LocalizedString GetLocalizedString(string key)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetService<GaspDbContext>();
            var item = db.LocalizedItems
                .Where(r => r.CultureCode == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                .FirstOrDefault(r => r.Key == key);
            if (item == null)
                CreateTranslationSet(db, key);
            return new LocalizedString(key, item?.Text ?? key, item == null);
        }

        public void CreateTranslationSet(GaspDbContext db, string key)
        {
            foreach (var cul in _cultureCodes)
            {
                db.LocalizedItems.Add(new LocalizedItem
                {
                    Automatic = false,
                    Key = key,
                    Text = cul == "en" ? key : "",
                    CultureCode = cul,
                });
            }
            db.SaveChanges();
        }
    }
}
