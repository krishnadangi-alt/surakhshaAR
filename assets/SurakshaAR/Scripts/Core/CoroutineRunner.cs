using System.Collections;
using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// CoroutineRunner (Day 6)
    /// =================
    /// Tiny helper that lets non-MonoBehaviour screen controllers
    /// (e.g. Screens.LoginController, which implements IScreenController)
    /// run Unity coroutines such as the POST /api/v1/auth/login flow.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static Coroutine Run(IEnumerator routine)
        {
            if (routine == null) return null;
            if (_instance == null)
            {
                var go = new GameObject("CoroutineRunner");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<CoroutineRunner>();
            }
            return _instance.StartCoroutine(routine);
        }
    }
}
