using UnityEngine;

public class GasLeakController : MonoBehaviour
{
    [Header("Gas Leak")]
    [SerializeField] private bool leakActive = true;

    [Tooltip("Leak point relative to GasScenarioRoot.")]
    [SerializeField] private Vector3 leakLocalPosition = new Vector3(0f, 1.7f, 0f);

    [Header("Realistic VFX")]
    [SerializeField] private float cloudRadius = 0.45f;
    [SerializeField] private int particleCount = 45;
    [SerializeField] private float particleLifetime = 3.5f;

    private ParticleSystem gasParticles;
    private Material gasMaterial;

    private void Start()
    {
        CreateRealisticGasLeak();

        if (leakActive)
            StartLeak();
    }

    private void CreateRealisticGasLeak()
    {
        GameObject gasObject = new GameObject("Realistic_Gas_Leak");
        gasObject.transform.SetParent(transform, false);
        gasObject.transform.localPosition = leakLocalPosition;

        gasParticles = gasObject.AddComponent<ParticleSystem>();

        // ---------------- MAIN ----------------
        var main = gasParticles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        main.startLifetime =
            new ParticleSystem.MinMaxCurve(
                particleLifetime * 0.65f,
                particleLifetime
            );

        main.startSpeed =
            new ParticleSystem.MinMaxCurve(0.03f, 0.12f);

        main.startSize =
            new ParticleSystem.MinMaxCurve(0.12f, 0.30f);

        main.startColor =
            new Color(0.82f, 0.90f, 0.78f, 0.16f);

        main.maxParticles = particleCount;

        // ---------------- EMISSION ----------------
        var emission = gasParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 10f;

        // ---------------- SHAPE ----------------
        var shape = gasParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = cloudRadius * 0.18f;

        // ---------------- UPWARD MOVEMENT ----------------
        var velocity = gasParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.025f, 0.025f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.035f, 0.10f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.025f, 0.025f);

        // ---------------- NATURAL MOVEMENT ----------------
        var noise = gasParticles.noise;
        noise.enabled = true;
        noise.strength = 0.22f;
        noise.frequency = 0.45f;
        noise.scrollSpeed = 0.20f;

        // ---------------- SIZE OVER LIFE ----------------
        var sizeOverLife = gasParticles.sizeOverLifetime;
        sizeOverLife.enabled = true;

        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(0.25f, 0.80f),
            new Keyframe(0.70f, 1.15f),
            new Keyframe(1f, 1.45f)
        );

        sizeOverLife.size =
            new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ---------------- COLOR OVER LIFE ----------------
        var colorOverLife = gasParticles.colorOverLifetime;
        colorOverLife.enabled = true;

        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(
                    new Color(0.78f, 0.88f, 0.76f),
                    0f
                ),
                new GradientColorKey(
                    new Color(0.88f, 0.92f, 0.84f),
                    0.65f
                ),
                new GradientColorKey(
                    new Color(0.95f, 0.95f, 0.90f),
                    1f
                )
            },

            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.13f, 0.12f),
                new GradientAlphaKey(0.10f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        colorOverLife.color =
            new ParticleSystem.MinMaxGradient(gradient);

        // ---------------- RENDERER ----------------
        var renderer =
            gasParticles.GetComponent<ParticleSystemRenderer>();

        renderer.renderMode =
            ParticleSystemRenderMode.Billboard;

        renderer.alignment =
            ParticleSystemRenderSpace.View;

        // Create a soft circular gas texture.
        Texture2D gasTexture = CreateSoftGasTexture();

        Shader shader =
            Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
            shader = Shader.Find("Particles/Standard Unlit");

        if (shader != null)
        {
            gasMaterial = new Material(shader);

            gasMaterial.mainTexture = gasTexture;

            gasMaterial.color =
                new Color(0.82f, 0.90f, 0.78f, 0.20f);

            renderer.material = gasMaterial;
        }

        gasParticles.Stop();
    }

    private Texture2D CreateSoftGasTexture()
    {
        int size = 64;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx =
                    (x - size * 0.5f) /
                    (size * 0.5f);

                float dy =
                    (y - size * 0.5f) /
                    (size * 0.5f);

                float distance =
                    Mathf.Sqrt(dx * dx + dy * dy);

                float alpha =
                    Mathf.Clamp01(1f - distance);

                alpha =
                    Mathf.Pow(alpha, 2.2f);

                Color pixel =
                    new Color(
                        0.82f,
                        0.88f,
                        0.80f,
                        alpha
                    );

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();

        return texture;
    }

    public void StartLeak()
    {
        leakActive = true;

        if (gasParticles != null &&
            !gasParticles.isPlaying)
        {
            gasParticles.Play();
        }
    }

    public void StopLeak()
    {
        leakActive = false;

        if (gasParticles != null)
            gasParticles.Stop();
    }

    public void ToggleLeak()
    {
        if (leakActive)
            StopLeak();
        else
            StartLeak();
    }

    public bool IsLeakActive()
    {
        return leakActive;
    }
}