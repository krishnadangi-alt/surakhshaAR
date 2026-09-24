using System;
using System.Globalization;
using SurakshaAR.Data;
using UnityEngine;

namespace SurakshaAR.Localization
{
    /// <summary>
    /// Safe Native Android and Platform Locale Resolver for SurakshaAR.
    /// Detects the Android system locale (e.g. "hi-IN", "sat-Olck-IN", "en-IN")
    /// and resolves it to a supported AppLanguage.
    /// 
    /// Precedence order:
    ///   1. Android Native java.util.Locale.getDefault() via safe JNI bridge
    ///   2. .NET System.Globalization.CultureInfo.CurrentCulture
    ///   3. Unity Application.systemLanguage
    ///   4. Canonical fallback (AppLanguage.English)
    /// </summary>
    public static class LocaleResolver
    {
        /// <summary>
        /// Detects the device locale and maps it to a supported SurakshaAR AppLanguage.
        /// </summary>
        public static AppLanguage DetectDeviceLanguage()
        {
            // 1. Android Native Locale via JNI
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var localeClass = new AndroidJavaClass("java.util.Locale"))
                using (var defaultLocale = localeClass.CallStatic<AndroidJavaObject>("getDefault"))
                {
                    if (defaultLocale != null)
                    {
                        string languageTag = defaultLocale.Call<string>("toLanguageTag"); // e.g. "hi-IN", "sat-Olck-IN", "en-US"
                        string language = defaultLocale.Call<string>("getLanguage");       // e.g. "hi", "sat", "en"
                        
                        var resolved = MapLocaleToAppLanguage(languageTag, language);
                        if (resolved.HasValue)
                        {
                            Debug.Log($"[LocaleResolver] Android JNI locale detected: tag='{languageTag}', lang='{language}' -> {resolved.Value}");
                            return resolved.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LocaleResolver] Android JNI locale detection error: {ex.Message}");
            }
#endif

            // 2. .NET CultureInfo
            try
            {
                var culture = CultureInfo.CurrentCulture;
                if (culture != null)
                {
                    var resolved = MapLocaleToAppLanguage(culture.Name, culture.TwoLetterISOLanguageName);
                    if (resolved.HasValue)
                    {
                        Debug.Log($"[LocaleResolver] CultureInfo locale detected: name='{culture.Name}', iso='{culture.TwoLetterISOLanguageName}' -> {resolved.Value}");
                        return resolved.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LocaleResolver] CultureInfo detection error: {ex.Message}");
            }

            // 3. Fallback to Unity Application.systemLanguage
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Hindi:
                    return AppLanguage.Hindi;
                case SystemLanguage.English:
                    return AppLanguage.English;
                default:
                    return AppLanguage.English;
            }
        }

        /// <summary>
        /// Maps an Android language tag (e.g. "hi-IN", "sat-Olck-IN", "en-US") or ISO language code to an AppLanguage.
        /// </summary>
        public static AppLanguage? MapLocaleToAppLanguage(string localeTag, string languageCode)
        {
            string tag = (localeTag ?? string.Empty).ToLowerInvariant().Trim();
            string code = (languageCode ?? string.Empty).ToLowerInvariant().Trim();

            // Hindi detection: "hi", "hi-IN", "hi_IN", "hin"
            if (code == "hi" || tag.StartsWith("hi-") || tag.StartsWith("hi_") || tag == "hi" || tag.StartsWith("hin"))
            {
                return AppLanguage.Hindi;
            }

            // Santali detection: "sat", "sat-IN", "sat_IN", "sat-Olck", "sat-Olck-IN"
            if (code == "sat" || tag.StartsWith("sat-") || tag.StartsWith("sat_") || tag == "sat" || tag.Contains("olck"))
            {
                return AppLanguage.Santali;
            }

            // English detection: "en", "en-IN", "en-US", "en-GB", "eng"
            if (code == "en" || tag.StartsWith("en-") || tag.StartsWith("en_") || tag == "en" || tag.StartsWith("eng"))
            {
                return AppLanguage.English;
            }

            return null;
        }

        /// <summary>
        /// Returns the script writing direction for the given AppLanguage.
        /// CRITICAL: Santali (written in the Ol Chiki alphabet, U+1C50-U+1C7F),
        /// Hindi (Devanagari), and English (Latin) are strictly Left-to-Right (LTR) scripts.
        /// Ol Chiki was created in 1925 by Pandit Raghunath Murmu as an alphabetic, left-to-right writing system.
        /// It must NEVER be classified or processed as Right-to-Left (RTL).
        /// </summary>
        public static TextDirection GetTextDirection(AppLanguage language)
        {
            switch (language)
            {
                case AppLanguage.English:
                case AppLanguage.Hindi:
                case AppLanguage.Santali:
                default:
                    return TextDirection.LeftToRight;
            }
        }

        /// <summary>
        /// Returns true if the language is Right-to-Left. Always false for English, Hindi, and Santali.
        /// </summary>
        public static bool IsRtl(AppLanguage language)
        {
            return false;
        }

        /// <summary>
        /// Validates a locale string against RTL script catalogs.
        /// Explicitly guards and excludes Santali ('sat', 'sat-Olck', 'sat-IN'), Hindi, and English,
        /// ensuring they are never routed into RTL layout or character reversal pipelines.
        /// </summary>
        public static bool IsRtlLocale(string localeTag)
        {
            if (string.IsNullOrEmpty(localeTag)) return false;
            string tag = localeTag.ToLowerInvariant().Trim();

            // Explicit exclusion: Santali (Ol Chiki) is strictly LTR
            if (tag.StartsWith("sat") || tag.Contains("olck") || tag.Contains("chiki"))
            {
                return false;
            }

            // Explicit exclusion: Hindi & English
            if (tag.StartsWith("hi") || tag.StartsWith("hin") || tag.StartsWith("en") || tag.StartsWith("eng"))
            {
                return false;
            }

            // Only recognized Semitic / Middle-Eastern RTL scripts
            return tag.StartsWith("ar") || tag.StartsWith("he") || tag.StartsWith("fa") || tag.StartsWith("ur") || tag.StartsWith("ps") || tag.StartsWith("syr");
        }
    }
}

