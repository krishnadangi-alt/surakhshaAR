using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HoseAdjustUI : MonoBehaviour
{
    [Header("Extinguisher")]
    public ExtinguisherPickup extinguisher;

    [Header("Rotation Sliders")]
    public Slider rotX;
    public Slider rotY;
    public Slider rotZ;

    [Header("Rotation Text")]
    public TMP_Text rotXText;
    public TMP_Text rotYText;
    public TMP_Text rotZText;

    [Header("Panel Layout")]
    public Vector2 panelSize = new Vector2(500f, 250f);

    [Header("Slider Layout")]
    public float sliderWidth = 300f;
    public float sliderHeight = 40f;

    [Header("Text Layout")]
    public float textWidth = 100f;
    public float textHeight = 40f;

    [Header("Screen Position")]
    public float leftMargin = 30f;
    public float topMargin = 40f;

    [Header("Row Spacing")]
    public float rowSpacing = 60f;

    private RectTransform panel;

    void Start()
    {
        panel = GetComponent<RectTransform>();

        if (panel == null)
        {
            Debug.LogError("HoseAdjustUI must be on hose adjust panel.");
            return;
        }

        if (extinguisher == null)
        {
            Debug.LogError("Extinguisher is not assigned.");
            return;
        }

        SetupPanel();
        SetupSliders();
        SetupTexts();
        ConnectSliders();

        // Do NOT force the hose here.
        // The pickup script controls the hose.
        UpdateTexts();
    }

    // =====================================================
    // PANEL
    // =====================================================

    void SetupPanel()
    {
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);

        panel.pivot = new Vector2(0f, 1f);

        panel.sizeDelta = panelSize;

        panel.anchoredPosition =
            new Vector2(leftMargin, -topMargin);
    }

    // =====================================================
    // SLIDERS
    // =====================================================

    void SetupSliders()
    {
        SetupSlider(rotX, 0);
        SetupSlider(rotY, 1);
        SetupSlider(rotZ, 2);
    }

    void SetupSlider(Slider slider, int row)
    {
        if (slider == null)
            return;

        RectTransform rect =
            slider.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);

        rect.pivot = new Vector2(0f, 1f);

        rect.sizeDelta =
            new Vector2(sliderWidth, sliderHeight);

        float y =
            -20f - (row * rowSpacing);

        rect.anchoredPosition =
            new Vector2(120f, y);

        slider.minValue = -180f;
        slider.maxValue = 180f;
        slider.wholeNumbers = false;
    }

    // =====================================================
    // TEXT
    // =====================================================

    void SetupTexts()
    {
        SetupText(rotXText, 0);
        SetupText(rotYText, 1);
        SetupText(rotZText, 2);
    }

    void SetupText(TMP_Text text, int row)
    {
        if (text == null)
            return;

        RectTransform rect =
            text.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);

        rect.pivot = new Vector2(0f, 1f);

        rect.sizeDelta =
            new Vector2(textWidth, textHeight);

        float y =
            -20f - (row * rowSpacing);

        rect.anchoredPosition =
            new Vector2(10f, y);

        text.alignment =
            TextAlignmentOptions.Center;

        text.fontSize = 24f;
    }

    // =====================================================
    // CONNECT SLIDERS
    // =====================================================

    void ConnectSliders()
    {
        rotX.onValueChanged.AddListener(OnSliderChanged);
        rotY.onValueChanged.AddListener(OnSliderChanged);
        rotZ.onValueChanged.AddListener(OnSliderChanged);
    }

    // =====================================================
    // SLIDER CHANGE
    // =====================================================

    void OnSliderChanged(float value)
    {
        // Do nothing before pickup
        if (!extinguisher.IsHeld())
            return;

        Vector3 rotation =
            new Vector3(
                rotX.value,
                rotY.value,
                rotZ.value
            );

        // Only send rotation.
        // Existing pickup script handles
        // hose/pivot/spray-point positioning.
        extinguisher.SetHoseRotation(rotation);

        UpdateTexts();
    }

    // =====================================================
    // TEXT VALUES
    // =====================================================

    void UpdateTexts()
    {
        if (rotXText != null)
        {
            rotXText.text =
                "X: " +
                Mathf.RoundToInt(rotX.value) +
                "°";
        }

        if (rotYText != null)
        {
            rotYText.text =
                "Y: " +
                Mathf.RoundToInt(rotY.value) +
                "°";
        }

        if (rotZText != null)
        {
            rotZText.text =
                "Z: " +
                Mathf.RoundToInt(rotZ.value) +
                "°";
        }
    }

    // =====================================================
    // SYNC UI WITH INSPECTOR VALUE
    // =====================================================

    public void SyncWithExtinguisher()
    {
        if (extinguisher == null)
            return;

        Vector3 rotation =
            extinguisher.CurrentHoseRotation;

        rotX.SetValueWithoutNotify(rotation.x);
        rotY.SetValueWithoutNotify(rotation.y);
        rotZ.SetValueWithoutNotify(rotation.z);

        UpdateTexts();
    }
}