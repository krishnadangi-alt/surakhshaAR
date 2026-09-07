using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI Toolkit AR HUD. It deliberately exposes the same small public surface
/// used by FireScenarioFlowManager, keeping every AR interaction untouched.
/// </summary>
public class FireScenarioUIController : MonoBehaviour
{
    private const string LayoutPath = "FireSafetyUI/ARTrainingHud";
    private const string ThemePath = "FireSafetyUI/FireSafetyTheme";

    private VisualElement root;
    private VisualElement card;
    private VisualElement hint;
    private VisualElement progress;
    private VisualElement progressFill;
    private VisualElement step;
    private VisualElement action;
    private VisualElement completion;
    private Label cardTitle;
    private Label cardBody;
    private Label cardFooter;
    private Label hintText;
    private Label progressText;
    private Label stepText;
    private Label actionText;
    private Label completionText;
    private System.Action retryAction;
    private System.Action homeAction;

    public bool HasCard { get; private set; }
    public bool HasCompletionPanel { get; set; }

    private void Awake()
    {
        VisualTreeAsset layout = Resources.Load<VisualTreeAsset>(LayoutPath);
        StyleSheet theme = Resources.Load<StyleSheet>(ThemePath);
        if (layout == null || theme == null)
        {
            Debug.LogError("AR HUD UI Toolkit assets are missing from Resources/FireSafetyUI.");
            return;
        }

        UIDocument document = gameObject.AddComponent<UIDocument>();
        document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        document.visualTreeAsset = layout;
        root = document.rootVisualElement;
        root.styleSheets.Add(theme);

        card = root.Q("ar-card"); hint = root.Q("ar-hint"); progress = root.Q("ar-progress");
        progressFill = root.Q("ar-progress-fill"); step = root.Q("ar-step"); action = root.Q("ar-action"); completion = root.Q("ar-completion");
        cardTitle = root.Q<Label>("ar-card-title"); cardBody = root.Q<Label>("ar-card-body"); cardFooter = root.Q<Label>("ar-card-footer");
        hintText = root.Q<Label>("ar-hint-text"); progressText = root.Q<Label>("ar-progress-text"); stepText = root.Q<Label>("ar-step-text");
        actionText = root.Q<Label>("ar-action-text"); completionText = root.Q<Label>("ar-completion-text");
        root.Q<Button>("ar-action-button").clicked += () => actionAction?.Invoke();
        root.Q<Button>("ar-retry").clicked += () => retryAction?.Invoke();
        root.Q<Button>("ar-home").clicked += () => homeAction?.Invoke();
        HideCard(); HideHint(); HideProgress(); HideStep(); HideActionButton(); HideCompletion();
    }

    private System.Action actionAction;

    public void ShowCard(string title, string body, string footer)
    {
        if (root == null) return;
        cardTitle.text = title; cardBody.text = body; cardFooter.text = footer;
        card.style.display = DisplayStyle.Flex; HasCard = true;
    }

    public void HideCard() { if (card != null) card.style.display = DisplayStyle.None; HasCard = false; }
    public void ShowHint(string message) { if (hint == null) return; hintText.text = message; hint.style.display = DisplayStyle.Flex; }
    public void HideHint() { if (hint != null) hint.style.display = DisplayStyle.None; }
    public void ShowStep(int current, int total) { if (step == null) return; stepText.text = "STEP " + current + " / " + total; step.style.display = DisplayStyle.Flex; }
    public void HideStep() { if (step != null) step.style.display = DisplayStyle.None; }

    public void ShowProgress(float value, string label)
    {
        if (progress == null) return;
        progressFill.style.width = Length.Percent(Mathf.Clamp01(value) * 100f);
        progressText.text = label; progress.style.display = DisplayStyle.Flex;
    }

    public void HideProgress() { if (progress != null) progress.style.display = DisplayStyle.None; }

    public void ShowActionButton(string label, UnityEngine.Events.UnityAction onClick)
    {
        if (action == null) return;
        actionText.text = label;
        actionAction = onClick == null ? null : () => onClick.Invoke();
        action.style.display = DisplayStyle.Flex;
    }

    public void HideActionButton() { if (action != null) action.style.display = DisplayStyle.None; actionAction = null; }

    public void ShowCompletion(string body, UnityEngine.Events.UnityAction retry, UnityEngine.Events.UnityAction home)
    {
        if (completion == null) return;
        completionText.text = body;
        retryAction = retry == null ? null : () => retry.Invoke();
        homeAction = home == null ? null : () => home.Invoke();
        completion.style.display = DisplayStyle.Flex; HasCompletionPanel = true;
    }

    public void HideCompletion() { if (completion != null) completion.style.display = DisplayStyle.None; HasCompletionPanel = false; }
}
