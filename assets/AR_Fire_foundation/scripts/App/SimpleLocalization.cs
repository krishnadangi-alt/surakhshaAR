using System;
using System.Collections.Generic;

/// <summary>
/// SimpleLocalization
/// ==================
/// Centralized, local string-translation system (no backend).
///
/// Supported now: English + Hindi. Add more languages by adding a
/// table in the static constructor - no other code changes needed.
///
/// Usage:
///   SimpleLocalization.SetLanguage("Hindi");
///   string text = SimpleLocalization.Get("login");
///
/// The menu UI subscribes to OnLanguageChanged and refreshes itself.
/// </summary>
public static class SimpleLocalization
{
    /// <summary>Raised after the language changes ("English"/"Hindi").</summary>
    public static event Action<string> OnLanguageChanged;

    public static readonly string[] SupportedLanguages = { "English", "Hindi", "Santali" };

    public static string CurrentLanguage { get; private set; } = "English";

    private static readonly Dictionary<string, string> english = new Dictionary<string, string>
    {
        { "app_name", "SURAKSHAAR" },
        { "app_tagline", "Industrial Safety Training" },
        { "employee_id", "EMPLOYEE ID" },
        { "password", "PASSWORD" },
        { "employee_id_placeholder", "Enter your employee ID" },
        { "password_placeholder", "Enter your password" },
        { "login", "LOGIN" },
        { "select_language", "SELECT LANGUAGE" },
        { "select_training", "SELECT TRAINING" },
        { "fire_safety", "FIRE SAFETY" },
        { "available", "Available" },
        { "coming_soon", "Coming Soon" },
        { "electrical_safety", "ELECTRICAL SAFETY" },
        { "ppe_safety", "PPE SAFETY" },
        { "emergency_response", "EMERGENCY RESPONSE" },
        { "fire_intro_title", "FIRE SAFETY TRAINING" },
        { "fire_intro_body",
            "What you will learn:\n" +
            "\u2022 Fire safety awareness\n" +
            "\u2022 Fire extinguisher familiarization\n" +
            "\u2022 Emergency response sequence\n" +
            "\u2022 Interactive AR practice" },
        { "equipment_title", "SAFETY EQUIPMENT" },
        { "equipment_body",
            "Know your safety equipment:\n" +
            "\u2022 Fire extinguisher - puts out small fires\n" +
            "\u2022 Safety helmet - protects your head\n" +
            "\u2022 Safety gloves - protects your hands\n" +
            "\u2022 Safety footwear - protects your feet\n" +
            "\u2022 Eye/face protection\n" +
            "\u2022 Emergency alarm - raise the alert first" },
        { "howto_title", "HOW TO USE THIS TRAINING" },
        { "howto_step1", "Place the fire scenario on the floor." },
        { "howto_step2", "Activate the alarm." },
        { "howto_step3", "Pick up the extinguisher." },
        { "howto_step4", "Remove the safety pin." },
        { "howto_step5", "Use the extinguisher on the fire." },
        { "howto_step6", "Stop the fire to complete the training." },
        { "start_training", "START TRAINING" },
        { "back", "BACK" },
        { "continue", "CONTINUE" },
        { "step", "STEP" },
        { "of", "OF" },
        { "welcome", "WELCOME" },
        { "training_complete", "TRAINING COMPLETE" },
        { "retry", "RETRY" },
        { "home", "HOME" }
    };

    private static readonly Dictionary<string, Dictionary<string, string>> tables =
        new Dictionary<string, Dictionary<string, string>>
    {
        { "English", english }
    };

    static SimpleLocalization()
    {
        tables["Hindi"] = BuildHindiTable();
        // Santali is selectable and persisted now. Its reviewed translation
        // pack can replace these English fallbacks without UI code changes.
        tables["Santali"] = new Dictionary<string, string>(english);
    }

    /// <summary>Change the session language and notify listeners.</summary>
    public static void SetLanguage(string language)
    {
        if (string.IsNullOrEmpty(language) || !tables.ContainsKey(language))
        {
            language = "English";
        }

        if (CurrentLanguage == language)
        {
            return;
        }

        CurrentLanguage = language;
        OnLanguageChanged?.Invoke(language);
    }

    /// <summary>
    /// Translate a key. Falls back to English, then to the key itself
    /// (so a missing key is visible instead of silently blank).
    /// </summary>
    public static string Get(string key)
    {
        if (tables.TryGetValue(CurrentLanguage, out Dictionary<string, string> table) &&
            table.TryGetValue(key, out string value))
        {
            return value;
        }

        if (english.TryGetValue(key, out string fallback))
        {
            return fallback;
        }

        return key;
    }

    private static Dictionary<string, string> BuildHindiTable()
    {
        return new Dictionary<string, string>
        {
            { "app_name", "\u0938\u0941\u0930\u0915\u094d\u0937\u093eAR" },
            { "app_tagline", "\u0906\u0927\u094d\u092f\u094b\u0917\u093f\u0915 \u0938\u0941\u0930\u0915\u094d\u0937\u093e \u092a\u094d\u0930\u0936\u093f\u0915\u094d\u0937\u0923" },
            { "employee_id", "\u0915\u0930\u094d\u092e\u091a\u093e\u0930\u0940 \u0906\u0908\u0921\u0940" },
            { "password", "\u092a\u093e\u0938\u0935\u0930\u094d\u0921" },
            { "employee_id_placeholder", "\u0905\u092a\u0928\u0940 \u0915\u0930\u094d\u092e\u091a\u093e\u0930\u0940 \u0906\u0908\u0921\u0940 \u0926\u0930\u094d\u091c \u0915\u0930\u0947\u0902" },
            { "password_placeholder", "\u0905\u092a\u0928\u093e \u092a\u093e\u0938\u0935\u0930\u094d\u0921 \u0926\u0930\u094d\u091c \u0915\u0930\u0947\u0902" },
            { "login", "\u0932\u0949\u0917 \u0907\u0928" },
            { "select_language", "\u092d\u093e\u0937\u093e \u091a\u0941\u0928\u0947\u0902" },
            { "select_training", "\u092a\u094d\u0930\u0936\u093f\u0915\u094d\u0937\u0923 \u091a\u0941\u0928\u0947\u0902" },
            { "fire_safety", "\u0905\u0917\u094d\u0928\u093f \u0938\u0941\u0930\u0915\u094d\u0937\u093e" },
            { "available", "\u0909\u092a\u0932\u092c\u094d\u0927" },
            { "coming_soon", "\u091c\u0932\u094d\u0926 \u0906 \u0930\u0939\u093e \u0939\u0948" },
            { "electrical_safety", "\u092c\u093f\u091c\u0932\u0940 \u0938\u0941\u0930\u0915\u094d\u0937\u093e" },
            { "ppe_safety", "\u0938\u0941\u0930\u0915\u094d\u0937\u093e \u0909\u092a\u0915\u0930\u0923 \u092a\u094d\u0930\u0936\u093f\u0915\u094d\u0937\u0923" },
            { "emergency_response", "\u0906\u092a\u093e\u0924\u0915\u093e\u0932\u0940\u0928 \u092a\u094d\u0930\u0924\u093f\u0915\u094d\u0930\u093f\u092f\u093e" },
            { "fire_intro_title", "\u0905\u0917\u094d\u0928\u093f \u0938\u0941\u0930\u0915\u094d\u0937\u093e \u092a\u094d\u0930\u0936\u093f\u0915\u094d\u0937\u0923" },
            { "fire_intro_body",
                "\u0906\u092a \u0915\u094d\u092f\u093e \u0938\u0940\u0916\u0947\u0902\u0917\u0947:\n" +
                "\u2022 \u0905\u0917\u094d\u0928\u093f \u0938\u0941\u0930\u0915\u094d\u0937\u093e \u091c\u093e\u0917\u0930\u0942\u0915\u0924\u093e\n" +
                "\u2022 \u0905\u0917\u094d\u0928\u093f\u0936\u093e\u092e\u0915 \u092f\u0902\u0924\u094d\u0930 \u0938\u0947 \u092a\u0930\u093f\u091a\u092f\n" +
                "\u2022 \u0906\u092a\u093e\u0924\u0915\u093e\u0932\u0940\u0928 \u0915\u094d\u0930\u092e\n" +
                "\u2022 \u0907\u0902\u091f\u0930\u0948\u0915\u094d\u091f\u093f\u0935 AR \u0905\u092d\u094d\u092f\u093e\u0938" },
            { "equipment_title", "\u0938\u0941\u0930\u0915\u094d\u0937\u093e \u0909\u092a\u0915\u0930\u0923" },
            { "equipment_body",
                "\u0905\u092a\u0928\u0947 \u0938\u0941\u0930\u0915\u094d\u0937\u093e \u0909\u092a\u0915\u0930\u0923 \u091c\u093e\u0928\u0947\u0902:\n" +
                "\u2022 \u0905\u0917\u094d\u0928\u093f\u0936\u093e\u092e\u0915 - \u091b\u094b\u091f\u0940 \u0906\u0917 \u092c\u0941\u091d\u093e\u090f\u0902\n" +
                "\u2022 \u0938\u0941\u0930\u0915\u094d\u0937\u093e \u0939\u0947\u0932\u092e\u0947\u091f - \u0938\u093f\u0930 \u0915\u0940 \u0930\u0915\u094d\u0937\u093e\n" +
                "\u2022 \u0938\u0941\u0930\u0915\u094d\u0937\u093e \u0926\u0938\u094d\u0924\u093e\u0928\u0947 - \u0939\u093e\u0925\u094b\u0902 \u0915\u0940 \u0930\u0915\u094d\u0937\u093e\n" +
                "\u2022 \u0938\u0941\u0930\u0915\u094d\u0937\u093e \u091c\u0942\u0924\u0947 - \u092a\u0948\u0930\u094b\u0902 \u0915\u0940 \u0930\u0915\u094d\u0937\u093e\n" +
                "\u2022 \u0906\u0901\u0916/\u091a\u0947\u0939\u0930\u0947 \u0915\u0940 \u0930\u0915\u094d\u0937\u093e\n" +
                "\u2022 \u0906\u092a\u093e\u0924\u0915\u093e\u0932 \u092c\u091c\u093e - \u092a\u0939\u0932\u0947 \u0905\u0932\u0930\u094d\u091f \u0915\u0930\u0947\u0902" },
            { "howto_title", "\u0907\u0938 \u092a\u094d\u0930\u0936\u093f\u0915\u094d\u0937\u0923 \u0915\u093e \u0909\u092a\u092f\u094b\u0917" },
            { "howto_step1", "\u0905\u0917\u094d\u0928\u093f \u0926\u0943\u0936\u094d\u092f\u0915\u094b\u0902 \u092b\u093c\u0930\u094d\u0936 \u092a\u0930 \u0930\u0916\u0947\u0902\u0964" },
            { "howto_step2", "\u0905\u0932\u0930\u094d\u091f \u091a\u0932\u093e\u090f\u0901\u0964" },
            { "howto_step3", "\u0905\u0917\u094d\u0928\u093f\u0936\u093e\u092e\u0915 \u0909\u0920\u093e\u090f\u0901\u0964" },
            { "howto_step4", "\u0938\u0941\u0930\u0915\u094d\u0937\u093e \u092a\u093f\u0928 \u0928\u093f\u0915\u093e\u0932\u0947\u0902\u0964" },
            { "howto_step5", "\u0906\u0917 \u092a\u0930 \u0905\u0917\u094d\u0928\u093f\u0936\u093e\u092e\u0915 \u092a\u094d\u0930\u092f\u094b\u0917 \u0915\u0930\u0947\u0902\u0964" },
            { "howto_step6", "\u0906\u0917 \u092c\u0941\u091d\u093e\u0915\u0930 \u092a\u094d\u0930\u0936\u093f\u0915\u094d\u0937\u0923 \u092a\u0942\u0930\u093e \u0915\u0930\u0947\u0902\u0964" },
            { "start_training", "\u092a\u094d\u0930\u0936\u093f\u0915\u094d\u0937\u0923 \u0936\u0941\u0930\u0942 \u0915\u0930\u0947\u0902" },
            { "back", "\u0935\u093e\u092a\u0938" },
            { "continue", "\u0906\u0917\u0947 \u092c\u0922\u0947\u0902" },
            { "step", "\u091a\u0930\u0923" },
            { "of", "OF" },
            { "welcome", "\u0938\u094d\u0935\u093e\u0917\u0924" },
            { "training_complete", "\u092a\u094d\u0930\u0936\u093f\u0915\u094d\u0937\u0923 \u092a\u0942\u0930\u094d\u0923" },
            { "retry", "\u092a\u0941\u0928\u0930\u094d\u092a\u094d\u0930\u092f\u093e\u0938" },
            { "home", "\u0939\u094b\u092e" }


        };
    }
}
