using UnityEngine;

namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// GasLeakVisualController — Controls the existing manually-created gas particle effect.
    /// Binds the existing ParticleSystem to the LeakPoint transform and controls emission playback.
    /// </summary>
    public class GasLeakVisualController : MonoBehaviour
    {
        [Header("Leak Location")]
        [Tooltip("The existing LeakPoint transform near the gas cylinder valve.")]
        public Transform leakPoint;

        [Header("Gas Particle Effect")]
        [Tooltip("The existing manually-created ParticleSystem for gas/smoke.")]
        public ParticleSystem gasParticles;

        [Header("Settings")]
        public bool playOnStart = true;

        private void Awake()
        {
            // Auto-assign ParticleSystem if attached to the same GameObject
            if (gasParticles == null)
            {
                gasParticles = GetComponent<ParticleSystem>();
            }

            // Fall back to current transform if no leakPoint assigned
            if (leakPoint == null)
            {
                leakPoint = transform;
            }
        }

        private void Start()
        {
            AlignWithLeakPoint();

            if (playOnStart)
            {
                StartLeak();
            }
            else
            {
                StopLeak();
            }
        }

        /// <summary>
        /// Parents and zeroes the particle system transform relative to LeakPoint 
        /// so it remains locked to the AR cylinder when moved, rotated, or scaled.
        /// </summary>
        public void AlignWithLeakPoint()
        {
            if (gasParticles != null && leakPoint != null)
            {
                if (gasParticles.transform.parent != leakPoint)
                {
                    gasParticles.transform.SetParent(leakPoint, false);
                }
                gasParticles.transform.localPosition = Vector3.zero;
                gasParticles.transform.localRotation = Quaternion.identity;
                gasParticles.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Starts emitting gas particles from the LeakPoint.
        /// </summary>
        public void StartLeak()
        {
            if (gasParticles == null) return;

            AlignWithLeakPoint();

            var emission = gasParticles.emission;
            emission.enabled = true;

            if (!gasParticles.isPlaying)
            {
                gasParticles.Play();
            }
        }

        /// <summary>
        /// Stops the gas particle emission cleanly.
        /// </summary>
        public void StopLeak()
        {
            if (gasParticles == null) return;

            var emission = gasParticles.emission;
            emission.enabled = false;

            gasParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        /// <summary>
        /// Checks if the gas leak particle effect is currently emitting.
        /// </summary>
        public bool IsLeaking()
        {
            return gasParticles != null && gasParticles.isPlaying && gasParticles.emission.enabled;
        }

        /// <summary>
        /// Adjusts the visual simulation speed / intensity.
        /// </summary>
        public void SetSimulationSpeed(float speed)
        {
            if (gasParticles == null) return;

            var main = gasParticles.main;
            main.simulationSpeed = Mathf.Clamp(speed, 0.1f, 3.0f);
        }
    }
}
