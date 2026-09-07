using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SurakshaAR.UI
{
    /// <summary>
    /// Main menu for the SurakshaAR app.
    ///
    /// Built entirely in code (UI Toolkit) so it can be auto-created at runtime
    /// without any scene or prefab wiring:
    ///   - SurakshaAR brand header (matching the app's FireSafety design theme)
    ///   - Language selector (English, Hindi, Santali)
    ///   - Fire module intro card
    ///   - Top 7 section (placeholder) - leaderboard stub
    ///   - "Start Module" button that launches the fire training scene.
    ///
    /// All user-facing text comes from MainMenuLocalization (JSON-backed,
    /// see app/Localization/{English,Hindi,Santali}/main_menu.json).
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        public const string RootName = "SurakshaAR_MainMenu";
        public const string DefaultFireTrainingScene = "FireTraining";

        // Design tokens (mirror of FireSafetyTheme.uss).
        private static readonly Color Bg = new Color(0.961f, 0.965f, 0.969f);   // #F5F6F7
        private static readonly Color Surface = Color.white;
        private static readonly Color Ink = new Color(0.09f, 0.13f, 0.17f);     // #17212B
        private static readonly Color InkSoft = new Color(0.2f, 0.26f, 0.3f);   // #34424D
        private static readonly Color Primary = new Color(0.7f, 0.15f, 0.12f);  // #B3261E
        private static readonly Color PrimaryDark = new Color(0.5f, 0.11f, 0.11f); // #7F1D1D
        private static readonly Color PrimarySoft = new Color(0.984f, 0.929f, 0.925f); // #FBEDEC
        private static readonly Color Muted = new Color(0.4f, 0.44f, 0.49f);     // #65717C
        private static readonly Color MutedLight = new Color(0.54f, 0.58f, 0.61f);   // #8A949C
        private static readonly Color BorderLine = new Color(0.882f, 0.898f, 0.910f);   // #E1E5E8
        private static readonly Color Gold = new Color(0.78f, 0.6f, 0.1f, 1f);

        [Header("Scene Launch")]
        [Tooltip("Name of the fire training scene to load. Must be present in Build Settings.")]
        [SerializeField]
        private string fireTrainingScene = DefaultFireTrainingScene;

        private UIDocument document;

        /// <summary>Called right before the training scene loads (bootstrapper uses this to suppress the menu on the next scene).</summary>
        private Action onStartTraining;

        /// <summary>Creates the main menu on a fresh GameObject. Used by the runtime bootstrapper.</summary>
        public static MainMenuController Create(Action onStartTraining = null)
        {
            var go = new GameObject(RootName);
            var controller = go.AddComponent<MainMenuController>();
            controller.onStartTraining = onStartTraining;
            controller.AddDocument();
            return controller;
        }

        private void AddDocument()
        {
            document = GetComponent<UIDocument>();
            if (document == null)
                document = gameObject.AddComponent<UIDocument>();

            document.sortingOrder = 100; // keep the menu above any runtime HUD
            if (document.panelSettings == null)
                document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        }

        private void OnEnable()
        {
            if (document == null)
                AddDocument();
            MainMenuLocalization.LoadSavedLanguage();
            BuildUI();
        }

        /// <summary>Launches the fire training scene ("Start Module" action).</summary>
        public void StartFireTraining()
        {
            Debug.Log("[MainMenu] Launching fire training scene: " + fireTrainingScene);
            var handler = onStartTraining;
            onStartTraining = null;
            handler?.Invoke();
            SceneManager.LoadScene(fireTrainingScene);
        }

        private void BuildUI()
        {
            var root = document.rootVisualElement;
            if (root == null)
                return;

            root.Clear();
            root.style.flexGrow = 1;
            root.style.backgroundColor = Bg;
            root.style.paddingLeft = 20;
            root.style.paddingRight = 20;
            root.style.paddingTop = 28;
            root.style.paddingBottom = 20;
            root.style.height = Length.Percent(100f);

            AddBrandHeader(root);
            AddLanguageSelector(root);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            root.Add(scroll);

            var content = new VisualElement();
            content.style.flexGrow = 1;
            scroll.Add(content);

            AddFireIntro(content);
            AddTopSeven(content);
            AddStartButton(content);
        }

        // ------------------------------------------------------------------
        // Brand header
        // ------------------------------------------------------------------

        private void AddBrandHeader(VisualElement parent)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 16;
            parent.Add(row);

            // Brand badge (red circle with "F" letter, like the splash screen badge).
            var badge = new Label("F");
            badge.style.width = 60;
            badge.style.height = 60;
            badge.style.backgroundColor = Primary;
            badge.style.color = Color.white;
            badge.style.fontSize = 30;
            badge.style.unityFontStyleAndWeight = FontStyle.Bold;
            badge.style.unityTextAlign = TextAnchor.MiddleCenter;
            badge.style.borderTopLeftRadius = 30;
            badge.style.borderTopRightRadius = 30;
            badge.style.borderBottomLeftRadius = 30;
            badge.style.borderBottomRightRadius = 30;
            badge.style.marginRight = 14;
            row.Add(badge);

            var textCol = new VisualElement();
            textCol.style.flexGrow = 1;
            row.Add(textCol);

            var eyebrow = new Label(MainMenuLocalization.Get("app_name"));
            eyebrow.style.fontSize = 11;
            eyebrow.style.color = PrimaryDark;
            eyebrow.style.unityFontStyleAndWeight = FontStyle.Bold;
            eyebrow.style.marginBottom = 1;
            textCol.Add(eyebrow);

            var title = new Label(MainMenuLocalization.Get("menu_title"));
            title.style.fontSize = 24;
            title.style.color = Ink;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 1;
            textCol.Add(title);

            var tagline = new Label(MainMenuLocalization.Get("app_tagline"));
            tagline.style.fontSize = 13;
            tagline.style.color = Muted;
            textCol.Add(tagline);
        }

        // ------------------------------------------------------------------
        // Language selector
        // ------------------------------------------------------------------

        private void AddLanguageSelector(VisualElement parent)
        {
            var label = new Label(MainMenuLocalization.Get("language_label"));
            label.style.fontSize = 11;
            label.style.color = Muted;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 6;
            parent.Add(label);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 16;
            parent.Add(row);

            foreach (string code in MainMenuLocalization.SupportedLanguages)
            {
                bool isActive = code == MainMenuLocalization.CurrentLanguage;
                var button = new Button();
                button.text = MainMenuLocalization.LanguageDisplayName(code);
                button.style.flexGrow = 1;
                button.style.height = 36;
                button.style.fontSize = 13;
                button.style.unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal;
                button.style.color = isActive ? Primary : InkSoft;
                button.style.backgroundColor = isActive ? PrimarySoft : Surface;
                button.style.borderTopWidth = isActive ? 2 : 1;
                button.style.borderBottomWidth = isActive ? 2 : 1;
                button.style.borderLeftWidth = isActive ? 2 : 1;
                button.style.borderRightWidth = isActive ? 2 : 1;
                button.style.borderTopColor = isActive ? Primary : BorderLine;
                button.style.borderBottomColor = isActive ? Primary : BorderLine;
                button.style.borderLeftColor = isActive ? Primary : BorderLine;
                button.style.borderRightColor = isActive ? Primary : BorderLine;
                button.style.borderTopLeftRadius = 18;
                button.style.borderTopRightRadius = 18;
                button.style.borderBottomLeftRadius = 18;
                button.style.borderBottomRightRadius = 18;
                button.style.marginRight = 6;

                string captured = code;
                button.clicked += () =>
                {
                    if (captured == MainMenuLocalization.CurrentLanguage) return;
                    MainMenuLocalization.SetLanguage(captured);
                    BuildUI();
                };
                row.Add(button);
            }
        }

        // ------------------------------------------------------------------
        // Fire module intro card
        // ------------------------------------------------------------------

        private void AddFireIntro(VisualElement parent)
        {
            var card = new VisualElement();
            card.style.backgroundColor = Surface;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = BorderLine;
            card.style.borderBottomColor = BorderLine;
            card.style.borderLeftColor = BorderLine;
            card.style.borderRightColor = BorderLine;
            card.style.borderTopLeftRadius = 16;
            card.style.borderTopRightRadius = 16;
            card.style.borderBottomLeftRadius = 16;
            card.style.borderBottomRightRadius = 16;
            card.style.paddingLeft = 16;
            card.style.paddingRight = 16;
            card.style.paddingTop = 14;
            card.style.paddingBottom = 14;
            card.style.marginBottom = 14;
            parent.Add(card);

            // Title row: module title + AR badge chip.
            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems = Align.Center;
            card.Add(titleRow);

            var title = new Label(MainMenuLocalization.Get("module_title"));
            title.style.fontSize = 18;
            title.style.color = Ink;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.flexGrow = 1;
            titleRow.Add(title);

            var badge = new Label(MainMenuLocalization.Get("module_badge"));
            badge.style.backgroundColor = PrimarySoft;
            badge.style.color = Primary;
            badge.style.fontSize = 11;
            badge.style.unityFontStyleAndWeight = FontStyle.Bold;
            badge.style.borderTopLeftRadius = 10;
            badge.style.borderTopRightRadius = 10;
            badge.style.borderBottomLeftRadius = 10;
            badge.style.borderBottomRightRadius = 10;
            badge.style.paddingLeft = 8;
            badge.style.paddingRight = 8;
            badge.style.paddingTop = 3;
            badge.style.paddingBottom = 3;
            titleRow.Add(badge);

            var subtitle = new Label(MainMenuLocalization.Get("module_subtitle"));
            subtitle.style.fontSize = 14;
            subtitle.style.color = PrimaryDark;
            subtitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            subtitle.style.marginTop = 2;
            subtitle.style.marginBottom = 10;
            card.Add(subtitle);

            var description = new Label(MainMenuLocalization.Get("module_description"));
            description.style.fontSize = 13;
            description.style.color = InkSoft;
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.marginBottom = 10;
            card.Add(description);

            var learnTitle = new Label(MainMenuLocalization.Get("learn_title"));
            learnTitle.style.fontSize = 13;
            learnTitle.style.color = Ink;
            learnTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            learnTitle.style.marginBottom = 4;
            card.Add(learnTitle);

            AddBullet(card, MainMenuLocalization.Get("bullet_1"));
            AddBullet(card, MainMenuLocalization.Get("bullet_2"));
            AddBullet(card, MainMenuLocalization.Get("bullet_3"));
            AddBullet(card, MainMenuLocalization.Get("bullet_4"));

            var meta = new Label(MainMenuLocalization.Get("meta"));
            meta.style.fontSize = 11;
            meta.style.color = Muted;
            meta.style.marginTop = 8;
            card.Add(meta);
        }

        private void AddBullet(VisualElement parent, string text)
        {
            var bullet = new Label("•  " + text);
            bullet.style.fontSize = 12;
            bullet.style.color = InkSoft;
            bullet.style.whiteSpace = WhiteSpace.Normal;
            bullet.style.marginBottom = 3;
            parent.Add(bullet);
        }

        // ------------------------------------------------------------------
        // Top 7 - placeholder leaderboard
        // ------------------------------------------------------------------

        private void AddTopSeven(VisualElement parent)
        {
            var card = new VisualElement();
            card.style.backgroundColor = Surface;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = BorderLine;
            card.style.borderBottomColor = BorderLine;
            card.style.borderLeftColor = BorderLine;
            card.style.borderRightColor = BorderLine;
            card.style.borderTopLeftRadius = 16;
            card.style.borderTopRightRadius = 16;
            card.style.borderBottomLeftRadius = 16;
            card.style.borderBottomRightRadius = 16;
            card.style.paddingLeft = 16;
            card.style.paddingRight = 16;
            card.style.paddingTop = 14;
            card.style.paddingBottom = 14;
            card.style.marginBottom = 14;
            parent.Add(card);

            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            card.Add(headerRow);

            var header = new Label(MainMenuLocalization.Get("top7_title"));
            header.style.fontSize = 16;
            header.style.color = Ink;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.flexGrow = 1;
            headerRow.Add(header);

            var placeholderBadge = new Label(MainMenuLocalization.Get("top7_badge"));
            placeholderBadge.style.backgroundColor = BorderLine;
            placeholderBadge.style.color = Muted;
            placeholderBadge.style.fontSize = 11;
            placeholderBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            placeholderBadge.style.borderTopLeftRadius = 10;
            placeholderBadge.style.borderTopRightRadius = 10;
            placeholderBadge.style.borderBottomLeftRadius = 10;
            placeholderBadge.style.borderBottomRightRadius = 10;
            placeholderBadge.style.paddingLeft = 8;
            placeholderBadge.style.paddingRight = 8;
            placeholderBadge.style.paddingTop = 2;
            placeholderBadge.style.paddingBottom = 2;
            headerRow.Add(placeholderBadge);

            var caption = new Label(MainMenuLocalization.Get("top7_caption"));
            caption.style.fontSize = 11;
            caption.style.color = Muted;
            caption.style.whiteSpace = WhiteSpace.Normal;
            caption.style.marginTop = 6;
            caption.style.marginBottom = 8;
            card.Add(caption);

            string traineeLabel = MainMenuLocalization.Get("top7_trainee");
            string pointsLabel = MainMenuLocalization.Get("top7_points");

            // 7 placeholder rows.
            for (int i = 1; i <= 7; i++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 10;
                row.style.paddingRight = 10;
                row.style.paddingTop = 8;
                row.style.paddingBottom = 8;
                row.style.borderBottomWidth = i < 7 ? 1 : 0;
                row.style.borderBottomColor = BorderLine;
                card.Add(row);

                var rank = new Label(i.ToString());
                rank.style.width = 24;
                rank.style.fontSize = 14;
                rank.style.color = i == 1 ? Gold : InkSoft;
                rank.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(rank);

                var nameLabel = new Label(traineeLabel + " #" + i);
                nameLabel.style.flexGrow = 1;
                nameLabel.style.fontSize = 13;
                nameLabel.style.color = InkSoft;
                row.Add(nameLabel);

                var score = new Label("— " + pointsLabel);
                score.style.fontSize = 11;
                score.style.color = MutedLight;
                row.Add(score);
            }
        }

        // ------------------------------------------------------------------
        // Start Module button
        // ------------------------------------------------------------------

        private void AddStartButton(VisualElement parent)
        {
            var startButton = new Button();
            startButton.text = MainMenuLocalization.Get("start_module");
            startButton.style.height = 60;
            startButton.style.fontSize = 16;
            startButton.style.color = Color.white;
            startButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            startButton.style.backgroundColor = Primary;
            startButton.style.borderTopLeftRadius = 14;
            startButton.style.borderTopRightRadius = 14;
            startButton.style.borderBottomLeftRadius = 14;
            startButton.style.borderBottomRightRadius = 14;
            startButton.style.marginBottom = 6;
            startButton.clicked += StartFireTraining;
            parent.Add(startButton);

            var hint = new Label(MainMenuLocalization.Get("start_hint"));
            hint.style.fontSize = 11;
            hint.style.color = Muted;
            hint.style.unityTextAlign = TextAnchor.MiddleCenter;
            parent.Add(hint);
        }

    }
}