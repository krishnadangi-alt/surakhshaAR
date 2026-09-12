using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public sealed class CertificateController : IScreenController
    {
        private Button _btnTopbarBack, _btnBackHome, _btnDownload, _btnShare;

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;
            var state = AppState.Instance;

            string workerName = state != null ? state.WorkerName : "Ramesh Kumar";
            string workerId = state != null ? state.EmployeeId : "M10234";
            bool isGuest = state != null && state.IsGuestMode;

            var labelWorkerName = UIHelper.FindTMP(root, "label-worker-name");
            if (labelWorkerName != null) labelWorkerName.text = workerName;

            var labelWorkerId = UIHelper.FindTMP(root, "label-worker-id");
            if (labelWorkerId != null)
            {
                labelWorkerId.text = isGuest
                    ? $"Guest Worker | ID: {workerId}"
                    : $"Mine Worker | ID: {workerId}";
            }

            var module = state != null ? state.SelectedModule : null;
            module = module ?? AppManager.Instance?.GetModule(ModuleId.FireAndExplosion);

            var labelModuleTitle = UIHelper.FindTMP(root, "label-module-title");
            if (labelModuleTitle != null)
            {
                if (module != null)
                {
                    string title = loc != null ? loc.Get(module.titleKey) : module.titleKey;
                    labelModuleTitle.text = "🔥 " + title;
                }
                else
                {
                    labelModuleTitle.text = "🔥 Fire & Explosion Response";
                }
            }

            var scoreGrade = UIHelper.FindTMP(root, "label-score-grade");
            if (scoreGrade != null && state != null)
            {
                scoreGrade.text = $"Competency Grade: Level 1 ({state.AssessmentScore}% Score • {state.CriticalErrorsCount} Critical Errors)";
            }

            var certId = UIHelper.FindTMP(root, "label-cert-id");
            if (certId != null && state != null)
            {
                certId.text = $"Cert ID: {state.CertificateId}";
            }

            var certDate = UIHelper.FindTMP(root, "label-cert-date");
            if (certDate != null && state != null)
            {
                certDate.text = $"Date Issued: {state.CertificationDate}";
            }

            var labelTitle = UIHelper.FindTMP(root, "label-title");
            if (labelTitle != null && loc != null) labelTitle.text = loc.Get("certificate.title");

            _btnTopbarBack = UIHelper.FindButton(root, "btn-back");
            if (_btnTopbarBack != null) _btnTopbarBack.onClick.AddListener(GoBack);

            _btnBackHome = UIHelper.FindButton(root, "btn-back-home");
            if (_btnBackHome != null) _btnBackHome.onClick.AddListener(GoHome);

            _btnDownload = UIHelper.FindButton(root, "btn-download-cert");
            if (_btnDownload != null) _btnDownload.onClick.AddListener(OnDownload);

            _btnShare = UIHelper.FindButton(root, "btn-share-cert");
            if (_btnShare != null) _btnShare.onClick.AddListener(OnShare);
        }

        private void OnDownload()
        {
            Debug.Log("[CERTIFICATE] Downloading official verified PDF certificate for " + (AppState.Instance?.WorkerName ?? "Worker"));
        }

        private void OnShare()
        {
            Debug.Log("[CERTIFICATE] Sharing verified certificate link: https://mines.gov.in/surakshaar/verify/" + (AppState.Instance?.CertificateId ?? "IND-SAR-2026-0941"));
        }

        private void GoBack()
        {
            var manager = UIManager.Instance;
            if (manager != null && manager.HasPreviousScreen && manager.PreviousScreen == ScreenId.Result)
            {
                manager.ShowScreen(ScreenId.Result);
                return;
            }
            GoHome();
        }

        private void GoHome()
        {
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnTopbarBack != null) _btnTopbarBack.onClick.RemoveListener(GoBack);
            if (_btnBackHome != null) _btnBackHome.onClick.RemoveListener(GoHome);
            if (_btnDownload != null) _btnDownload.onClick.RemoveListener(OnDownload);
            if (_btnShare != null) _btnShare.onClick.RemoveListener(OnShare);
            _btnTopbarBack = null;
            _btnBackHome = null;
            _btnDownload = null;
            _btnShare = null;
        }
    }
}