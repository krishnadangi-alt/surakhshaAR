using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FireEquipmentInteraction : MonoBehaviour
{
    [Header("Complete Fire Scenario")]
    public GameObject fireScenario;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    private readonly System.Collections.Generic.List<ARRaycastHit> hits =
        new System.Collections.Generic.List<ARRaycastHit>();

    private bool scenarioPlaced = false;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        if (raycastManager == null)
        {
            Debug.LogError(
                "FireEquipmentInteraction: ARRaycastManager not found."
            );
        }

        if (planeManager == null)
        {
            Debug.LogError(
                "FireEquipmentInteraction: ARPlaneManager not found."
            );
        }

        if (fireScenario == null)
        {
            Debug.LogError(
                "FireEquipmentInteraction: Fire Scenario is not assigned."
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

    private void Start()
    {
        if (fireScenario != null)
        {
            fireScenario.SetActive(false);
        }
    }

    private void Update()
    {
        // Scenario can only be placed once.
        if (scenarioPlaced)
            return;

        if (Touch.activeTouches.Count == 0)
            return;

        Touch touch = Touch.activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        bool foundPlane = raycastManager.Raycast(
            touch.screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon
        );

        if (!foundPlane)
            return;

        PlaceScenario(hits[0].pose);
    }

    private void PlaceScenario(Pose hitPose)
    {
        if (fireScenario == null)
            return;

        // ---------------------------------------
        // POSITION
        // ---------------------------------------

        fireScenario.transform.position = hitPose.position;

        // ---------------------------------------
        // ROTATION
        // Keep the scenario upright.
        // ---------------------------------------

        fireScenario.transform.rotation =
            Quaternion.Euler(
                0f,
                hitPose.rotation.eulerAngles.y,
                0f
            );

        // ---------------------------------------
        // ACTIVATE SCENARIO
        // ---------------------------------------

        fireScenario.SetActive(true);

        // ---------------------------------------
        // ADD ANCHOR
        // ---------------------------------------

        ARAnchor anchor =
            fireScenario.GetComponent<ARAnchor>();

        if (anchor == null)
        {
            anchor = fireScenario.AddComponent<ARAnchor>();
        }

        // ---------------------------------------
        // MARK AS PLACED
        // ---------------------------------------

        scenarioPlaced = true;

        // ---------------------------------------
        // HIDE DETECTED PLANES
        // ---------------------------------------

        HidePlanes();

        Debug.Log(
            "COMPLETE FIRE SCENARIO PLACED AND ANCHORED."
        );
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

        planeManager.enabled = false;
    }
}