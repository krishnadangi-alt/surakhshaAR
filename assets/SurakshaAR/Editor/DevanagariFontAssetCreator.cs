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
            if (existing == null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
                if (font != null)
                {
                    var fontAsset = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                    if (fontAsset != null)
                    {
                        AssetDatabase.CreateAsset(fontAsset, assetPath);
                        AssetDatabase.SaveAssets();
                        Debug.Log($"[DevanagariFontAssetCreator] Created {assetPath}");
                        existing = fontAsset;
                    }
                }
            }

            // Also ensure it is registered in LiberationSans fallback table
            var liberation = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (liberation != null && existing != null)
            {
                if (liberation.fallbackFontAssetTable == null)
                    liberation.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

                if (!liberation.fallbackFontAssetTable.Contains(existing))
                {
                    liberation.fallbackFontAssetTable.Add(existing);
                    EditorUtility.SetDirty(liberation);
                    AssetDatabase.SaveAssets();
                    Debug.Log("[DevanagariFontAssetCreator] Added NotoSansDevanagari SDF to LiberationSans fallback table");
                }
            }
        }
    }
}
