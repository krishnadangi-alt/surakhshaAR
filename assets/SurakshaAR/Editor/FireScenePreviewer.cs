using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Editor
{
    /// <summary>
    /// FireScenePreviewer
    /// ==================
    /// Ensures that Fire VFX particle systems (Flames, Sparks, Ashes, Lights, Distortion)
    /// actively simulate and render in the Unity Scene View even in Edit Mode.
    /// </summary>
    [InitializeOnLoad]
    public static class FireScenePreviewer
    {
        private static double _lastTime;
        private static GameObject _cachedVfxGo;
        private static ParticleSystem[] _cachedSystems;

        static FireScenePreviewer()
        {
            _lastTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (Application.isPlaying) return;

            double now = EditorApplication.timeSinceStartup;
            float dt = (float)(now - _lastTime);
            _lastTime = now;

            if (dt <= 0f || dt > 0.1f) dt = 0.016f;

            if (_cachedVfxGo == null || !_cachedVfxGo.activeInHierarchy)
            {
                _cachedVfxGo = GameObject.Find("VFX_Fire_01_Small");
                if (_cachedVfxGo == null)
                {
                    var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
                    if (hazard != null)
                    {
                        var t = hazard.transform.Find("Electric Box/VFX_Fire_01_Small") ?? hazard.transform.Find("VFX_Fire_01_Small");
                        if (t != null) _cachedVfxGo = t.gameObject;
                    }
                }

                if (_cachedVfxGo != null)
                {
                    _cachedSystems = _cachedVfxGo.GetComponentsInChildren<ParticleSystem>(true);
                }
            }

            if (_cachedVfxGo == null || !_cachedVfxGo.activeInHierarchy || _cachedSystems == null) return;

            bool simulatedAny = false;
            foreach (var ps in _cachedSystems)
            {
                if (ps == null || !ps.gameObject.activeInHierarchy) continue;

                // Ensure looping and prewarm are enabled
                var main = ps.main;
                main.loop = true;
                main.playOnAwake = true;

                var em = ps.emission;
                em.enabled = true;

                var rend = ps.GetComponent<ParticleSystemRenderer>();
                if (rend != null && ps.name != "VFX_Fire_01_Small") rend.enabled = true;

                // Advance simulation in Edit Mode
                ps.Simulate(dt, false, false, true);
                simulatedAny = true;
            }

            if (simulatedAny)
            {
                SceneView.RepaintAll();
            }
        }
    }
}
