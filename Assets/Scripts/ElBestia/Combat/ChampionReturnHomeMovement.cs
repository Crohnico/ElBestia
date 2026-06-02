using System;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionReturnHomeMovement
    {
        private readonly Transform ownerTransform;
        private readonly Func<string> getOwnerLabel;
        private readonly Func<int> getFlowId;
        private readonly Func<bool> isDebugEnabled;
        private Vector3 homeWorldPosition;
        private float lockedHeight;
        private float moveSpeed;
        private float turnSpeedDegrees;

        public ChampionReturnHomeMovement(
            Transform ownerTransform,
            Func<string> getOwnerLabel,
            Func<int> getFlowId,
            Func<bool> isDebugEnabled)
        {
            this.ownerTransform = ownerTransform;
            this.getOwnerLabel = getOwnerLabel;
            this.getFlowId = getFlowId;
            this.isDebugEnabled = isDebugEnabled;
        }

        public bool IsReturning { get; private set; }
        public float MovementFloat { get; private set; }

        public void Initialize(Vector3 homeWorldPosition, float lockedHeight, float moveSpeed, float turnSpeedDegrees)
        {
            this.homeWorldPosition = homeWorldPosition;
            this.lockedHeight = lockedHeight;
            this.moveSpeed = moveSpeed;
            this.turnSpeedDegrees = turnSpeedDegrees;
            IsReturning = false;
            MovementFloat = 0f;
        }

        public void Start()
        {
            IsReturning = FlatDistance(ownerTransform.position, homeWorldPosition) > 0.02f;
            Debug("ReturnHome", $"Start returning={IsReturning} distance={FlatDistance(ownerTransform.position, homeWorldPosition):0.00}");
        }

        public void Cancel()
        {
            IsReturning = false;
            MovementFloat = 0f;
            Debug("ReturnHome", "Cancel");
        }

        public void Update(CombatCrono crono, bool isCombatMovementActive)
        {
            if (!IsReturning)
            {
                return;
            }

            if (isCombatMovementActive)
            {
                MovementFloat = 0f;
                return;
            }

            float deltaTime = crono != null ? crono.DeltaTime : Time.deltaTime;
            if (deltaTime <= 0f)
            {
                MovementFloat = 0f;
                return;
            }

            MoveTowards(homeWorldPosition, deltaTime);
            if (FlatDistance(ownerTransform.position, homeWorldPosition) <= 0.02f)
            {
                Vector3 position = homeWorldPosition;
                position.y = lockedHeight;
                ownerTransform.position = position;
                IsReturning = false;
                MovementFloat = 0f;
                Debug("ReturnHome", "Complete");
            }
        }

        private void MoveTowards(Vector3 targetPosition, float deltaTime)
        {
            Vector3 current = ownerTransform.position;
            targetPosition.y = lockedHeight;
            Vector3 next = Vector3.MoveTowards(current, targetPosition, moveSpeed * deltaTime);
            next.y = lockedHeight;

            Vector3 direction = next - current;
            FaceMovementDirection(direction, deltaTime);
            ownerTransform.position = next;
            MovementFloat = direction.sqrMagnitude > 0.000001f ? 1f : 0f;
        }

        private void FaceMovementDirection(Vector3 direction, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            ownerTransform.rotation = Quaternion.RotateTowards(ownerTransform.rotation, targetRotation, turnSpeedDegrees * deltaTime);
        }

        private void Debug(string step, string message)
        {
            if (!isDebugEnabled())
            {
                return;
            }

            CombatDebug.Log(nameof(ChampionReturnHomeMovement), getOwnerLabel(), step, message, CombatDebug.ChampionColor, getFlowId());
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
