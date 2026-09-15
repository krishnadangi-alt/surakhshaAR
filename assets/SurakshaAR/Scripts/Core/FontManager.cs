using System;
using System.Collections.Generic;
using SurakshaAR.Data;
using TMPro;
using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Centralized font management system for SurakshaAR.
    /// Manages Latin, Devanagari, and Ol Chiki fonts with robust fallback chains.
    /// Eliminates missing glyph boxes (□□□□) across English, Hindi, and Santali.
    /// </summary>
    public class FontManager
    {
        private static FontManager _instance;
        public static FontManager Instance => _instance ?? (_instance = new FontManager());

        private TMP_FontAsset _latinFont;
        private TMP_FontAsset _devanagariFont;
        private TMP_FontAsset _olChikiFont;

        public TMP_FontAsset LatinFont => _latinFont ?? GetDefaultFont();
        public TMP_FontAsset DevanagariFont => _devanagariFont ?? GetDevanagariFont();
        public TMP_FontAsset OlChikiFont => _olChikiFont ?? GetOlChikiFont();

        public FontManager()
        {
            _instance = this;
            InitializeFonts();
        }

        private void InitializeFonts()
        {
            _latinFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (_latinFont == null)
            {
                _latinFont = Resources.Load<TMP_FontAsset>("Fonts/LiberationSans SDF") ?? TMP_Settings.defaultFontAsset;
            }

            _devanagariFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansDevanagari SDF")
                           ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansDevanagari SDF");

            _olChikiFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansOlChiki SDF")
                        ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansOlChiki SDF");

            EnsureFallbackChains();
        }

        public TMP_FontAsset GetFontForLanguage(AppLanguage language)
        {
            switch (language)
            {
                case AppLanguage.Hindi:
                    return DevanagariFont;
                case AppLanguage.Santali:
                    return OlChikiFont;
                case AppLanguage.English:
                default:
                    return LatinFont;
            }
        }

        private TMP_FontAsset GetDefaultFont()
        {
            if (_latinFont != null && _latinFont.material != null) return _latinFont;
            _latinFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF") ?? TMP_Settings.defaultFontAsset;
            return _latinFont;
        }

        private TMP_FontAsset GetDevanagariFont()
        {
            if (_devanagariFont != null && _devanagariFont.material != null) return _devanagariFont;

            _devanagariFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansDevanagari SDF")
                           ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansDevanagari SDF");

            if (_devanagariFont == null || _devanagariFont.material == null)
            {
                var font = Resources.Load<Font>("Fonts/NotoSansDevanagari");
                if (font != null)
                {
                    try
                    {
                        var created = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                        if (created != null && created.material != null)
                        {
                            created.name = "NotoSansDevanagari Dynamic";
                            _devanagariFont = created;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[FontManager] Dynamic Devanagari font asset creation failed: {ex.Message}");
                    }
                }
            }

            return _devanagariFont ?? LatinFont;
        }

        private TMP_FontAsset GetOlChikiFont()
        {
            if (_olChikiFont != null && _olChikiFont.material != null) return _olChikiFont;

            _olChikiFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansOlChiki SDF")
                        ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansOlChiki SDF");

            if (_olChikiFont == null || _olChikiFont.material == null)
            {
                var font = Resources.Load<Font>("Fonts/NotoSansOlChiki");
                if (font != null)
                {
                    try
                    {
                        var created = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                        if (created != null && created.material != null)
                        {
                            created.name = "NotoSansOlChiki Dynamic";
                            _olChikiFont = created;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[FontManager] Dynamic Ol Chiki font asset creation failed: {ex.Message}");
                    }
                }
            }

            return _olChikiFont ?? LatinFont;
        }

        public void EnsureFallbackChains()
        {
            var latin = LatinFont;
            var dev = GetDevanagariFont();
            var ol = GetOlChikiFont();

            if (latin != null)
            {
                if (latin.fallbackFontAssetTable == null)
                    latin.fallbackFontAssetTable = new List<TMP_FontAsset>();

                latin.fallbackFontAssetTable.RemoveAll(f => f == null || f.material == null);

                if (dev != null && dev.material != null && dev != latin && !latin.fallbackFontAssetTable.Contains(dev))
                    latin.fallbackFontAssetTable.Add(dev);

                if (ol != null && ol.material != null && ol != latin && !latin.fallbackFontAssetTable.Contains(ol))
                    latin.fallbackFontAssetTable.Add(ol);
            }

            if (dev != null && dev != latin)
            {
                if (dev.fallbackFontAssetTable == null)
                    dev.fallbackFontAssetTable = new List<TMP_FontAsset>();

                dev.fallbackFontAssetTable.RemoveAll(f => f == null || f.material == null);

                if (latin != null && !dev.fallbackFontAssetTable.Contains(latin))
                    dev.fallbackFontAssetTable.Add(latin);
                if (ol != null && ol != dev && !dev.fallbackFontAssetTable.Contains(ol))
                    dev.fallbackFontAssetTable.Add(ol);
            }

            if (ol != null && ol != latin)
            {
                if (ol.fallbackFontAssetTable == null)
                    ol.fallbackFontAssetTable = new List<TMP_FontAsset>();

                ol.fallbackFontAssetTable.RemoveAll(f => f == null || f.material == null);

                if (latin != null && !ol.fallbackFontAssetTable.Contains(latin))
                    ol.fallbackFontAssetTable.Add(latin);
                if (dev != null && dev != ol && !ol.fallbackFontAssetTable.Contains(dev))
                    ol.fallbackFontAssetTable.Add(dev);
            }
        }
    }
}
