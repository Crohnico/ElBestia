using System;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionMovement
    {
        private readonly Transform ownerTransform;
        private readonly Func<bool> isCounterattacking;
        private readonly Func<string> getOwnerLabel;
        private readonly Func<int> getFlowId;
        private readonly Func<bool> isDebugEnabled;
        private readonly ChampionMoveToRange moveToRange;

        private CombatCrono crono;
        private float turnSpeedDegrees;
        private float actionAnimationSeconds;
        private float actionMovementFloat;
        private readonly ChampionReturnHomeMovement returnHomeMovement;
        private Action pendingAsyncComplete;
        private float actionAnimationEndsAt;
        private float nextPendingStateLogAt;
        private string pendingStateLabel = string.Empty;

        public ChampionMovement(
            Transform ownerTransform,
            Func<ChampionBehaviour> getRival,
            Func<bool> isOwnerAlive,
            Func<bool> isCounterattacking,
            Func<string> getOwnerLabel,
            Func<int> getFlowId,
            Func<bool> isDebugEnabled)
        {
            this.ownerTransform = ownerTransform;
            this.isCounterattacking = isCounterattacking;
            this.getOwnerLabel = getOwnerLabel;
            this.getFlowId = getFlowId;
            this.isDebugEnabled = isDebugEnabled;
            returnHomeMovement = new ChampionReturnHomeMovement(ownerTransform, getOwnerLabel, getFlowId, isDebugEnabled);
            moveToRange = new ChampionMoveToRange(ownerTransform, getRival, isOwnerAlive, getOwnerLabel, getFlowId, isDebugEnabled);
        }

        public float MovementFloat => Mathf.Max(actionMovementFloat, Mathf.Max(returnHomeMovement.MovementFloat, moveToRange.MovementFloat));

        public void Initialize(
            Transform homePosition,
            CombatCrono combatCrono,
            float moveSpeed,
            float turnSpeedDegrees,
            float minimumVisualRange,
            float actionAnimationSeconds)
        {
            crono = combatCrono;
            Vector3 homeWorldPosition = ownerTransform.position;
            float lockedHeight = ownerTransform.position.y;
            this.turnSpeedDegrees = turnSpeedDegrees;
            this.actionAnimationSeconds = actionAnimationSeconds;
            ForceClearAsyncStep();

            if (homePosition != null)
            {
                homeWorldPosition = homePosition.position;
                homeWorldPosition.y = lockedHeight;
            }

            returnHomeMovement.Initialize(homeWorldPosition, lockedHeight, moveSpeed, turnSpeedDegrees);
            moveToRange.Initialize(lockedHeight, moveSpeed, turnSpeedDegrees, minimumVisualRange);
        }

        public void Update()
        {
            UpdateActionFlow();
            returnHomeMovement.Update(crono, IsCombatMovementActive());
        }

        public void SetCrono(CombatCrono combatCrono)
        {
            crono = combatCrono;
        }

        public void MoveIntoSkillRange(SkillData skill, Action onComplete)
        {
            moveToRange.Begin(skill, onComplete);
        }

        public void PlayActionAnimation(Action onComplete)
        {
            actionMovementFloat = 0f;
            if (actionAnimationSeconds > 0f)
            {
                pendingStateLabel = "AnimWait";
                nextPendingStateLogAt = Time.unscaledTime + 1f;
                actionAnimationEndsAt = Time.unscaledTime + actionAnimationSeconds;
                Debug("Anim", $"Start wait={actionAnimationSeconds:0.00} endAt={actionAnimationEndsAt:0.00}");
                pendingAsyncComplete = onComplete;
                return;
            }

            Debug("Anim", "Instant complete");
            onComplete?.Invoke();
        }

        public void StartReturnHome()
        {
            returnHomeMovement.Start();
        }

        public void CancelReturnHome()
        {
            returnHomeMovement.Cancel();
            actionMovementFloat = 0f;
        }

        public void FaceTarget(ChampionBehaviour target, float deltaTime)
        {
            if (target == null)
            {
                return;
            }

            FaceMovementDirection(target.transform.position - ownerTransform.position, Mathf.Max(deltaTime, 0.016f));
        }

        public void ForceClearAsyncStep()
        {
            pendingAsyncComplete = null;
            actionAnimationEndsAt = 0f;
            pendingStateLabel = string.Empty;
            nextPendingStateLogAt = 0f;
            actionMovementFloat = 0f;
            moveToRange.ForceClear();
            Debug("Async", "Force clear");
        }

        public string BuildDebugStateSummary()
        {
            string pendingLabel = moveToRange.HasPending ? moveToRange.PendingLabel : pendingStateLabel;
            return $"returning={returnHomeMovement.IsReturning} pending={pendingLabel} hasPending={pendingAsyncComplete != null || moveToRange.HasPending} moveSkill={moveToRange.PendingSkillName} animEnd={actionAnimationEndsAt:0.00} dist={GetRivalDistance():0.00}";
        }

        public float GetRivalDistance()
        {
            return moveToRange.GetRivalDistance();
        }

        private void UpdateActionFlow()
        {
            if (moveToRange.HasPending)
            {
                moveToRange.Update();
                return;
            }

            UpdateActionAnimationStep();
        }

        private void UpdateActionAnimationStep()
        {
            if (pendingAsyncComplete == null || actionAnimationEndsAt <= 0f || Time.unscaledTime < actionAnimationEndsAt)
            {
                if (pendingAsyncComplete != null && actionAnimationEndsAt > 0f && Time.unscaledTime >= nextPendingStateLogAt)
                {
                    nextPendingStateLogAt = Time.unscaledTime + 1f;
                    Debug("Anim", $"Waiting now={Time.unscaledTime:0.00} endAt={actionAnimationEndsAt:0.00} delta={Time.unscaledDeltaTime:0.000}");
                }

                return;
            }

            actionAnimationEndsAt = 0f;
            Debug("Anim", "Complete");
            CompleteAsyncStep();
        }

        private void CompleteAsyncStep()
        {
            Action complete = pendingAsyncComplete;
            pendingAsyncComplete = null;
            pendingStateLabel = string.Empty;
            nextPendingStateLogAt = 0f;
            actionMovementFloat = 0f;
            Debug("Async", "Complete pending step");
            complete?.Invoke();
        }

        private bool IsCombatMovementActive()
        {
            return isCounterattacking() || pendingAsyncComplete != null || moveToRange.HasPending || actionAnimationEndsAt > 0f;
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

            CombatDebug.Log(nameof(ChampionMovement), getOwnerLabel(), step, message, CombatDebug.ChampionColor, getFlowId());
        }

    }
}
