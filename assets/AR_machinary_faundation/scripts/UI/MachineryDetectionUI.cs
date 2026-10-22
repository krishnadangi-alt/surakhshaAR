using UnityEngine;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryDetectionUI
    /// ====================
    /// Manages world-to-screen target bounding calculation and 4-corner glowing teal bracket rendering.
    /// Works with Camera.main to ensure detection frames follow real 3D equipment transforms.
    /// </summary>
    public class MachineryDetectionUI : MonoBehaviour
    {
        public static Vector3 CalculateTargetCenter(Transform target)
        {
            if (target == null) return Vector3.zero;

            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                return b.center;
            }
            return target.position + Vector3.up * 0.2f;
        }

        public static float CalculateDistanceScaledSize(float distance, float baseSize = 720f, float minSize = 220f, float maxSize = 420f)
        {
            return Mathf.Clamp(baseSize / Mathf.Max(distance, 0.8f), minSize, maxSize);
        }
    }
}
