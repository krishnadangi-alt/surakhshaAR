using UnityEngine;

public class GasLeakVisualController : MonoBehaviour
{
    [Header("Gas Appearance")]
    [SerializeField] private Color gasColor = new Color(0.82f, 0.94f, 0.86f, 0.16f);
    [SerializeField] private float emissionRate = 24f;

    [Header("Gas Movement")]
    [SerializeField] private float upwardSpeed = 0.10f;
    [SerializeField] private float outwardSpeed = 0.055f;
    [SerializeField] private float particleLifetime = 4.5f;

    [Header("Gas Size")]
    [SerializeField] private float startSize = 0.045f;
    [SerializeField] private float endSize = 0.32f;

    [Header("Leak Point")]
    [Tooltip("Exact child name used first. If missing, Valve/Regulator/Gauge names are searched.")]
    [SerializeField] private string leakPointName = "LeakPoint";
    [SerializeField] private float leakOffset = 0.005f;

    [Header("GasLeakVFX Prefab")]
    [Tooltip("Assign the Particle System prefab you created. It is instantiated at the real leak point.")]
    [SerializeField] private GameObject gasLeakVFXPrefab;

    [Header("Fallback")]
    [SerializeField] private float fallbackHeightOffset = 0.04f;

    private ParticleSystem gasParticles;
    private GameObject gasEffectObject;
    private Material gasMaterial;
    private Texture2D gasTexture;

    public void StartGasLeak(GameObject cylinder)
    {
        StopGasLeak();

        if (cylinder == null) return;

        Transform leakPoint = FindLeakPoint(cylinder.transform);

        if (leakPoint != null)
        {
            Debug.Log("[GasAR] GAS LEAK: Leak source found at " + leakPoint.name + " (local pos: " + leakPoint.localPosition + ")");
        }
        else
        {
            Debug.LogWarning("[GasAR] Gas Leak: No LeakPoint found on cylinder. Creating default at upper valve.");
            GameObject lpObj = new GameObject("LeakPoint");
            lpObj.transform.SetParent(cylinder.transform, false);
            lpObj.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            lpObj.transform.localRotation = Quaternion.identity;
            leakPoint = lpObj.transform;
        }

        if (gasLeakVFXPrefab != null)
        {
            gasEffectObject = Instantiate(gasLeakVFXPrefab, leakPoint);
            gasEffectObject.transform.localPosition = Vector3.up * leakOffset;
            gasEffectObject.transform.localRotation = Quaternion.identity;
            gasEffectObject.name = "REALISTIC_GAS_LEAK_VFX";

            gasParticles = gasEffectObject.GetComponentInChildren<ParticleSystem>(true);
            if (gasParticles == null)
            {
                Debug.LogError("[GasAR] GasLeakVFX prefab has no ParticleSystem.");
                Destroy(gasEffectObject);
                gasEffectObject = null;
                return;
            }

            ConfigureParticleSystem(gasParticles);
        }
        else
        {
            gasEffectObject = new GameObject("REALISTIC_GAS_LEAK_VFX");
            gasEffectObject.transform.SetParent(leakPoint, false);
            gasEffectObject.transform.localPosition = Vector3.up * leakOffset;
            gasEffectObject.transform.localRotation = Quaternion.identity;

            gasParticles = gasEffectObject.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(gasParticles);
        }

        gasParticles.Play(true);
        Debug.Log("[GasAR] REALISTIC GAS LEAK VFX ATTACHED TO LEAK POINT & STARTED");
    }

    private Transform FindLeakPoint(Transform root)
    {
        Transform exact = FindChildRecursive(root, leakPointName);
        if (exact != null)
            return exact;

        string[] possibleNames =
        {
            "Valve",
            "GasPipelineValve",
            "Regulator",
            "Pressure Gauge",
            "PressureGauge",
            "Gauge"
        };

        foreach (string target in possibleNames)
        {
            Transform found = FindChildRecursive(root, target);
            if (found != null)
                return found;
        }

        return null;
    }

    private void ConfigureParticleSystem(ParticleSystem ps)
    {
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.duration = 5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(
            particleLifetime * 0.65f,
            particleLifetime
        );
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.045f);
        main.startSize = new ParticleSystem.MinMaxCurve(
            startSize * 0.7f,
            startSize
        );
        main.startColor = gasColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 160;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = emissionRate;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.012f;

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-outwardSpeed, outwardSpeed);
        velocity.y = new ParticleSystem.MinMaxCurve(upwardSpeed * 0.45f, upwardSpeed);
        velocity.z = new ParticleSystem.MinMaxCurve(-outwardSpeed, outwardSpeed);

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.28f;
        noise.frequency = 0.35f;
        noise.scrollSpeed = 0.18f;
        noise.octaveCount = 2;

        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;

        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.16f),
            new Keyframe(0.18f, 0.38f),
            new Keyframe(0.45f, 0.72f),
            new Keyframe(1f, 1f)
        );

        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(
                    new Color(gasColor.r, gasColor.g, gasColor.b),
                    0f
                ),
                new GradientColorKey(
                    new Color(gasColor.r, gasColor.g, gasColor.b),
                    0.45f
                ),
                new GradientColorKey(
                    new Color(gasColor.r, gasColor.g, gasColor.b),
                    1f
                )
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(gasColor.a, 0.08f),
                new GradientAlphaKey(gasColor.a * 0.8f, 0.32f),
                new GradientAlphaKey(gasColor.a * 0.42f, 0.72f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        color.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateGasMaterial();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent.name == targetName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildRecursive(
                parent.GetChild(i),
                targetName
            );

            if (result != null)
                return result;
        }

        return null;
    }

    private Material CreateGasMaterial()
    {
        if (gasMaterial != null)
            return gasMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError("[GasAR] Gas Leak: No compatible particle shader found.");
            return null;
        }

        gasMaterial = new Material(shader);
        gasMaterial.name = "MAT_Realistic_Gas_Leak_Runtime";

        gasTexture = CreateSoftGasTexture();

        // URP & Standard Transparent Particle Configuration (Eliminates black card/rectangle artifacts)
        if (gasMaterial.HasProperty("_Surface")) gasMaterial.SetFloat("_Surface", 1f); // 1 = Transparent
        if (gasMaterial.HasProperty("_Blend")) gasMaterial.SetFloat("_Blend", 0f); // 0 = Alpha Blend
        if (gasMaterial.HasProperty("_SrcBlend")) gasMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (gasMaterial.HasProperty("_DstBlend")) gasMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (gasMaterial.HasProperty("_ZWrite")) gasMaterial.SetFloat("_ZWrite", 0f);
        if (gasMaterial.HasProperty("_Mode")) gasMaterial.SetFloat("_Mode", 2f); // 2 = Fade/Transparent for Standard

        gasMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        gasMaterial.EnableKeyword("_ALPHABLEND_ON");
        gasMaterial.DisableKeyword("_ALPHATEST_ON");
        gasMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        if (gasMaterial.HasProperty("_BaseMap"))
            gasMaterial.SetTexture("_BaseMap", gasTexture);
        if (gasMaterial.HasProperty("_MainTex"))
            gasMaterial.SetTexture("_MainTex", gasTexture);
        if (gasMaterial.HasProperty("_BaseColor"))
            gasMaterial.SetColor("_BaseColor", gasColor);
        if (gasMaterial.HasProperty("_Color"))
            gasMaterial.SetColor("_Color", gasColor);

        gasMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        return gasMaterial;
    }

    private Texture2D CreateSoftGasTexture()
    {
        const int resolution = 64;

        Texture2D texture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "Soft_Gas_Alpha_Texture";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float nx = (x / (float)(resolution - 1)) * 2f - 1f;
                float ny = (y / (float)(resolution - 1)) * 2f - 1f;

                float distance = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = Mathf.Pow(alpha, 2.2f);

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    public void StopGasLeak()
    {
        if (gasParticles != null)
        {
            gasParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        if (gasEffectObject != null)
        {
            Destroy(gasEffectObject);
            gasEffectObject = null;
        }

        gasParticles = null;
    }

    private void OnDestroy()
    {
        StopGasLeak();

        if (gasTexture != null)
        {
            Destroy(gasTexture);
            gasTexture = null;
        }

        if (gasMaterial != null)
        {
            Destroy(gasMaterial);
            gasMaterial = null;
        }
    }
}
