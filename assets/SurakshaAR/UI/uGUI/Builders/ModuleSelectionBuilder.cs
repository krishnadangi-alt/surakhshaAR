using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Module Selection screen builder:
    /// Navy header with back button and "Safety Training Modules" title.
    /// Scrollable list container ("module-list") populated with module cards.
    /// </summary>
    public static class ModuleSelectionBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("ModuleSelectionScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.Background;
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Top Header ───────────────────────────────────────────────
            var header = UIHelper.MakeRect("Header", root.transform);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = Vector2.one;
            header.offsetMin = new Vector2(0, -140);
            header.offsetMax = Vector2.zero;

            var headerImg = header.gameObject.AddComponent<Image>();
            headerImg.color = UIColors.Primary;
            headerImg.sprite = UIHelper.GetWhiteSprite();

            var headerRow = UIHelper.MakeHorizontal("HeaderRow", header, 16, new RectOffset(24, 24, 0, 0));
            headerRow.anchorMin = new Vector2(0, 0);
            headerRow.anchorMax = Vector2.one;
            headerRow.offsetMin = Vector2.zero;
            headerRow.offsetMax = Vector2.zero;

            var backBtn = UIHelper.MakeButton("btn-back", headerRow, "‹", 38, new Color(1, 1, 1, 0.16f), Color.white, 20);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 56, preferredHeight: 56);

            var titleLbl = UIHelper.MakeLabel("label-title", headerRow, "Training Modules", 28, Color.white, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            // ── Scrollable Module List ───────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = Vector2.zero;
            scrollRoot.offsetMax = new Vector2(0, -140);

            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 25f;

            var viewport = UIHelper.MakeRect("Viewport", scrollRoot);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport;

            var content = UIHelper.MakeRect("module-list", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 1200);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 18;
            vlg.padding = new RectOffset(28, 28, 24, 40);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return root;
        }
    }
}
