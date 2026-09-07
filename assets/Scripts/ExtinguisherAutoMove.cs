using UnityEngine;
using System.Collections;

public class ExtinguisherAutoMove : MonoBehaviour
{
    [Header("Extinguisher Target")]
    public Transform extinguisherTarget;

    [Header("Movement")]
    public float moveDuration = 1f;

    private bool hasMoved = false;

    public void MoveToTarget()
    {
        if (hasMoved)
            return;

        if (extinguisherTarget == null)
        {
            Debug.LogError("Extinguisher Target is not assigned.");
            return;
        }

        hasMoved = true;
        StartCoroutine(MoveExtinguisher());
    }

    private IEnumerator MoveExtinguisher()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = extinguisherTarget.position;

        // Keep whatever rotation FireExt already has.
        Quaternion fixedRotation = transform.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            transform.rotation = fixedRotation;

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = fixedRotation;

        Debug.Log("Extinguisher moved and kept correct direction.");
    }
}