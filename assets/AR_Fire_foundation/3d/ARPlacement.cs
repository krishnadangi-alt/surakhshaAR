using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARPlacement : MonoBehaviour
{
    [Header("Fire Extinguisher")]
    public GameObject fireExtinguisher;

    [Header("Size")]
    [Tooltip("Adjust this if the extinguisher is too big or too small.")]
    [Range(0.01f, 1.0f)]
    public float extinguisherScale = 0.15f;

    [Header("Placement")]
    [Tooltip("Keeps the rotation saved in the prefab.")]
    public bool usePrefabRotation = true;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    private readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    private GameObject spawnedObject;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        if (raycastManager == null)
        {
            Debug.LogError(
                "ARPlacement needs an ARRaycastManager on XR Origin."
            );
        }

        if (planeManager == null)
        {
            Debug.LogError(
                "ARPlacement needs an ARPlaneManager on XR Origin."
            );
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        // Only allow one placement.
        if (spawnedObject != null)
            return;

        // No touch.
        if (Touch.activeTouches.Count == 0)
            return;

        Touch touch = Touch.activeTouches[0];

        // Only respond to the initial tap.
        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        Vector2 screenPosition = touch.screenPosition;

        // Check whether the user tapped a detected AR plane.
        if (raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            PlaceFireExtinguisher(hitPose);
        }
    }

    private void PlaceFireExtinguisher(Pose hitPose)
    {
        if (fireExtinguisher == null)
        {
            Debug.LogError(
                "FIRE EXTINGUISHER PREFAB IS NOT ASSIGNED!"
            );

            return;
        }

        /*
         * IMPORTANT:
         *
         * We preserve the rotation of the prefab.
         *
         * Your prefab has:
         * X = -90
         * Y = 0
         * Z = 0
         *
         * So we do NOT replace it with Quaternion.identity.
         */

        Quaternion rotation;

        if (usePrefabRotation)
        {
            rotation = fireExtinguisher.transform.rotation;
        }
        else
        {
            rotation = hitPose.rotation;
        }

        // Create the extinguisher.
        spawnedObject = Instantiate(
            fireExtinguisher,
            hitPose.position,
            rotation
        );

        // Set the size.
        spawnedObject.transform.localScale =
            Vector3.one * extinguisherScale;

        /*
         * Make the bottom of the extinguisher
         * sit on the detected surface.
         */
        PlaceBottomOnSurface(hitPose.position);

        // Hide yellow AR planes.
        HidePlanes();

        Debug.Log(
            "FIRE EXTINGUISHER PLACED SUCCESSFULLY!"
        );
    }

    private void PlaceBottomOnSurface(Vector3 surfacePosition)
    {
        if (spawnedObject == null)
            return;

        Renderer[] renderers =
            spawnedObject.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }

        // Move model upward/downward so its bottom
        // touches the detected surface.
        float difference =
            surfacePosition.y - bounds.min.y;

        spawnedObject.transform.position +=
            Vector3.up * difference;
    }

    private void HidePlanes()
    {
        if (planeManager == null)
            return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(false);
            }
        }

        // Stop detecting new planes after placement.
        planeManager.enabled = false;
    }
}