using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// Notifications screen controller.
    /// Shows empty state with full localization.
    /// Does not generate fake notifications — displays honest zero-data state when empty.
    /// </summary>
    public class NotificationsController : IScreenController
    {
        private Button _btnBack;

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            ApplyLanguageFonts(root, currentLang);

            // ── Back button ────────────────────────────────────────────────
            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard));
            }

            // ── Screen title ───────────────────────────────────────────────
            var titleLbl = UIHelper.FindTMP(root, "label-title");
            if (titleLbl != null)
                titleLbl.text = loc?.Get("notif.title") ?? "Notifications";

            // ── Empty state ────────────────────────────────────────────────
            var emptyHeading = UIHelper.FindTMP(root, "label-empty-heading");
            if (emptyHeading != null)
                emptyHeading.text = loc?.Get("notif.noNew") ?? "No new notifications";

            var emptySubLbl = UIHelper.FindTMP(root, "label-empty-sub");
            if (emptySubLbl != null)
                emptySubLbl.text = loc?.Get("notif.caughtUp") ?? "You're all caught up!";

            // ── Zero-data state honest display ─────────────────────────────
            var emptyStateGO = UIHelper.FindRect(root, "notifications-empty")?.gameObject;
            if (emptyStateGO != null) emptyStateGO.SetActive(true);

            var listGO = UIHelper.FindRect(root, "notifications-list")?.gameObject;
            if (listGO != null) listGO.SetActive(false);
        }

        private static void ApplyLanguageFonts(GameObject root, AppLanguage lang)
        {
            var font = UIHelper.GetDefaultFont();
            if (font == null) return;
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.font = font;
        }

        public void OnHide()
        {
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack = null;
            }
        }
    }
}
