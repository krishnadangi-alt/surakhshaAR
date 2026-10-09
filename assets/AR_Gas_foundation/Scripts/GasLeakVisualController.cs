using UnityEngine;

public class GasLeakVisualController : MonoBehaviour
{
    public static GasLeakVisualController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

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
        if (leakPoint == null)
        {
            GameObject lpObj = new GameObject("LeakPoint");
            lpObj.transform.SetParent(cylinder.transform, false);
            leakPoint = lpObj.transform;
        }

        // Align leakPoint to the physical upper valve / regulator connection point
        GameObject regulatorObj = GameObject.Find("Real_Gas_Regulator");
        if (regulatorObj != null)
        {
            leakPoint.position = regulatorObj.transform.position + new Vector3(-0.035f, -0.01f, 0f);
        }
        else
        {
            leakPoint.position = cylinder.transform.position + new Vector3(0.04f, 0.61f, 0f);
        }

        Debug.Log($"[GasAR] GAS LEAK: Leak source positioned at {leakPoint.name} (world pos: {leakPoint.position})");

        gasEffectObject = new GameObject("REALISTIC_GAS_LEAK_VFX");
        gasEffectObject.transform.SetParent(leakPoint, false);
        gasEffectObject.transform.localPosition = Vector3.zero;
        gasEffectObject.transform.localRotation = Quaternion.identity;

        gasParticles = gasEffectObject.AddComponent<ParticleSystem>();
        ConfigureParticleSystem(gasParticles);

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
        main.duration = 4f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.7f); // Short lifetime: limits vertical column and disperses
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.10f, 0.20f); // Gentle escape speed
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.07f);  // Starts small at the nozzle (4-7cm)
        main.startColor = new Color(0.92f, 0.95f, 0.97f, 0.25f);        // Soft semi-transparent white/very-light-gray vapor
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.maxParticles = 160;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 38f; // Dense overlapping puffs that merge seamlessly into vapor

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 16f;
        shape.radius = 0.012f; // Localized at the connection

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.035f, 0.035f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.08f, 0.17f); // Gentle rising
        velocity.z = new ParticleSystem.MinMaxCurve(-0.035f, 0.035f);

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.12f;
        noise.frequency = 0.45f;
        noise.scrollSpeed = 0.20f;
        noise.octaveCount = 2;

        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;

        // Billows and expands smoothly as it moves away from nozzle
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(0.25f, 0.70f),
            new Keyframe(0.60f, 1.20f),
            new Keyframe(1f, 1.65f)
        );
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.93f, 0.96f, 0.98f), 0f),
                new GradientColorKey(new Color(0.90f, 0.94f, 0.96f), 0.5f),
                new GradientColorKey(new Color(0.88f, 0.92f, 0.95f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.0f, 0f),
                new GradientAlphaKey(0.35f, 0.12f),  // Dense near the leak source (first 20-30 cm)
                new GradientAlphaKey(0.22f, 0.40f),  // Gradually expands and softens
                new GradientAlphaKey(0.07f, 0.70f),  // Disperses
                new GradientAlphaKey(0.0f, 1.0f)     // Fully dissipates
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

        // URP & Standard Transparent Particle Configuration
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
            gasMaterial.SetColor("_BaseColor", Color.white);
        if (gasMaterial.HasProperty("_Color"))
            gasMaterial.SetColor("_Color", Color.white);

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
                float r = Mathf.Clamp01(distance);
                // Gaussian-like smooth cubic radial falloff to eliminate any harsh dots/points
                float alpha = Mathf.Clamp01(1f - r);
                alpha = alpha * alpha * (3f - 2f * alpha);
                // Soft vapor opacity
                alpha *= 0.50f;

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    public void StartLeak()
    {
        StartGasLeak(gameObject);
    }

    public void StopLeak()
    {
        StopGasLeak();
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
