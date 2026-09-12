using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// UI Toolkit application shell for the pre-AR experience. Visual structure
/// lives in FireSafetyApp.uxml and FireSafetyTheme.uss; this class only owns
/// navigation, local session data and training events.
/// </summary>
public sealed class MenuUIController : MonoBehaviour
{
    private const string LayoutPath = "FireSafetyUI/FireSafetyApp";
    private const string ThemePath = "FireSafetyUI/FireSafetyTheme";

    private UIDocument document;
    private VisualElement root;
    private string selectedLanguage = "English";

    private void Start()
    {
        AppSession.EnsureExists();
        selectedLanguage = AppSession.Instance.language;

        VisualTreeAsset layout = Resources.Load<VisualTreeAsset>(LayoutPath);
        StyleSheet theme = Resources.Load<StyleSheet>(ThemePath);
        if (layout == null || theme == null)
        {
            Debug.LogError("Menu UI Toolkit assets could not be loaded from Resources/FireSafetyUI.");
            return;
        }

        document = gameObject.AddComponent<UIDocument>();
        document.panelSettings = CreatePanelSettings();
        document.visualTreeAsset = layout;
        root = document.rootVisualElement;
        root.styleSheets.Add(theme);

        // Apply Android safe-area insets (status bar / gesture bar).
        var safeArea = SurakshaAR.Core.SafeAreaDriver.Instance;
        if (safeArea == null)
        {
            safeArea = new GameObject("SafeAreaDriver").AddComponent<SurakshaAR.Core.SafeAreaDriver>();
        }
        safeArea.RegisterRoot(root);

        WireButtons();
        Show("splash-screen");
    }

    /// <summary>
    /// Shared mobile scaling: 500x1111 (9:20) width-matched reference, so the
    /// CSS-scale USS values (11-22px) render at Figma-proportional sizes on
    /// every portrait Android resolution.
    /// </summary>
    private static PanelSettings CreatePanelSettings()
    {
        PanelSettings settings = ScriptableObject.CreateInstance<PanelSettings>();
        settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        settings.referenceResolution = new Vector2Int(500, 1111);
        settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        settings.match = 0f; // match width
        return settings;
    }

    private void WireButtons()
    {
        Button("get-started").clicked += () => Show("language-screen");
        Button("login-back").clicked += () => Show("splash-screen");
        Button("login-continue").clicked += Login;
        Button("language-back").clicked += () => Show("splash-screen");
        Button("language-continue").clicked += ContinueFromLanguage;
        Button("intro-back").clicked += () => Show("catalogue-screen");
        Button("intro-next").clicked += () => Show("equipment-screen");
        Button("equipment-back").clicked += () => Show("intro-screen");
        Button("equipment-next").clicked += () => Show("howto-screen");
        Button("howto-back").clicked += () => Show("equipment-screen");
        Button("start-ar").clicked += StartFireTraining;
        Button("home-start").clicked += () => Show("catalogue-screen");
        Button("home-modules").clicked += () => Show("catalogue-screen");

        foreach (string language in new[] { "English", "Hindi", "Santali" })
        {
            string value = language;
            Button("language-" + language.ToLowerInvariant()).clicked += () => SelectLanguage(value);
        }

        Button("fire-training").clicked += () =>
        {
            AppSession.Instance.currentTraining = "FireSafety";
            TrainingEventManager.RaiseTrainingSelected("FireSafety");
            Show("intro-screen");
        };

        TextField search = root.Q<TextField>("training-search");
        search.RegisterValueChangedCallback(evt => FilterTraining(evt.newValue));
        SelectLanguage(selectedLanguage);
    }

    private void Login()
    {
        string name = root.Q<TextField>("worker-name").value?.Trim() ?? "";
        string employeeId = root.Q<TextField>("employee-id").value?.Trim() ?? "";
        Label error = root.Q<Label>("login-error");

        if (name.Length == 0 || employeeId.Length < 3)
        {
            error.text = "Enter your name and a valid employee ID to continue.";
            return;
        }

        error.text = string.Empty;
        AppSession.Instance.CompleteLogin(employeeId);
        TrainingEventManager.RaiseLogin(employeeId);
        Show("home-screen");
    }

    private void SelectLanguage(string language)
    {
        selectedLanguage = language;
        foreach (string item in new[] { "English", "Hindi", "Santali" })
        {
            VisualElement card = root.Q("language-" + item.ToLowerInvariant());
            card.EnableInClassList("language-card--selected", item == language);
        }
        root.Q<Label>("language-selection").text = "Selected: " + language;
    }

    private void ContinueFromLanguage()
    {
        AppSession.Instance.SetLanguage(selectedLanguage);
        SimpleLocalization.SetLanguage(selectedLanguage);
        TrainingEventManager.RaiseLanguageSelected(selectedLanguage);
        Show("login-screen");
    }

    private void FilterTraining(string query)
    {
        bool showFire = string.IsNullOrWhiteSpace(query) ||
            "fire safety extinguisher emergency".Contains(query.Trim().ToLowerInvariant());
        root.Q("fire-training").style.display = showFire ? DisplayStyle.Flex : DisplayStyle.None;
        root.Q<Label>("search-empty").style.display = showFire ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void StartFireTraining()
    {
        AppSession.Instance.BeginTraining("FireSafety");
        TrainingEventManager.RaiseTrainingStarted();
        SceneManager.LoadScene("FireTraining");
    }

    private Button Button(string name)
    {
        Button button = root.Q<Button>(name);
        if (button == null)
            Debug.LogError("Menu UI is missing button: " + name);
        return button;
    }

    private void Show(string screenName)
    {
        foreach (VisualElement screen in root.Query<VisualElement>(className: "screen").ToList())
            screen.style.display = screen.name == screenName ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
