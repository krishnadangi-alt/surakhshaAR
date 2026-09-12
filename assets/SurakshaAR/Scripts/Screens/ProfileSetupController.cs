using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public class ProfileSetupController : IScreenController
    {
        private TMP_InputField _fieldName, _fieldWorkerId;
        private Button _btnNext;

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;
            var profile = AppManager.Instance?.CurrentProfile;

            var nameBox = UIHelper.FindRect(root, "field-fullname");
            if (nameBox != null) _fieldName = nameBox.GetComponentInChildren<TMP_InputField>();

            var idBox = UIHelper.FindRect(root, "field-workerid");
            if (idBox != null) _fieldWorkerId = idBox.GetComponentInChildren<TMP_InputField>();

            if (profile != null)
            {
                if (_fieldName != null) _fieldName.text = profile.fullName;
                if (_fieldWorkerId != null) _fieldWorkerId.text = profile.workerId;
            }

            _btnNext = UIHelper.FindButton(root, "btn-next");
            if (_btnNext != null)
            {
                _btnNext.onClick.AddListener(OnNext);
            }
        }

        private void OnNext()
        {
            var profile = AppManager.Instance?.CurrentProfile ?? new UserProfileData();
            profile.fullName = _fieldName != null ? _fieldName.text?.Trim() : "Ramesh Kumar";
            profile.workerId = _fieldWorkerId != null ? _fieldWorkerId.text?.Trim() : "JH-MN-004821";

            AppManager.Instance?.SaveProfile(profile);
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnNext != null) _btnNext.onClick.RemoveListener(OnNext);
        }
    }
}
