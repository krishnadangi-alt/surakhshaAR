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
            EditorApplication.delayCall += EnsureDevanagariAsset;
        }

        [MenuItem("SurakshaAR/Ensure Devanagari Font Asset")]
        public static void EnsureDevanagariAsset()
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
                        Debug.Log($"[DevanagariFontAssetCreator] Created {assetPath} with material and texture sub-assets");
                        existing = fontAsset;
                    }
                }
            }

            // Also clean up LiberationSans fallback table and ensure valid registration
            var liberation = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (liberation != null)
            {
                if (liberation.fallbackFontAssetTable != null)
                {
                    liberation.fallbackFontAssetTable.RemoveAll(f => f == null || f.material == null);
                }

                if (existing != null && existing.material != null)
                {
                    if (liberation.fallbackFontAssetTable == null)
                        liberation.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

                    if (!liberation.fallbackFontAssetTable.Contains(existing))
                    {
                        liberation.fallbackFontAssetTable.Add(existing);
                        EditorUtility.SetDirty(liberation);
                        AssetDatabase.SaveAssets();
                        Debug.Log("[DevanagariFontAssetCreator] Added valid NotoSansDevanagari SDF to LiberationSans fallback table");
                    }
                }
            }

            // Ensure Santali / Ol Chiki font if Nirmala is available on Windows
            EnsureSantaliAsset(liberation);
        }

        private static void EnsureSantaliAsset(TMP_FontAsset fallbackHost)
        {
            const string nirmalaAssetPath = "Assets/Resources/Fonts/Nirmala SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(nirmalaAssetPath);
            if (existing == null)
            {
                string sysFonts = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts);
                string nirmalaSrc = System.IO.Path.Combine(sysFonts, "Nirmala.ttf");
                if (System.IO.File.Exists(nirmalaSrc))
                {
                    string targetTtf = "Assets/Resources/Fonts/Nirmala.ttf";
                    if (!System.IO.File.Exists(targetTtf))
                    {
                        System.IO.File.Copy(nirmalaSrc, targetTtf, true);
                        AssetDatabase.Refresh();
                    }

                    var font = AssetDatabase.LoadAssetAtPath<Font>(targetTtf);
                    if (font != null)
                    {
                        var fontAsset = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                        if (fontAsset != null)
                        {
                            AssetDatabase.CreateAsset(fontAsset, nirmalaAssetPath);
                            if (fontAsset.atlasTexture != null)
                            {
                                fontAsset.atlasTexture.name = "Nirmala Atlas";
                                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
                            }
                            if (fontAsset.material != null)
                            {
                                fontAsset.material.name = "Nirmala Material";
                                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                            }
                            AssetDatabase.SaveAssets();
                            existing = fontAsset;
                            Debug.Log($"[DevanagariFontAssetCreator] Created {nirmalaAssetPath} for Santali/Ol Chiki support");
                        }
                    }
                }
            }

            if (existing != null && fallbackHost != null && fallbackHost.fallbackFontAssetTable != null)
            {
                if (!fallbackHost.fallbackFontAssetTable.Contains(existing))
                {
                    fallbackHost.fallbackFontAssetTable.Add(existing);
                    EditorUtility.SetDirty(fallbackHost);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
