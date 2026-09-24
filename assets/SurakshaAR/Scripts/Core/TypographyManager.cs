using System;
using System.Collections.Generic;
using SurakshaAR.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Core
{
    public enum SemanticStyle
    {
        Display,        // Hero / Brand: 52px (48-60)
        PageTitle,      // Main Page Title: 44px (40-48)
        SectionTitle,   // Section Header: 36px (32-38)
        WorkerName,     // Worker Profile Name: 34px (32-38)
        CardTitle,      // Card / Tile Title: 30px (28-34)
        Body,           // Standard Body text: 26px (24-28)
        BodyMedium,     // Emphasized Body: 26px (24-28, SemiBold)
        Secondary,      // Secondary / Subtitle: 23px (22-24)
        Metadata,       // Metadata / Timestamps / Badges: 21px (20-23)
        Button,         // Button CTA text: 26px (24-28)
        Input,          // Text Input Fields: 25px (24-26)
        Navigation,     // Bottom Navigation Items: 21px (20-22)
        ARStep,         // AR Step Badge: 30px (28-32)
        ARInstruction,  // AR Main Instruction: 38px (36-40, 1.5x-2x enlarged)
        ARSupporting,   // AR Supporting Description: 27px (26-28)
        ARTimer,        // AR Countdown / Stopwatch: 34px (32-36)
        ARWarning       // AR Safety Alert / Warning: 34px (32-36)
    }

    public struct TypographyConfig
    {
        public float FontSize;
        public FontWeight Weight;
        public float LineSpacing;
        public float CharacterSpacing;
        public TextOverflowModes OverflowMode;
        public bool WordWrap;

        public TypographyConfig(float size, FontWeight weight = FontWeight.Regular, float lineSpacing = 0f, float charSpacing = 0f, TextOverflowModes overflow = TextOverflowModes.Overflow, bool wrap = true)
        {
            FontSize = size;
            Weight = weight;
            LineSpacing = lineSpacing;
            CharacterSpacing = charSpacing;
            OverflowMode = overflow;
            WordWrap = wrap;
        }
    }

    /// <summary>
    /// Centralized Typography Manager enforcing global semantic typography across all screens.
    /// Manages reference resolution scaling (1080x2400) and ensures text wraps and containers grow,
    /// rather than text shrinking into unreadable sizes.
    /// </summary>
    public static class TypographyManager
    {
        private static readonly Dictionary<SemanticStyle, TypographyConfig> Styles = new Dictionary<SemanticStyle, TypographyConfig>
        {
            { SemanticStyle.Display,       new TypographyConfig(66f, FontWeight.Bold,     2f, 0.5f, TextOverflowModes.Overflow, true) },
            { SemanticStyle.PageTitle,     new TypographyConfig(54f, FontWeight.Bold,     2f, 0f,   TextOverflowModes.Overflow, true) },
            { SemanticStyle.SectionTitle,  new TypographyConfig(44f, FontWeight.SemiBold, 2f, 0f,   TextOverflowModes.Overflow, true) },
            { SemanticStyle.WorkerName,    new TypographyConfig(46f, FontWeight.Bold,     0f, 0f,   TextOverflowModes.Ellipsis, false) },
            { SemanticStyle.CardTitle,     new TypographyConfig(42f, FontWeight.SemiBold, 2f, 0f,   TextOverflowModes.Overflow, true) },
            { SemanticStyle.Body,          new TypographyConfig(34f, FontWeight.Regular,  4f, 0f,   TextOverflowModes.Overflow, true) },
            { SemanticStyle.BodyMedium,    new TypographyConfig(34f, FontWeight.Medium,   4f, 0f,   TextOverflowModes.Overflow, true) },
            { SemanticStyle.Secondary,     new TypographyConfig(30f, FontWeight.Regular,  2f, 0f,   TextOverflowModes.Overflow, true) },
            { SemanticStyle.Metadata,      new TypographyConfig(28f, FontWeight.Medium,   0f, 0.5f, TextOverflowModes.Ellipsis, false) },
            { SemanticStyle.Button,        new TypographyConfig(36f, FontWeight.SemiBold, 0f, 0.5f, TextOverflowModes.Overflow, false) },
            { SemanticStyle.Input,         new TypographyConfig(34f, FontWeight.Regular,  0f, 0f,   TextOverflowModes.Ellipsis, false) },
            { SemanticStyle.Navigation,    new TypographyConfig(30f, FontWeight.Bold,     0f, 0f,   TextOverflowModes.Ellipsis, false) },
            { SemanticStyle.ARStep,        new TypographyConfig(40f, FontWeight.Bold,     0f, 1f,   TextOverflowModes.Overflow, false) },
            { SemanticStyle.ARInstruction, new TypographyConfig(50f, FontWeight.Bold,     6f, 0f,   TextOverflowModes.Overflow, true) },
            { SemanticStyle.ARSupporting,  new TypographyConfig(36f, FontWeight.Regular,  4f, 0f,   TextOverflowModes.Overflow, true) },
            { SemanticStyle.ARTimer,       new TypographyConfig(48f, FontWeight.Bold,     0f, 1f,   TextOverflowModes.Overflow, false) },
            { SemanticStyle.ARWarning,     new TypographyConfig(44f, FontWeight.Bold,     4f, 0f,   TextOverflowModes.Overflow, true) }
        };

        public static TypographyConfig GetConfig(SemanticStyle style)
        {
            if (Styles.TryGetValue(style, out var config)) return config;
            return Styles[SemanticStyle.Body];
        }

        /// <summary>
        /// Applies centralized semantic typography tokens to a TextMeshProUGUI element.
        /// Configures font asset, font size, font weight, line spacing, overflow, and word wrapping.
        /// </summary>
        public static void ApplyStyle(TextMeshProUGUI tmp, SemanticStyle style, AppLanguage lang = AppLanguage.English)
        {
            if (tmp == null) return;

            var config = GetConfig(style);
            tmp.font = FontManager.Instance.GetFontForLanguage(lang);
            tmp.fontSize = config.FontSize;
            tmp.fontWeight = config.Weight;
            tmp.lineSpacing = config.LineSpacing;
            tmp.characterSpacing = config.CharacterSpacing;
            tmp.textWrappingMode = config.WordWrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.overflowMode = config.OverflowMode;

            // Never allow auto-shrink into tiny unreadable text
            tmp.enableAutoSizing = false;

            // Strict LTR guarantee: Ol Chiki (Santali), Devanagari (Hindi), and Latin (English) are strictly LTR
            tmp.isRightToLeftText = false;
            var rt = tmp.rectTransform;
            if (rt != null && rt.localScale.x < 0f)
            {
                var s = rt.localScale;
                s.x = Mathf.Abs(s.x);
                rt.localScale = s;
            }
        }

        /// <summary>
        /// Updates the font of an existing text element when language changes, preserving its semantic style.
        /// </summary>
        public static void UpdateFontForLanguage(TextMeshProUGUI tmp, AppLanguage lang)
        {
            if (tmp == null) return;
            tmp.font = FontManager.Instance.GetFontForLanguage(lang);
        }
    }
}
