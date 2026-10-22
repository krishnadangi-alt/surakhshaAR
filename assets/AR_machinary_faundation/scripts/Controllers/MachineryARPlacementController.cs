using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using SurakshaAR.Data;
using SurakshaAR.UI;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryARPlacementController
    /// ==============================
    /// Handles AR Foundation horizontal plane detection, blue placement circle indicator,
    /// dynamic runtime bounds calculation, floor alignment, initial user-facing orientation,
    /// and world-space tap-to-place anchoring for the Machinery scenario.
    /// </summary>
    public class MachineryARPlacementController : MonoBehaviour
    {
        [Header("Machinery Scenario Target")]
        [Tooltip("Root GameObject containing complete machinery equipment assembly.")]
        [SerializeField] private GameObject machineryScenarioTarget;
        [SerializeField] private float targetBoundingWidth = 2.0f; // Target real-world horizontal width in meters
        [SerializeField] private bool hidePlanesAfterPlacement = true;

        [Header("Placement Indicator Settings")]
        [SerializeField] private float indicatorRadius = 0.22f;
        [SerializeField] private float indicatorWidth = 0.035f;
        [SerializeField] private float indicatorHeight = 0.005f;
        [SerializeField] private Color indicatorColor = new Color(0.0f, 0.65f, 1.0f, 1.0f); // Electric Blue

        [Header("AR References")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARAnchorManager anchorManager;
        [SerializeField] private Camera arCamera;
        [SerializeField] private bool autoStartScanning = false;

        public static MachineryARPlacementController Instance { get; private set; }

        private GameObject placementIndicator;
        private GameObject placementRoot;
        private LineRenderer indicatorLine;
        private Pose currentPose;
        private ARPlane currentPlane;
        private bool hasValidPose;
        private bool isPlaced;
        private bool isScanning;

        private static readonly List<ARRaycastHit> RaycastHits = new List<ARRaycastHit>();

        public bool IsPlaced => isPlaced;
        public bool HasValidPose => hasValidPose;
        public bool IsScanning => isScanning;

        public void StartScanning()
        {
            isScanning = true;
            if (planeManager != null)
            {
                planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
                planeManager.enabled = true;
            }
            Debug.Log("[MachineryARPlacementController] Floor plane scanning started.");
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            // 1. FIRST PRIORITY: Find machineryScenarioTarget and IMMEDIATELY hide it
            if (machineryScenarioTarget == null)
            {
                machineryScenarioTarget = GameObject.Find("MachineryScenario");
                if (machineryScenarioTarget == null)
                {
                    foreach (var rootGo in gameObject.scene.GetRootGameObjects())
                    {
                        if (rootGo.name.Contains("Machinery") && rootGo != gameObject && !rootGo.name.Contains("Session"))
                        {
                            machineryScenarioTarget = rootGo;
                            break;
                        }
                    }
                }
            }

            if (machineryScenarioTarget != null)
            {
                machineryScenarioTarget.SetActive(false);
            }

            // 2. Wire AR components
            if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>() ?? FindAnyObjectByType<ARRaycastManager>();
            if (planeManager == null) planeManager = GetComponent<ARPlaneManager>() ?? FindAnyObjectByType<ARPlaneManager>();
            if (anchorManager == null) anchorManager = GetComponent<ARAnchorManager>() ?? FindAnyObjectByType<ARAnchorManager>();

            EnsureCamera();
            EnsureCameraTracking();

            // 3. Force auto-start scanning
            autoStartScanning = true;
            isScanning = true;

            if (planeManager != null)
            {
                planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
                planeManager.enabled = true;
            }

            // 4. Create Placement Indicator and Bottom Placement Button
            CreatePlacementIndicator();
            CreateBottomPlacementButton();
        }

        private GameObject bottomButtonCanvas;
        private RectTransform placeButtonRT;

        private void CreateBottomPlacementButton()
        {
            if (FindAnyObjectByType<MachineryScenarioUIController>() != null)
            {
                if (bottomButtonCanvas != null) Destroy(bottomButtonCanvas);
                return;
            }

            if (bottomButtonCanvas != null)
            {
                Destroy(bottomButtonCanvas);
            }

            bottomButtonCanvas = new GameObject("MachineryPlacementButtonCanvas");
            var canvas = bottomButtonCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var scaler = bottomButtonCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400);
            scaler.matchWidthOrHeight = 0.5f;

            bottomButtonCanvas.AddComponent<GraphicRaycaster>();

            // Button Root
            var btnGO = new GameObject("PlaceMachineryBottomButton", typeof(RectTransform));
            btnGO.transform.SetParent(bottomButtonCanvas.transform, false);

            placeButtonRT = btnGO.GetComponent<RectTransform>();
            placeButtonRT.anchorMin = new Vector2(0.5f, 0f);
            placeButtonRT.anchorMax = new Vector2(0.5f, 0f);
            placeButtonRT.pivot = new Vector2(0.5f, 0f);
            placeButtonRT.anchoredPosition = new Vector2(0f, 90f);
            placeButtonRT.sizeDelta = new Vector2(850f, 140f);

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.04f, 0.45f, 0.88f, 0.96f);
            UIHelper.SetImageRoundedSprite(img, 28f);

            var btn = btnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.04f, 0.45f, 0.88f, 0.96f);
            colors.highlightedColor = new Color(0.12f, 0.58f, 0.98f, 1f);
            colors.pressedColor = new Color(0.02f, 0.32f, 0.65f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(OnBottomPlaceButtonClicked);

            // Button Text
            var label = UIHelper.MakeLabel("ButtonText", placeButtonRT, "📍 TAP TO PLACE MACHINERY", 34, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(label.rectTransform);
        }

        private void OnBottomPlaceButtonClicked()
        {
            Debug.Log("[MachineryARPlacementController] Bottom placement button clicked!");
            PlaceMachineryNow();
        }

        private void EnsureCamera()
        {
            if (arCamera == null) arCamera = Camera.main;
            if (arCamera == null && raycastManager != null) arCamera = raycastManager.GetComponentInChildren<Camera>(true);
            if (arCamera == null) arCamera = Object.FindAnyObjectByType<Camera>();
        }

        private void EnsureCameraTracking()
        {
            try
            {
                EnsureCamera();
                if (arCamera == null) return;

                var poseDriver = arCamera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if (poseDriver == null)
                {
                    poseDriver = arCamera.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                }

                if (poseDriver != null)
                {
                    var posAction = poseDriver.positionInput.action;
                    if (posAction != null)
                    {
                        if (posAction.bindings.Count == 0)
                        {
                            if (posAction.enabled) posAction.Disable();
                            posAction.AddBinding("<HandheldARInputDevice>/devicePosition");
                            posAction.AddBinding("<XRHMD>/centerEyePosition");
                        }
                        if (!posAction.enabled) posAction.Enable();
                    }

                    var rotAction = poseDriver.rotationInput.action;
                    if (rotAction != null)
                    {
                        if (rotAction.bindings.Count == 0)
                        {
                            if (rotAction.enabled) rotAction.Disable();
                            rotAction.AddBinding("<HandheldARInputDevice>/deviceRotation");
                            rotAction.AddBinding("<XRHMD>/centerEyeRotation");
                        }
                        if (!rotAction.enabled) rotAction.Enable();
                    }

                    var trackAction = poseDriver.trackingStateInput.action;
                    if (trackAction != null)
                    {
                        if (trackAction.bindings.Count == 0)
                        {
                            if (trackAction.enabled) trackAction.Disable();
                            trackAction.AddBinding("<HandheldARInputDevice>/trackingState");
                            trackAction.AddBinding("<XRHMD>/trackingState");
                        }
                        if (!trackAction.enabled) trackAction.Enable();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MachineryARPlacementController] Exception in EnsureCameraTracking (safe): {ex.Message}");
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            EnsureCameraTracking();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            if (isPlaced) return;

            if (machineryScenarioTarget == null)
            {
                machineryScenarioTarget = GameObject.Find("MachineryScenario");
                if (machineryScenarioTarget != null) machineryScenarioTarget.SetActive(false);
            }

            // Animate bottom placement button pulse
            if (placeButtonRT != null)
            {
                float btnPulse = 1.0f + Mathf.Sin(Time.time * 3.5f) * 0.025f;
                placeButtonRT.localScale = new Vector3(btnPulse, btnPulse, 1f);
            }

            if (!isScanning) return;

            UpdatePlacementPosition();
            CheckForTap();
        }

        private void UpdatePlacementPosition()
        {
            hasValidPose = false;
            currentPlane = null;

            EnsureCamera();
            if (raycastManager == null || arCamera == null) return;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            RaycastHits.Clear();

            TrackableType trackTypes = TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds | TrackableType.PlaneEstimated | TrackableType.Planes;
            bool hit = raycastManager.Raycast(screenCenter, RaycastHits, trackTypes);
            if (!hit || RaycastHits.Count == 0)
            {
                if (Application.isEditor)
                {
                    Camera cam = arCamera != null ? arCamera : Camera.main;
                    if (cam != null)
                    {
                        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                        Plane floorPlane = new Plane(Vector3.up, Vector3.zero);
                        if (floorPlane.Raycast(ray, out float enter))
                        {
                            Vector3 hitPoint = ray.GetPoint(enter);
                            currentPose = new Pose(hitPoint, Quaternion.identity);
                            currentPlane = null;
                            hasValidPose = true;
                            UpdateIndicator();
                            return;
                        }
                    }
                }

                hasValidPose = false;
                if (placementIndicator != null)
                {
                    placementIndicator.SetActive(false);
                }
                return;
            }

            var hitResult = RaycastHits[0];
            currentPose = hitResult.pose;
            if (planeManager != null)
            {
                currentPlane = planeManager.GetPlane(hitResult.trackableId);
            }
            hasValidPose = true;

            UpdateIndicator();
        }

        private GameObject indicatorDisc;

        private void CreatePlacementIndicator()
        {
            if (placementIndicator != null)
            {
                Destroy(placementIndicator);
            }

            placementIndicator = new GameObject("MachineryRuntimePlacementIndicator");
            placementIndicator.transform.SetParent(null, false);

            // 1. Outer electric blue ring (LineRenderer)
            indicatorLine = placementIndicator.AddComponent<LineRenderer>();
            indicatorLine.useWorldSpace = false;
            indicatorLine.loop = true;
            indicatorLine.startWidth = 0.04f;
            indicatorLine.endWidth = 0.04f;
            indicatorLine.numCapVertices = 8;
            indicatorLine.numCornerVertices = 8;

            Shader ringShader = Shader.Find("Mobile/Particles/Alpha Blended")
                             ?? Shader.Find("Sprites/Default")
                             ?? Shader.Find("Universal Render Pipeline/Unlit")
                             ?? Shader.Find("Unlit/Color")
                             ?? Shader.Find("Standard");

            Material ringMat = ringShader != null ? new Material(ringShader) : new Material(Shader.Find("Sprites/Default"));
            Color brightBlue = new Color(0.0f, 0.80f, 1.0f, 0.95f);
            ringMat.color = brightBlue;
            if (ringMat.HasProperty("_BaseColor")) ringMat.SetColor("_BaseColor", brightBlue);
            if (ringMat.HasProperty("_EmissionColor"))
            {
                ringMat.EnableKeyword("_EMISSION");
                ringMat.SetColor("_EmissionColor", brightBlue);
            }
            indicatorLine.material = ringMat;

            float radius = 0.28f;
            int segments = 48;
            indicatorLine.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                indicatorLine.SetPosition(i, new Vector3(x, 0.002f, z));
            }

            // 2. Inner glowing flat blue disk
            indicatorDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicatorDisc.name = "IndicatorDisc";
            indicatorDisc.transform.SetParent(placementIndicator.transform, false);
            indicatorDisc.transform.localPosition = new Vector3(0f, 0.001f, 0f);
            indicatorDisc.transform.localScale = new Vector3(radius * 1.8f, 0.001f, radius * 1.8f);

            var col = indicatorDisc.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var discRenderer = indicatorDisc.GetComponent<Renderer>();
            if (discRenderer != null)
            {
                Material discMat = ringShader != null ? new Material(ringShader) : new Material(ringMat);
                Color discColor = new Color(0.0f, 0.65f, 1.0f, 0.35f);
                discMat.color = discColor;
                if (discMat.HasProperty("_BaseColor")) discMat.SetColor("_BaseColor", discColor);
                discRenderer.material = discMat;
            }

            placementIndicator.SetActive(false);
        }

        private void UpdateIndicator()
        {
            if (placementIndicator == null) return;

            if (!hasValidPose)
            {
                placementIndicator.SetActive(false);
                return;
            }

            placementIndicator.SetActive(true);
            placementIndicator.transform.position = currentPose.position + Vector3.up * 0.005f;
            placementIndicator.transform.rotation = currentPose.rotation;

            // Gentle pulsing animation to clearly show it's active and ready for tap
            float pulse = 1.0f + Mathf.Sin(Time.time * 4f) * 0.04f;
            placementIndicator.transform.localScale = new Vector3(pulse, 1f, pulse);
        }

        private void CheckForTap()
        {
            if (isPlaced) return;

            bool tapBegan = false;
            Vector2 touchPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            // 1. New Input System Touchscreen
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                tapBegan = true;
                touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            // 2. EnhancedTouch
            if (!tapBegan && EnhancedTouchSupport.enabled && Touch.activeTouches.Count > 0)
            {
                Touch touch = Touch.activeTouches[0];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    tapBegan = true;
                    touchPosition = touch.screenPosition;
                }
            }

            // 3. Legacy Input.touches
            if (!tapBegan && Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                if (t.phase == UnityEngine.TouchPhase.Began)
                {
                    tapBegan = true;
                    touchPosition = t.position;
                }
            }

            // 4. Mouse / Editor tap simulation
            if (!tapBegan && Input.GetMouseButtonDown(0))
            {
                tapBegan = true;
                touchPosition = Input.mousePosition;
            }

            if (!tapBegan) return;

            Debug.Log($"[MachineryARPlacementController] Screen tap detected at {touchPosition}. Placing machinery now!");
            PlaceMachineryNow(touchPosition);
        }

        public void PlaceMachineryNow(Vector2? screenPos = null)
        {
            if (isPlaced) return;

            // 1. Direct placement if indicator is currently active on floor
            if (hasValidPose)
            {
                Debug.Log("[MachineryARPlacementController] Placing machinery directly at active indicator pose.");
                PlaceMachineryScenario(currentPose, currentPlane);
                return;
            }

            // 2. Perform raycast on tap position or screen center
            Vector2 raycastPos = screenPos ?? new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (raycastManager != null)
            {
                RaycastHits.Clear();
                TrackableType trackTypes = TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds | TrackableType.PlaneEstimated | TrackableType.Planes;
                if (raycastManager.Raycast(raycastPos, RaycastHits, trackTypes) && RaycastHits.Count > 0)
                {
                    var hitResult = RaycastHits[0];
                    Pose hitPose = hitResult.pose;
                    ARPlane hitPlane = planeManager != null ? planeManager.GetPlane(hitResult.trackableId) : null;
                    PlaceMachineryScenario(hitPose, hitPlane);
                    return;
                }

                // Try screen center fallback
                Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                if (raycastManager.Raycast(center, RaycastHits, trackTypes) && RaycastHits.Count > 0)
                {
                    var hitResult = RaycastHits[0];
                    Pose hitPose = hitResult.pose;
                    ARPlane hitPlane = planeManager != null ? planeManager.GetPlane(hitResult.trackableId) : null;
                    PlaceMachineryScenario(hitPose, hitPlane);
                    return;
                }
            }

            // 3. RELIABLE FALLBACK PLACEMENT:
            // If the user tapped anywhere or pressed the button and plane detection hasn't returned a polygon yet,
            // place the machinery 1.8m in front of the camera at floor level so it is 100% GUARANTEED to appear!
            EnsureCamera();
            Camera cam = arCamera != null ? arCamera : Camera.main;
            Vector3 camPos = cam != null ? cam.transform.position : Vector3.up * 1.4f;
            Vector3 camFwd = cam != null ? cam.transform.forward : Vector3.forward;
            camFwd.y = 0f;
            if (camFwd.sqrMagnitude < 0.001f) camFwd = Vector3.forward;
            camFwd.Normalize();

            float floorY = camPos.y > 0.5f ? camPos.y - 1.2f : 0f;
            Vector3 fallbackPos = camPos + camFwd * 1.8f;
            fallbackPos.y = floorY;

            Quaternion fallbackRot = Quaternion.LookRotation(camFwd, Vector3.up);
            Pose fallbackPose = new Pose(fallbackPos, fallbackRot);

            Debug.Log($"[MachineryARPlacementController] Fallback placing machinery 1.8m in front of camera at {fallbackPos}");
            PlaceMachineryScenario(fallbackPose, null);
        }

        private void PlaceMachineryScenario(Pose hitPose, ARPlane hitPlane)
        {
            if (machineryScenarioTarget == null) return;

            // 1. Activate scenario target and ensure all child renderers & GameObjects are enabled
            machineryScenarioTarget.SetActive(true);

            Renderer[] renderers = machineryScenarioTarget.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogWarning("[MachineryARPlacementController] No renderers found under MachineryScenario!");
                return;
            }

            int machineryLayerMask = 0;
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                r.enabled = true;
                if (!r.gameObject.activeSelf)
                {
                    r.gameObject.SetActive(true);
                }
                machineryLayerMask |= (1 << r.gameObject.layer);
            }

            // Ensure AR Camera renders all machinery layers with proper clipping planes
            EnsureCamera();
            if (arCamera != null)
            {
                arCamera.cullingMask |= machineryLayerMask;
                if (arCamera.nearClipPlane > 0.05f) arCamera.nearClipPlane = 0.05f;
                if (arCamera.farClipPlane < 50f) arCamera.farClipPlane = 50f;
            }

            // Calculate combined bounds in machineryScenarioTarget's LOCAL space
            Bounds localCombinedBounds = new Bounds();
            bool hasBounds = false;
            Matrix4x4 rootWorldToLocal = machineryScenarioTarget.transform.worldToLocalMatrix;

            foreach (Renderer r in renderers)
            {
                if (r == null || !r.enabled) continue;

                MeshFilter mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    Bounds b = mf.sharedMesh.bounds;
                    Matrix4x4 localMatrix = rootWorldToLocal * r.transform.localToWorldMatrix;

                    Vector3 min = b.min;
                    Vector3 max = b.max;
                    Vector3[] corners = new Vector3[8]
                    {
                        new Vector3(min.x, min.y, min.z),
                        new Vector3(min.x, min.y, max.z),
                        new Vector3(min.x, max.y, min.z),
                        new Vector3(min.x, max.y, max.z),
                        new Vector3(max.x, min.y, min.z),
                        new Vector3(max.x, min.y, max.z),
                        new Vector3(max.x, max.y, min.z),
                        new Vector3(max.x, max.y, max.z)
                    };

                    for (int c = 0; c < 8; c++)
                    {
                        Vector3 pt = localMatrix.MultiplyPoint(corners[c]);
                        if (!hasBounds)
                        {
                            localCombinedBounds = new Bounds(pt, Vector3.zero);
                            hasBounds = true;
                        }
                        else
                        {
                            localCombinedBounds.Encapsulate(pt);
                        }
                    }
                }
                else
                {
                    Bounds rb = r.bounds;
                    Vector3 min = rb.min;
                    Vector3 max = rb.max;
                    Vector3[] corners = new Vector3[8]
                    {
                        new Vector3(min.x, min.y, min.z),
                        new Vector3(min.x, min.y, max.z),
                        new Vector3(min.x, max.y, min.z),
                        new Vector3(min.x, max.y, max.z),
                        new Vector3(max.x, min.y, min.z),
                        new Vector3(max.x, min.y, max.z),
                        new Vector3(max.x, max.y, min.z),
                        new Vector3(max.x, max.y, max.z)
                    };

                    for (int c = 0; c < 8; c++)
                    {
                        Vector3 pt = rootWorldToLocal.MultiplyPoint(corners[c]);
                        if (!hasBounds)
                        {
                            localCombinedBounds = new Bounds(pt, Vector3.zero);
                            hasBounds = true;
                        }
                        else
                        {
                            localCombinedBounds.Encapsulate(pt);
                        }
                    }
                }
            }

            float widthX = localCombinedBounds.size.x;
            float depthZ = localCombinedBounds.size.z;
            float maxHorizSpan = Mathf.Max(widthX, depthZ);
            Debug.Log($"[MachineryARPlacementController] Local Bounds: Size={localCombinedBounds.size}, MinY={localCombinedBounds.min.y}, MinZ={localCombinedBounds.min.z}, MaxHorizSpan={maxHorizSpan}");

            // 2. Determine runtime-only scale so complete machinery fits human/room scale cleanly (~1.6 meters wide)
            float targetWidth = 1.6f;
            float runtimeScale = 1.0f;
            if (maxHorizSpan > targetWidth && targetWidth > 0f)
            {
                runtimeScale = targetWidth / maxHorizSpan;
            }
            runtimeScale = Mathf.Clamp(runtimeScale, 0.15f, 1.0f);

            // 3. Determine initial user-facing orientation
            Vector3 cameraForward = Vector3.forward;
            if (arCamera != null)
            {
                cameraForward = arCamera.transform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();
            }
            if (cameraForward.sqrMagnitude < 0.001f)
            {
                cameraForward = Vector3.forward;
            }
            Quaternion initialRotation = Quaternion.LookRotation(cameraForward, Vector3.up);

            // 4. Create dedicated world-space placement root at hitPose on detected plane
            placementRoot = new GameObject("MachineryARPlacementRoot");
            placementRoot.transform.position = hitPose.position;
            placementRoot.transform.rotation = initialRotation;
            placementRoot.transform.localScale = Vector3.one * runtimeScale;

            // Attach ARAnchor to anchor the root in world space
            if (anchorManager != null && hitPlane != null)
            {
                var anchor = anchorManager.AttachAnchor(hitPlane, hitPose);
                if (anchor != null)
                {
                    placementRoot.transform.SetParent(anchor.transform, true);
                }
            }

            // 5. Parent machineryScenarioTarget under placementRoot with exact bounds alignment:
            // - Bottom of lowest renderer rests EXACTLY on detected floor plane (y = -localCombinedBounds.min.y)
            // - Horizontal X and Z centered exactly on the blue circle placement point
            machineryScenarioTarget.transform.SetParent(placementRoot.transform, false);
            machineryScenarioTarget.transform.localPosition = new Vector3(
                -localCombinedBounds.center.x,
                -localCombinedBounds.min.y,
                -localCombinedBounds.center.z
            );
            machineryScenarioTarget.transform.localRotation = Quaternion.identity;
            machineryScenarioTarget.SetActive(true);

            isPlaced = true;

            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }

            if (bottomButtonCanvas != null)
            {
                Destroy(bottomButtonCanvas);
            }

            if (hidePlanesAfterPlacement)
            {
                HidePlanes();
            }

            // Raise central placement event
            TrainingEventManager.RaiseScenarioPlaced();

            if (MachinerySession.Instance != null)
            {
                MachinerySession.Instance.SetState(MachinerySession.SessionState.Placed);
            }

            if (MachineryScenarioUIController.Instance != null)
            {
                MachineryScenarioUIController.Instance.OnMachineryPlaced();
            }

            Debug.Log($"[MachineryARPlacementController] MachineryScenario cleanly placed in AR world space at {hitPose.position} with scale {runtimeScale:F3}.");
        }

        private void HidePlanes()
        {
            if (planeManager == null) return;

            foreach (ARPlane plane in planeManager.trackables)
            {
                if (plane != null)
                {
                    plane.gameObject.SetActive(false);
                }
            }
            planeManager.enabled = false;
        }

        public void ResetPlacement()
        {
            if (machineryScenarioTarget != null)
            {
                machineryScenarioTarget.SetActive(false);
                machineryScenarioTarget.transform.SetParent(null, true);
            }

            if (placementRoot != null)
            {
                Destroy(placementRoot);
                placementRoot = null;
            }

            if (planeManager != null)
            {
                planeManager.enabled = true;
                foreach (ARPlane plane in planeManager.trackables)
                {
                    if (plane != null) plane.gameObject.SetActive(true);
                }
            }

            isPlaced = false;

            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }

            CreateBottomPlacementButton();

            if (MachinerySession.Instance != null)
            {
                MachinerySession.Instance.SetState(MachinerySession.SessionState.SearchingForPlanes);
            }

            if (MachineryScenarioUIController.Instance != null)
            {
                MachineryScenarioUIController.Instance.OnPlacementReset();
            }

            Debug.Log("[MachineryARPlacementController] AR Placement Reset. Ready for plane detection.");
        }

        private void OnDestroy()
        {
            if (placementIndicator != null)
            {
                Destroy(placementIndicator);
            }
            if (placementRoot != null)
            {
                Destroy(placementRoot);
            }
        }
    }
}
