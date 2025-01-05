using System.Globalization;
using System.Reflection;
using System.Resources;

namespace CoursFlairy.Language
{
    public static class LangManager
    {
        private static ResourceManager _rm;

        static LangManager()
        {
            _rm = new ResourceManager("CoursFlairy.Language.en", Assembly.GetExecutingAssembly());
        }

        public static string? GetString(string key)
        {
            return _rm.GetString(key);
        }

        public static void ChangeLanguage(string language)
        {
            _rm = new ResourceManager($"DifferentLanguages.Language.{language}", Assembly.GetExecutingAssembly());
        }

        public static string T(string key)
        {
            return GetString(key) ?? key;
        }
    }
}
