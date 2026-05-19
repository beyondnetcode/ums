using Ums.Globalization.Core;

namespace Ums.Globalization;

public interface ILocalizationService
{
    string T(string key);
    string T(string key, LanguageCode language);
    LocalizedMessage Localize(string key);
    LocalizedMessage Localize(string key, LanguageCode language);
}
