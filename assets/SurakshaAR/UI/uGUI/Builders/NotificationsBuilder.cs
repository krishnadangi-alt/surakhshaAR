using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    public static class NotificationsBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("NotificationsScreen");
            var rootRT = root.AddComponent<RectTransform>();
            UIHelper.Stretch(rootRT, 0, 0, 0, 0);

            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // Header
            var header = UIHelper.MakeHorizontal("Header", root.transform, 16, childForceWidth: false, childForceHeight: false);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = new Vector2(1, 1);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0, 120);
            header.anchoredPosition = new Vector2(0, -60);
            var hlg = header.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(36, 36, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            
            var backBtn = UIHelper.MakeButton("btn-back", header, "", 14, Color.white, Color.white, 30);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 60, minWidth: 60, preferredHeight: 60, minHeight: 60);
            var backIcon = UIHelper.MakeRect("Icon", backBtn.transform);
            backIcon.anchorMin = new Vector2(0.5f, 0.5f);
            backIcon.anchorMax = new Vector2(0.5f, 0.5f);
            backIcon.pivot = new Vector2(0.5f, 0.5f);
            backIcon.sizeDelta = new Vector2(24, 24);
            var img = backIcon.gameObject.AddComponent<Image>();
            img.color = UIColors.Hex("#0F172A");
            img.sprite = UIHelper.GetCircleSprite();

            var titleLbl = UIHelper.MakeLabel("Title", header, "Notifications", 42, UIColors.Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 60, minHeight: 60);

            // Empty state
            var empty = UIHelper.MakeVertical("EmptyState", root.transform, 16, childForceWidth: false, childForceHeight: false);
            empty.anchorMin = new Vector2(0.5f, 0.5f);
            empty.anchorMax = new Vector2(0.5f, 0.5f);
            empty.pivot = new Vector2(0.5f, 0.5f);
            empty.sizeDelta = new Vector2(800, 400);
            empty.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var bellBox = UIHelper.MakeRect("BellBox", empty);
            UIHelper.SetLayout(bellBox.gameObject, preferredWidth: 120, minWidth: 120, preferredHeight: 120, minHeight: 120);
            var bellBg = bellBox.gameObject.AddComponent<Image>();
            bellBg.color = UIColors.Hex("#F1F5F9");
            UIHelper.SetImageRoundedSprite(bellBg, 60);
            
            var bell = UIHelper.MakeRect("Bell", bellBox);
            bell.anchorMin = new Vector2(0.5f, 0.5f);
            bell.anchorMax = new Vector2(0.5f, 0.5f);
            bell.pivot = new Vector2(0.5f, 0.5f);
            bell.sizeDelta = new Vector2(48, 48);
            var bellImg = bell.gameObject.AddComponent<Image>();
            bellImg.color = UIColors.Hex("#94A3B8");
            bellImg.sprite = UIHelper.GetBellSprite();

            var eTitle = UIHelper.MakeLabel("ETitle", empty, "No new notifications", 32, UIColors.Hex("#0F172A"), TextAlignmentOptions.Center, bold: true);
            var eSub = UIHelper.MakeLabel("ESub", empty, "You're all caught up!", 24, UIColors.Hex("#64748B"), TextAlignmentOptions.Center);

            return root;
        }
    }
}
