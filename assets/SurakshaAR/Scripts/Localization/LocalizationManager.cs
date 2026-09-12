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
    /// Runtime lookup for localized UI strings supporting English, Hindi, and Santali.
    /// Loads from JSON (Assets/Resources/Localization/localization.json) with fallback to LocalizedStrings.Table.
    /// Not a MonoBehaviour — owned and created by AppManager so it survives screen changes.
    /// </summary>
    public class LocalizationManager
    {
        public AppLanguage CurrentLanguage { get; private set; } = AppLanguage.English;

        private const string PrefsKey = "SurakshaAR_Language";

        private readonly Dictionary<string, LocalizationEntry> _entries = new Dictionary<string, LocalizationEntry>();
        private bool _isJsonLoaded = false;

        public LocalizationManager()
        {
            if (PlayerPrefs.HasKey(PrefsKey))
            {
                CurrentLanguage = (AppLanguage)PlayerPrefs.GetInt(PrefsKey);
            }

            LoadJsonLocalization();
        }

        public void SetLanguage(AppLanguage language)
        {
            CurrentLanguage = language;
            PlayerPrefs.SetInt(PrefsKey, (int)language);
            PlayerPrefs.Save();
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
                        Debug.Log($"[LocalizationManager] Successfully loaded {_entries.Count} localized strings from JSON.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[LocalizationManager] Failed to load JSON localization: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns the localized string for a key in the current language.
        /// Resolves from JSON first, then falls back to LocalizedStrings.Table, then English, then [key].
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
                        if (UI.UIHelper.HasDevanagariSupport() && !string.IsNullOrEmpty(entry.hi))
                            return entry.hi;
                        if (!string.IsNullOrEmpty(entry.hi_roman))
                            return entry.hi_roman;
                        if (!string.IsNullOrEmpty(entry.hi))
                            return DevanagariTransliteration.Transliterate(entry.hi);
                        break;

                    case AppLanguage.Santali:
                        if (UI.UIHelper.HasOlChikiSupport() && !string.IsNullOrEmpty(entry.sat))
                            return entry.sat;
                        if (!string.IsNullOrEmpty(entry.sat_roman))
                            return entry.sat_roman;
                        if (!string.IsNullOrEmpty(entry.sat))
                            return entry.sat;
                        break;
                }

                // Fallback to English from JSON if current language value is missing
                if (!string.IsNullOrEmpty(entry.en))
                    return entry.en;
            }

            // 2. Fallback to hardcoded C# LocalizedStrings.Table
            if (LocalizedStrings.Table.TryGetValue(key, out var languages))
            {
                if (languages.TryGetValue(CurrentLanguage, out var value) && !string.IsNullOrEmpty(value))
                {
                    if (CurrentLanguage == AppLanguage.Hindi && !UI.UIHelper.HasDevanagariSupport())
                    {
                        return DevanagariTransliteration.Transliterate(value);
                    }
                    return value;
                }
                if (languages.TryGetValue(AppLanguage.English, out var fallback))
                {
                    return fallback;
                }
            }

            return $"[{key}]";
        }
    }
}