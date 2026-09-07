using UnityEngine;

namespace SurakshaAR.UI
{
    /// <summary>
    /// Automatically shows the SurakshaAR main menu when the app launches,
    /// without requiring any scene or prefab wiring.
    ///
    /// Behaviour:
    ///   - After every scene load the menu is (re-)created - so if the app ever
    ///     launches on a different scene, or a future menu scene is added, the
    ///     menu still appears.
    ///   - Right before "Start Module" loads the fire training scene a one-shot
    ///     suppression flag is set, so the menu never floats over the AR
    ///     training experience.
    /// </summary>
    public static class MainMenuBootstrapper
    {
        private static bool s_suppressNextSceneLoad;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            if (s_suppressNextSceneLoad)
            {
                // The player just started the fire training module; do not
                // re-create the menu over the AR scene.
                s_suppressNextSceneLoad = false;
                return;
            }

            if (GameObject.Find(MainMenuController.RootName) != null)
                return; // menu already present in this scene

            MainMenuController.Create(() =>
            {
                s_suppressNextSceneLoad = true;
            });
        }

        /// <summary>
        /// Suppresses the menu on the next scene load. Intended for code paths
        /// other than the main menu button that still transition into the fire
        /// training scene (e.g. quick-launch / deep-link flows).
        /// </summary>
        public static void SuppressMenuForNextSceneLoad()
        {
            s_suppressNextSceneLoad = true;
        }
    }
}