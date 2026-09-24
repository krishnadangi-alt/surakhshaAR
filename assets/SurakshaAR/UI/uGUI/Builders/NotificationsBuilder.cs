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

            // Header (Clean mobile header with safe-area support)
            var header = UIHelper.MakeHorizontal("Header", root.transform, 20, childForceWidth: false, childForceHeight: false);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = new Vector2(1, 1);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0, 180);
            header.anchoredPosition = Vector2.zero;
            var hlg = header.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(36, 36, 60, 20);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            
            var backBtn = UIHelper.MakeButton("btn-back", header, "‹", 48, Color.white, UIColors.Hex("#0F172A"), 24);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 76, minWidth: 76, preferredHeight: 76, minHeight: 76);
            var backBorder = backBtn.gameObject.AddComponent<Outline>();
            backBorder.effectColor = UIColors.Hex("#E2E8F0");
            backBorder.effectDistance = new Vector2(1.2f, -1.2f);

            var titleLbl = UIHelper.MakeLabel("label-title", header, "Notifications", 52, UIColors.Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 76, minHeight: 76);

            // Empty state
            var empty = UIHelper.MakeVertical("notifications-empty", root.transform, 16, childForceWidth: false, childForceHeight: false);
            empty.anchorMin = new Vector2(0.5f, 0.5f);
            empty.anchorMax = new Vector2(0.5f, 0.5f);
            empty.pivot = new Vector2(0.5f, 0.5f);
            empty.sizeDelta = new Vector2(840, 440);
            empty.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var bellBox = UIHelper.MakeRect("BellBox", empty);
            UIHelper.SetLayout(bellBox.gameObject, preferredWidth: 140, minWidth: 140, preferredHeight: 140, minHeight: 140);
            var bellBg = bellBox.gameObject.AddComponent<Image>();
            bellBg.color = UIColors.Hex("#F1F5F9");
            UIHelper.SetImageRoundedSprite(bellBg, 70);
            
            var bell = UIHelper.MakeRect("Bell", bellBox);
            bell.anchorMin = new Vector2(0.5f, 0.5f);
            bell.anchorMax = new Vector2(0.5f, 0.5f);
            bell.pivot = new Vector2(0.5f, 0.5f);
            bell.sizeDelta = new Vector2(56, 56);
            var bellImg = bell.gameObject.AddComponent<Image>();
            bellImg.color = UIColors.Hex("#94A3B8");
            bellImg.sprite = UIHelper.GetBellSprite();

            var eTitle = UIHelper.MakeLabel("label-empty-heading", empty, "No new notifications", 44, UIColors.Hex("#0F172A"), TextAlignmentOptions.Center, bold: true);
            var eSub = UIHelper.MakeLabel("label-empty-sub", empty, "You're all caught up!", 32, UIColors.Hex("#64748B"), TextAlignmentOptions.Center);

            return root;
        }
    }
}
