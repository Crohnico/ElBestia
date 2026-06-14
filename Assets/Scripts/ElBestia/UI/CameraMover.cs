using System;
using System.Collections.Generic;
using DG.Tweening;
using ElBestia.Core;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class CameraMover : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform pivotTransform;
        [SerializeField] private CameraSnapshot[] snapshots = Array.Empty<CameraSnapshot>();
        [SerializeField] private float moveDuration = 0.45f;
        [SerializeField] private Ease moveEase = Ease.InOutSine;

        private readonly Dictionary<CameraSnapshotId, Transform> snapshotsById = new Dictionary<CameraSnapshotId, Transform>();
        private IDisposable subscription;
        private Tween activeTween;
        private CameraSnapshotId? currentSnapshotId;
        private CameraSnapshotId? activeTargetId;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            BuildSnapshotLookup();
        }

        private void OnEnable()
        {
            subscription = SignalBus.Subscribe<CameraMoveSignal>(OnCameraMove);
        }

        private void OnDisable()
        {
            subscription?.Dispose();
            subscription = null;

            activeTween?.Kill();
            activeTween = null;
            activeTargetId = null;
        }

        private void BuildSnapshotLookup()
        {
            snapshotsById.Clear();

            for (int i = 0; i < snapshots.Length; i++)
            {
                CameraSnapshot snapshot = snapshots[i];
                if (snapshot == null || snapshot.transform == null)
                {
                    continue;
                }

                snapshotsById[snapshot.id] = snapshot.transform;
            }
        }

        private void OnCameraMove(CameraMoveSignal signal)
        {
            if (targetCamera == null)
            {
                Debug.LogWarning("CameraMover cannot move because Target Camera is not assigned.");
                return;
            }

            if (!snapshotsById.TryGetValue(signal.Target, out Transform targetSnapshot))
            {
                Debug.LogWarning($"CameraMover has no snapshot configured for {signal.Target}.");
                return;
            }

            if (currentSnapshotId == signal.Target || activeTargetId == signal.Target)
            {
                if (currentSnapshotId == signal.Target)
                {
                    SignalBus.Fire(new CameraMoveCompletedSignal(signal.Target));
                }

                return;
            }

            MoveTo(signal.Target, targetSnapshot);
        }

        private void MoveTo(CameraSnapshotId targetId, Transform targetSnapshot)
        {
            activeTween?.Kill();
            activeTargetId = targetId;

            Transform cameraTransform = targetCamera.transform;
            Sequence sequence = DOTween.Sequence().SetTarget(cameraTransform);
            float pathDuration = pivotTransform != null ? moveDuration * 2f : moveDuration;

            if (pivotTransform != null)
            {
                Vector3 startPosition = cameraTransform.position;
                Vector3 pivotPosition = pivotTransform.position;
                Vector3 targetPosition = targetSnapshot.position;

                sequence.Join(DOVirtual.Float(0f, 1f, pathDuration, t =>
                {
                    cameraTransform.position = EvaluateQuadraticBezier(startPosition, pivotPosition, targetPosition, t);
                }).SetEase(moveEase));
            }
            else
            {
                Debug.LogWarning("CameraMover has no Pivot Transform configured. Moving directly to destination.");
                sequence.Join(cameraTransform.DOMove(targetSnapshot.position, pathDuration).SetEase(moveEase));
            }

            sequence.Join(cameraTransform.DORotateQuaternion(targetSnapshot.rotation, pathDuration).SetEase(moveEase));
            sequence.OnComplete(() =>
            {
                currentSnapshotId = targetId;
                activeTargetId = null;
                activeTween = null;
                SignalBus.Fire(new CameraMoveCompletedSignal(targetId));
            });

            activeTween = sequence;
        }

        private static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            float inverseT = 1f - t;
            return inverseT * inverseT * start
                + 2f * inverseT * t * control
                + t * t * end;
        }
    }
}
