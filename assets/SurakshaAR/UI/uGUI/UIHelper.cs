using System.Collections.Generic;
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
            // Try NotoSans which includes Devanagari glyphs for Hindi
            _defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSans SDF");
            if (_defaultFont == null)
                _defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSans-Regular SDF");
            if (_defaultFont == null)
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

            if (_devanagariFont != null && !baseFont.fallbackFontAssetTable.Contains(_devanagariFont))
            {
                baseFont.fallbackFontAssetTable.Add(_devanagariFont);
                return;
            }

            // Check if a Devanagari font asset is available in Resources
            var devAsset = Resources.Load<TMP_FontAsset>("Fonts/NotoSansDevanagari SDF")
                        ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansDevanagari SDF")
                        ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSans SDF");

            if (devAsset == null)
            {
                var font = Resources.Load<Font>("Fonts/NotoSansDevanagari");
                if (font != null)
                {
                    try
                    {
                        devAsset = TMP_FontAsset.CreateFontAsset(font, 36, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                        if (devAsset != null) devAsset.name = "NotoSansDevanagari Dynamic";
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[UIHelper] Dynamic font asset creation failed: {ex.Message}");
                    }
                }
            }

            if (devAsset != null)
            {
                _devanagariFont = devAsset;
                if (!baseFont.fallbackFontAssetTable.Contains(devAsset))
                {
                    baseFont.fallbackFontAssetTable.Add(devAsset);
                }
            }
        }

        /// <summary>
        /// Returns true if the active TMP font asset or any loaded fallback
        /// contains glyphs for the Devanagari script (checked via 'क' U+0915).
        /// </summary>
        public static bool HasDevanagariSupport()
        {
            var font = GetDefaultFont();
            if (font == null) return false;
            if (font.HasCharacter(0x0915)) return true;
            if (font.fallbackFontAssetTable != null)
            {
                foreach (var fb in font.fallbackFontAssetTable)
                {
                    if (fb != null && fb.HasCharacter(0x0915)) return true;
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
            var font = GetDefaultFont();
            if (font == null) return false;
            if (font.HasCharacter(0x1C5A)) return true;
            if (font.fallbackFontAssetTable != null)
            {
                foreach (var fb in font.fallbackFontAssetTable)
                {
                    if (fb != null && fb.HasCharacter(0x1C5A)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a font suitable for Devanagari (Hindi) text.
        /// Tries dedicated Devanagari/NotoSans assets, then falls back to default.
        /// </summary>
        public static TMP_FontAsset GetDevanagariFont()
        {
            if (_devanagariFont != null) return _devanagariFont;
            _devanagariFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansDevanagari SDF");
            if (_devanagariFont == null)
                _devanagariFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSans SDF");
            if (_devanagariFont == null)
                _devanagariFont = GetDefaultFont();
            return _devanagariFont;
        }

        /// <summary>Adds a TextMeshProUGUI component with default font pre-assigned.</summary>
        public static TextMeshProUGUI AddTMP(GameObject go)
        {
            if (go == null) return null;
            var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            if (tmp == null) return null;
            var font = GetDefaultFont();
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
            rt.sizeDelta = new Vector2(0, fontSize * 1.35f);

            var le = go.AddComponent<LayoutElement>();
            if (!wrap)
                le.preferredHeight = fontSize * 1.35f;
            else
                le.flexibleWidth = 1f;

            var tmp = AddTMP(go);
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
            return tmp;
        }

        /// <summary>Create a Button with a colored background Image and TMP label.</summary>
        public static Button MakeButton(string name, Transform parent,
            string label, float fontSize, Color bgColor, Color textColor,
            float cornerRadius = 12f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 56);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 56;

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

            // Label child (stretches to fill button with padding)
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(12, 4);
            labelRT.offsetMax = new Vector2(-12, -4);

            var tmp = AddTMP(labelGO);
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;

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
            return rt;
        }

        /// <summary>Add a LayoutElement with preferred/minimum sizes.</summary>
        public static LayoutElement SetLayout(GameObject go,
            float preferredWidth = -1, float preferredHeight = -1,
            float minWidth = -1, float minHeight = -1,
            bool flexibleWidth = false, float flexWidth = 1,
            bool flexibleHeight = false, float flexHeight = 1)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (preferredWidth >= 0) le.preferredWidth = preferredWidth;
            if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
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

        /// <summary>Size a RectTransform to a fixed width/height, anchored at center.</summary>
        public static void SetSize(RectTransform rt, float width, float height)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
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
        public static void SetImageRoundedSprite(Image img, float radius = 12f)
        {
            // Use the built-in rounded background sprite that ships with UI.
            img.sprite = Resources.Load<Sprite>("UI/Skin/Background") ??
                         CreateRoundedRectSprite(radius);
            img.type = Image.Type.Sliced;
        }

        /// <summary>Create a simple white circle sprite for rounded UI elements.</summary>
        public static Sprite CreateCircleSprite()
        {
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            float center = res / 2f;
            float r = center - 1f;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dx = x - center, dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    tex.SetPixel(x, y, dist <= r ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>Create a simple rounded-rectangle sprite.</summary>
        public static Sprite CreateRoundedRectSprite(float radius = 12f)
        {
            int res = 64;
            int r = Mathf.RoundToInt(radius);
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    int cx = Mathf.Clamp(x, r, res - r - 1);
                    int cy = Mathf.Clamp(y, r, res - r - 1);
                    float dx = cx - x, dy = cy - y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    tex.SetPixel(x, y, dist <= r ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            int b = r + 2;
            return Sprite.Create(tex, new Rect(0, 0, res, res),
                new Vector2(0.5f, 0.5f), 100f,
                0, SpriteMeshType.FullRect,
                new Vector4(b, b, b, b));
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
            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = res / 2f;
            float outerR = center - 1f;
            float innerR = outerR - thickness;
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
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite _cachedCircleOutline;
        public static Sprite GetCircleOutlineSprite()
        {
            if (_cachedCircleOutline == null) _cachedCircleOutline = CreateCircleOutlineSprite(5f);
            return _cachedCircleOutline;
        }

        // ── PROJECT IMAGE LOADER ───────────────────────────────────────
        private static readonly Dictionary<string, Sprite> _loadedProjectSprites = new Dictionary<string, Sprite>();

        /// <summary>Loads an image from Resources/Images or Assets/SurakshaAR/UI/images as a Texture/Sprite at runtime.</summary>
        public static Sprite LoadProjectSprite(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            if (_loadedProjectSprites.TryGetValue(filename, out var cached) && cached != null)
                return cached;

            // 1. Try Resources.Load directly (works on Android APK, iOS, and Editor)
            var cleanName = System.IO.Path.GetFileNameWithoutExtension(filename);
            var resSpr = Resources.Load<Sprite>("Images/" + cleanName);
            if (resSpr != null)
            {
                _loadedProjectSprites[filename] = resSpr;
                return resSpr;
            }
            var resTex = Resources.Load<Texture2D>("Images/" + cleanName);
            if (resTex != null)
            {
                var spr = Sprite.Create(resTex, new Rect(0, 0, resTex.width, resTex.height), new Vector2(0.5f, 0.5f), 100f);
                _loadedProjectSprites[filename] = spr;
                return spr;
            }

            string[] searchPaths = new string[]
            {
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
            return null;
        }
    }
}
