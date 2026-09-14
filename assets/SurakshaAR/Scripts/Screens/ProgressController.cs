using SurakshaAR.Core;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public sealed class ProgressController : IScreenController
    {
        private Button _btnBack;

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;
            var state = AppState.Instance;

            int completed = state != null ? state.CompletedModulesCount : 3;
            int total = state != null ? state.TotalModulesCount : 5;
            int pct = total > 0 ? Mathf.RoundToInt((float)completed / total * 100f) : 60;

            var labelTitle = UIHelper.FindTMP(root, "label-title");
            if (labelTitle != null && loc != null) labelTitle.text = loc.Get("progress.title");

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null) _btnBack.onClick.AddListener(GoHome);

            var barFill = UIHelper.FindRect(root, "progress-bar-fill");
            if (barFill != null)
            {
                barFill.anchorMax = new Vector2(Mathf.Clamp01((float)pct / 100f), 1f);
            }

            var labelModulesSummary = UIHelper.FindTMP(root, "label-modules-summary");
            if (labelModulesSummary != null)
            {
                labelModulesSummary.text = $"{completed} of {total} Modules Completed";
            }

            var labelPercentage = UIHelper.FindTMP(root, "label-percentage");
            if (labelPercentage != null) labelPercentage.text = $"{pct}%";
        }

        private void GoHome()
        {
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveListener(GoHome);
            _btnBack = null;
        }
    }
}