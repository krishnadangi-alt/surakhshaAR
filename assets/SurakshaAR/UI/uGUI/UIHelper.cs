using System.Collections.Generic;
using SurakshaAR.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI
{
    /// <summary>
    /// Utility helpers for building uGUI hierarchies at runtime.
    /// All screen builders use these helpers to create consistent components.
    /// </summary>
    public static class UIHelper
    {
        // ──────────────────────────────────────────────────────────────
        //  FINDING
        // ──────────────────────────────────────────────────────────────

        /// <summary>Search all child Buttons recursively and return the one with matching name.</summary>
        public static Button FindButton(GameObject root, string name)
        {
            foreach (var b in root.GetComponentsInChildren<Button>(true))
                if (b.gameObject.name == name) return b;
            return null;
        }

        /// <summary>Find a TextMeshProUGUI by name in children.</summary>
        public static TextMeshProUGUI FindTMP(GameObject root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                if (t.gameObject.name == name) return t;
            return null;
        }

        /// <summary>Find an Image by name in children.</summary>
        public static Image FindImage(GameObject root, string name)
        {
            foreach (var img in root.GetComponentsInChildren<Image>(true))
                if (img.gameObject.name == name) return img;
            return null;
        }

        /// <summary>Find a RectTransform by name in children.</summary>
        public static RectTransform FindRect(GameObject root, string name)
        {
            foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
                if (rt.gameObject.name == name) return rt;
            return null;
        }

        /// <summary>Find a TMP_InputField by name in children.</summary>
        public static TMP_InputField FindInputField(GameObject root, string name)
        {
            foreach (var f in root.GetComponentsInChildren<TMP_InputField>(true))
                if (f.gameObject.name == name) return f;
            return null;
        }

        /// <summary>Safely destroys a GameObject or Component whether in Play Mode or Edit Mode.</summary>
        public static void SafeDestroy(UnityEngine.Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  CREATING GAME OBJECTS
        // ──────────────────────────────────────────────────────────────

        /// <summary>Create a plain RectTransform GameObject parented to parent.</summary>
        public static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Create a full-stretch RectTransform (fills parent entirely).</summary>
        public static RectTransform MakeStretchRect(string name, Transform parent)
        {
            var rt = MakeRect(name, parent);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>Create a panel (Image) with specified background color.</summary>
        public static Image MakePanel(string name, Transform parent, Color color)
        {
            var rt = MakeStretchRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private static TMP_FontAsset _defaultFont;
        private static TMP_FontAsset _devanagariFont;

        /// <summary>
        /// Returns the primary TMP Font Asset. Tries NotoSans (covers Devanagari/Hindi)
        /// then falls back to LiberationSans, then TMP_Settings default.
        /// </summary>
        public static TMP_FontAsset GetDefaultFont()
        {
            if (_defaultFont != null) return _defaultFont;
            // Primary font: LiberationSans SDF matching the navigation menu
            _defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (_defaultFont == null)
                _defaultFont = TMP_Settings.defaultFontAsset;

            EnsureFontFallbacks(_defaultFont);
            return _defaultFont;
        }

        /// <summary>
        /// Registers bundled Devanagari or fallback font assets if present in Resources
        /// so that regional glyphs render cleanly.
        /// </summary>
        public static void EnsureFontFallbacks(TMP_FontAsset baseFont)
        {
            if (baseFont == null) return;
            if (baseFont.fallbackFontAssetTable == null)
                baseFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

            // Purge any corrupted or material-less fallback assets
            baseFont.fallbackFontAssetTable.RemoveAll(f => f == null || f.material == null);

            var dev = GetDevanagariFont();
            if (dev != null && dev.material != null && dev != baseFont && !baseFont.fallbackFontAssetTable.Contains(dev))
            {
                baseFont.fallbackFontAssetTable.Add(dev);
            }

            var santali = GetSantaliFont();
            if (santali != null && santali.material != null && santali != baseFont && !baseFont.fallbackFontAssetTable.Contains(santali))
            {
                baseFont.fallbackFontAssetTable.Add(santali);
            }
        }

        /// <summary>
        /// Returns true if the active TMP font asset or any loaded fallback
        /// contains glyphs for the Devanagari script (checked via 'क' U+0915).
        /// </summary>
        public static bool HasDevanagariSupport()
        {
            var font = GetDevanagariFont();
            if (font == null) return false;
            if (font.HasCharacter(0x0915)) return true;
            if (font.fallbackFontAssetTable != null)
            {
                foreach (var fb in font.fallbackFontAssetTable)
                {
                    if (fb != null && fb.material != null && fb.HasCharacter(0x0915)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the active TMP font asset or any loaded fallback
        /// contains glyphs for the Santali Ol Chiki script (checked via 'ᱚ' U+1C5A).
        /// </summary>
        public static bool HasOlChikiSupport()
        {
            var font = GetSantaliFont();
            if (font == null) return false;
            if (font.HasCharacter(0x1C5A)) return true;
            if (font.fallbackFontAssetTable != null)
            {
                foreach (var fb in font.fallbackFontAssetTable)
                {
                    if (fb != null && fb.material != null && fb.HasCharacter(0x1C5A)) return true;
                }
            }
            return false;
        }

        private static TMP_FontAsset _santaliFont;

        /// <summary>
        /// Returns a font suitable for Devanagari (Hindi) text.
        /// Loads NotoSansDevanagari SDF or dynamically generates it from the bundled TTF font,
        /// ensuring Devanagari glyphs (U+0900-U+097F) render cleanly without boxes.
        /// </summary>
        public static TMP_FontAsset GetDevanagariFont()
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

            var sb = new System.Text.StringBuilder(1024);
            for (int c = 0x0900; c <= 0x097F; c++) sb.Append((char)c);
            int[] puaGlyphs = new int[]
            {
                0xE02B, 0xE02C, 0xE02D, 0xE02F, 0xE030, 0xE031, 0xE032, 0xE034, 0xE035,
                0xE036, 0xE037, 0xE038, 0xE03D, 0xE03E, 0xE03F, 0xE041, 0xE042, 0xE043,
                0xE044, 0xE045, 0xE046, 0xE047, 0xE048, 0xE04A, 0xE04C, 0xE04D, 0xE04E,
                0xE04F, 0xE050, 0xE076, 0xE078, 0xE085, 0xE08A, 0xE094, 0xE14E, 0xE166,
                0xE181, 0xE189, 0xE190, 0xE1AA, 0xE1AD
            };
            foreach (int p in puaGlyphs) sb.Append((char)p);
            sb.Append(" 0123456789%+-=()[]{}<>/\\|:;.,!?~@#$^&*'\"•✓★→➔↗›‹✔✓");
            sb.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
            string devanagariGlyphs = sb.ToString();

            TMP_FontAsset asset = null;
            try
            {
                asset = Resources.Load<TMP_FontAsset>("Fonts/NotoSansDevanagari SDF")
                     ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansDevanagari SDF")
                     ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSans SDF");

                if (asset != null)
                {
                    // Check for null textures in array
                    if (asset.atlasTextures == null || asset.atlasTextures.Length == 0 || asset.atlasTextures[0] == null)
                    {
                        asset = null;
                    }
                    else
                    {
                        for (int i = 0; i < asset.atlasTextures.Length; i++)
                        {
                            if (asset.atlasTextures[i] == null) { asset = null; break; }
                        }
                    }
                }

                if (asset != null)
                {
                    if (asset.material == null && asset.atlasTexture != null)
                    {
                        var shader = Shader.Find("TextMeshPro/Distance Field") ?? Shader.Find("TextMeshPro/Mobile/Distance Field");
                        if (shader != null)
                        {
                            asset.material = new Material(shader);
                            asset.material.mainTexture = asset.atlasTexture;
                        }
                    }
                    asset.TryAddCharacters(devanagariGlyphs);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[UIHelper] Existing Devanagari asset invalid: {ex.Message}");
                asset = null;
            }

            if (asset == null || asset.material == null || !asset.HasCharacter(0x0915) || !asset.HasCharacter(0xE02B))
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
                            if (created.HasCharacter(0x0915)) asset = created;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[UIHelper] Dynamic Devanagari font creation from TTF failed: {ex.Message}");
                    }
                }
            }

            // Fallback to system fonts if needed
            if (asset == null || !asset.HasCharacter(0x0915))
            {
                try
                {
                    var sysFont = Font.CreateDynamicFontFromOSFont(new string[] { "Nirmala UI", "Mangal", "Aparajita", "Utsaah" }, 36);
                    if (sysFont != null)
                    {
                        var created = TMP_FontAsset.CreateFontAsset(sysFont, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                        if (created != null)
                        {
                            created.name = "NirmalaUI Dynamic";
                            created.TryAddCharacters(devanagariGlyphs);
                            if (created.HasCharacter(0x0915)) asset = created;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[UIHelper] OS Devanagari font fallback failed: {ex.Message}");
                }
            }

            if (asset != null && asset.material != null)
            {
                _devanagariFont = asset;
                var defFont = _defaultFont;
                if (defFont != null && defFont.fallbackFontAssetTable != null && !defFont.fallbackFontAssetTable.Contains(asset))
                {
                    defFont.fallbackFontAssetTable.Add(asset);
                }
                return _devanagariFont;
            }

            return GetDefaultFont();
        }

        /// <summary>
        /// Returns a font suitable for Santali text (Ol Chiki U+1C50-U+1C7F).
        /// Loads NotoSansOlChiki SDF or dynamically creates it from the bundled TTF font.
        /// </summary>
        public static TMP_FontAsset GetSantaliFont()
        {
            if (_santaliFont != null && _santaliFont.material != null && _santaliFont.HasCharacter(0x1C5A))
                return _santaliFont;

            const string olChikiGlyphs = "᱐᱑᱒᱓᱔᱕᱖᱗᱘᱙ᱚᱛᱜᱝᱞᱟᱠᱡᱢᱣᱤᱥᱦᱧᱨᱩᱪᱫᱬᱭᱮᱯᱰᱱᱲᱳᱴᱵᱶᱷᱸᱹᱺᱻᱼᱽ᱾᱿ ";

            var asset = Resources.Load<TMP_FontAsset>("Fonts/NotoSansOlChiki SDF")
                     ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansOlChiki SDF");

            if (asset != null)
            {
                if (asset.material == null && asset.atlasTexture != null)
                {
                    var shader = Shader.Find("TextMeshPro/Distance Field") ?? Shader.Find("TextMeshPro/Mobile/Distance Field");
                    if (shader != null)
                    {
                        asset.material = new Material(shader);
                        asset.material.mainTexture = asset.atlasTexture;
                    }
                }
                asset.TryAddCharacters(olChikiGlyphs);
            }

            if (asset == null || asset.material == null || !asset.HasCharacter(0x1C5A))
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
                            if (created.HasCharacter(0x1C5A)) asset = created;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[UIHelper] Dynamic Ol Chiki font creation failed: {ex.Message}");
                    }
                }
            }

            if (asset != null && asset.material != null)
            {
                _santaliFont = asset;
                var defFont = _defaultFont;
                if (defFont != null && defFont.fallbackFontAssetTable != null && !defFont.fallbackFontAssetTable.Contains(asset))
                {
                    defFont.fallbackFontAssetTable.Add(asset);
                }
                return _santaliFont;
            }

            // Fall back to Devanagari font or default font
            var dev = GetDevanagariFont();
            if (dev != null && dev.material != null) return dev;
            return GetDefaultFont();
        }

        /// <summary>Returns the font asset corresponding to the given language.</summary>
        public static TMP_FontAsset GetFontForLanguage(SurakshaAR.Data.AppLanguage lang)
        {
            switch (lang)
            {
                case SurakshaAR.Data.AppLanguage.Hindi:
                    return GetDevanagariFont();
                case SurakshaAR.Data.AppLanguage.Santali:
                    return GetSantaliFont();
                case SurakshaAR.Data.AppLanguage.English:
                default:
                    return GetDefaultFont();
            }
        }

        /// <summary>Returns the active font asset based on current AppState / Localization selection.</summary>
        public static TMP_FontAsset GetCurrentFont()
        {
            var lang = SurakshaAR.Core.AppState.Instance != null
                ? SurakshaAR.Core.AppState.Instance.CurrentLanguage
                : (SurakshaAR.Localization.LocalizationManager.Instance != null
                    ? SurakshaAR.Localization.LocalizationManager.Instance.CurrentLanguage
                    : SurakshaAR.Data.AppLanguage.English);
            return GetFontForLanguage(lang);
        }

        /// <summary>Adds a TextMeshProUGUI component with language-aware font pre-assigned.</summary>
        public static TextMeshProUGUI AddTMP(GameObject go)
        {
            if (go == null) return null;
            var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            if (tmp == null) return null;
            tmp.isRightToLeftText = false;
            var rt = tmp.rectTransform;
            if (rt != null && rt.localScale.x < 0f)
            {
                var s = rt.localScale;
                s.x = Mathf.Abs(s.x);
                rt.localScale = s;
            }
            var font = GetCurrentFont();
            if (font != null) tmp.font = font;
            return tmp;
        }

        /// <summary>Create a TextMeshProUGUI label with automatic responsive layout element.</summary>
        public static TextMeshProUGUI MakeLabel(string name, Transform parent,
            string text, float fontSize, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left,
            bool bold = false, bool wrap = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            float initialWidth = wrap ? 400f : 240f;
            rt.sizeDelta = new Vector2(initialWidth, fontSize * 1.45f);

            var le = go.AddComponent<LayoutElement>();
            if (!wrap)
            {
                le.preferredHeight = fontSize * 1.45f;
                le.minWidth = 20f;
            }
            else
            {
                le.flexibleWidth = 1f;
            }

            var tmp = AddTMP(go);
            string cleanText = SanitizeText(text);
            if (DevanagariShaper.HasDevanagari(cleanText))
            {
                cleanText = DevanagariShaper.Shape(cleanText);
                try { tmp.font = GetDevanagariFont(); } catch {}
            }
            tmp.text = cleanText;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            if (!wrap)
            {
                tmp.overflowMode = TextOverflowModes.Overflow;
            }
            tmp.raycastTarget = false;
            return tmp;
        }

        /// <summary>
        /// Sanitizes text to remove or normalize unicode emojis and special symbols that are
        /// not contained in the default font asset (LiberationSans SDF), preventing missing character warnings.
        /// </summary>
        public static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            bool hasSpecial = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsSurrogate(c) || c == '✓' || c == '➔' || c == '→' || c == '↗' || c == '›' || c == '‹' || c == '⚡' ||
                    c == '⚙' || c == '▦' || c == '⬇' || c == '⏱' || c == '☁')
                {
                    hasSpecial = true;
                    break;
                }
            }

            if (!hasSpecial) return text;

            return text
                .Replace("✓", "OK")
                .Replace("➔", ">")
                .Replace("→", ">")
                .Replace("↗", ">")
                .Replace("›", ">")
                .Replace("‹", "<")
                .Replace("🎯", "")
                .Replace("🔥", "")
                .Replace("⚡", "")
                .Replace("🧪", "")
                .Replace("⚙️", "")
                .Replace("⚙", "")
                .Replace("💡", "")
                .Replace("📈", "")
                .Replace("🛡️", "")
                .Replace("🛡", "")
                .Replace("▦", "")
                .Replace("⬇", "v")
                .Replace("🔗", "")
                .Replace("🧠", "")
                .Replace("⏱", "")
                .Replace("☁️", "")
                .Replace("☁", "")
                .Replace("🧱", "")
                .Replace("🔒", "")
                .Replace("📑", "")
                .Replace("📊", "")
                .Trim();
        }

        /// <summary>Create a Button with a colored background Image and TMP label.</summary>
        public static Button MakeButton(string name, Transform parent,
            string label, float fontSize, Color bgColor, Color textColor,
            float cornerRadius = 12f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 96);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 96;
            le.flexibleWidth = 1f;

            var img = go.AddComponent<Image>();
            img.color = bgColor;
            if (cornerRadius > 0) SetImageRoundedSprite(img, cornerRadius);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.15f);
            colors.selectedColor = bgColor;
            btn.colors = colors;

            // Label child (stretches to fill button with padding, only created when label text is provided)
            if (!string.IsNullOrEmpty(label))
            {
                var labelGO = new GameObject("Label");
                labelGO.transform.SetParent(go.transform, false);
                var labelRT = labelGO.AddComponent<RectTransform>();
                labelRT.anchorMin = Vector2.zero;
                labelRT.anchorMax = Vector2.one;
                labelRT.offsetMin = new Vector2(18, 6);
                labelRT.offsetMax = new Vector2(-18, -6);

                var tmp = AddTMP(labelGO);
                string cleanLabel = SanitizeText(label);
                tmp.text = cleanLabel;
                // Enforce mobile minimum font size of 32-36px for text-bearing buttons
                float effectiveFontSize = (fontSize < 30f && fontSize > 0f) ? 34f : fontSize;
                tmp.fontSize = effectiveFontSize;
                tmp.color = textColor;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontStyle = FontStyles.Bold;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = Mathf.Max(26f, effectiveFontSize * 0.85f);
                tmp.fontSizeMax = effectiveFontSize;
                tmp.raycastTarget = false;
            }

            return btn;
        }

        /// <summary>Create a VerticalLayoutGroup container with proper child driving.</summary>
        public static RectTransform MakeVertical(string name, Transform parent,
            float spacing = 0, RectOffset padding = null,
            bool childForceWidth = true, bool childForceHeight = false,
            bool controlChildHeight = true, bool controlChildWidth = true)
        {
            var rt = MakeRect(name, parent);
            var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.padding = padding ?? new RectOffset();
            vlg.childForceExpandWidth = childForceWidth;
            vlg.childForceExpandHeight = childForceHeight;
            vlg.childControlHeight = controlChildHeight;
            vlg.childControlWidth = controlChildWidth;
            return rt;
        }

        /// <summary>Create a HorizontalLayoutGroup container with proper child driving.</summary>
        public static RectTransform MakeHorizontal(string name, Transform parent,
            float spacing = 0, RectOffset padding = null,
            bool childForceWidth = false, bool childForceHeight = false,
            bool childControlWidth = true, bool childControlHeight = true)
        {
            var rt = MakeRect(name, parent);
            var hlg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.padding = padding ?? new RectOffset();
            hlg.childForceExpandWidth = childForceWidth;
            hlg.childForceExpandHeight = childForceHeight;
            hlg.childControlWidth = childControlWidth;
            hlg.childControlHeight = childControlHeight;
            hlg.reverseArrangement = false;
            return rt;
        }

        /// <summary>
        /// Recursively resets any mirrored/negative scaleX and forces strict LTR on all RectTransforms,
        /// HorizontalLayoutGroups, and TextMeshPro components.
        /// Guaranteed safe execution path for Santali (Ol Chiki), Hindi, and English.
        /// </summary>
        public static void EnforceLtrLayout(Transform root)
        {
            if (root == null) return;
            foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
            {
                var s = rt.localScale;
                if (s.x < 0f)
                {
                    s.x = Mathf.Abs(s.x);
                    rt.localScale = s;
                }
            }
            foreach (var hlg in root.GetComponentsInChildren<HorizontalLayoutGroup>(true))
            {
                hlg.reverseArrangement = false;
            }
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.isRightToLeftText = false;
            }
        }

        /// <summary>Add a LayoutElement with preferred/minimum sizes.</summary>
        public static LayoutElement SetLayout(GameObject go,
            float preferredWidth = -1, float preferredHeight = -1,
            float minWidth = -1, float minHeight = -1,
            bool flexibleWidth = false, float flexWidth = 1,
            bool flexibleHeight = false, float flexHeight = 1)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            var rt = go.GetComponent<RectTransform>();
            if (preferredWidth >= 0)
            {
                le.preferredWidth = preferredWidth;
                if (rt != null) rt.sizeDelta = new Vector2(preferredWidth, rt.sizeDelta.y > 0 ? rt.sizeDelta.y : (preferredHeight >= 0 ? preferredHeight : 30f));
            }
            if (preferredHeight >= 0)
            {
                le.preferredHeight = preferredHeight;
                if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x > 0 ? rt.sizeDelta.x : (preferredWidth >= 0 ? preferredWidth : 100f), preferredHeight);
            }
            if (minWidth >= 0) le.minWidth = minWidth;
            if (minHeight >= 0) le.minHeight = minHeight;
            if (flexibleWidth) le.flexibleWidth = flexWidth;
            if (flexibleHeight) le.flexibleHeight = flexHeight;
            return le;
        }

        /// <summary>Add a ContentSizeFitter to auto-size to content.</summary>
        public static ContentSizeFitter MakeSizeFitter(GameObject go,
            ContentSizeFitter.FitMode horizontal = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode vertical = ContentSizeFitter.FitMode.PreferredSize)
        {
            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = horizontal;
            csf.verticalFit = vertical;
            return csf;
        }

        /// <summary>Add a CanvasGroup for alpha/interactability control.</summary>
        public static CanvasGroup MakeCanvasGroup(GameObject go, float alpha = 1f)
        {
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
            return cg;
        }

        // ──────────────────────────────────────────────────────────────
        //  RECT TRANSFORM HELPERS
        // ──────────────────────────────────────────────────────────────

        /// <summary>Stretch RectTransform to fill parent with optional insets.</summary>
        public static void Stretch(RectTransform rt,
            float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Anchor RectTransform at center with fixed width and height.</summary>
        public static void AnchorCenter(RectTransform rt, float width, float height)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = Vector2.zero;
        }

        /// <summary>Size a RectTransform to a fixed width/height, anchored at center.</summary>
        public static void SetSize(RectTransform rt, float width, float height)
        {
            AnchorCenter(rt, width, height);
        }

        /// <summary>Anchor top, stretch horizontally.</summary>
        public static void AnchorTopStretch(RectTransform rt, float height, float top = 0)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, -top - height);
            rt.offsetMax = new Vector2(0, -top);
        }

        /// <summary>Anchor bottom, stretch horizontally.</summary>
        public static void AnchorBottomStretch(RectTransform rt, float height, float bottom = 0)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.offsetMin = new Vector2(0, bottom);
            rt.offsetMax = new Vector2(0, bottom + height);
        }

        // ──────────────────────────────────────────────────────────────
        //  IMAGE HELPERS
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Set image sprite to Unity's built-in rounded rect background.
        /// This gives Image components rounded corners.
        /// </summary>
        private static readonly Dictionary<int, Sprite> _cachedRoundedSprites = new Dictionary<int, Sprite>();

        public static void SetImageRoundedSprite(Image img, float radius = 12f)
        {
            img.sprite = CreateRoundedRectSprite(radius);
            img.type = Image.Type.Sliced;
        }

        /// <summary>Create a simple white circle sprite for rounded UI elements.</summary>
        public static Sprite CreateCircleSprite()
        {
            int res = 128;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = (res - 1) / 2f;
            float r = center - 1f;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dx = x - center, dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(r - dist + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        /// <summary>Create a simple rounded-rectangle sprite with anti-aliasing and dynamic resolution.</summary>
        public static Sprite CreateRoundedRectSprite(float radius = 12f)
        {
            int r = Mathf.Max(2, Mathf.RoundToInt(radius));
            if (_cachedRoundedSprites.TryGetValue(r, out var cached) && cached != null)
                return cached;

            int res = Mathf.Max(128, (r + 4) * 2);
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                bool inBottom = (y < r);
                bool inTop = (y > res - 1 - r);
                for (int x = 0; x < res; x++)
                {
                    bool inLeft = (x < r);
                    bool inRight = (x > res - 1 - r);
                    float alpha = 1f;
                    if ((inLeft || inRight) && (inTop || inBottom))
                    {
                        float dx = inLeft ? (r - x) : (x - (res - 1 - r));
                        float dy = inBottom ? (r - y) : (y - (res - 1 - r));
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        alpha = Mathf.Clamp01(r - dist + 0.5f);
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            int b = r + 2;
            var sprite = Sprite.Create(tex, new Rect(0, 0, res, res),
                new Vector2(0.5f, 0.5f), 100f,
                0, SpriteMeshType.FullRect,
                new Vector4(b, b, b, b));
            _cachedRoundedSprites[r] = sprite;
            return sprite;
        }

        private static readonly Dictionary<int, Sprite> _cachedTopRoundedSprites = new Dictionary<int, Sprite>();

        /// <summary>Create a rectangle sprite with only the top two corners rounded.</summary>
        public static Sprite CreateTopRoundedRectSprite(float radius = 44f)
        {
            int r = Mathf.Max(2, Mathf.RoundToInt(radius));
            if (_cachedTopRoundedSprites.TryGetValue(r, out var cached) && cached != null)
                return cached;

            int res = Mathf.Max(128, (r + 4) * 2);
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                bool inTop = (y > res - 1 - r);
                for (int x = 0; x < res; x++)
                {
                    bool inLeft = (x < r);
                    bool inRight = (x > res - 1 - r);
                    float alpha = 1f;
                    if (inTop && (inLeft || inRight))
                    {
                        float dx = inLeft ? (r - x) : (x - (res - 1 - r));
                        float dy = y - (res - 1 - r);
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        alpha = Mathf.Clamp01(r - dist + 0.5f);
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            int b = r + 2;
            var sprite = Sprite.Create(tex, new Rect(0, 0, res, res),
                new Vector2(0.5f, 0.5f), 100f,
                0, SpriteMeshType.FullRect,
                new Vector4(b, 4, b, b));
            _cachedTopRoundedSprites[r] = sprite;
            return sprite;
        }

        /// <summary>Create a simple white Sprite (1x1 pixel) for solid-color images.</summary>
        public static Sprite WhiteSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        // Cached white sprite
        private static Sprite _cachedWhite;
        public static Sprite GetWhiteSprite()
        {
            if (_cachedWhite == null) _cachedWhite = WhiteSprite();
            return _cachedWhite;
        }

        // Cached circle sprite
        private static Sprite _cachedCircle;
        public static Sprite GetCircleSprite()
        {
            if (_cachedCircle == null) _cachedCircle = CreateCircleSprite();
            return _cachedCircle;
        }

        /// <summary>Create a circular outline ring sprite for unselected radio buttons.</summary>
        public static Sprite CreateCircleOutlineSprite(float thickness = 5f)
        {
            int res = 128;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = (res - 1) / 2f;
            float outerR = center - 1f;
            float innerR = outerR - thickness * 2f;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dx = x - center, dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= outerR && dist >= innerR)
                    {
                        float alphaOuter = Mathf.Clamp01(outerR - dist + 0.5f);
                        float alphaInner = Mathf.Clamp01(dist - innerR + 0.5f);
                        float alpha = Mathf.Min(alphaOuter, alphaInner);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        private static Sprite _cachedCircleOutline;
        public static Sprite GetCircleOutlineSprite()
        {
            if (_cachedCircleOutline == null) _cachedCircleOutline = CreateCircleOutlineSprite(5f);
            return _cachedCircleOutline;
        }

        /// <summary>Create a crisp procedural right-pointing triangle sprite (play button) with anti-aliasing.</summary>
        public static Sprite CreatePlayTriangleSprite()
        {
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float px = (float)x / res;
                    float py = (float)y / res;
                    float halfH = (0.82f - px) * 0.72f;
                    if (px >= 0.22f && px <= 0.82f && Mathf.Abs(py - 0.5f) <= halfH)
                    {
                        float edgeDist = Mathf.Min(px - 0.22f, halfH - Mathf.Abs(py - 0.5f));
                        float alpha = Mathf.Clamp01(edgeDist * res * 0.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite _cachedPlayTriangle;
        public static Sprite GetPlayTriangleSprite()
        {
            if (_cachedPlayTriangle == null) _cachedPlayTriangle = CreatePlayTriangleSprite();
            return _cachedPlayTriangle;
        }

        /// <summary>Create a crisp procedural downward-pointing chevron sprite with anti-aliasing.</summary>
        public static Sprite CreateDownChevronSprite()
        {
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float px = (float)x / res;
                    float py = (float)y / res;
                    float halfW = (py - 0.22f) * 0.72f;
                    if (py >= 0.22f && py <= 0.78f && Mathf.Abs(px - 0.5f) <= halfW)
                    {
                        float edgeDist = Mathf.Min(0.78f - py, halfW - Mathf.Abs(px - 0.5f));
                        float alpha = Mathf.Clamp01(edgeDist * res * 0.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }

        // ──────────────────────────────────────────────────────────────
        //  PROCEDURAL ICONS
        // ──────────────────────────────────────────────────────────────

        public static bool ContainsDevanagari(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= 0x0900 && text[i] <= 0x097F) return true;
            }
            return false;
        }

        public static void SetTMPText(TextMeshProUGUI tmp, string text)
        {
            if (tmp == null) return;
            string clean = SanitizeText(text);
            tmp.text = clean;
            var font = GetDefaultFont();
            if (font != null && tmp.font != font) tmp.font = font;
        }

        private static float DistToSegment(float px, float py, float x1, float y1, float x2, float y2)
        {
            float l2 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
            if (l2 == 0) return Mathf.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1));
            float t = Mathf.Clamp01(((px - x1) * (x2 - x1) + (py - y1) * (y2 - y1)) / l2);
            float projX = x1 + t * (x2 - x1);
            float projY = y1 + t * (y2 - y1);
            return Mathf.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
        }

        private static Sprite _cachedTricolor;
        public static Sprite GetTricolorSprite()
        {
            if (_cachedTricolor != null) return _cachedTricolor;
            var tex = new Texture2D(3, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixel(0, 0, new Color(1.0f, 0.6f, 0.2f));    // Saffron/Orange
            tex.SetPixel(1, 0, Color.white);                     // White
            tex.SetPixel(2, 0, new Color(0.01f, 0.52f, 0.78f)); // Blue
            tex.Apply();
            _cachedTricolor = Sprite.Create(tex, new Rect(0, 0, 3, 1), new Vector2(0.5f, 0.5f), 100f);
            return _cachedTricolor;
        }

        private static Sprite _cachedDownChevron;
        public static Sprite GetDownChevronSprite()
        {
            if (_cachedDownChevron == null) _cachedDownChevron = CreateDownChevronSprite();
            return _cachedDownChevron;
        }

        private static Sprite _cachedCheckmarkCircle;
        public static Sprite GetCheckmarkCircleSprite()
        {
            if (_cachedCheckmarkCircle != null) return _cachedCheckmarkCircle;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float c = (res - 1) / 2f;
            float r = c - 2f;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dist = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    if (dist <= r)
                    {
                        bool isCheck = false;
                        float d1 = DistToSegment(new Vector2(x, y), new Vector2(18, 30), new Vector2(27, 21));
                        float d2 = DistToSegment(new Vector2(x, y), new Vector2(27, 21), new Vector2(46, 42));
                        if (Mathf.Min(d1, d2) <= 3.2f) isCheck = true;

                        tex.SetPixel(x, y, isCheck ? Color.white : UIColors.Hex("#10B981"));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            _cachedCheckmarkCircle = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedCheckmarkCircle;
        }

        private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float lenSq = ba.sqrMagnitude;
            if (lenSq < 0.0001f) return (p - a).magnitude;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / lenSq);
            return (pa - ba * h).magnitude;
        }

        private static Sprite _cachedHome;
        public static Sprite GetHomeSprite()
        {
            if (_cachedHome != null) return _cachedHome;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    if (y >= 32 && y <= 54)
                    {
                        float halfW = (54 - y) * 1.05f + 2f;
                        if (Mathf.Abs(x - 32) <= halfW) filled = true;
                    }
                    if (y >= 12 && y <= 34 && x >= 18 && x <= 46)
                    {
                        if (!(x >= 27 && x <= 37 && y <= 24))
                            filled = true;
                    }
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedHome = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedHome;
        }

        private static Sprite _cachedBook;
        public static Sprite GetBookSprite()
        {
            if (_cachedBook != null) return _cachedBook;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    if (y >= 16 && y <= 50 && x >= 14 && x <= 30)
                    {
                        if (x != 30 || y <= 46) filled = true;
                    }
                    if (y >= 16 && y <= 50 && x >= 34 && x <= 50)
                    {
                        if (x != 34 || y <= 46) filled = true;
                    }
                    if (filled && (y == 24 || y == 32 || y == 40) && ((x >= 18 && x <= 26) || (x >= 38 && x <= 46)))
                        filled = false;

                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedBook = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedBook;
        }

        private static Sprite _cachedChart;
        public static Sprite GetChartSprite()
        {
            if (_cachedChart != null) return _cachedChart;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    if (x >= 14 && x <= 24 && y >= 12 && y <= 28) filled = true;
                    if (x >= 27 && x <= 37 && y >= 12 && y <= 40) filled = true;
                    if (x >= 40 && x <= 50 && y >= 12 && y <= 52) filled = true;

                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedChart = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedChart;
        }

        private static Sprite _cachedMedal;
        public static Sprite GetMedalSprite()
        {
            if (_cachedMedal != null) return _cachedMedal;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    float dx = x - 32, dy = y - 40;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= 15) filled = true;

                    if (y >= 10 && y <= 30 && x >= 20 && x <= 28)
                    {
                        if (!(y < 16 && Mathf.Abs(x - 24) < (16 - y)))
                            filled = true;
                    }
                    if (y >= 10 && y <= 30 && x >= 36 && x <= 44)
                    {
                        if (!(y < 16 && Mathf.Abs(x - 40) < (16 - y)))
                            filled = true;
                    }
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedMedal = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedMedal;
        }

        private static Sprite _cachedBell;
        public static Sprite GetBellSprite()
        {
            if (_cachedBell != null) return _cachedBell;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    if (y >= 20 && y <= 48)
                    {
                        float halfW = Mathf.Lerp(6f, 18f, Mathf.Pow((48 - y) / 28f, 1.4f));
                        if (Mathf.Abs(x - 32) <= halfW) filled = true;
                    }
                    if (y >= 18 && y <= 21 && Mathf.Abs(x - 32) <= 20) filled = true;
                    float dx = x - 32, dy = y - 14;
                    if (dx * dx + dy * dy <= 16) filled = true;
                    if (y >= 48 && y <= 53 && Mathf.Abs(x - 32) <= 3) filled = true;

                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedBell = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedBell;
        }

        private static Sprite _cachedProfile;
        public static Sprite GetProfileSprite()
        {
            if (_cachedProfile != null) return _cachedProfile;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    float dx = x - 32, dy = y - 42;
                    if (dx * dx + dy * dy <= 121) filled = true;

                    if (y >= 12 && y <= 28)
                    {
                        float halfW = Mathf.Sqrt(Mathf.Max(0, 24 * 24 - (y - 12) * (y - 12) * 1.5f));
                        if (Mathf.Abs(x - 32) <= halfW) filled = true;
                    }
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedProfile = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedProfile;
        }

        private static Sprite _cachedPin;
        public static Sprite GetPinSprite()
        {
            if (_cachedPin != null) return _cachedPin;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    float dx = x - 32, dy = y - 40;
                    float distSq = dx * dx + dy * dy;
                    if (distSq <= 225) filled = true;
                    if (y >= 12 && y <= 40)
                    {
                        float halfW = (y - 12) * 0.53f;
                        if (Mathf.Abs(x - 32) <= halfW) filled = true;
                    }
                    if (distSq <= 36) filled = false;

                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedPin = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedPin;
        }

        private static Sprite _cachedCheckmark;
        public static Sprite GetCheckmarkSprite()
        {
            if (_cachedCheckmark != null) return _cachedCheckmark;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float d1 = DistToSegment(x, y, 16, 30, 27, 18);
                    float d2 = DistToSegment(x, y, 27, 18, 48, 46);
                    float minDist = Mathf.Min(d1, d2);
                    float alpha = Mathf.Clamp01((3.8f - minDist) * 1.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedCheckmark = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedCheckmark;
        }

        private static Sprite _cachedClock;
        public static Sprite GetClockSprite()
        {
            if (_cachedClock != null) return _cachedClock;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dx = x - 32, dy = y - 32;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float ring = Mathf.Abs(dist - 20);
                    float alphaRing = Mathf.Clamp01((3f - ring) * 1.5f);
                    float dHand1 = (x >= 30 && x <= 34 && y >= 32 && y <= 44) ? 1f : 0f;
                    float dHand2 = (y >= 30 && y <= 34 && x >= 32 && x <= 42) ? 1f : 0f;
                    float alpha = Mathf.Max(alphaRing, Mathf.Max(dHand1, dHand2));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedClock = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedClock;
        }

        private static Sprite _cachedClipboard;
        public static Sprite GetClipboardSprite()
        {
            if (_cachedClipboard != null) return _cachedClipboard;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    if (x >= 16 && x <= 48 && y >= 10 && y <= 52) filled = true;
                    if (x >= 26 && x <= 38 && y >= 50 && y <= 56) filled = true;
                    if (filled && (y == 22 || y == 30 || y == 38) && (x >= 22 && x <= 42))
                        filled = false;
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedClipboard = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedClipboard;
        }

        private static Sprite _cachedBookmark;
        public static Sprite GetBookmarkSprite()
        {
            if (_cachedBookmark != null) return _cachedBookmark;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    if (x >= 18 && x <= 46 && y >= 10 && y <= 54)
                    {
                        if (!(y <= 24 && Mathf.Abs(x - 32) <= (24 - y) * 1.15f))
                            filled = true;
                    }
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedBookmark = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedBookmark;
        }

        private static Sprite _cachedGlobe;
        public static Sprite GetGlobeSprite()
        {
            if (_cachedGlobe != null) return _cachedGlobe;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dx = x - 32, dy = y - 32;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 0f;
                    if (Mathf.Abs(dist - 20) <= 2.5f) alpha = 1f;
                    if (dist <= 20 && Mathf.Abs(dy) <= 1.5f) alpha = 1f;
                    if (dist <= 20 && Mathf.Abs(dx) <= 1.5f) alpha = 1f;
                    if (dist <= 20)
                    {
                        float ex = dx / 11f;
                        float ey = dy / 20f;
                        if (Mathf.Abs(ex * ex + ey * ey - 1f) <= 0.25f) alpha = 1f;
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedGlobe = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedGlobe;
        }

        private static Sprite _cachedShield;
        public static Sprite GetShieldSprite()
        {
            if (_cachedShield != null) return _cachedShield;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dx = Mathf.Abs(x - 32);
                    float py = y;
                    bool inShield = false;
                    if (py <= 54 && py >= 10)
                    {
                        float maxDx = 22f;
                        if (py < 36)
                        {
                            float t = (py - 10f) / 26f;
                            maxDx = 22f * Mathf.Sqrt(Mathf.Clamp01(t));
                        }
                        if (dx <= maxDx) inShield = true;
                    }
                    bool onOutline = false;
                    if (inShield)
                    {
                        float innerMaxDx = 18f;
                        if (py < 36)
                        {
                            float t = (py - 14f) / 22f;
                            innerMaxDx = 18f * (t > 0 ? Mathf.Sqrt(t) : 0f);
                        }
                        if (py > 50 || dx > innerMaxDx || py < 14) onOutline = true;
                    }
                    bool innerCore = (py >= 20 && py <= 44 && dx <= 10 && (py >= 32 ? true : dx <= (py - 20) * 0.8f));
                    float alpha = onOutline ? 1f : (innerCore ? 0.85f : 0f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedShield = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedShield;
        }

        private static Sprite _cachedWorkforce;
        public static Sprite GetWorkforceSprite()
        {
            if (_cachedWorkforce != null) return _cachedWorkforce;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float alpha = 0f;
                    float dHeadCenter = Mathf.Sqrt((x - 32)*(x - 32) + (y - 42)*(y - 42));
                    if (dHeadCenter <= 7.5f) alpha = 1f;

                    if (y >= 14 && y <= 32 && Mathf.Abs(x - 32) <= 12f)
                    {
                        float dy = 32 - y;
                        float maxW = 7f + dy * 0.45f;
                        if (Mathf.Abs(x - 32) <= maxW) alpha = 1f;
                    }

                    float dHeadLeft = Mathf.Sqrt((x - 18)*(x - 18) + (y - 36)*(y - 36));
                    if (dHeadLeft <= 6f) alpha = 1f;

                    if (y >= 14 && y <= 28 && x >= 8 && x <= 24)
                    {
                        float dy = 28 - y;
                        if (Mathf.Abs(x - 18) <= 4f + dy * 0.35f) alpha = 1f;
                    }

                    float dHeadRight = Mathf.Sqrt((x - 46)*(x - 46) + (y - 36)*(y - 36));
                    if (dHeadRight <= 6f) alpha = 1f;

                    if (y >= 14 && y <= 28 && x >= 40 && x <= 56)
                    {
                        float dy = 28 - y;
                        if (Mathf.Abs(x - 46) <= 4f + dy * 0.35f) alpha = 1f;
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedWorkforce = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedWorkforce;
        }

        private static Sprite _cachedLeaf;
        public static Sprite GetLeafSprite()
        {
            if (_cachedLeaf != null) return _cachedLeaf;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float alpha = 0f;
                    float u = (x - 16f) * 0.707f + (y - 14f) * 0.707f;
                    float v = -(x - 16f) * 0.707f + (y - 14f) * 0.707f;
                    if (u >= 0 && u <= 48f)
                    {
                        float maxHalfW = Mathf.Sin(u / 48f * Mathf.PI) * 16f;
                        if (Mathf.Abs(v) <= maxHalfW)
                        {
                            if (Mathf.Abs(Mathf.Abs(v) - maxHalfW) <= 2.8f || Mathf.Abs(v) <= 1.4f)
                                alpha = 1f;
                        }
                    }
                    if (u >= -4 && u <= 8 && Mathf.Abs(v) <= 1.8f) alpha = 1f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedLeaf = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedLeaf;
        }

        private static Sprite _cachedCloud;
        public static Sprite GetCloudSprite()
        {
            if (_cachedCloud != null) return _cachedCloud;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float alpha = 0f;
                    if (y >= 18 && y <= 30 && x >= 16 && x <= 48) alpha = 1f;
                    float dL = Mathf.Sqrt((x - 24) * (x - 24) + (y - 28) * (y - 28));
                    if (dL <= 10f) alpha = Mathf.Max(alpha, Mathf.Clamp01((10.5f - dL) * 1.5f));
                    float dC = Mathf.Sqrt((x - 34) * (x - 34) + (y - 34) * (y - 34));
                    if (dC <= 13f) alpha = Mathf.Max(alpha, Mathf.Clamp01((13.5f - dC) * 1.5f));
                    float dR = Mathf.Sqrt((x - 44) * (x - 44) + (y - 28) * (y - 28));
                    if (dR <= 9f) alpha = Mathf.Max(alpha, Mathf.Clamp01((9.5f - dR) * 1.5f));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedCloud = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedCloud;
        }

        private static Sprite _cachedLock;
        public static Sprite GetLockSprite()
        {
            if (_cachedLock != null) return _cachedLock;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    // Lock body
                    if (x >= 16 && x <= 48 && y >= 10 && y <= 34)
                    {
                        // Body rounded corners
                        bool corner = (x < 20 && y < 14) || (x > 44 && y < 14) || (x < 20 && y > 30) || (x > 44 && y > 30);
                        if (!corner) filled = true;
                        // Keyhole slot
                        if (x >= 30 && x <= 34 && y >= 16 && y <= 26) filled = false;
                        float dxK = x - 32f, dyK = y - 25f;
                        if (dxK * dxK + dyK * dyK <= 9f) filled = false;
                    }
                    // Shackle
                    if (y >= 32 && y <= 52 && x >= 22 && x <= 42)
                    {
                        float dx = Mathf.Abs(x - 32f);
                        float dy = y - 40f;
                        if (y >= 40)
                        {
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d <= 11f && d >= 5.5f) filled = true;
                        }
                        else
                        {
                            if ((dx >= 5.5f && dx <= 11f)) filled = true;
                        }
                    }
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedLock = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedLock;
        }

        private static Sprite _cachedIdCard;
        public static Sprite GetIdCardSprite()
        {
            if (_cachedIdCard != null) return _cachedIdCard;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    // Card outline
                    if (x >= 10 && x <= 54 && y >= 14 && y <= 50)
                    {
                        bool corner = (x < 14 && y < 18) || (x > 50 && y < 18) || (x < 14 && y > 46) || (x > 50 && y > 46);
                        if (!corner)
                        {
                            // Card border (2px)
                            if (x <= 13 || x >= 51 || y <= 17 || y >= 47) filled = true;

                            // Avatar head & shoulders
                            float dxH = x - 23f, dyH = y - 36f;
                            if (dxH * dxH + dyH * dyH <= 16f) filled = true;
                            if (y >= 20 && y <= 28 && Mathf.Abs(x - 23f) <= (28 - y) * 1.2f && Mathf.Abs(x - 23f) <= 9f) filled = true;

                            // Data lines on right
                            if (x >= 34 && x <= 48)
                            {
                                if ((y >= 38 && y <= 41) || (y >= 30 && y <= 33) || (y >= 22 && y <= 25)) filled = true;
                            }
                        }
                    }
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedIdCard = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedIdCard;
        }

        private static Sprite _cachedQR;
        public static Sprite GetQRSprite()
        {
            if (_cachedQR != null) return _cachedQR;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool filled = false;
                    // Helper to draw QR finder pattern at (ox, oy)
                    bool InFinder(int ox, int oy)
                    {
                        int lx = x - ox, ly = y - oy;
                        if (lx >= 0 && lx < 16 && ly >= 0 && ly < 16)
                        {
                            if (lx <= 2 || lx >= 13 || ly <= 2 || ly >= 13) return true;
                            if (lx >= 5 && lx <= 10 && ly >= 5 && ly <= 10) return true;
                        }
                        return false;
                    }

                    if (InFinder(10, 38)) filled = true; // Top-left
                    if (InFinder(38, 38)) filled = true; // Top-right
                    if (InFinder(10, 10)) filled = true; // Bottom-left

                    // Center / bottom-right data pixels
                    if (x >= 38 && x <= 42 && y >= 22 && y <= 26) filled = true;
                    if (x >= 48 && x <= 52 && y >= 14 && y <= 18) filled = true;
                    if (x >= 44 && x <= 48 && y >= 28 && y <= 32) filled = true;
                    if (x >= 28 && x <= 32 && y >= 26 && y <= 30) filled = true;
                    if (x >= 28 && x <= 32 && y >= 12 && y <= 16) filled = true;

                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedQR = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedQR;
        }

        private static Sprite _cachedRightChevron;
        public static Sprite GetRightChevronSprite()
        {
            if (_cachedRightChevron != null) return _cachedRightChevron;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float d1 = DistToSegment(x, y, 24f, 50f, 42f, 32f);
                    float d2 = DistToSegment(x, y, 24f, 14f, 42f, 32f);
                    float minD = Mathf.Min(d1, d2);
                    float alpha = Mathf.Clamp01((2.8f - minD) / 1.1f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedRightChevron = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedRightChevron;
        }

        private static Sprite _cachedLogout;
        public static Sprite GetLogoutSprite()
        {
            if (_cachedLogout != null) return _cachedLogout;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float alpha = 0f;
                    // Door bracket lines
                    float dLeft = DistToSegment(x, y, 16f, 14f, 16f, 50f);
                    float dTop  = DistToSegment(x, y, 16f, 50f, 34f, 50f);
                    float dBot  = DistToSegment(x, y, 16f, 14f, 34f, 14f);
                    float minDoor = Mathf.Min(dLeft, Mathf.Min(dTop, dBot));
                    if (minDoor <= 3f) alpha = Mathf.Max(alpha, Mathf.Clamp01((3f - minDoor) * 1.5f));

                    // Exit arrow
                    float dStem = DistToSegment(x, y, 24f, 32f, 48f, 32f);
                    float dHead1 = DistToSegment(x, y, 48f, 32f, 38f, 42f);
                    float dHead2 = DistToSegment(x, y, 48f, 32f, 38f, 22f);
                    float minArrow = Mathf.Min(dStem, Mathf.Min(dHead1, dHead2));
                    if (minArrow <= 3f) alpha = Mathf.Max(alpha, Mathf.Clamp01((3f - minArrow) * 1.5f));

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedLogout = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedLogout;
        }

        private static Sprite _cachedPencil;
        public static Sprite GetPencilSprite()
        {
            if (_cachedPencil != null) return _cachedPencil;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float alpha = 0f;
                    float dShaft = DistToSegment(x, y, 22f, 22f, 48f, 48f);
                    if (dShaft <= 6.5f) alpha = Mathf.Clamp01((6.5f - dShaft) * 1.5f);
                    float dTip1 = DistToSegment(x, y, 22f, 22f, 14f, 14f);
                    if (dTip1 <= 5.5f && (x + y <= 42)) alpha = Mathf.Max(alpha, Mathf.Clamp01((5.5f - dTip1) * 1.5f));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedPencil = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedPencil;
        }


        private static Sprite _cachedAshokaEmblem;
        public static Sprite GetAshokaEmblemSprite()
        {
            if (_cachedAshokaEmblem != null) return _cachedAshokaEmblem;
            int w = 128, h = 160;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float alpha = 0f;
                    float nx = x, ny = y;
                    float cx = w * 0.5f; // 64

                    // 1. Center Lion Head & Mane (y: 100..152)
                    float dHeadCenter = Mathf.Sqrt((nx - cx) * (nx - cx) + (ny - 128f) * (ny - 128f));
                    if (dHeadCenter <= 18f) alpha = 1f;
                    float dManeL = Mathf.Sqrt((nx - (cx - 14f)) * (nx - (cx - 14f)) + (ny - 122f) * (ny - 122f));
                    float dManeR = Mathf.Sqrt((nx - (cx + 14f)) * (nx - (cx + 14f)) + (ny - 122f) * (ny - 122f));
                    if (dManeL <= 15f || dManeR <= 15f) alpha = 1f;
                    float dEarL = Mathf.Sqrt((nx - (cx - 12f)) * (nx - (cx - 12f)) + (ny - 144f) * (ny - 144f));
                    float dEarR = Mathf.Sqrt((nx - (cx + 12f)) * (nx - (cx + 12f)) + (ny - 144f) * (ny - 144f));
                    if (dEarL <= 5f || dEarR <= 5f) alpha = 1f;

                    // 2. Left Lion
                    float dHeadLeft = Mathf.Sqrt((nx - 36f) * (nx - 36f) + (ny - 120f) * (ny - 120f));
                    if (dHeadLeft <= 14f) alpha = 1f;
                    if (nx >= 18f && nx <= 32f && ny >= 114f && ny <= 126f) alpha = 1f;
                    float dManeLeft = Mathf.Sqrt((nx - 38f) * (nx - 38f) + (ny - 105f) * (ny - 105f));
                    if (dManeLeft <= 16f) alpha = 1f;

                    // 3. Right Lion
                    float dHeadRight = Mathf.Sqrt((nx - 92f) * (nx - 92f) + (ny - 120f) * (ny - 120f));
                    if (dHeadRight <= 14f) alpha = 1f;
                    if (nx >= 96f && nx <= 110f && ny >= 114f && ny <= 126f) alpha = 1f;
                    float dManeRight = Mathf.Sqrt((nx - 90f) * (nx - 90f) + (ny - 105f) * (ny - 105f));
                    if (dManeRight <= 16f) alpha = 1f;

                    // 4. Chest / Mane flowing downward
                    if (ny >= 80f && ny <= 108f)
                    {
                        float halfW = 28f - (108f - ny) * 0.35f;
                        if (Mathf.Abs(nx - cx) <= halfW) alpha = 1f;
                    }

                    // 5. Forelegs standing on abacus
                    if (ny >= 62f && ny <= 82f)
                    {
                        if ((nx >= 34f && nx <= 42f) || (nx >= 52f && nx <= 60f) ||
                            (nx >= 68f && nx <= 76f) || (nx >= 86f && nx <= 94f))
                            alpha = 1f;
                    }

                    // 6. Abacus Upper Rim
                    if (ny >= 56f && ny <= 62f && nx >= 18f && nx <= 110f) alpha = 1f;

                    // 7. Abacus Middle Band with Ashoka Chakra wheel
                    float dChakra = Mathf.Sqrt((nx - cx) * (nx - cx) + (ny - 45f) * (ny - 45f));
                    if (dChakra <= 12f)
                    {
                        if (dChakra >= 9.5f && dChakra <= 12f) alpha = 1f;
                        else if (dChakra <= 3.5f) alpha = 1f;
                        else
                        {
                            float angle = Mathf.Atan2(ny - 45f, nx - cx);
                            float spokeVal = Mathf.Abs(Mathf.Sin(angle * 12f));
                            if (spokeVal >= 0.75f) alpha = 1f;
                        }
                    }

                    // Left Animal silhouette (Horse)
                    if (nx >= 26f && nx <= 46f && ny >= 38f && ny <= 52f)
                    {
                        float dHorse = Mathf.Sqrt((nx - 36f) * (nx - 36f) + (ny - 45f) * (ny - 45f));
                        if (dHorse <= 7f) alpha = 1f;
                        if (nx >= 26f && nx <= 34f && ny >= 43f && ny <= 51f) alpha = 1f;
                    }

                    // Right Animal silhouette (Bull)
                    if (nx >= 82f && nx <= 102f && ny >= 38f && ny <= 52f)
                    {
                        float dBull = Mathf.Sqrt((nx - 92f) * (nx - 92f) + (ny - 45f) * (ny - 45f));
                        if (dBull <= 7.5f) alpha = 1f;
                        if (nx >= 94f && nx <= 102f && ny >= 42f && ny <= 50f) alpha = 1f;
                    }

                    // 8. Abacus Lower Rim
                    if (ny >= 28f && ny <= 34f && nx >= 18f && nx <= 110f) alpha = 1f;

                    // 9. Lotus Bell Pedestal (Inverted lotus bell)
                    if (ny >= 12f && ny <= 28f)
                    {
                        float t = (ny - 12f) / 16f;
                        float halfW = Mathf.Lerp(42f, 32f, t);
                        if (Mathf.Abs(nx - cx) <= halfW)
                        {
                            float flutes = Mathf.Sin((nx - cx) * 0.6f);
                            if (flutes >= -0.3f || ny <= 16f) alpha = 1f;
                        }
                    }

                    // 10. Plinth Base Step
                    if (ny >= 4f && ny <= 12f && nx >= 24f && nx <= 104f) alpha = 1f;

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _cachedAshokaEmblem = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            return _cachedAshokaEmblem;
        }

        private static Sprite _cachedFireEmoji;
        /// <summary>Crisp procedural vector Fire Emoji sprite with multi-tone gradient on transparent background.</summary>
        public static Sprite GetFireEmojiSprite()
        {
            if (_cachedFireEmoji != null) return _cachedFireEmoji;
            int res = 128;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color colRed = new Color(0.88f, 0.15f, 0.12f, 1f);     // Deep Flame Red
            Color colOrange = new Color(0.98f, 0.45f, 0.08f, 1f);  // Vivid Safety Orange
            Color colYellow = new Color(1.00f, 0.82f, 0.16f, 1f);  // Golden Yellow
            Color colWhite = new Color(1.00f, 0.96f, 0.80f, 1f);   // Core Hot Light

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float nx = x, ny = y;
                    // Main flame teardrop
                    float dx1 = nx - 64f;
                    float baseT = Mathf.Clamp01((ny - 20f) / 96f);
                    float mainW = 0f;
                    if (ny >= 18f && ny <= 116f)
                    {
                        float curve = Mathf.Sin(baseT * Mathf.PI);
                        mainW = 38f * Mathf.Pow(curve, 0.7f) * (1.15f - baseT * 0.9f);
                        float sway = Mathf.Sin(baseT * 3.5f) * 6f * baseT;
                        dx1 -= sway;
                    }

                    // Left secondary flame lick
                    float dx2 = nx - 42f, dy2 = ny - 65f;
                    float distLickL = Mathf.Sqrt(dx2 * dx2 * 1.5f + dy2 * dy2);

                    // Right secondary flame lick
                    float dx3 = nx - 84f, dy3 = ny - 62f;
                    float distLickR = Mathf.Sqrt(dx3 * dx3 * 1.4f + dy3 * dy3);

                    float outerAlpha = 0f;
                    if (ny >= 18f && ny <= 116f && Mathf.Abs(dx1) <= mainW)
                    {
                        float edgeDist = mainW - Mathf.Abs(dx1);
                        outerAlpha = Mathf.Clamp01(edgeDist / 1.8f);
                    }
                    if (distLickL <= 18f)
                    {
                        outerAlpha = Mathf.Max(outerAlpha, Mathf.Clamp01((18f - distLickL) / 1.8f));
                    }
                    if (distLickR <= 19f)
                    {
                        outerAlpha = Mathf.Max(outerAlpha, Mathf.Clamp01((19f - distLickR) / 1.8f));
                    }

                    if (outerAlpha <= 0.001f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Inner flame shape (Yellow core)
                    float inT = Mathf.Clamp01((ny - 24f) / 70f);
                    float innerW = (ny >= 24f && ny <= 94f) ? (24f * Mathf.Sin(inT * Mathf.PI) * (1.1f - inT * 0.8f)) : 0f;
                    float inSway = Mathf.Sin(inT * 3.5f) * 4f * inT;
                    float inDx = (nx - 64f) - inSway;
                    float innerAlpha = 0f;
                    if (ny >= 24f && ny <= 94f && Mathf.Abs(inDx) <= innerW)
                    {
                        innerAlpha = Mathf.Clamp01((innerW - Mathf.Abs(inDx)) / 2.0f);
                    }

                    // White-hot core
                    float coreT = Mathf.Clamp01((ny - 28f) / 45f);
                    float coreW = (ny >= 28f && ny <= 72f) ? (13f * Mathf.Sin(coreT * Mathf.PI)) : 0f;
                    float coreAlpha = 0f;
                    if (ny >= 28f && ny <= 72f && Mathf.Abs(nx - 64f) <= coreW)
                    {
                        coreAlpha = Mathf.Clamp01((coreW - Mathf.Abs(nx - 64f)) / 1.8f);
                    }

                    // Color blending: Red/Orange -> Yellow mid -> White hot center
                    Color c = Color.Lerp(colRed, colOrange, Mathf.Clamp01((ny - 20f) / 60f));
                    c = Color.Lerp(c, colYellow, innerAlpha);
                    c = Color.Lerp(c, colWhite, coreAlpha * 0.9f);
                    c.a = outerAlpha;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            _cachedFireEmoji = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedFireEmoji;
        }

        private static Sprite _cachedGasEmoji;
        /// <summary>Crisp procedural vector Gas/Vapor Emoji sprite with wind swirls and puffs on transparent background.</summary>
        public static Sprite GetGasEmojiSprite()
        {
            if (_cachedGasEmoji != null) return _cachedGasEmoji;
            int res = 128;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color cDarkBlue = new Color(0.01f, 0.48f, 0.78f, 1f);  // #027AC7
            Color cMidBlue = new Color(0.12f, 0.65f, 0.95f, 1f);   // #1EA6F3
            Color cLightCyan = new Color(0.56f, 0.88f, 0.99f, 1f); // #8FE0FC
            Color cWhite = new Color(0.92f, 0.98f, 1.00f, 1f);

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float nx = x, ny = y;
                    float alpha = 0f;

                    // Stream 1: Top vapor swirl (y ~ 82..98, x: 28..108)
                    float dS1 = DistToSegment(nx, ny, 32f, 88f, 74f, 92f);
                    float dS1b = DistToSegment(nx, ny, 74f, 92f, 108f, 84f);
                    float dStream1 = Mathf.Min(dS1, dS1b);
                    float t1 = Mathf.Clamp01((nx - 32f) / 76f);
                    float w1 = Mathf.Lerp(7f, 3.5f, t1);
                    if (dStream1 <= w1) alpha = Mathf.Max(alpha, Mathf.Clamp01((w1 - dStream1) / 1.5f));

                    // Main Cloud / Puff Cluster in center (x: 30..102, y: 38..78)
                    float dL1 = Mathf.Sqrt((nx - 52f) * (nx - 52f) + (ny - 58f) * (ny - 58f));
                    if (dL1 <= 24f) alpha = Mathf.Max(alpha, Mathf.Clamp01((24f - dL1) / 1.8f));

                    float dL2 = Mathf.Sqrt((nx - 72f) * (nx - 72f) + (ny - 66f) * (ny - 66f));
                    if (dL2 <= 21f) alpha = Mathf.Max(alpha, Mathf.Clamp01((21f - dL2) / 1.8f));

                    float dL3 = Mathf.Sqrt((nx - 88f) * (nx - 88f) + (ny - 54f) * (ny - 54f));
                    if (dL3 <= 18f) alpha = Mathf.Max(alpha, Mathf.Clamp01((18f - dL3) / 1.8f));

                    // Stream 2: Lower trailing vapor stream (y ~ 32..44, x: 18..96)
                    float dS2 = DistToSegment(nx, ny, 20f, 42f, 62f, 40f);
                    float dS2b = DistToSegment(nx, ny, 62f, 40f, 96f, 32f);
                    float dStream2 = Mathf.Min(dS2, dS2b);
                    float t2 = Mathf.Clamp01((nx - 20f) / 76f);
                    float w2 = Mathf.Lerp(6.5f, 3.0f, t2);
                    if (dStream2 <= w2) alpha = Mathf.Max(alpha, Mathf.Clamp01((w2 - dStream2) / 1.5f));

                    // Wind curl tail on bottom left
                    float dTail = Mathf.Sqrt((nx - 22f) * (nx - 22f) + (ny - 48f) * (ny - 48f));
                    if (dTail <= 8f && dTail >= 4f && nx <= 22f) alpha = Mathf.Max(alpha, 1f);

                    if (alpha <= 0.001f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float centerDist = Mathf.Sqrt((nx - 68f) * (nx - 68f) + (ny - 62f) * (ny - 62f));
                    float corePct = Mathf.Clamp01(1f - centerDist / 34f);
                    Color c = Color.Lerp(cDarkBlue, cMidBlue, Mathf.Clamp01(nx / 128f));
                    c = Color.Lerp(c, cLightCyan, corePct * 0.85f);
                    c = Color.Lerp(c, cWhite, Mathf.Pow(corePct, 2.5f));
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            _cachedGasEmoji = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedGasEmoji;
        }

        private static Sprite _cachedGearEmoji;
        /// <summary>Crisp procedural vector Machinery Gear Emoji sprite with 8 teeth and axle hole on transparent background.</summary>
        public static Sprite GetGearEmojiSprite()
        {
            if (_cachedGearEmoji != null) return _cachedGearEmoji;
            int res = 128;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color cDarkGreen = new Color(0.02f, 0.44f, 0.31f, 1f); // #05704F Dark Emerald
            Color cMidGreen  = new Color(0.06f, 0.65f, 0.45f, 1f); // #10A773 Vivid Green
            Color cLightRim  = new Color(0.40f, 0.90f, 0.68f, 1f); // #66E6AD Light Highlight

            float cx = 64f, cy = 64f;
            float rOuter = 54f; // Tooth tips
            float rRoot  = 41f; // Tooth root
            float rHole  = 18f; // Axle hole
            int numTeeth = 8;
            float toothAngle = (2f * Mathf.PI) / numTeeth;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < rHole - 1.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float holeAlpha = Mathf.Clamp01((dist - (rHole - 1.2f)) / 1.5f);
                    float angle = Mathf.Atan2(dy, dx) + Mathf.PI;
                    float modAngle = angle % toothAngle;
                    float halfTooth = toothAngle * 0.5f;

                    float toothProfile = Mathf.Abs(modAngle - halfTooth) / halfTooth;
                    float targetR = Mathf.Lerp(rOuter, rRoot, Mathf.SmoothStep(0.25f, 0.75f, toothProfile));

                    if (dist > targetR + 1.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float outerAlpha = Mathf.Clamp01((targetR + 1.2f - dist) / 1.5f);
                    float alpha = Mathf.Min(outerAlpha, holeAlpha);

                    float light = Mathf.Clamp01((-dx + dy) / 90f + 0.5f);
                    float axleRim = (dist >= rHole && dist <= rHole + 5f) ? 0.35f : 0f;

                    Color c = Color.Lerp(cDarkGreen, cMidGreen, light);
                    c = Color.Lerp(c, cLightRim, axleRim);
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            _cachedGearEmoji = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedGearEmoji;
        }

        // ── BOLT SPRITE (Electrical Safety) ───────────────────────────
        private static Sprite _cachedBolt;
        public static Sprite GetBoltSprite()
        {
            if (_cachedBolt != null) return _cachedBolt;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float px = (float)x / res;
                    float py = (float)y / res;
                    bool filled = false;
                    // Simple lightning bolt shape: two triangles
                    // Upper half: from top-right to center-left
                    if (py >= 0.42f && py <= 0.92f)
                    {
                        float mid = 0.5f + (py - 0.42f) / (0.92f - 0.42f) * (-0.22f);
                        float w   = 0.18f;
                        if (px >= mid - w && px <= mid + w) filled = true;
                    }
                    if (py >= 0.08f && py <= 0.58f)
                    {
                        float mid = 0.5f - (py - 0.08f) / (0.58f - 0.08f) * (-0.22f);
                        float w   = 0.18f;
                        if (px >= mid - w && px <= mid + w) filled = true;
                    }
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedBolt = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedBolt;
        }

        // ── HARD HAT SPRITE (Mine Hazard) ──────────────────────────────
        private static Sprite _cachedHardHat;
        public static Sprite GetHardHatSprite()
        {
            if (_cachedHardHat != null) return _cachedHardHat;
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float px = (float)x / res;
                    float py = (float)y / res;
                    bool filled = false;
                    // Brim
                    if (py >= 0.24f && py <= 0.34f && px >= 0.1f && px <= 0.9f) filled = true;
                    // Dome
                    float cx = 0.5f, cy = 0.52f, rx = 0.36f, ry = 0.28f;
                    float dx2 = (px - cx) / rx;
                    float dy2 = (py - cy) / ry;
                    if (dx2 * dx2 + dy2 * dy2 <= 1f && py >= cy - ry) filled = true;
                    // Rim band
                    if (py >= 0.34f && py <= 0.40f && px >= 0.18f && px <= 0.82f) filled = true;
                    tex.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _cachedHardHat = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return _cachedHardHat;
        }

        // ── PROJECT IMAGE LOADER ───────────────────────────────────────
        private static readonly Dictionary<string, Sprite> _loadedProjectSprites = new Dictionary<string, Sprite>();

        /// <summary>Clears the cached project sprites to force re-loading and re-processing.</summary>
        public static void ClearSpriteCache() => _loadedProjectSprites.Clear();

        /// <summary>Loads an image from Resources/Images or Assets/SurakshaAR/UI/images as a Texture/Sprite at runtime.</summary>
        public static Sprite LoadProjectSprite(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;

            if (_loadedProjectSprites.TryGetValue(filename, out var cached) && cached != null)
                return cached;

            string lower = filename.ToLower();

            // 1. Try Resources.Load directly across normalized name variants (works inside compiled APKs on Android)
            var cleanName = System.IO.Path.GetFileNameWithoutExtension(filename);
            var nameVariants = new string[]
            {
                cleanName,
                cleanName.Replace(" ", "_"),
                cleanName.Replace("_", " "),
                cleanName.ToLower(),
                cleanName.ToLower().Replace(" ", "_"),
                cleanName.ToLower().Replace("_", " ")
            };

            foreach (var name in nameVariants)
            {
                var resSpr = Resources.Load<Sprite>("Images/" + name);
                if (resSpr != null)
                {
                    _loadedProjectSprites[filename] = resSpr;
                    return resSpr;
                }
                var resTex = Resources.Load<Texture2D>("Images/" + name);
                if (resTex != null)
                {
                    var spr = Sprite.Create(resTex, new Rect(0, 0, resTex.width, resTex.height), new Vector2(0.5f, 0.5f), 100f);
                    _loadedProjectSprites[filename] = spr;
                    return spr;
                }
            }

            // 2. Role-based fallback for key screens on mobile APKs
            if (lower.Contains("splash"))
            {
                var fallback = Resources.Load<Sprite>("Images/splashbackgroundimage")
                            ?? Resources.Load<Sprite>("Images/home_menu_mine_baground")
                            ?? Resources.Load<Sprite>("Images/jharkhand_mine_banner_clean");
                if (fallback != null) { _loadedProjectSprites[filename] = fallback; return fallback; }
            }
            if (lower.Contains("fire") && (lower.Contains("intro") || lower.Contains("header") || lower.Contains("hero")))
            {
                var fallback = Resources.Load<Sprite>("Images/fire_intro")
                            ?? Resources.Load<Sprite>("Images/jharkhand_miner_hero")
                            ?? Resources.Load<Sprite>("Images/login_header_perfect");
                if (fallback != null) { _loadedProjectSprites[filename] = fallback; return fallback; }
            }
            if (lower.Contains("home") || lower.Contains("menu") || lower.Contains("baground") || lower.Contains("background"))
            {
                var fallback = Resources.Load<Sprite>("Images/home_menu_mine_baground")
                            ?? Resources.Load<Sprite>("Images/jharkhand_mine_banner_clean")
                            ?? Resources.Load<Sprite>("Images/login_header_perfect");
                if (fallback != null) { _loadedProjectSprites[filename] = fallback; return fallback; }
            }
            if (lower.Contains("mission"))
            {
                var fallback = Resources.Load<Sprite>("Images/mission_banner_perfect")
                            ?? Resources.Load<Sprite>("Images/miners_team_banner");
                if (fallback != null) { _loadedProjectSprites[filename] = fallback; return fallback; }
            }
            if (lower.Contains("avatar") || lower.Contains("worker"))
            {
                var fallback = Resources.Load<Sprite>("Images/profile_avatar_ref2")
                            ?? Resources.Load<Sprite>("Images/worker_avatar_ref1")
                            ?? Resources.Load<Sprite>("Images/worker_miner_avatar");
                if (fallback != null) { _loadedProjectSprites[filename] = fallback; return fallback; }
            }

            // 3. Filesystem search fallback (useful in Editor / Standalone desktop)
            string[] searchPaths = new string[]
            {
                System.IO.Path.Combine(Application.dataPath, "Resources", "Images", filename),
                System.IO.Path.Combine(Application.dataPath, "Resources", "Images", filename + ".png"),
                System.IO.Path.Combine(Application.dataPath, "Resources", "Images", filename + ".jpg"),
                System.IO.Path.Combine(Application.dataPath, "Resources", "Images", filename + ".jpeg"),
                System.IO.Path.Combine(Application.dataPath, "SurakshaAR", "UI", "images", filename),
                System.IO.Path.Combine(Application.dataPath, "SurakshaAR", "UI", "images", filename + ".png"),
                System.IO.Path.Combine(Application.dataPath, "SurakshaAR", "UI", "images", filename + ".jpg"),
                System.IO.Path.Combine(Application.dataPath, "SurakshaAR", "UI", "images", filename + ".jpeg"),
                System.IO.Path.Combine(Application.streamingAssetsPath, filename),
                System.IO.Path.Combine(Application.streamingAssetsPath, filename + ".png"),
                System.IO.Path.Combine(Application.streamingAssetsPath, filename + ".jpg")
            };

            foreach (var path in searchPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        byte[] data = System.IO.File.ReadAllBytes(path);
                        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        if (ImageConversion.LoadImage(tex, data))
                        {
                            tex.name = filename;

                            // If this is an emoji icon, make the white background transparent using RGBA32
                            if (filename.ToLower().Contains("emoji"))
                            {
                                var srcPixels = tex.GetPixels();
                                var rgbaTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
                                rgbaTex.filterMode = FilterMode.Bilinear;
                                var dstPixels = new Color[srcPixels.Length];
                                for (int i = 0; i < srcPixels.Length; i++)
                                {
                                    var p = srcPixels[i];
                                    if (p.r > 0.88f && p.g > 0.88f && p.b > 0.88f)
                                    {
                                        float minVal = Mathf.Min(p.r, Mathf.Min(p.g, p.b));
                                        if (minVal > 0.94f)
                                        {
                                            dstPixels[i] = new Color(1f, 1f, 1f, 0f);
                                        }
                                        else
                                        {
                                            float alpha = Mathf.Clamp01((0.94f - minVal) / 0.06f);
                                            dstPixels[i] = new Color(p.r, p.g, p.b, alpha);
                                        }
                                    }
                                    else
                                    {
                                        dstPixels[i] = new Color(p.r, p.g, p.b, 1f);
                                    }
                                }
                                rgbaTex.SetPixels(dstPixels);
                                rgbaTex.Apply();
                                UnityEngine.Object.DestroyImmediate(tex);
                                tex = rgbaTex;
                            }

                            var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                            _loadedProjectSprites[filename] = spr;
                            return spr;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[UIHelper] Failed to load sprite '{filename}': {ex.Message}");
                    }
                }
            }

            // 4. Guaranteed procedural vector fallbacks for emblem and emojis if image assets are missing
            if (lower.Contains("ashoka_lion_emblem") || lower.Contains("emblem"))
            {
                var emblem = GetAshokaEmblemSprite();
                _loadedProjectSprites[filename] = emblem;
                return emblem;
            }
            if (lower.Contains("fire_emoji") || lower.Contains("fire_module") || (lower.Contains("fire") && lower.Contains("emoji")))
            {
                var fire = GetFireEmojiSprite();
                _loadedProjectSprites[filename] = fire;
                return fire;
            }
            if (lower.Contains("gas_emoji") || lower.Contains("gas_module") || (lower.Contains("gas") && lower.Contains("emoji")))
            {
                var gas = GetGasEmojiSprite();
                _loadedProjectSprites[filename] = gas;
                return gas;
            }
            if (lower.Contains("gear_emoji") || lower.Contains("machinery_module") || (lower.Contains("gear") && lower.Contains("emoji")) || (lower.Contains("machinery") && lower.Contains("emoji")))
            {
                var gear = GetGearEmojiSprite();
                _loadedProjectSprites[filename] = gear;
                return gear;
            }

            return null;
        }
    }
}
