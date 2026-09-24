using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// ResultController — shows Training Result with passed/failed state.
    /// All visible strings come from LocalizationManager. No hardcoded English.
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

            var loc   = AppManager.Instance?.Localization;
            var state = AppState.Instance;
            bool passed         = state != null ? state.IsPassed : false;
            int  score          = state != null ? state.AssessmentScore : 0;
            int  criticalErrors = state != null ? state.CriticalErrorsCount : 0;

            // ── 1. Result Title & Subtitle ─────────────────────────────────
            var titleLbl = UIHelper.FindTMP(root, "label-result-title");
            if (titleLbl != null)
            {
                titleLbl.text  = passed
                    ? (loc?.Get("result.passed")  ?? "Assessment Passed!")
                    : (loc?.Get("result.retrain") ?? "Retraining Recommended");
                titleLbl.color = passed ? UIColors.PrimaryDark : UIColors.Hex("#DC2626");
            }

            var subLbl = UIHelper.FindTMP(root, "label-result-sub");
            if (subLbl != null)
            {
                if (passed)
                {
                    subLbl.text = loc?.Get("result.sopCompliance")
                        ?? "Demonstrated compliance with Ministry of Mines Industrial Safety SOP.";
                }
                else if (criticalErrors > 0)
                {
                    string template = loc?.Get("result.criticalViolation")
                        ?? "Critical safety violation recorded ({0}). Mandatory re-practice required.";
                    subLbl.text = string.Format(template, criticalErrors);
                }
                else
                {
                    subLbl.text = loc?.Get("result.scoreBelowThreshold")
                        ?? "Score below 80% threshold. Targeted drill practice recommended.";
                }
            }

            // ── 2. Score & Pass Chip ───────────────────────────────────────
            var scoreLbl = UIHelper.FindTMP(root, "label-score");
            if (scoreLbl != null)
            {
                scoreLbl.text  = $"{score}%";
                scoreLbl.color = passed ? UIColors.SafetyGreen : UIColors.Hex("#DC2626");
            }

            var passChip = UIHelper.FindTMP(root, "label-pass-chip");
            if (passChip != null)
            {
                passChip.text = passed
                    ? (loc?.Get("result.competent")          ?? "COMPETENT")
                    : (loc?.Get("result.retrainingRequired") ?? "RETRAINING REQUIRED");
            }

            // ── 3. Navigation Buttons ──────────────────────────────────────
            _btnCertificate = UIHelper.FindButton(root, "btn-view-certificate");
            if (_btnCertificate != null)
            {
                var btnTxt = _btnCertificate.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null)
                {
                    btnTxt.text = passed
                        ? (loc?.Get("result.viewOfficialCertificate") ?? "View Official Certificate")
                        : (loc?.Get("result.startRetraining")         ?? "Start Targeted Retraining");
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
                var homeTxt = _btnHome.GetComponentInChildren<TextMeshProUGUI>();
                if (homeTxt != null)
                    homeTxt.text = loc?.Get("result.returnHome") ?? "Return to Dashboard";

                _btnHome.onClick.RemoveAllListeners();
                _btnHome.onClick.AddListener(() =>
                {
                    UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
                });
            }

            // Localize additional result text elements
            if (loc != null)
            {
                var compTitles = root.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var tmp in compTitles)
                {
                    if (tmp.gameObject.name == "CompTitle")
                    {
                        if (tmp.text.Contains("Improvement") || tmp.text.Contains("सुधार") || tmp.text.Contains("ᱥᱩᱫᱷᱟᱹᱨ"))
                            tmp.text = loc.Get("result.continuousImprovement");
                        else
                            tmp.text = loc.Get("result.competencyBreakdown");
                    }
                    else if (tmp.gameObject.name == "Sub" && (tmp.text.Contains("critical errors") || tmp.text.Contains("त्रुटियां") || tmp.text.Contains("ᱵᱷᱩᱞ")))
                    {
                        tmp.text = loc.Get("result.zeroCriticalErrors");
                    }
                }
            }

            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        public void OnHide()
        {
            if (_btnCertificate != null) _btnCertificate.onClick.RemoveAllListeners();
            if (_btnHome != null) _btnHome.onClick.RemoveAllListeners();
            _root = null;
        }
    }
}
