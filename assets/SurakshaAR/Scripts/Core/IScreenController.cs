using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Every screen controller implements this interface.
    /// UIManager calls OnShow right after instantiating the screen's
    /// uGUI hierarchy into the ScreenContainer, and OnHide before destroying it.
    /// </summary>
    public interface IScreenController
    {
        /// <param name="root">The root GameObject of this screen's uGUI hierarchy.</param>
        /// <param name="param">Optional data passed by the screen that navigated here (e.g. a ModuleData).</param>
        void OnShow(GameObject root, object param);

        /// <summary>Unsubscribe any events here. Called before the screen is torn down.</summary>
        void OnHide();
    }
}
