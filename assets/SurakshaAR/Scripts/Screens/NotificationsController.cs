using SurakshaAR.Core;
using SurakshaAR.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public class NotificationsController : IScreenController
    {
        private Button _btnBack;

        public void OnShow(GameObject root, object param)
        {
            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard));
            }
        }

        public void OnHide()
        {
        }
    }
}
