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

        private static string GetDevanagariCharacterSet()
        {
            var sb = new System.Text.StringBuilder(1024);
            // Complete Unicode Devanagari block: U+0900 to U+097F
            for (int c = 0x0900; c <= 0x097F; c++)
            {
                sb.Append((char)c);
            }
            // Unicode PUA range for shaped Devanagari ligatures: 0xE000 to 0xE2B0
            for (int c = 0xE000; c <= 0xE2B0; c++)
            {
                sb.Append((char)c);
            }
            // Essential ASCII, digits, punctuation, and UI symbols
            sb.Append(" 0123456789%+-=()[]{}<>/\\|:;.,!?~@#$^&*'\"•✓★→➔↗›‹✔✓");
            sb.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
            return sb.ToString();
        }

        private TMP_FontAsset GetDevanagariFont()
        {
            if (_devanagariFont != null)
            {
                try
                {
                    if (_devanagariFont.material != null && _devanagariFont.atlasTexture != null && _devanagariFont.HasCharacter(0x0915))
                        return _devanagariFont;
                }
                catch
                {
                    _devanagariFont = null;
                }
            }

            string devanagariGlyphs = GetDevanagariCharacterSet();

            try
            {
                _devanagariFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansDevanagari SDF")
                               ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansDevanagari SDF");

                if (_devanagariFont != null)
                {
                    if (_devanagariFont.atlasTextures == null || _devanagariFont.atlasTextures.Length == 0 || _devanagariFont.atlasTextures[0] == null)
                    {
                        _devanagariFont = null;
                    }
                    else
                    {
                        for (int i = 0; i < _devanagariFont.atlasTextures.Length; i++)
                        {
                            if (_devanagariFont.atlasTextures[i] == null) { _devanagariFont = null; break; }
                        }
                    }
                }

                if (_devanagariFont != null)
                {
                    if (_devanagariFont.material == null && _devanagariFont.atlasTexture != null)
                    {
                        var shader = Shader.Find("TextMeshPro/Distance Field") ?? Shader.Find("TextMeshPro/Mobile/Distance Field");
                        if (shader != null)
                        {
                            _devanagariFont.material = new Material(shader);
                            _devanagariFont.material.mainTexture = _devanagariFont.atlasTexture;
                        }
                    }
                    _devanagariFont.TryAddCharacters(devanagariGlyphs);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FontManager] Failed loading Devanagari asset: {ex.Message}");
                _devanagariFont = null;
            }

            if (_devanagariFont == null || _devanagariFont.material == null || !_devanagariFont.HasCharacter(0x0915) || !_devanagariFont.HasCharacter(0xE02B))
            {
                var font = Resources.Load<Font>("Fonts/NotoSansDevanagari");
                if (font != null)
                {
                    try
                    {
                        var created = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
                        if (created != null)
                        {
                            if (created.material == null && created.atlasTexture != null)
                            {
                                var shader = Shader.Find("TextMeshPro/Distance Field") ?? Shader.Find("TextMeshPro/Mobile/Distance Field");
                                if (shader != null)
                                {
                                    created.material = new Material(shader);
                                    created.material.mainTexture = created.atlasTexture;
                                }
                            }
                            created.name = "NotoSansDevanagari Dynamic";
                            created.isMultiAtlasTexturesEnabled = false;
                            created.TryAddCharacters(devanagariGlyphs);
                            if (created.HasCharacter(0x0915)) _devanagariFont = created;
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
            if (_olChikiFont != null && _olChikiFont.material != null && _olChikiFont.HasCharacter(0x1C5A))
                return _olChikiFont;

            const string olChikiGlyphs = "᱐᱑᱒᱓᱔᱕᱖᱗᱘᱙ᱚᱛᱜᱝᱞᱟᱠᱡᱢᱣᱤᱥᱦᱧᱨᱩᱪᱫᱬᱭᱮᱯᱰᱱᱲᱳᱴᱵᱶᱷᱸᱹᱺᱻᱼᱽ᱾᱿ ";

            _olChikiFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansOlChiki SDF")
                        ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansOlChiki SDF");

            if (_olChikiFont != null)
            {
                if (_olChikiFont.material == null && _olChikiFont.atlasTexture != null)
                {
                    var shader = Shader.Find("TextMeshPro/Distance Field") ?? Shader.Find("TextMeshPro/Mobile/Distance Field");
                    if (shader != null)
                    {
                        _olChikiFont.material = new Material(shader);
                        _olChikiFont.material.mainTexture = _olChikiFont.atlasTexture;
                    }
                }
                _olChikiFont.TryAddCharacters(olChikiGlyphs);
            }

            if (_olChikiFont == null || _olChikiFont.material == null || !_olChikiFont.HasCharacter(0x1C5A))
            {
                var font = Resources.Load<Font>("Fonts/NotoSansOlChiki");
                if (font != null)
                {
                    try
                    {
                        var created = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                        if (created != null)
                        {
                            if (created.material == null && created.atlasTexture != null)
                            {
                                var shader = Shader.Find("TextMeshPro/Distance Field") ?? Shader.Find("TextMeshPro/Mobile/Distance Field");
                                if (shader != null)
                                {
                                    created.material = new Material(shader);
                                    created.material.mainTexture = created.atlasTexture;
                                }
                            }
                            created.name = "NotoSansOlChiki Dynamic";
                            created.TryAddCharacters(olChikiGlyphs);
                            if (created.HasCharacter(0x1C5A)) _olChikiFont = created;
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
