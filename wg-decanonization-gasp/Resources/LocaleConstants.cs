using System.Globalization;

namespace GaspApp
{
	public static class LocaleConstants
	{
		public static readonly string[] SUPPORTED_LOCALES = new string[]
		{
			"en-US", "ar-sa", "zh-Hans", "fr", "ru", "es"
		};

		public static readonly string[] SUPPORTED_LOCALES_TWOLETTERS = 
			SUPPORTED_LOCALES.Select(x => new CultureInfo(x)).Select(x => x.TwoLetterISOLanguageName).ToArray();
	}
}
