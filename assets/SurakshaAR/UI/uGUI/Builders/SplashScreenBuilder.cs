using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Builds the Splash screen hierarchy matching Reference 3 (Left image).
    /// Full-bleed daytime industrial mining sunrise artwork with
    /// Ashoka emblem, 4 feature badges, and pill-shaped loading bar.
    /// </summary>
    public static class SplashScreenBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("SplashScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // 1. Full-bleed background artwork matching Reference 3 Left
            var bg = root.AddComponent<Image>();
            var splashSpr = UIHelper.LoadProjectSprite("splashbackgroundimage.png")
                         ?? UIHelper.LoadProjectSprite("splashbackgroundimage");
            if (splashSpr != null)
            {
                bg.sprite = splashSpr;
                bg.color = Color.white;
                bg.preserveAspect = false;
            }
            else
            {
                bg.color = UIColors.Hex("#F8FAFC");
                bg.sprite = UIHelper.GetWhiteSprite();
            }

            // 2. Training description subtitle located right under the 4 badges
            var subGO = UIHelper.MakeRect("label-tagline", root.transform);
            subGO.anchorMin = new Vector2(0.5f, 0.47f);
            subGO.anchorMax = new Vector2(0.5f, 0.47f);
            subGO.pivot = new Vector2(0.5f, 0.5f);
            subGO.sizeDelta = new Vector2(700f, 70f);

            var subTMP = UIHelper.AddTMP(subGO.gameObject);
            subTMP.text = "AR-Based Safety Training\nfor Industrial Workers";
            subTMP.fontSize = 28f;
            subTMP.fontStyle = FontStyles.Normal;
            subTMP.color = UIColors.Hex("#475569");
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.lineSpacing = 10f;
            subTMP.raycastTarget = false;

            // 3. Frosted Pill Loading Bar (matching Reference 3 Left)
            var barArea = UIHelper.MakeRect("ProgressBarArea", root.transform);
            barArea.anchorMin = new Vector2(0.5f, 0.165f);
            barArea.anchorMax = new Vector2(0.5f, 0.165f);
            barArea.pivot = new Vector2(0.5f, 0.5f);
            barArea.sizeDelta = new Vector2(560f, 44f);

            var barBg = barArea.gameObject.AddComponent<Image>();
            barBg.color = new Color(1f, 1f, 1f, 0.55f);
            barBg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barBg, 22);

            var barBorder = barArea.gameObject.AddComponent<Outline>();
            barBorder.effectColor = new Color(1f, 1f, 1f, 0.85f);
            barBorder.effectDistance = new Vector2(1.5f, -1.5f);

            var barFillRT = UIHelper.MakeRect("progress-fill", barArea);
            barFillRT.anchorMin = new Vector2(0.02f, 0.14f);
            barFillRT.anchorMax = new Vector2(0.35f, 0.86f); // default initial progress for static preview
            barFillRT.offsetMin = Vector2.zero;
            barFillRT.offsetMax = Vector2.zero;

            var barFillImg = barFillRT.gameObject.AddComponent<Image>();
            barFillImg.color = UIColors.Hex("#0B1B32"); // Dark Navy
            barFillImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barFillImg, 16);

            return root;
        }
    }
}
