using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.UI
{
    /// <summary>
    /// JSON-based localization for the SurakshaAR main menu.
    ///
    /// Supported languages:
    ///   - English  ("en")
    ///   - Hindi    ("hi")
    ///   - Santali  ("sat")
    ///
    /// Source-of-truth translations live in the app Localization folder:
    ///   app/Localization/English/main_menu.json
    ///   app/Localization/Hindi/main_menu.json
    ///   app/Localization/Santali/main_menu.json
    ///
    /// At runtime the menu first tries to load those JSON files through
    /// Resources (path "Localization/main_menu_en" etc.). If the JSON files are
    /// not present under Assets/Resources at build time, the built-in tables in
    /// this class are used instead - they contain the same strings as the JSON
    /// files, so the menu works fully in all three languages either way.
    /// </summary>
    public static class MainMenuLocalization
    {
        public const string CodeEnglish = "en";
        public const string CodeHindi = "hi";
        public const string CodeSantali = "sat";
        public const string DefaultLanguage = CodeEnglish;

        public static readonly string[] SupportedLanguages =
        {
            CodeEnglish, CodeHindi, CodeSantali
        };

        private static string currentLanguage = DefaultLanguage;
        private static Dictionary<string, string> currentTable;

        static MainMenuLocalization()
        {
            LoadSavedLanguage();
        }

        public static string CurrentLanguage
        {
            get { return currentLanguage; }
        }

        public static bool IsSupported(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            for (int i = 0; i < SupportedLanguages.Length; i++)
            {
                if (SupportedLanguages[i] == code) return true;
            }
            return false;
        }

        /// <summary>Loads the saved language from PlayerPrefs (key "app_language").</summary>
        public static void LoadSavedLanguage()
        {
            string saved = PlayerPrefs.GetString("app_language", DefaultLanguage);
            SetLanguageRaw(saved);
        }

        /// <summary>Switches the active language and persists the choice.</summary>
        public static void SetLanguage(string code)
        {
            SetLanguageRaw(code);
            PlayerPrefs.SetString("app_language", currentLanguage);
            PlayerPrefs.Save();
        }

        /// <summary>Localized label for a language code (e.g. "en" -&gt; "English").</summary>
        public static string LanguageDisplayName(string code)
        {
            return Get("lang_" + code);
        }

        /// <summary>Returns the localized string for a key, falling back to English then to the key itself.</summary>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;

            if (currentTable != null &&
                currentTable.TryGetValue(key, out string value) &&
                !string.IsNullOrEmpty(value))
            {
                return value;
            }

            // Fall back to the English table.
            if (Tables.TryGetValue(DefaultLanguage, out Dictionary<string, string> english) &&
                english.TryGetValue(key, out string fallback) &&
                !string.IsNullOrEmpty(fallback))
            {
                return fallback;
            }

            return key;
        }

        private static void SetLanguageRaw(string code)
        {
            string normalized = string.IsNullOrEmpty(code) ? DefaultLanguage : code.ToLowerInvariant();
            if (!IsSupported(normalized))
                normalized = DefaultLanguage;

            currentLanguage = normalized;
            currentTable = TryLoadJsonOverlay(normalized) ?? Tables[normalized];
        }

        /// <summary>
        /// Tries to load the JSON translation file from Resources
        /// ("Localization/main_menu_en" etc.). Returns null when unavailable.
        /// </summary>
        private static Dictionary<string, string> TryLoadJsonOverlay(string code)
        {
            TextAsset asset = Resources.Load<TextAsset>("Localization/main_menu_" + code);
            if (asset == null || string.IsNullOrEmpty(asset.text))
                return null;

            try
            {
                MainMenuJson data = JsonUtility.FromJson<MainMenuJson>(asset.text);
                if (data == null)
                    return null;

                var table = new Dictionary<string, string>(Tables[code]);
                data.ApplyTo(table);
                return table;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MainMenuLocalization] Failed to parse JSON for '" + code + "': " + e.Message);
                return null;
            }
        }

        // ------------------------------------------------------------------
        // Built-in translation tables (mirror of the JSON files)
        // ------------------------------------------------------------------

        private static readonly Dictionary<string, Dictionary<string, string>> Tables =
            new Dictionary<string, Dictionary<string, string>>
            {
                { CodeEnglish, English },
                { CodeHindi, Hindi },
                { CodeSantali, Santali }
            };

        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "app_name", "SURAKSHAAR" },
            { "menu_title", "Fire Safety Training" },
            { "app_tagline", "AI-Powered Safety Training" },
            { "language_label", "Language" },
            { "lang_en", "English" },
            { "lang_hi", "Hindi" },
            { "lang_sat", "Santali" },
            { "module_badge", "AR" },
            { "module_title", "Fire Safety Module" },
            { "module_subtitle", "Fire & Explosion Response" },
            { "module_description",
                "Respond correctly during a fire emergency with interactive AR training. " +
                "This module covers extinguisher types, the PASS technique, hose and alarm " +
                "operation, and safe evacuation procedures." },
            { "learn_title", "What you will learn" },
            { "bullet_1", "Types of fire and choosing the right extinguisher" },
            { "bullet_2", "Extinguisher handling - PIN, AIM, SQUEEZE, SWEEP" },
            { "bullet_3", "Hose, alarm and evacuation response drills" },
            { "bullet_4", "End-to-end AR fire scenario practice" },
            { "meta", "10-15 min  |  AR-based  |  Offline-ready" },
            { "top7_title", "Top 7" },
            { "top7_badge", "Placeholder" },
            { "top7_caption", "Leaderboard placeholder - top performing trainees will appear here" },
            { "top7_trainee", "Trainee" },
            { "top7_points", "pts" },
            { "start_module", "Start Module" },
            { "start_hint", "Launches the Fire Training module in Augmented Reality" }
        };

        private static readonly Dictionary<string, string> Hindi = new Dictionary<string, string>
        {
            { "app_name", "SURAKSHAAR" },
            { "menu_title", "अग्नि सुरक्षा प्रशिक्षण" },
            { "app_tagline", "AI आधारित सुरक्षा प्रशिक्षण" },
            { "language_label", "भाषा" },
            { "lang_en", "English" },
            { "lang_hi", "हिन्दी" },
            { "lang_sat", "संताली" },
            { "module_badge", "AR" },
            { "module_title", "अग्नि सुरक्षा मॉड्यूल" },
            { "module_subtitle", "आग एवं विस्फोट प्रतिक्रिया" },
            { "module_description",
                "आग लगने पर Augmented Reality की सहायता से सही प्रतिक्रिया करना सीखें। " +
                "इस मॉड्यूल में अग्निशामक यंत्रों के प्रकार, PASS तकनीक, होज़ एवं अलार्म " +
                "संचालन और सुरक्षित निकासी प्रक्रियाओं का अभ्यास शामिल है।" },
            { "learn_title", "आप क्या सीखेंगे" },
            { "bullet_1", "आग के प्रकार और सही आगशामक का चुनाव" },
            { "bullet_2", "आगशामक उपयोग - पिन, निशाना, दबाव, सफाई" },
            { "bullet_3", "होज़, अलार्म और निकासी अभ्यास" },
            { "bullet_4", "संपूर्ण AR आग दृश्य अभ्यास" },
            { "meta", "10-15 मिनट  |  AR आधारित  |  ऑफ़लाइन तैयार" },
            { "top7_title", "शीर्ष 7" },
            { "top7_badge", "स्थान रखकर्ता" },
            { "top7_caption", "लीडरबोर्ड की जगह - शीर्ष करने वाले यहाँ दिखाए जाएँगे" },
            { "top7_trainee", "प्रशिक्षणार्थी" },
            { "top7_points", "अंक" },
            { "start_module", "मॉड्यूल शुरू करें" },
            { "start_hint", "Augmented Reality में आग सुरक्षा प्रशिक्षण शुरू होगा" }
        };

        // NOTE: Santali strings below are best-effort (Devanagari script) and
        // should be reviewed by a native speaker. Update them in
        // app/Localization/Santali/main_menu.json - no code changes needed.
        private static readonly Dictionary<string, string> Santali = new Dictionary<string, string>
        {
            { "app_name", "SURAKSHAAR" },
            { "menu_title", "आग सुरक्षा प्रशिक्षण" },
            { "app_tagline", "AI दिरि सुरक्षा प्रशिक्षण" },
            { "language_label", "भाषा" },
            { "lang_en", "English" },
            { "lang_hi", "हिन्दी" },
            { "lang_sat", "संताली" },
            { "module_badge", "AR" },
            { "module_title", "आग सुरक्षा मॉड्यूल" },
            { "module_subtitle", "आग आर विस्फोट उत्तर" },
            { "module_description",
                "Augmented Reality हेल्प रे आग आपद रे सही उत्तर रूप। " +
                "ई मॉड्यूल रे अग्निशामक प्रकार, PASS तकनीक, होज़ आर अलार्म " +
                "संचालन आर सुरक्षित निकासी प्रक्रिया अभ्यास रे चीं।" },
            { "learn_title", "आपन सिकोड़ चीति" },
            { "bullet_1", "आग रे प्रकार आर ठीक अग्निशामक चुनाव" },
            { "bullet_2", "अग्निशामक उपयोग - पिन, निशाना, दबाव, सफाई" },
            { "bullet_3", "होज़, अलार्म आर निकासी अभ्यास" },
            { "bullet_4", "पूरा AR आग दृश्य अभ्यास" },
            { "meta", "10-15 मिनट  |  AR आधारे  |  ऑफ़लाइन तैयार" },
            { "top7_title", "शीर्ष 7" },
            { "top7_badge", "खाली जगह" },
            { "top7_caption", "लीडरबोर्ड खाली जगह - सब उत्तम प्रशिक्षार्थी हेँ दिखाया जावा" },
            { "top7_trainee", "प्रशिक्षार्थी" },
            { "top7_points", "अंक" },
            { "start_module", "मॉड्यूल चालू करें" },
            { "start_hint", "Augmented Reality रे आग सुरक्षा प्रशिक्षण चालू जावा" }
        };
    }

    /// <summary>
    /// Serializable mirror of the main menu JSON translation file.
    /// Used to load app/Localization/*/main_menu.json at runtime via Resources.
    /// </summary>
    [Serializable]
    public class MainMenuJson
    {
        public string app_name;
        public string menu_title;
        public string app_tagline;
        public string language_label;
        public string lang_en;
        public string lang_hi;
        public string lang_sat;
        public string module_badge;
        public string module_title;
        public string module_subtitle;
        public string module_description;
        public string learn_title;
        public string bullet_1;
        public string bullet_2;
        public string bullet_3;
        public string bullet_4;
        public string meta;
        public string top7_title;
        public string top7_badge;
        public string top7_caption;
        public string top7_trainee;
        public string top7_points;
        public string start_module;
        public string start_hint;

        /// <summary>Overwrites non-empty fields onto the given translation table.</summary>
        public void ApplyTo(Dictionary<string, string> table)
        {
            Overwrite(table, "app_name", app_name);
            Overwrite(table, "menu_title", menu_title);
            Overwrite(table, "app_tagline", app_tagline);
            Overwrite(table, "language_label", language_label);
            Overwrite(table, "lang_en", lang_en);
            Overwrite(table, "lang_hi", lang_hi);
            Overwrite(table, "lang_sat", lang_sat);
            Overwrite(table, "module_badge", module_badge);
            Overwrite(table, "module_title", module_title);
            Overwrite(table, "module_subtitle", module_subtitle);
            Overwrite(table, "module_description", module_description);
            Overwrite(table, "learn_title", learn_title);
            Overwrite(table, "bullet_1", bullet_1);
            Overwrite(table, "bullet_2", bullet_2);
            Overwrite(table, "bullet_3", bullet_3);
            Overwrite(table, "bullet_4", bullet_4);
            Overwrite(table, "meta", meta);
            Overwrite(table, "top7_title", top7_title);
            Overwrite(table, "top7_badge", top7_badge);
            Overwrite(table, "top7_caption", top7_caption);
            Overwrite(table, "top7_trainee", top7_trainee);
            Overwrite(table, "top7_points", top7_points);
            Overwrite(table, "start_module", start_module);
            Overwrite(table, "start_hint", start_hint);
        }

        private static void Overwrite(Dictionary<string, string> table, string key, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            table[key] = value;
        }
    }
}