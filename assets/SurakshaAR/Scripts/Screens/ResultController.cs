using SurakshaAR.Core;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public class ResultController : IScreenController
    {
        private Button _btnViewCert, _btnHome;

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;
            var state = AppState.Instance;

            bool isPassed = state != null ? state.IsPassed : true;
            int score = state != null ? state.AssessmentScore : 95;

            bool hasCriticalError = state != null && state.CriticalErrorsCount > 0;

            var labelTitle = UIHelper.FindTMP(root, "label-result-title");
            if (labelTitle != null)
            {
                if (hasCriticalError)
                {
                    labelTitle.text = "CRITICAL SAFETY VIOLATION (FAIL)";
                    labelTitle.color = UIColors.Danger;
                }
                else
                {
                    labelTitle.text = isPassed ? "Assessment Passed!" : "Additional Training Required";
                    labelTitle.color = isPassed ? UIColors.PrimaryDark : UIColors.Warning;
                }
            }

            var labelSub = UIHelper.FindTMP(root, "label-result-sub");
            if (labelSub != null && hasCriticalError)
            {
                labelSub.text = "Automatic FAIL triggered. High scores do not override safety-critical procedural violations.";
                labelSub.color = UIColors.Danger;
            }

            var labelIcon = UIHelper.FindTMP(root, "label-badge-icon");
            if (labelIcon != null)
            {
                labelIcon.text = isPassed ? "✓" : (hasCriticalError ? "✕" : "⚠");
                labelIcon.color = isPassed ? UIColors.SafetyGreen : (hasCriticalError ? UIColors.Danger : UIColors.Warning);
            }

            var labelScore = UIHelper.FindTMP(root, "label-score");
            if (labelScore != null)
            {
                labelScore.text = $"{score}%";
                labelScore.color = isPassed ? UIColors.SafetyGreen : (hasCriticalError ? UIColors.Danger : UIColors.Warning);
            }

            var passChipBtn = UIHelper.FindButton(root, "label-pass-chip");
            if (passChipBtn != null)
            {
                var chipText = passChipBtn.GetComponentInChildren<TextMeshProUGUI>();
                var chipImg = passChipBtn.GetComponent<Image>();
                if (chipText != null)
                {
                    chipText.text = isPassed ? "COMPETENT" : (hasCriticalError ? "AUTO-FAIL" : "NEEDS RETRAINING");
                    chipText.color = isPassed ? UIColors.SafetyGreen : (hasCriticalError ? UIColors.Danger : UIColors.Warning);
                }
                if (chipImg != null)
                {
                    chipImg.color = isPassed ? UIColors.Hex("#E6F4EC") : (hasCriticalError ? UIColors.Hex("#FDECEC") : UIColors.Hex("#FEF3C7"));
                }
            }

            var timeVal = UIHelper.FindTMP(root, "label-time-val");
            if (timeVal != null && state != null)
            {
                int mins = Mathf.FloorToInt(state.LastARTimerSeconds / 60f);
                int secs = Mathf.FloorToInt(state.LastARTimerSeconds % 60f);
                timeVal.text = string.Format("{0:00}:{1:00}", mins, secs);
            }

            var critVal = UIHelper.FindTMP(root, "label-crit-val");
            if (critVal != null && state != null)
            {
                critVal.text = state.CriticalErrorsCount.ToString();
                critVal.color = state.CriticalErrorsCount == 0 ? UIColors.SafetyGreen : UIColors.Danger;
            }

            var stepsVal = UIHelper.FindTMP(root, "label-steps-val");
            if (stepsVal != null && state != null)
            {
                stepsVal.text = $"{state.CorrectActionsCount} / {state.AssessmentTotalQuestions}";
            }

            var penaltyVal = UIHelper.FindTMP(root, "label-penalty-val");
            if (penaltyVal != null && state != null)
            {
                int totalPenalties = (state.WrongActionsCount * 5) + (state.UnsafeActionsCount * 10);
                penaltyVal.text = totalPenalties > 0 ? $"-{totalPenalties}" : "0";
            }

            var compDesc = UIHelper.FindTMP(root, "label-comp-desc");
            if (compDesc != null && state != null)
            {
                compDesc.text = $"Previous Attempt: {state.PreviousAttemptScore}%  ➔  Current Performance: {score}%";
            }

            _btnViewCert = UIHelper.FindButton(root, "btn-view-certificate");
            if (_btnViewCert != null)
            {
                var tmp = _btnViewCert.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = isPassed ? "View Official Certificate" : "Start Targeted Retraining";
                }
                _btnViewCert.onClick.AddListener(OnPrimaryAction);
            }

            _btnHome = UIHelper.FindButton(root, "btn-home");
            if (_btnHome != null)
            {
                _btnHome.onClick.AddListener(OnReturnHome);
            }
        }

        private void OnPrimaryAction()
        {
            var state = AppState.Instance;
            if (state != null && !state.IsPassed)
            {
                // Launch targeted retraining
                state.IsRetrainingMode = true;
                if (state.SelectedModule != null && ARModuleLauncher.Instance != null)
                {
                    ARModuleLauncher.Instance.TryLaunchModule(state.SelectedModule);
                    return;
                }
            }

            UIManager.Instance?.ShowScreen(ScreenId.Certificate);
        }

        private void OnReturnHome()
        {
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnViewCert != null) _btnViewCert.onClick.RemoveListener(OnPrimaryAction);
            if (_btnHome != null) _btnHome.onClick.RemoveListener(OnReturnHome);
            _btnViewCert = null;
            _btnHome = null;
        }
    }
}
