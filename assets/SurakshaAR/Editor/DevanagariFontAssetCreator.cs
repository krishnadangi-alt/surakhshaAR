using TMPro;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class DevanagariFontAssetCreator
    {
        static DevanagariFontAssetCreator()
        {
            EditorApplication.delayCall += EnsureAllFontAssets;
        }

        [MenuItem("SurakshaAR/Ensure All Font Assets (Hindi + Santali)")]
        public static void EnsureAllFontAssets()
        {
            try
            {
                EnsureDevanagariAsset();
                EnsureOlChikiAsset();
                EnsureFallbackChain();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[DevanagariFontAssetCreator] Font asset generation deferred: {ex.Message}");
            }
        }

        private static TMP_FontAsset EnsureDevanagariAsset()
        {
            const string ttfPath = "Assets/Resources/Fonts/NotoSansDevanagari.ttf";
            const string assetPath = "Assets/Resources/Fonts/NotoSansDevanagari SDF.asset";

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            bool isCorrupt = false;
            if (existing != null)
            {
                try
                {
                    if (existing.material == null || !existing.HasCharacter(0x0915) || !existing.HasCharacter(0xE02B) || !existing.HasCharacter(0xE17D) || !existing.HasCharacter(0xE1B5) || !existing.HasCharacter(0xE1D7))
                        isCorrupt = true;
                    if (existing.atlasTextures == null || existing.atlasTextures.Length == 0 || existing.atlasTextures[0] == null)
                        isCorrupt = true;
                    else
                    {
                        for (int i = 0; i < existing.atlasTextures.Length; i++)
                        {
                            if (existing.atlasTextures[i] == null) { isCorrupt = true; break; }
                        }
                    }
                }
                catch
                {
                    isCorrupt = true;
                }

                if (isCorrupt)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    existing = null;
                }
            }

            if (existing == null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
                if (font != null)
                {
                    var fontAsset = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
                    if (fontAsset != null)
                    {
                        fontAsset.isMultiAtlasTexturesEnabled = false;
                        var sb = new System.Text.StringBuilder(4096);
                        for (int c = 0x0900; c <= 0x097F; c++) sb.Append((char)c);
                        for (int c = 0xE000; c <= 0xE250; c++) sb.Append((char)c);

                        sb.Append(" 0123456789%+-=()[]{}<>/\\|:;.,!?~@#$^&*'\"•✓★→➔↗›‹✔✓");
                        sb.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

                        fontAsset.TryAddCharacters(sb.ToString());
                        fontAsset.isMultiAtlasTexturesEnabled = false;

                        // Calibrate zero-advance combining marks for Devanagari in TextMeshPro
                        if (fontAsset.characterTable != null && fontAsset.glyphTable != null)
                        {
                            foreach (var ch in fontAsset.characterTable)
                            {
                                if (ch.unicode == 0x093F ||
                                    (ch.unicode >= 0xE1D4 && ch.unicode <= 0xE1DF) ||
                                    (ch.unicode >= 0xE1E1 && ch.unicode <= 0xE1EB) ||
                                    ch.unicode == 0xE204)
                                {
                                    var g = fontAsset.glyphTable.Find(gl => gl.index == ch.glyphIndex);
                                    if (g != null)
                                    {
                                        var gm = g.metrics;
                                        float bx = ch.unicode == 0xE204 ? 0f : -3.5f;
                                        g.metrics = new UnityEngine.TextCore.GlyphMetrics(gm.width, gm.height, bx, gm.horizontalBearingY, 0f);
                                    }
                                }
                            }
                        }

                        AssetDatabase.CreateAsset(fontAsset, assetPath);
                        if (fontAsset.atlasTexture != null)
                        {
                            fontAsset.atlasTexture.name = "NotoSansDevanagari Atlas";
                            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
                        }
                        if (fontAsset.material != null)
                        {
                            fontAsset.material.name = "NotoSansDevanagari Material";
                            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                        }
                        AssetDatabase.SaveAssets();
                        Debug.Log($"[FontAssetCreator] Created clean single-atlas {assetPath} with material and texture sub-assets");
                        existing = fontAsset;
                    }
                }
            }
            return existing;
        }

        private static TMP_FontAsset EnsureOlChikiAsset()
        {
            const string ttfPath = "Assets/Resources/Fonts/NotoSansOlChiki.ttf";
            const string assetPath = "Assets/Resources/Fonts/NotoSansOlChiki SDF.asset";

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null && (existing.material == null || !existing.HasCharacter(0x1C5A)))
            {
                AssetDatabase.DeleteAsset(assetPath);
                existing = null;
            }

            if (existing == null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
                if (font != null)
                {
                    var fontAsset = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                    if (fontAsset != null)
                    {
                        // Ol Chiki Unicode block: U+1C50 to U+1C7F
                        const string olChikiGlyphs = "᱐᱑᱒᱓᱔᱕᱖᱗᱘᱙ᱚᱛᱜᱝᱞᱟᱠᱡᱢᱣᱤᱥᱦᱧᱨᱩᱪᱫᱬᱭᱮᱯᱰᱱᱲᱳᱴᱵᱶᱷᱸᱹᱺᱻᱼᱽ᱾᱿";
                        fontAsset.TryAddCharacters(olChikiGlyphs);

                        AssetDatabase.CreateAsset(fontAsset, assetPath);
                        if (fontAsset.atlasTexture != null)
                        {
                            fontAsset.atlasTexture.name = "NotoSansOlChiki Atlas";
                            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
                        }
                        if (fontAsset.material != null)
                        {
                            fontAsset.material.name = "NotoSansOlChiki Material";
                            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                        }
                        AssetDatabase.SaveAssets();
                        Debug.Log($"[FontAssetCreator] Created {assetPath} for Santali/Ol Chiki support");
                        existing = fontAsset;
                    }
                }
            }
            return existing;
        }

        private static void EnsureFallbackChain()
        {
            var liberation = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (liberation == null) return;

            if (liberation.fallbackFontAssetTable == null)
                liberation.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

            liberation.fallbackFontAssetTable.RemoveAll(f => f == null || f.material == null);

            var devAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Fonts/NotoSansDevanagari SDF.asset");
            if (devAsset != null && devAsset.material != null && !liberation.fallbackFontAssetTable.Contains(devAsset))
            {
                liberation.fallbackFontAssetTable.Add(devAsset);
            }

            var olChikiAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Fonts/NotoSansOlChiki SDF.asset");
            if (olChikiAsset != null && olChikiAsset.material != null && !liberation.fallbackFontAssetTable.Contains(olChikiAsset))
            {
                liberation.fallbackFontAssetTable.Add(olChikiAsset);
            }

            EditorUtility.SetDirty(liberation);
            AssetDatabase.SaveAssets();
            Debug.Log("[FontAssetCreator] Updated LiberationSans fallback chain with Devanagari and Ol Chiki");
        }
    }
}
