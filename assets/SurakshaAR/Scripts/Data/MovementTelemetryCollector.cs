using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Data
{
    /// <summary>
    /// MovementTelemetryCollector
    /// ===========================
    /// High-resolution spatial and kinematic telemetry logger.
    /// Samples AR camera pose, distance to fire hazard, aim vector,
    /// and device orientation at a controlled rate (5Hz) during active scenarios.
    /// Feeds behavioral analysis and spatial positioning compliance models.
    /// </summary>
    [DisallowMultipleComponent]
    public class MovementTelemetryCollector : MonoBehaviour
    {
        public static MovementTelemetryCollector Instance { get; private set; }

        [Header("Sampling Configuration")]
        [Tooltip("Sampling rate in Hz (default 5Hz = 200ms)")]
        [SerializeField] private float _sampleRateHz = 5f;
        [SerializeField] private bool _isCollecting = false;

        [Header("Hazard References")]
        [SerializeField] private Transform _hazardTarget;
        [SerializeField] private Camera _arCamera;

        [Serializable]
        public struct MovementSample
        {
            public float timestamp;
            public Vector3 cameraPosition;
            public Vector3 cameraForward;
            public float distanceToHazard;
            public float angleDeviationDegrees;
            public bool isSafeDistance; // 1.5m - 3.5m safe operating zone
        }

        private readonly List<MovementSample> _sampleBuffer = new List<MovementSample>(128);
        private Coroutine _sampleRoutine;

        public bool IsCollecting => _isCollecting;
        public IReadOnlyList<MovementSample> Samples => _sampleBuffer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            if (_arCamera == null) _arCamera = Camera.main;
        }

        public void SetHazardTarget(Transform target)
        {
            _hazardTarget = target;
        }

        public void StartCollection(Transform hazardTarget = null)
        {
            if (hazardTarget != null) _hazardTarget = hazardTarget;
            if (_arCamera == null) _arCamera = Camera.main;

            _sampleBuffer.Clear();
            _isCollecting = true;

            if (_sampleRoutine != null) StopCoroutine(_sampleRoutine);
            _sampleRoutine = StartCoroutine(CollectRoutine());
            Debug.Log("[TELEMETRY] Movement telemetry collection started at 5Hz.");
        }

        public void StopCollection()
        {
            _isCollecting = false;
            if (_sampleRoutine != null)
            {
                StopCoroutine(_sampleRoutine);
                _sampleRoutine = null;
            }
            Debug.Log($"[TELEMETRY] Movement telemetry stopped. Total samples recorded: {_sampleBuffer.Count}");
        }

        private IEnumerator CollectRoutine()
        {
            float interval = 1f / Mathf.Max(1f, _sampleRateHz);

            while (_isCollecting)
            {
                RecordSample();
                yield return new WaitForSeconds(interval);
            }
        }

        private void RecordSample()
        {
            if (_arCamera == null)
            {
                _arCamera = Camera.main;
                if (_arCamera == null) return;
            }

            Vector3 camPos = _arCamera.transform.position;
            Vector3 camFwd = _arCamera.transform.forward;

            float dist = -1f;
            float angle = 0f;
            bool safe = false;

            if (_hazardTarget != null)
            {
                Vector3 toTarget = _hazardTarget.position - camPos;
                dist = toTarget.magnitude;
                angle = Vector3.Angle(camFwd, toTarget.normalized);
                safe = dist >= 1.5f && dist <= 3.5f;
            }

            var sample = new MovementSample
            {
                timestamp = Time.time,
                cameraPosition = camPos,
                cameraForward = camFwd,
                distanceToHazard = dist,
                angleDeviationDegrees = angle,
                isSafeDistance = safe
            };

            _sampleBuffer.Add(sample);
        }
    }
}
