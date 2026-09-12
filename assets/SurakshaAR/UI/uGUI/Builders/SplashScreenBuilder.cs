using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Builds the Splash screen uGUI hierarchy at runtime.
    /// Navy background, centered logo + app name, loading progress bar.
    /// </summary>
    public static class SplashScreenBuilder
    {
        public static GameObject Build()
        {
            // Root
            var root = new GameObject("SplashScreen");
            var rootRT = root.AddComponent<RectTransform>();

            // Background
            var bg = root.AddComponent<Image>();
            var splashSpr = UIHelper.LoadProjectSprite("splashbackgroundimage");
            if (splashSpr != null)
            {
                bg.sprite = splashSpr;
                bg.color = Color.white;
            }
            else
            {
                bg.color = UIColors.PrimaryDark;
                bg.sprite = UIHelper.GetWhiteSprite();
            }

            // Dark overlay for readability
            var overlay = UIHelper.MakeStretchRect("BgOverlay", root.transform);
            var overImg = overlay.gameObject.AddComponent<Image>();
            overImg.color = new Color(0.04f, 0.10f, 0.16f, 0.75f);

            // Center content column
            var center = UIHelper.MakeRect("CenterContent", root.transform);
            center.anchorMin = new Vector2(0.08f, 0.22f);
            center.anchorMax = new Vector2(0.92f, 0.88f);
            center.offsetMin = Vector2.zero;
            center.offsetMax = Vector2.zero;

            var centerVLG = center.gameObject.AddComponent<VerticalLayoutGroup>();
            centerVLG.childAlignment = TextAnchor.MiddleCenter;
            centerVLG.spacing = 14;
            centerVLG.childForceExpandWidth = true;
            centerVLG.childForceExpandHeight = false;
            centerVLG.childControlHeight = false;

            // Government Header
            var govtLbl = UIHelper.MakeLabel("GovtHeader", center, "GOVERNMENT OF INDIA\nMINISTRY OF MINES", 18, new Color(1f, 1f, 1f, 0.85f), TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(govtLbl.gameObject, preferredHeight: 46);

            // Shield / emblem circle
            var emblemRT = UIHelper.MakeRect("EmblemCircle", center);
            emblemRT.sizeDelta = new Vector2(104, 104);
            var emblemLE = emblemRT.gameObject.AddComponent<LayoutElement>();
            emblemLE.preferredWidth = 104;
            emblemLE.preferredHeight = 104;

            var emblemImg = emblemRT.gameObject.AddComponent<Image>();
            emblemImg.sprite = UIHelper.GetCircleSprite();
            emblemImg.color = new Color(1f, 1f, 1f, 0.15f);

            var emblemLabelRT = UIHelper.MakeStretchRect("Icon", emblemRT);
            var emblemTMP = UIHelper.AddTMP(emblemLabelRT.gameObject);
            emblemTMP.text = "AR";
            emblemTMP.fontSize = 44;
            emblemTMP.fontStyle = FontStyles.Bold;
            emblemTMP.alignment = TextAlignmentOptions.Center;
            emblemTMP.color = Color.white;
            emblemTMP.raycastTarget = false;

            // App Name
            var appNameGO = new GameObject("label-app-name");
            appNameGO.transform.SetParent(center, false);
            var appNameLE = appNameGO.AddComponent<LayoutElement>();
            appNameLE.preferredHeight = 64;
            var appNameTMP = UIHelper.AddTMP(appNameGO);
            appNameTMP.text = "SURAKSHAAR";
            appNameTMP.fontSize = 50;
            appNameTMP.fontStyle = FontStyles.Bold;
            appNameTMP.color = Color.white;
            appNameTMP.alignment = TextAlignmentOptions.Center;
            appNameTMP.raycastTarget = false;

            // Tagline
            var taglineGO = new GameObject("label-tagline");
            taglineGO.transform.SetParent(center, false);
            var taglineLE = taglineGO.AddComponent<LayoutElement>();
            taglineLE.preferredHeight = 40;
            var taglineTMP = UIHelper.AddTMP(taglineGO);
            taglineTMP.text = "AR-Based Safety Training for Industrial Workers";
            taglineTMP.fontSize = 20;
            taglineTMP.color = new Color(1f, 1f, 1f, 0.80f);
            taglineTMP.alignment = TextAlignmentOptions.Center;
            taglineTMP.raycastTarget = false;

            // Progress bar at bottom
            var barArea = UIHelper.MakeRect("ProgressBarArea", root.transform);
            barArea.anchorMin = new Vector2(0.12f, 0.12f);
            barArea.anchorMax = new Vector2(0.88f, 0.15f);
            barArea.offsetMin = Vector2.zero;
            barArea.offsetMax = Vector2.zero;

            var barBg = barArea.gameObject.AddComponent<Image>();
            barBg.color = new Color(1f, 1f, 1f, 0.18f);
            barBg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barBg, 6);

            var barFillRT = UIHelper.MakeRect("progress-fill", barArea);
            barFillRT.anchorMin = Vector2.zero;
            barFillRT.anchorMax = new Vector2(0f, 1f); // starts at 0 width
            barFillRT.offsetMin = Vector2.zero;
            barFillRT.offsetMax = Vector2.zero;

            var barFillImg = barFillRT.gameObject.AddComponent<Image>();
            barFillImg.color = UIColors.SafetyGreen;
            barFillImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barFillImg, 6);

            // Motto at very bottom
            var footerRT = UIHelper.MakeRect("footer", root.transform);
            footerRT.anchorMin = new Vector2(0f, 0.04f);
            footerRT.anchorMax = new Vector2(1f, 0.09f);
            footerRT.offsetMin = Vector2.zero;
            footerRT.offsetMax = Vector2.zero;
            var footerTMP = UIHelper.AddTMP(footerRT.gameObject);
            footerTMP.text = "SAFE WORKERS, STRONGER INDIA";
            footerTMP.fontSize = 18;
            footerTMP.fontStyle = FontStyles.Bold;
            footerTMP.color = new Color(1f, 1f, 1f, 0.70f);
            footerTMP.alignment = TextAlignmentOptions.Center;
            footerTMP.raycastTarget = false;

            return root;
        }
    }
}
