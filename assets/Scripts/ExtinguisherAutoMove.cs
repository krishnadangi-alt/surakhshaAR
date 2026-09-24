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
        // Guard: If the extinguisher is held by the worker (ExtinguisherPickup.IsHeld()),
        // DO NOT allow ExtinguisherAutoMove to fight or override camera attachment.
        var pickup = GetComponent<ExtinguisherPickup>() ?? GetComponentInParent<ExtinguisherPickup>();
        if (pickup != null && pickup.IsHeld())
        {
            Debug.Log("[ExtinguisherAutoMove] Extinguisher is held by camera. Skipping auto-move to preserve camera attachment.");
            return;
        }

        if (hasMoved)
            return;

        if (extinguisherTarget == null)
        {
            Debug.LogWarning("[ExtinguisherAutoMove] Extinguisher Target is not assigned. Skipping auto-move.");
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