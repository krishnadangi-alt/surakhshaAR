using System.Collections.Generic;
using SurakshaAR.Data;
using UnityEngine;

namespace SurakshaAR.Localization
{
    [System.Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string en;
        public string hi;
        public string hi_roman;
        public string sat;
        public string sat_roman;
    }

    [System.Serializable]
    public class LocalizationPackage
    {
        public List<LocalizationEntry> entries;
    }

    /// <summary>
    /// Runtime lookup for localized UI strings supporting English, Hindi (Devanagari), and Santali (Ol Chiki).
    /// Loads from JSON (Assets/Resources/Localization/localization.json) with fallback to LocalizedStrings.Table.
    /// Not a MonoBehaviour — owned and created by AppManager so it survives screen changes.
    ///
    /// Santali integrity:
    ///   sat field must contain genuine Ol Chiki script only.
    ///   Empty sat → falls back to English (not Hindi, not Romanized).
    /// </summary>
    public class LocalizationManager
    {
        public static LocalizationManager Instance { get; private set; }
        public AppLanguage CurrentLanguage { get; private set; } = AppLanguage.English;

        /// <summary>
        /// Writing direction for the current language.
        /// Santali (Ol Chiki), Hindi (Devanagari), and English (Latin) are strictly LeftToRight.
        /// </summary>
        public TextDirection Direction => LocaleResolver.GetTextDirection(CurrentLanguage);

        /// <summary>Always false. All supported locales (including Santali) are Left-to-Right.</summary>
        public bool IsRightToLeft => false;

        public TextDirection GetTextDirection(AppLanguage language) => LocaleResolver.GetTextDirection(language);
        public bool IsLanguageRightToLeft(AppLanguage language) => false;

        private const string PrefsKey = "SurakshaAR_Language";
        private const string PrefsManualKey = "SurakshaAR_Language_Manual";

        private readonly Dictionary<string, LocalizationEntry> _entries = new Dictionary<string, LocalizationEntry>();
        private bool _isJsonLoaded = false;

        /// <summary>Fired whenever the active language changes. Subscribe to refresh visible UI.</summary>
        public event System.Action<AppLanguage> OnLanguageChanged;

        public static bool HasSavedPreference => PlayerPrefs.HasKey(PrefsManualKey) || PlayerPrefs.HasKey(PrefsKey);

        public LocalizationManager()
        {
            Instance = this;
            if (PlayerPrefs.HasKey(PrefsManualKey) || PlayerPrefs.HasKey(PrefsKey))
            {
                CurrentLanguage = (AppLanguage)PlayerPrefs.GetInt(PrefsKey, (int)AppLanguage.English);
                Debug.Log($"[LocalizationManager] Restored saved language preference: {CurrentLanguage}");
            }
            else
            {
                // First launch: automatically detect and match Android system locale
                CurrentLanguage = LocaleResolver.DetectDeviceLanguage();
                Debug.Log($"[LocalizationManager] Initialized with detected device language: {CurrentLanguage}");
            }

            LoadJsonLocalization();
        }

        /// <summary>Switch language, persist the choice, and notify all subscribers.</summary>
        public void SetLanguage(AppLanguage language)
        {
            if (CurrentLanguage == language && PlayerPrefs.HasKey(PrefsManualKey)) return;
            CurrentLanguage = language;
            PlayerPrefs.SetInt(PrefsKey, (int)language);
            PlayerPrefs.SetInt(PrefsManualKey, 1);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke(language);
        }

        private void LoadJsonLocalization()
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>("Localization/localization");
                if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
                {
                    var package = JsonUtility.FromJson<LocalizationPackage>(textAsset.text);
                    if (package != null && package.entries != null)
                    {
                        _entries.Clear();
                        foreach (var entry in package.entries)
                        {
                            if (!string.IsNullOrEmpty(entry.key))
                            {
                                _entries[entry.key] = entry;
                            }
                        }
                        _isJsonLoaded = true;
                        Debug.Log($"[LocalizationManager] Loaded {_entries.Count} strings from JSON.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[LocalizationManager] Failed to load JSON localization: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates that text contains genuine Ol Chiki characters (U+1C50 - U+1C7F).
        /// </summary>
        public static bool IsVerifiedOlChiki(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if (c >= 0x1C50 && c <= 0x1C7F)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the localized string for a key in the current language.
        /// Resolution order:
        ///   1. JSON package (Assets/Resources/Localization/localization.json)
        ///   2. C# LocalizedStrings.Table
        ///   3. Language-specific fallback:
        ///      - English: returns key placeholder [key]
        ///      - Hindi: falls back to English if missing
        ///      - Santali: strictly returns "MISSING_SANTALI_TRANSLATION" if unverified or empty (Rule 31: no English fallback)
        /// </summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            // 1. Try resolving from loaded JSON package
            if (_isJsonLoaded && _entries.TryGetValue(key, out var entry))
            {
                switch (CurrentLanguage)
                {
                    case AppLanguage.English:
                        if (!string.IsNullOrEmpty(entry.en)) return entry.en;
                        break;

                    case AppLanguage.Hindi:
                        if (!string.IsNullOrEmpty(entry.hi)) return DevanagariShaper.Shape(entry.hi);
                        if (!string.IsNullOrEmpty(entry.en)) return entry.en;
                        break;

                    case AppLanguage.Santali:
                        // Only genuine verified Ol Chiki allowed. Never Romanized, never silent English fallback.
                        if (IsVerifiedOlChiki(entry.sat)) return entry.sat;
                        return "MISSING_SANTALI_TRANSLATION";
                }
            }

            // 2. Fallback to hardcoded C# LocalizedStrings.Table
            if (LocalizedStrings.Table.TryGetValue(key, out var languages))
            {
                if (CurrentLanguage == AppLanguage.Santali)
                {
                    // Only genuine verified Ol Chiki allowed. Never Romanized, never silent English fallback.
                    if (languages.TryGetValue(AppLanguage.Santali, out var satValue) && IsVerifiedOlChiki(satValue))
                        return satValue;
                    return "MISSING_SANTALI_TRANSLATION";
                }
                else
                {
                    if (languages.TryGetValue(CurrentLanguage, out var value) && !string.IsNullOrEmpty(value))
                        return CurrentLanguage == AppLanguage.Hindi ? DevanagariShaper.Shape(value) : value;
                    if (languages.TryGetValue(AppLanguage.English, out var fallback))
                        return fallback;
                }
            }

            // 3. Key not found in table
            if (CurrentLanguage == AppLanguage.Santali)
            {
                return "MISSING_SANTALI_TRANSLATION";
            }

            Debug.LogWarning($"[LocalizationManager] Missing key: \"{key}\" for language {CurrentLanguage}");
            return $"[{key}]";
        }

        /// <summary>
        /// Format helper — substitutes {0},{1}... args after localization lookup.
        /// </summary>
        public string GetFormat(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                string formatted = string.Format(template, args);
                return CurrentLanguage == AppLanguage.Hindi ? DevanagariShaper.Shape(formatted) : formatted;
            }
            catch { return template; }
        }
    }
}