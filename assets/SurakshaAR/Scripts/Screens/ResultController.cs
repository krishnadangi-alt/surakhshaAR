using SurakshaAR.Core;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// ResultController
    /// ================
    /// Controls the Assessment & Scenario Result Evaluation screen.
    /// Binds real competency performance metrics from AppState,
    /// formats Pass/Retraining states, and provides navigation to
    /// Certificate verification or Dashboard.
    /// </summary>
    public class ResultController : IScreenController
    {
        private GameObject _root;
        private Button _btnCertificate;
        private Button _btnHome;

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            if (_root == null) return;

            var state = AppState.Instance;
            bool passed = state != null ? state.IsPassed : false;
            int score = state != null ? state.AssessmentScore : 0;
            int criticalErrors = state != null ? state.CriticalErrorsCount : 0;

            // 1. Result Title & Subtitle
            var titleLbl = UIHelper.FindTMP(root, "label-result-title");
            if (titleLbl != null)
            {
                titleLbl.text = passed ? "Assessment Passed!" : "Retraining Recommended";
                titleLbl.color = passed ? UIColors.PrimaryDark : UIColors.Hex("#DC2626");
            }

            var subLbl = UIHelper.FindTMP(root, "label-result-sub");
            if (subLbl != null)
            {
                subLbl.text = passed
                    ? "Demonstrated compliance with Ministry of Mines Industrial Safety SOP."
                    : (criticalErrors > 0
                        ? $"Critical safety violation recorded ({criticalErrors}). Mandatory re-practice required."
                        : "Score below 75% threshold. Targeted drill practice recommended.");
            }

            // 2. Score & Pass Chip
            var scoreLbl = UIHelper.FindTMP(root, "label-score");
            if (scoreLbl != null)
            {
                scoreLbl.text = $"{score}%";
                scoreLbl.color = passed ? UIColors.SafetyGreen : UIColors.Hex("#DC2626");
            }

            var passChip = UIHelper.FindTMP(root, "label-pass-chip");
            if (passChip != null)
            {
                passChip.text = passed ? "COMPETENT" : "RETRAINING REQUIRED";
            }

            // 3. Navigation Buttons
            _btnCertificate = UIHelper.FindButton(root, "btn-view-certificate");
            if (_btnCertificate != null)
            {
                var btnTxt = _btnCertificate.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null)
                {
                    btnTxt.text = passed ? "View Official Certificate" : "Start Targeted Retraining";
                }

                _btnCertificate.onClick.RemoveAllListeners();
                _btnCertificate.onClick.AddListener(() =>
                {
                    if (passed)
                    {
                        UIManager.Instance?.ShowScreen(ScreenId.Certificate);
                    }
                    else
                    {
                        if (AppState.Instance != null)
                            AppState.Instance.IsRetrainingMode = true;
                        UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail);
                    }
                });
            }

            _btnHome = UIHelper.FindButton(root, "btn-home");
            if (_btnHome != null)
            {
                _btnHome.onClick.RemoveAllListeners();
                _btnHome.onClick.AddListener(() =>
                {
                    UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
                });
            }
        }

        public void OnHide()
        {
            if (_btnCertificate != null) _btnCertificate.onClick.RemoveAllListeners();
            if (_btnHome != null) _btnHome.onClick.RemoveAllListeners();
            _root = null;
        }
    }
}
