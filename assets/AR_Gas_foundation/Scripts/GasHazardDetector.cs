using UnityEngine;

public class GasHazardDetector : MonoBehaviour
{
    private GasEnvironmentBuilder environmentBuilder;

    private void Awake()
    {
        environmentBuilder = GetComponent<GasEnvironmentBuilder>();
        if (environmentBuilder == null)
        {
            environmentBuilder = FindFirstObjectByType<GasEnvironmentBuilder>();
        }
    }

    public void BuildEnvironment()
    {
        if (environmentBuilder != null)
        {
            environmentBuilder.BuildEnvironment();
        }
    }

    public void ClearEnvironment()
    {
        if (environmentBuilder != null)
        {
            environmentBuilder.ClearEnvironment();
        }
    }
}