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
            EnsureDevanagariAsset();
            EnsureOlChikiAsset();
            EnsureFallbackChain();
        }

        private static TMP_FontAsset EnsureDevanagariAsset()
        {
            const string ttfPath = "Assets/Resources/Fonts/NotoSansDevanagari.ttf";
            const string assetPath = "Assets/Resources/Fonts/NotoSansDevanagari SDF.asset";

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null && existing.material == null)
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
                    if (fontAsset != null && fontAsset.material != null)
                    {
                        const string devanagariGlyphs = "अआइईउऊऋएऐओऔकखगघङचछजझञटठडढणतथदधनपफबभमयरलवशषसहक्षत्रज्ञािीुूृेैोौंः्०१२३४५६७८९";
                        fontAsset.TryAddCharacters(devanagariGlyphs);

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
                        Debug.Log($"[FontAssetCreator] Created {assetPath} with material and texture sub-assets");
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
            if (existing != null && existing.material == null)
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
